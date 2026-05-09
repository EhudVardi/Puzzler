#!/usr/bin/env python3
"""
yakugo_generate.py
==================
Generates a Yakugo puzzle JSON from a vocabulary (word-pair list).

Yakugo is a translation crossword: clue cells hold a source-language word and
its target-language translation; letter cells are shared by crossing words
exactly like a crossword, so every character must satisfy all groups that pass
through it.

The placement algorithm is greedy with random tiebreaking:
  1. Sort words by target length (longest first).
  2. Place the first word Down near the top-centre of the grid.
  3. For each subsequent word try every (origin, direction) candidate that
     produces at least one intersection with already-placed letters.  Among
     candidates pick the one with the most intersections; break ties randomly.
  4. Words with no valid placement are skipped.

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

import json
import argparse
import os
import sys
import random
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

    @property
    def letter_cells(self) -> set:
        return set(self.grid.keys())

    def dirs_at(self, pos: Tuple[int, int]) -> set:
        return {e["dir"] for e in self.clue_cells.get(pos, [])}


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
    for i, (r, c) in enumerate(letter_positions(origin_r, origin_c, direction, n)):
        state.grid[(r, c)] = letters[i]
    # Reserve the cell past the last letter so no future word can place a letter there
    state.end_guards.add((origin_r + dr * (n + 1), origin_c + dc * (n + 1)))


# ── Generator ──────────────────────────────────────────────────────────────────

def generate(words: List[Word], rows: int, cols: int,
             seed: int = 0,
             target_lang: str = "en") -> Tuple[State, List[Word]]:
    """
    Greedy crossword placement.  Returns (state, list-of-placed-words).
    """
    rng = random.Random(seed)
    horiz = horizontal_dir(target_lang)

    # Group by target length, shuffle within each group for variety, then sort
    # longest-first so long words claim good positions before short ones.
    by_len: Dict[int, List[Word]] = {}
    for w in words:
        n = len(strip_spaces(w["Target"]))
        by_len.setdefault(n, []).append(w)
    for group in by_len.values():
        rng.shuffle(group)
    sorted_words = [w for n in sorted(by_len, reverse=True) for w in by_len[n]]

    state = State()
    placed: List[Word] = []

    for word in sorted_words:
        letters = strip_spaces(word["Target"])
        if len(letters) < 2:
            continue  # single-char targets can't form meaningful crossword entries

        best_score: float = -1.0
        best: Optional[Tuple[int, int, str]] = None

        for direction in ("Down", horiz):
            for origin_r in range(rows):
                for origin_c in range(cols):
                    if not can_place(state, origin_r, origin_c,
                                     direction, letters, rows, cols):
                        continue
                    score = count_intersections(
                        state, origin_r, origin_c, direction, letters)
                    # After the first word every placement must intersect
                    if placed and score < 1:
                        continue
                    # Small random jitter breaks ties without bias
                    score_j = score + rng.random() * 0.01
                    if score_j > best_score:
                        best_score = score_j
                        best = (origin_r, origin_c, direction)

        if best is None:
            continue

        origin_r, origin_c, direction = best
        place(state, origin_r, origin_c, direction,
              word["Source"], word["Target"], letters)
        placed.append(word)

    return state, placed


def best_of(words: List[Word], rows: int, cols: int,
            count: int = 1, base_seed: int = 0,
            target_lang: str = "en") -> Tuple[State, List[Word]]:
    """Run `count` seeds and return the result that placed the most words."""
    best_state, best_placed = generate(words, rows, cols, base_seed, target_lang)
    for i in range(1, count):
        state, placed = generate(words, rows, cols, base_seed + i, target_lang)
        if len(placed) > len(best_placed):
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
    return (
        data["Words"],
        data.get("SourceLanguage", "en"),
        data.get("TargetLanguage", "he"),
    )


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
