#!/usr/bin/env python3
"""
griddler_from_image.py
======================
Converts a binary pixel grid into a Griddler (nonogram) puzzle JSON file.

Each pixel maps to one cell.  Groups follow the ordering used by
FactoryGriddler.CreateBoardFromPuzzleObject:

  Row    i : left-to-right across column 0..C-1
  Column j : top-to-bottom across row  0..R-1

Usage
-----
  # Generate the 4 built-in example puzzles into <outdir>:
  python griddler_from_image.py --examples [--outdir path]

  # Convert a PNG file to a puzzle (requires Pillow):
  python griddler_from_image.py input.png output.json --rows 15 --cols 15

  # Print the puzzle JSON to stdout:
  python griddler_from_image.py input.png - --rows 10 --cols 10
"""

import json
import argparse
import os
import sys
from typing import List, Tuple


# ── Core algorithm ─────────────────────────────────────────────────────────────

Grid = List[List[bool]]


def rle(seq: List[bool]) -> List[int]:
    """Run-length encoding -- returns lengths of consecutive True runs."""
    result, count = [], 0
    for v in seq:
        if v:
            count += 1
        elif count:
            result.append(count)
            count = 0
    if count:
        result.append(count)
    return result


def compute_groups(P: Grid) -> Tuple[List[List[int]], List[List[int]]]:
    """
    P[R][C] -- boolean grid (truthy = filled cell).
    Returns (rows, cols) as RLE lists.
    """
    R, C = len(P), len(P[0])
    rows = [rle(P[i]) for i in range(R)]
    cols = [rle([P[i][j] for i in range(R)]) for j in range(C)]
    return rows, cols


def grid_to_puzzle(P: Grid) -> dict:
    rows, cols = compute_groups(P)
    R, C = len(P), len(P[0])
    return {
        "Rows":         rows,
        "Columns":      cols,
        "RowsLength":   C,   # = number of columns (matches FactoryGriddler)
        "ColumnLength": R,   # = number of rows
    }


def save_puzzle(puzzle: dict, path: str) -> None:
    if path == "-":
        print(json.dumps(puzzle, indent=2))
    else:
        os.makedirs(os.path.dirname(path) or ".", exist_ok=True)
        with open(path, "w", encoding="utf-8") as f:
            json.dump(puzzle, f, indent=2)


# ── PNG converter ──────────────────────────────────────────────────────────────

def png_to_grid(path: str, rows: int, cols: int, threshold: int = 128) -> Grid:
    """
    Read a PNG / JPEG, resize to rows x cols, threshold to filled/empty.
    Dark pixels (value < threshold after grayscale conversion) -> filled.
    Requires Pillow: pip install Pillow
    """
    try:
        from PIL import Image
    except ImportError:
        sys.exit("Pillow is required for image input.  Install it with:  pip install Pillow")

    img = Image.open(path).convert("L")          # grayscale
    img = img.resize((cols, rows), Image.LANCZOS)
    px  = list(img.getdata())
    return [[px[i * cols + j] < threshold for j in range(cols)] for i in range(rows)]


# ── Built-in example pixel art ─────────────────────────────────────────────────

# 1 = filled cell, 0 = empty.  All rows and columns must have at least one
# filled cell (otherwise the clue is [] which gives a trivial / ambiguous row).

