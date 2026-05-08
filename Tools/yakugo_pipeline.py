#!/usr/bin/env python3
"""
yakugo_pipeline.py
==================
End-to-end pipeline: vocabulary JSON → batch of Yakugo puzzle JSON files.

Runs the generator across three grid sizes (small / medium / large) with
multiple random seeds, discards results that placed too few words, and saves
the keepers to the output directory.

Usage
-----
  # Generate puzzles from the bundled wordlist into the Documents folder:
  python yakugo_pipeline.py

  # Supply your own vocabulary and output directory:
  python yakugo_pipeline.py --wordlist my_words.json --outdir path/to/output

  # More seeds per size and a higher quality threshold:
  python yakugo_pipeline.py --seeds 50 --min-words 5 --preview
"""

import argparse
import os
import sys
from datetime import datetime

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")

# Allow importing from the same directory without installing as a package
sys.path.insert(0, os.path.dirname(__file__))
from yakugo_generate import best_of, load_wordlist, preview, save_puzzle, state_to_puzzle

# ── Pipeline configuration ─────────────────────────────────────────────────────

# Default vocabulary bundled alongside this script
_HERE = os.path.dirname(__file__)
DEFAULT_WORDLIST = os.path.join(_HERE, "yakugo_wordlist.json")

# Default output: Documents/Yakugo/Puzzles/FromGenerator  (relative to repo root)
DEFAULT_OUTDIR = os.path.join(_HERE, "..", "Documents", "Yakugo", "Puzzles", "FromGenerator")

# Grid sizes to generate, with a per-size minimum word count
SIZES = [
    {"label": "small",  "rows": 5, "cols": 5, "min_words": 3},
    {"label": "medium", "rows": 7, "cols": 7, "min_words": 5},
    {"label": "large",  "rows": 9, "cols": 9, "min_words": 7},
]


# ── Pipeline ───────────────────────────────────────────────────────────────────

def run_pipeline(
    wordlist_path: str,
    outdir: str,
    seeds_per_size: int,
    min_words: int,
    show_preview: bool,
) -> None:
    words, src_lang, tgt_lang = load_wordlist(wordlist_path)
    print(f"Vocabulary : {os.path.abspath(wordlist_path)}")
    print(f"            {len(words)} word pairs  ({src_lang} → {tgt_lang})")
    print(f"Output     : {os.path.abspath(outdir)}")
    print(f"Seeds/size : {seeds_per_size}  |  min-words override: {min_words}")
    print()

    os.makedirs(outdir, exist_ok=True)
    timestamp = datetime.now().strftime("%Y-%m-%d.%H.%M.%S")
    total_saved = 0

    for size in SIZES:
        rows, cols   = size["rows"], size["cols"]
        threshold    = max(min_words, size["min_words"])
        saved        = 0
        skipped      = 0

        print(f"── {size['label']:6s}  {rows}×{cols}  (need ≥{threshold} words) ──")

        for seed in range(seeds_per_size):
            # Each seed tries 5 internal variants; keeps the best
            state, placed = best_of(
                words, rows, cols, count=5, base_seed=seed * 100,
                target_lang=tgt_lang,
            )

            if len(placed) < threshold:
                skipped += 1
                continue

            puzzle = state_to_puzzle(state, rows, cols, src_lang, tgt_lang)
            fname  = f"yakugo_{size['label']}_{rows}x{cols}_{timestamp}_s{seed:03d}.json"
            path   = os.path.join(outdir, fname)
            save_puzzle(puzzle, path)

            if show_preview:
                preview(state, rows, cols, placed)

            print(f"  {fname}  —  {len(placed)}/{len(words)} words")
            saved += 1
            total_saved += 1

        if saved == 0:
            print(f"  [WARN] no puzzle reached {threshold} words "
                  f"({skipped} seed(s) skipped)")
        else:
            print(f"  → {saved} puzzle(s) saved  ({skipped} skipped below threshold)")
        print()

    print(f"Done.  {total_saved} puzzle(s) written to: {os.path.abspath(outdir)}")


# ── CLI ────────────────────────────────────────────────────────────────────────

def main() -> None:
    ap = argparse.ArgumentParser(
        description="Batch-generate Yakugo puzzles from a vocabulary file.",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog=__doc__,
    )
    ap.add_argument("--wordlist",   default=DEFAULT_WORDLIST,
                    help=f"Vocabulary JSON  (default: {DEFAULT_WORDLIST})")
    ap.add_argument("--outdir",     default=DEFAULT_OUTDIR,
                    help=f"Output directory  (default: {DEFAULT_OUTDIR})")
    ap.add_argument("--seeds",      type=int, default=10,
                    help="Random seeds to try per grid size  (default: 10)")
    ap.add_argument("--min-words",  type=int, default=3,
                    help="Minimum words placed to accept a puzzle  (default: 3)")
    ap.add_argument("--preview",    action="store_true",
                    help="Print ASCII grid for each saved puzzle")
    args = ap.parse_args()

    run_pipeline(
        wordlist_path=args.wordlist,
        outdir=args.outdir,
        seeds_per_size=args.seeds,
        min_words=args.min_words,
        show_preview=args.preview,
    )


if __name__ == "__main__":
    main()
