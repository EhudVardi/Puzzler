#!/usr/bin/env python3
"""
yakugo_generate.py
==================
Generates a Yakugo puzzle JSON from a vocabulary (word-pair list).

Yakugo is a translation crossword: clue cells hold a source-language word and
its target-language translation; letter cells are shared by crossing words
exactly like a crossword, so every character must satisfy all groups that pass
through it.

The placement algorithm is greedy with random tiebreaking and several
quality improvements over a naive greedy placer:

  1. First word: pick the "anchor" (longest word whose letters are most common
     in the rest of the pool) and place it centred in the grid.
  2. Placement score = current_intersections + λ·lookahead_score, where
     lookahead counts how many remaining words share at least one letter with
     each new cell.
  3. best_of() uses a composite quality metric (crossings, density, wasted
     cells, isolated words) rather than just word count.
  4. Restart / backtrack: if a greedy pass stalls, undo the last K placements
     and retry with the remaining words re-shuffled (up to 3 retries/seed).

Usage
-----
  # Generate a single puzzle from a vocabulary file:
  python yakugo_generate.py wordlist.json output.json --rows 7 --cols 7

  # Try 20 seeds and keep the result that places the most words:
  python yakugo_generate.py wordlist.json output.json --rows 7 --cols 7 --count 20

  # Print the puzzle JSON to stdout:
  python yakugo_generate.py wordlist.json - --rows 5 --cols 5 --preview

  # Generate all built-in example puzzles into <outdir>:
  python yakugo_generate.py --examples [--outdir path]
"""

import copy
import json
import argparse
import os
import sys
import random
from collections import Counter
from typing import Any, Dict, List, Optional, Tuple

# Hebrew output requires UTF-8; reconfigure stdout if the terminal uses a
# narrow encoding (common on Windows with the default cp1252 console).
if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")

# ── Types ──────────────────────────────────────────────────────────────────────

Word = Dict[str, str]   # {"Source": "...", "Target": "..."}

DIRS: Dict[str, Tuple[int, int]] = {
    "Right": (0,  1),
    "Down":  (1,  0),
    "Left":  (0, -1),
}

RTL_LANGUAGES = {"he", "ar", "fa", "ur"}

# Lookahead weight: contribution of potential-future-word letters per new cell
LOOKAHEAD_WEIGHT = 0.15

# Quality metric weights for best_of scoring
W_PLACED    = 1.0   # words placed
W_CROSSINGS = 0.6   # total intersections across all placed words
W_DENSITY   = 2.0   # letter_cells / total_cells
W_WASTED    = 0.3   # penalty per wasted cell
W_ISOLATED  = 0.5   # penalty per isolated word (0 crossings)

# Backtrack parameters
MAX_RETRIES     = 3
BACKTRACK_STEPS = [3, 5, 8]   # undo this many placements per retry attempt


def horizontal_dir(target_lang: str) -> str:
    """Return the correct horizontal direction for the target language."""
    return "Left" if target_lang in RTL_LANGUAGES else "Right"


# ── Core helpers ───────────────────────────────────────────────────────────────

def strip_spaces(s: str) -> str:
    """Remove spaces and hyphens — these are skipped when filling letter cells."""
    return "".join(c for c in s if c not in " -")


def letter_positions(origin_r: int, origin_c: int,
                     direction: str, n: int) -> List[Tuple[int, int]]:
    """Return the n letter-cell positions that follow the clue cell in direction."""
    dr, dc = DIRS[direction]
    return [(origin_r + dr * (i + 1), origin_c + dc * (i + 1)) for i in range(n)]


# ── Placement state ────────────────────────────────────────────────────────────