EXAMPLES = {

    # ── Small (10x10) -- HEART ───────────────────────────────────────────
    # Classic heart pointing downward.  Symmetric column clues.
    "heart_small": {
        "description": "Heart (10x10)",
        "filename":    "from_image_heart_small.json",
        "grid": [
            [0, 1, 1, 0, 0, 0, 1, 1, 0, 0],
            [1, 1, 1, 1, 0, 1, 1, 1, 1, 0],
            [1, 1, 1, 1, 1, 1, 1, 1, 1, 0],
            [1, 1, 1, 1, 1, 1, 1, 1, 1, 0],
            [0, 1, 1, 1, 1, 1, 1, 1, 0, 0],
            [0, 0, 1, 1, 1, 1, 1, 0, 0, 0],
            [0, 0, 0, 1, 1, 1, 0, 0, 0, 0],
            [0, 0, 0, 0, 1, 0, 0, 0, 0, 0],
            [1, 0, 0, 0, 0, 0, 0, 0, 0, 1],  # border row so col 0,9 aren't empty
            [1, 1, 1, 1, 1, 1, 1, 1, 1, 1],  # baseline
        ],
    },

    # ── Medium (15x15) -- HOUSE ──────────────────────────────────────────
    # Triangle roof + rectangular walls + central door.
    "house_medium": {
        "description": "House (15x15)",
        "filename":    "from_image_house_medium.json",
        "grid": [
            [0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0],
            [0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0],
            [0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0],
            [0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0],
            [0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0],
            [0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0],
            [0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0],
            [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1],
            [1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1],
            [1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1],
            [1, 1, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 1, 1],
            [1, 1, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 1, 1],
            [1, 1, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 1, 1],
            [1, 1, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 1, 1],
            [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1],
        ],
    },

    # ── Big (15x25) -- FISH ───────────────────────────────────────────────
    # Fish body (torpedo oval) with V-tail on the left and eye on the right.
    "fish_big": {
        "description": "Fish (15 rows x 25 cols)",
        "filename":    "from_image_fish_big.json",
        "grid": [
            [1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
            [1, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
            [0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0],
            [0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0],
            [0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0],
            [0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0],
            [0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 0, 0],
            [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0],
            [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0],
            [0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 0, 0],
            [0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0],
            [0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0],
            [0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0],
            [1, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
            [1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1],
        ],
    },

    # ── Huge (25x20) -- CHRISTMAS TREE ───────────────────────────────────
    # Two overlapping triangle canopies + 2-wide trunk at the bottom.
    "tree_huge": {
        "description": "Christmas tree (25 rows x 20 cols)",
        "filename":    "from_image_tree_huge.json",
        "grid": [
            [0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0],
            [0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0],
            [0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0],
            [0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0],
            [0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0],
            [0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0],
            [0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0],
            [0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0],
            [0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0],
            [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1],
            [0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0],
            [0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0],
            [0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0],
            [0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0],
            [0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0],
            [0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0],
            [0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0],
            [0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0],
            [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1],
            [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1],
            [0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0],
            [0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0],
            [0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0],
            [0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0],
            [0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0],  # base
        ],
    },
}


def preview(P: Grid, name: str) -> None:
    """Print a compact ASCII preview of the pixel grid."""
    print(f"\n  {name}  ({len(P)}x{len(P[0])})")
    for row in P:
        print("    " + "".join("#" if v else "." for v in row))


# ── CLI ────────────────────────────────────────────────────────────────────────

def main() -> None:
    ap = argparse.ArgumentParser(
        description="Convert a pixel grid or PNG image to a Griddler puzzle JSON.",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog=__doc__,
    )
    ap.add_argument("input",  nargs="?", help="PNG/JPEG path (omit for --examples)")
    ap.add_argument("output", nargs="?", help="JSON output path, or '-' for stdout")
    ap.add_argument("--rows",      type=int, default=15, help="Target grid rows (PNG mode)")
    ap.add_argument("--cols",      type=int, default=15, help="Target grid cols (PNG mode)")
    ap.add_argument("--threshold", type=int, default=128,
                    help="Grayscale threshold: pixels darker than this -> filled (PNG mode)")
    ap.add_argument("--examples",  action="store_true",
                    help="Generate all built-in example puzzles")
    ap.add_argument("--outdir", default=".",
                    help="Output directory for --examples  (default: current dir)")
    ap.add_argument("--preview",   action="store_true",
                    help="Print ASCII preview of each pixel grid")
    ap.add_argument("--validate",  action="store_true",
                    help="Warn about rows/columns with empty clues (ambiguous puzzle)")
    args = ap.parse_args()

    if args.examples:
        generated = []
        for name, ex in EXAMPLES.items():
            if args.preview:
                preview(ex["grid"], name)
            puzzle = grid_to_puzzle(ex["grid"])
            path   = os.path.join(args.outdir, ex["filename"])
            save_puzzle(puzzle, path)
            R, C = len(ex["grid"]), len(ex["grid"][0])
            if args.validate:
                _validate(puzzle, name)
            print(f"  OK  {ex['filename']:48s}  {R}x{C}  --  {ex['description']}")
            generated.append(path)
        print(f"\n{len(generated)} puzzle(s) written to: {os.path.abspath(args.outdir)}")
        return

    if not args.input:
        ap.print_help()
        return

    grid   = png_to_grid(args.input, args.rows, args.cols, args.threshold)
    puzzle = grid_to_puzzle(grid)
    if args.preview:
        preview(grid, os.path.basename(args.input))
    if args.validate:
        _validate(puzzle, args.input)
    out = args.output or (os.path.splitext(args.input)[0] + ".json")
    save_puzzle(puzzle, out)
    if out != "-":
        print(f"Saved {args.rows}x{args.cols} puzzle -> {out}")


def _validate(puzzle: dict, name: str) -> None:
    issues = []
    for i, r in enumerate(puzzle["Rows"]):
        if not r:
            issues.append(f"  row {i} is empty (trivial clue [])")
    for j, c in enumerate(puzzle["Columns"]):
        if not c:
            issues.append(f"  col {j} is empty (trivial clue [])")
    if issues:
        print(f"  [WARN] {name} has ambiguous clues:")
        for s in issues:
            print(s)


if __name__ == "__main__":
    main()