class State:
    """Mutable grid state built up word by word."""

    def __init__(self) -> None:
        # letter cells: (r, c) -> character placed there
        self.grid: Dict[Tuple[int, int], str] = {}
        # clue cell origins: (r, c) -> list of {dir, src, tgt}
        self.clue_cells: Dict[Tuple[int, int], List[Dict[str, str]]] = {}
        # positions that must stay void — the cell immediately past each word's last letter
        self.end_guards: set = set()
        # per-placed-word intersection count (same order as placement)
        self.word_crossings: List[int] = []

    @property
    def letter_cells(self) -> set:
        return set(self.grid.keys())

    def dirs_at(self, pos: Tuple[int, int]) -> set:
        return {e["dir"] for e in self.clue_cells.get(pos, [])}

    def snapshot(self) -> "State":
        s = State()
        s.grid = dict(self.grid)
        s.clue_cells = {k: list(v) for k, v in self.clue_cells.items()}
        s.end_guards = set(self.end_guards)
        s.word_crossings = list(self.word_crossings)
        return s

    def restore(self, snap: "State") -> None:
        self.grid = dict(snap.grid)
        self.clue_cells = {k: list(v) for k, v in snap.clue_cells.items()}
        self.end_guards = set(snap.end_guards)
        self.word_crossings = list(snap.word_crossings)


def can_place(state: State, origin_r: int, origin_c: int,
              direction: str, letters: str, rows: int, cols: int) -> bool:
    pos = (origin_r, origin_c)

    if not (0 <= origin_r < rows and 0 <= origin_c < cols):
        return False

    # Clue origin must not already be a letter cell
    if pos in state.grid:
        return False

    # At most 2 clues per origin, directions must be perpendicular
    existing_dirs = state.dirs_at(pos)
    if direction in existing_dirs:
        return False
    if len(existing_dirs) >= 2:
        return False
    if existing_dirs:
        is_horiz = lambda d: d in ("Right", "Left")
        if is_horiz(direction) == is_horiz(next(iter(existing_dirs))):
            return False  # parallel directions not allowed in same cell

    positions = letter_positions(origin_r, origin_c, direction, len(letters))
    for i, (r, c) in enumerate(positions):
        if not (0 <= r < rows and 0 <= c < cols):
            return False
        # Letter cell must not land on an existing clue origin
        if (r, c) in state.clue_cells:
            return False
        # Letter cell must not land on an end-guard (void separator of a prior word)
        if (r, c) in state.end_guards:
            return False
        # If already occupied the character must match
        if (r, c) in state.grid and state.grid[(r, c)] != letters[i]:
            return False

    # The cell immediately past the last letter must be void — not a letter cell.
    # Without this, two consecutive words in the same direction would fuse visually.
    dr, dc = DIRS[direction]
    n = len(letters)
    end_r, end_c = origin_r + dr * (n + 1), origin_c + dc * (n + 1)
    if 0 <= end_r < rows and 0 <= end_c < cols:
        if (end_r, end_c) in state.grid:
            return False

    return True


def count_intersections(state: State, origin_r: int, origin_c: int,
                        direction: str, letters: str) -> int:
    positions = letter_positions(origin_r, origin_c, direction, len(letters))
    return sum(1 for r, c in positions if (r, c) in state.grid)


def place(state: State, origin_r: int, origin_c: int, direction: str,
          source: str, target: str, letters: str) -> None:
    pos = (origin_r, origin_c)
    state.clue_cells.setdefault(pos, []).append(
        {"dir": direction, "src": source, "tgt": target}
    )
    dr, dc = DIRS[direction]
    n = len(letters)
    crossings = 0
    for i, (r, c) in enumerate(letter_positions(origin_r, origin_c, direction, n)):
        if (r, c) in state.grid:
            crossings += 1
        state.grid[(r, c)] = letters[i]
    state.word_crossings.append(crossings)
    # Reserve the cell past the last letter so no future word can place a letter there
    state.end_guards.add((origin_r + dr * (n + 1), origin_c + dc * (n + 1)))


# ── Quality metric ─────────────────────────────────────────────────────────────

def compute_quality(state: State, rows: int, cols: int, placed_count: int) -> float:
    """Composite quality score for a completed generation state."""
    total_cells = rows * cols
    filled = len(state.letter_cells)
    total_crossings = sum(state.word_crossings)
    isolated = sum(1 for c in state.word_crossings if c == 0)

    # Wasted cells: in-bounds cells that are not filled, not clue, but adjacent
    # to a filled cell AND blocked from any future use (end-guard neighbours).
    wasted = 0
    for r in range(rows):
        for c in range(cols):
            pos = (r, c)
            if pos in state.grid or pos in state.clue_cells:
                continue
            has_letter_neighbour = any(
                (r + dr, c + dc) in state.grid
                for dr, dc in ((0, 1), (0, -1), (1, 0), (-1, 0))
            )
            if has_letter_neighbour and pos in state.end_guards:
                wasted += 1

    density = filled / total_cells if total_cells else 0.0

    return (
        W_PLACED    * placed_count
        + W_CROSSINGS * total_crossings
        + W_DENSITY   * density * total_cells   # scale to be comparable
        - W_WASTED    * wasted
        - W_ISOLATED  * isolated
    )


# ── Lookahead helper ───────────────────────────────────────────────────────────

def build_letter_freq(remaining_words: List[Word]) -> Counter:
    """Count how many remaining words contain each target letter."""
    freq: Counter = Counter()
    for w in remaining_words:
        for ch in set(strip_spaces(w["Target"])):
            freq[ch] += 1
    return freq


def lookahead_score(new_positions: List[Tuple[int, int]], state: State,
                    letters: str, letter_freq: Counter) -> float:
    """Sum of letter-frequency contributions for non-pre-occupied new positions."""
    score = 0.0
    for i, (r, c) in enumerate(new_positions):
        if (r, c) not in state.grid:   # only new cells contribute
            score += letter_freq.get(letters[i], 0)
    return score


# ── Anchor word selection ──────────────────────────────────────────────────────

def pick_anchor(sorted_words: List[Word]) -> Word:
    """
    From the longest words, pick the one whose letters are most common
    in the rest of the pool — maximising future crossing potential.
    """
    if not sorted_words:
        return sorted_words[0]

    max_len = len(strip_spaces(sorted_words[0]["Target"]))
    candidates = [w for w in sorted_words if len(strip_spaces(w["Target"])) == max_len]

    rest = [w for w in sorted_words if w not in candidates]
    freq = build_letter_freq(rest) if rest else Counter()

    def anchor_value(w: Word) -> float:
        return sum(freq.get(ch, 0) for ch in set(strip_spaces(w["Target"])))

    return max(candidates, key=anchor_value)


# ── Generator ──────────────────────────────────────────────────────────────────

def _greedy_pass(
    state: State,
    words_to_place: List[Word],
    placed: List[Word],
    rows: int,
    cols: int,
    horiz: str,
    rng: random.Random,
    letter_freq: Counter,
) -> None:
    """Single greedy sweep: try to place every word in words_to_place."""
    for word in words_to_place:
        letters = strip_spaces(word["Target"])
        if len(letters) < 2:
            continue

        best_score: float = -1.0
        best: Optional[Tuple[int, int, str]] = None

        remaining = [w for w in words_to_place if w is not word]
        live_freq = build_letter_freq(remaining)

        for direction in ("Down", horiz):
            for origin_r in range(rows):
                for origin_c in range(cols):
                    if not can_place(state, origin_r, origin_c,
                                     direction, letters, rows, cols):
                        continue
                    cur_x = count_intersections(
                        state, origin_r, origin_c, direction, letters)
                    if placed and cur_x < 1:
                        continue
                    new_pos = letter_positions(origin_r, origin_c, direction, len(letters))
                    la = lookahead_score(new_pos, state, letters, live_freq)
                    score = cur_x + LOOKAHEAD_WEIGHT * la + rng.random() * 0.01
                    if score > best_score:
                        best_score = score
                        best = (origin_r, origin_c, direction)

        if best is None:
            continue

        origin_r, origin_c, direction = best
        place(state, origin_r, origin_c, direction,
              word["Source"], word["Target"], letters)
        placed.append(word)


def generate(words: List[Word], rows: int, cols: int,
             seed: int = 0,
             target_lang: str = "en") -> Tuple[State, List[Word]]:
    """
    Greedy crossword placement with lookahead, smart anchor, and backtrack.
    Returns (state, list-of-placed-words).
    """
    rng = random.Random(seed)
    horiz = horizontal_dir(target_lang)

    # Group by target length, shuffle within each group, then sort longest-first
    by_len: Dict[int, List[Word]] = {}
    for w in words:
        n = len(strip_spaces(w["Target"]))
        by_len.setdefault(n, []).append(w)
    for group in by_len.values():
        rng.shuffle(group)
    sorted_words = [w for n in sorted(by_len, reverse=True) for w in by_len[n]]

    state = State()
    placed: List[Word] = []

    # ── Place anchor word first, centred ──────────────────────────────────────
    anchor = pick_anchor(sorted_words)
    anchor_letters = strip_spaces(anchor["Target"])
    remaining_after_anchor = [w for w in sorted_words if w is not anchor]

    # Alternate anchor direction across seeds: even seeds → Down, odd → horiz
    anchor_dir = "Down" if seed % 2 == 0 else horiz

    dr, dc = DIRS[anchor_dir]
    n = len(anchor_letters)
    # Centre the clue cell so the word straddles the grid centre
    centre_r, centre_c = rows // 2, cols // 2
    # Offset the origin so the middle letter lands near centre
    anchor_r = max(0, min(rows - 1, centre_r - dr * (n // 2 + 1)))
    anchor_c = max(0, min(cols - 1, centre_c - dc * (n // 2 + 1)))

    if can_place(state, anchor_r, anchor_c, anchor_dir, anchor_letters, rows, cols):
        place(state, anchor_r, anchor_c, anchor_dir,
              anchor["Source"], anchor["Target"], anchor_letters)
        placed.append(anchor)

    letter_freq = build_letter_freq(remaining_after_anchor)

    # ── Main greedy pass with backtrack ───────────────────────────────────────
    unplaced = [w for w in sorted_words if w not in placed]
    _greedy_pass(state, unplaced, placed, rows, cols, horiz, rng, letter_freq)

    # Backtrack if quality is low: undo last K placements and retry
    best_quality = compute_quality(state, rows, cols, len(placed))

    for retry in range(MAX_RETRIES):
        if len(placed) < 2:
            break

        k = BACKTRACK_STEPS[retry]
        if k >= len(placed):
            k = max(1, len(placed) - 1)

        # Build state from scratch without the last k placements
        # (cheaper than trying to undo in a mutable structure)
        keep = placed[:-k]
        undo = placed[-k:]

        state_retry = State()
        placed_retry: List[Word] = []
        for w in keep:
            letters_r = strip_spaces(w["Target"])
            # Re-place kept words greedily (they fit since we replay them)
            for direction in ("Down", horiz):
                for origin_r in range(rows):
                    for origin_c in range(cols):
                        if can_place(state_retry, origin_r, origin_c,
                                     direction, letters_r, rows, cols):
                            cx = count_intersections(
                                state_retry, origin_r, origin_c, direction, letters_r)
                            if placed_retry and cx < 1:
                                continue
                            place(state_retry, origin_r, origin_c, direction,
                                  w["Source"], w["Target"], letters_r)
                            placed_retry.append(w)
                            break
                    if placed_retry and placed_retry[-1] is w:
                        break

        # Re-add unplaced (undo set + words that never placed)
        placed_set = {id(w) for w in placed_retry}
        retry_pool = [w for w in sorted_words if id(w) not in placed_set]
        rng.shuffle(retry_pool)

        _greedy_pass(state_retry, retry_pool, placed_retry,
                     rows, cols, horiz, rng, build_letter_freq(retry_pool))

        retry_quality = compute_quality(state_retry, rows, cols, len(placed_retry))
        if retry_quality > best_quality:
            best_quality = retry_quality
            state = state_retry
            placed = placed_retry

    return state, placed


def best_of(words: List[Word], rows: int, cols: int,
            count: int = 1, base_seed: int = 0,
            target_lang: str = "en") -> Tuple[State, List[Word]]:
    """Run `count` seeds and return the result with the highest composite quality."""
    best_state, best_placed = generate(words, rows, cols, base_seed, target_lang)
    best_quality = compute_quality(best_state, rows, cols, len(best_placed))

    for i in range(1, count):
        state, placed = generate(words, rows, cols, base_seed + i, target_lang)
        q = compute_quality(state, rows, cols, len(placed))
        if q > best_quality:
            best_quality = q
            best_state, best_placed = state, placed

    return best_state, best_placed


# ── Puzzle serialiser ──────────────────────────────────────────────────────────

def state_to_puzzle(state: State, rows: int, cols: int,
                    source_lang: str, target_lang: str) -> Dict[str, Any]:
    cells: List[Dict[str, Any]] = []

    # Clue cells (sorted row-major for deterministic output)
    for (r, c), clues in sorted(state.clue_cells.items()):
        entry: Dict[str, Any] = {"Row": r, "Col": c, "Kind": "Clue", "Clues": []}
        for cl in clues:
            entry["Clues"].append(
                {"Source": cl["src"], "Target": cl["tgt"], "Dir": cl["dir"]}
            )
        cells.append(entry)

    # Letter cells (exclude any position that is also a clue origin — safety)
    letter_only = state.letter_cells - set(state.clue_cells.keys())
    for (r, c) in sorted(letter_only):
        cells.append({"Row": r, "Col": c, "Kind": "Letter"})

    return {
        "Rows":           rows,
        "Cols":           cols,
        "SourceLanguage": source_lang,
        "TargetLanguage": target_lang,
        "Cells":          cells,
    }


# ── Preview ────────────────────────────────────────────────────────────────────

def preview(state: State, rows: int, cols: int, placed: List[Word]) -> None:
    print(f"\n  Grid {rows}×{cols}  —  {len(placed)} word(s) placed")
    for r in range(rows):
        row_str = ""
        for c in range(cols):
            if (r, c) in state.clue_cells:
                row_str += "@ "
            elif (r, c) in state.grid:
                row_str += state.grid[(r, c)] + " "
            else:
                row_str += ". "
        print("    " + row_str)
    print()
    for w in placed:
        print(f"    {w['Source']:20s} → {w['Target']}")
    print()


# ── I/O ────────────────────────────────────────────────────────────────────────

def load_wordlist(path: str) -> Tuple[List[Word], str, str]:
    with open(path, encoding="utf-8") as f:
        data = json.load(f)
    src_lang = data.get("SourceLanguage", "en")
    tgt_lang = data.get("TargetLanguage", "he")

    # New grouped schema
    if "Categories" in data:
        words: List[Word] = []
        for entries in data["Categories"].values():
            for e in entries:
                words.append({"Source": e["Source"], "Target": e["Target"]})
        return words, src_lang, tgt_lang

    # Legacy flat schema
    return data.get("Words", []), src_lang, tgt_lang


def save_puzzle(puzzle: Dict[str, Any], path: str) -> None:
    if path == "-":
        print(json.dumps(puzzle, indent=2, ensure_ascii=False))
    else:
        os.makedirs(os.path.dirname(path) or ".", exist_ok=True)
        with open(path, "w", encoding="utf-8") as f:
            json.dump(puzzle, f, indent=2, ensure_ascii=False)


# ── Built-in examples ──────────────────────────────────────────────────────────

EXAMPLES = {
    "small_en_he": {
        "description": "Small English→Hebrew (5×5)",
        "filename":    "yakugo_small_en_he.json",
        "rows": 5, "cols": 5,
        "source_lang": "en", "target_lang": "he",
        "words": [
            {"Source": "day",    "Target": "יומ"},
            {"Source": "child",  "Target": "ילד"},
            {"Source": "king",   "Target": "מלכ"},
            {"Source": "road",   "Target": "דרכ"},
            {"Source": "milk",   "Target": "חלב"},
            {"Source": "moon",   "Target": "ירח"},
            {"Source": "door",   "Target": "דלת"},
            {"Source": "friend", "Target": "חבר"},
        ],
    },
    "medium_en_he": {
        "description": "Medium English→Hebrew (7×7)",
        "filename":    "yakugo_medium_en_he.json",
        "rows": 7, "cols": 7,
        "source_lang": "en", "target_lang": "he",
        "words": [
            {"Source": "day",          "Target": "יומ"},
            {"Source": "child",        "Target": "ילד"},
            {"Source": "king",         "Target": "מלכ"},
            {"Source": "road",         "Target": "דרכ"},
            {"Source": "bread",        "Target": "לחמ"},
            {"Source": "milk",         "Target": "חלב"},
            {"Source": "moon",         "Target": "ירח"},
            {"Source": "door",         "Target": "דלת"},
            {"Source": "teacher",      "Target": "מורה"},
            {"Source": "morning",      "Target": "בוקר"},
            {"Source": "love",         "Target": "אהבה"},
            {"Source": "chicken coop", "Target": "לול"},
            {"Source": "flower",       "Target": "פרח"},
            {"Source": "book",         "Target": "ספר"},
            {"Source": "cat",          "Target": "חתול"},
        ],
    },
    "large_en_he": {
        "description": "Large English→Hebrew (9×9)",
        "filename":    "yakugo_large_en_he.json",
        "rows": 9, "cols": 9,
        "source_lang": "en", "target_lang": "he",
        "words": [
            {"Source": "day",          "Target": "יומ"},
            {"Source": "child",        "Target": "ילד"},
            {"Source": "king",         "Target": "מלכ"},
            {"Source": "road",         "Target": "דרכ"},
            {"Source": "bread",        "Target": "לחמ"},
            {"Source": "milk",         "Target": "חלב"},
            {"Source": "moon",         "Target": "ירח"},
            {"Source": "door",         "Target": "דלת"},
            {"Source": "time",         "Target": "זמנ"},
            {"Source": "teacher",      "Target": "מורה"},
            {"Source": "morning",      "Target": "בוקר"},
            {"Source": "evening",      "Target": "ערב"},
            {"Source": "love",         "Target": "אהבה"},
            {"Source": "chicken coop", "Target": "לול"},
            {"Source": "flower",       "Target": "פרח"},
            {"Source": "stone",        "Target": "אבנ"},
            {"Source": "book",         "Target": "ספר"},
            {"Source": "star",         "Target": "כוכב"},
            {"Source": "year",         "Target": "שנה"},
            {"Source": "cat",          "Target": "חתול"},
            {"Source": "dog",          "Target": "כלב"},
            {"Source": "water",        "Target": "מים"},
        ],
    },
}


# ── CLI ────────────────────────────────────────────────────────────────────────

def main() -> None:
    ap = argparse.ArgumentParser(
        description="Generate a Yakugo translation-crossword puzzle JSON.",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog=__doc__,
    )
    ap.add_argument("wordlist", nargs="?",
                    help="Vocabulary JSON path  (omit for --examples)")
    ap.add_argument("output",   nargs="?",
                    help="JSON output path, or '-' for stdout")
    ap.add_argument("--rows",    type=int, default=7,
                    help="Grid rows  (default: 7)")
    ap.add_argument("--cols",    type=int, default=7,
                    help="Grid cols  (default: 7)")
    ap.add_argument("--seed",    type=int, default=0,
                    help="Base random seed  (default: 0)")
    ap.add_argument("--count",   type=int, default=1,
                    help="Seeds to try; keep the best result  (default: 1)")
    ap.add_argument("--examples", action="store_true",
                    help="Generate all built-in example puzzles")
    ap.add_argument("--outdir",   default=".",
                    help="Output directory for --examples  (default: current dir)")
    ap.add_argument("--preview",  action="store_true",
                    help="Print ASCII grid preview")
    args = ap.parse_args()

    if args.examples:
        generated = []
        for ex in EXAMPLES.values():
            state, placed = best_of(
                ex["words"], ex["rows"], ex["cols"], count=20, base_seed=0,
                target_lang=ex["target_lang"],
            )
            puzzle = state_to_puzzle(
                state, ex["rows"], ex["cols"], ex["source_lang"], ex["target_lang"]
            )
            path = os.path.join(args.outdir, ex["filename"])
            save_puzzle(puzzle, path)
            if args.preview:
                preview(state, ex["rows"], ex["cols"], placed)
            print(f"  OK  {ex['filename']:42s}  {ex['rows']}×{ex['cols']}  "
                  f"{len(placed)}/{len(ex['words'])} words  --  {ex['description']}")
            generated.append(path)
        print(f"\n{len(generated)} puzzle(s) written to: {os.path.abspath(args.outdir)}")
        return

    if not args.wordlist:
        ap.print_help()
        return

    words, src_lang, tgt_lang = load_wordlist(args.wordlist)
    state, placed = best_of(
        words, args.rows, args.cols, count=args.count, base_seed=args.seed,
        target_lang=tgt_lang,
    )

    if args.preview:
        preview(state, args.rows, args.cols, placed)

    puzzle = state_to_puzzle(state, args.rows, args.cols, src_lang, tgt_lang)
    out = args.output or f"yakugo_{args.rows}x{args.cols}.json"
    save_puzzle(puzzle, out)
    if out != "-":
        print(f"Saved {args.rows}×{args.cols} puzzle  "
              f"({len(placed)}/{len(words)} words placed)  →  {out}")


if __name__ == "__main__":
    main()
