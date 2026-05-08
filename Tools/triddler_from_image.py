#!/usr/bin/env python3
"""
triddler_from_image.py
======================
Converts a binary pixel grid into a Triddler puzzle JSON file.

Each grid square maps to TWO triangles (Left + Right) that share the same
filled/empty value — "square pixel" mode.  The groups follow the ordering
used by FactoryTriddler.CreateBoardFromPuzzleObject:

  Horizontal row i  : L[i,0], R[i,0], L[i,1], R[i,1], …, L[i,C-1], R[i,C-1]
  Vertical   col j  : R[0,j], L[0,j], R[1,j], L[1,j], …, R[R-1,j], L[R-1,j]
  Diagonal   group  : zigzag from each anchor on the right / bottom edges
                      (see first-loop / second-loop in FactoryTriddler)

Usage
-----
  # Generate the 4 built-in example puzzles into <outdir>:
  python triddler_from_image.py --examples [--outdir path]

  # Convert a PNG file to a puzzle (requires Pillow):
  python triddler_from_image.py input.png output.json --rows 10 --cols 12

  # Print the puzzle JSON to stdout (PNG mode):
  python triddler_from_image.py input.png - --rows 8 --cols 8
"""

import json
import argparse
import os
import sys
from typing import List, Tuple


# ── Core algorithm ─────────────────────────────────────────────────────────────

Grid = List[List[bool]]


def rle(seq: List[bool]) -> List[int]:
    """Run-length encoding — returns lengths of consecutive True runs."""
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


def compute_groups(P: Grid) -> Tuple[List[List[int]], List[List[int]], List[List[int]]]:
    """
    P[R][C] — boolean grid (truthy = filled triangle).
    Returns (horizontals, verticals, diagonals) as RLE lists.
    """
    R, C = len(P), len(P[0])

    # Horizontal: row i → L[i,0] R[i,0]  L[i,1] R[i,1] …
    horizontals = [rle([v for j in range(C) for v in (P[i][j], P[i][j])]) for i in range(R)]

    # Vertical: col j → R[0,j] L[0,j]  R[1,j] L[1,j] …
    verticals = [rle([v for i in range(R) for v in (P[i][j], P[i][j])]) for j in range(C)]

    # Diagonal — mirrors FactoryTriddler exactly:
    #   First loop  (i = 0..R-1): anchor at (i, C-1), IsRight starts True.
    #     IsRight → visit P[ii][jj], ii--
    #     IsLeft  → visit P[ii][jj], jj--
    #   Second loop (j = C-1..0): anchor at (R-1, j), IsRight starts False.
    #     IsLeft  → visit P[ii][jj], jj--
    #     IsRight → visit P[ii][jj], ii--
    diagonals: List[List[int]] = []

    for i in range(R):
        ii, jj, is_right, seq = i, C - 1, True, []
        while ii >= 0 and jj >= 0:
            seq.append(P[ii][jj])
            if is_right:
                ii -= 1
            else:
                jj -= 1
            is_right = not is_right
        diagonals.append(rle(seq))

    for j in range(C - 1, -1, -1):
        ii, jj, is_right, seq = R - 1, j, False, []
        while ii >= 0 and jj >= 0:
            seq.append(P[ii][jj])
            if is_right:
                ii -= 1
            else:
                jj -= 1
            is_right = not is_right
        diagonals.append(rle(seq))

    return horizontals, verticals, diagonals


def grid_to_puzzle(P: Grid) -> dict:
    h, v, d = compute_groups(P)
    return {
        "Horizontals": h,
        "Verticals":   v,
        "Diagonals":   d,
        "BaseRowsCount":    len(P),
        "BaseColumnCount":  len(P[0]),
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
    Read a PNG / JPEG, resize to rows×cols, threshold to filled/empty.
    Dark pixels (value < threshold after grayscale conversion) → filled.
    Requires Pillow: pip install Pillow
    """
    try:
        from PIL import Image
    except ImportError:
        sys.exit("Pillow is required for image input.  Install it with:  pip install Pillow")

    img = Image.open(path).convert("L")           # grayscale
    img = img.resize((cols, rows), Image.LANCZOS)
    px  = list(img.getdata())
    return [[px[i * cols + j] < threshold for j in range(cols)] for i in range(rows)]


# ── Built-in example pixel art ─────────────────────────────────────────────────

# Each value is truthy (1) = filled, falsy (0) = empty.
# Shapes are designed so the diagonal direction (anti-diagonal, top-right →
# bottom-left) produces interesting run-length numbers.

EXAMPLES = {

    # ── Small (5×5) — PLUS sign ──────────────────────────────────────────
    # Symmetric on all 3 axes; corner trimming produces a hexagonal outline.
    "plus_small": {
        "description": "Plus / cross sign (5x5)",
        "filename":    "from_image_plus_small.json",
        "grid": [
            [0, 0, 1, 0, 0],
            [0, 0, 1, 0, 0],
            [1, 1, 1, 1, 1],
            [0, 0, 1, 0, 0],
            [0, 0, 1, 0, 0],
        ],
    },

    # ── Medium (7×7) — DIAMOND ───────────────────────────────────────────
    # A filled diamond; the three axes see a symmetric run that expands and
    # contracts, making it a satisfying nonogram to solve.
    "diamond_medium": {
        "description": "Filled diamond (7x7)",
        "filename":    "from_image_diamond_medium.json",
        "grid": [
            [0, 0, 0, 1, 0, 0, 0],
            [0, 0, 1, 1, 1, 0, 0],
            [0, 1, 1, 1, 1, 1, 0],
            [1, 1, 1, 1, 1, 1, 1],
            [0, 1, 1, 1, 1, 1, 0],
            [0, 0, 1, 1, 1, 0, 0],
            [0, 0, 0, 1, 0, 0, 0],
        ],
    },

    # ── Big (8×12) — FISH swimming right ─────────────────────────────────
    # V-shaped tail on the left, torpedo body, slight taper at the mouth.
    # Tests a larger board with non-trivial multi-run diagonal clues.
    "fish_big": {
        "description": "Fish swimming right (8x12)",
        "filename":    "from_image_fish_big.json",
        "grid": [
            [1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
            [1, 0, 0, 0, 1, 1, 1, 1, 1, 1, 0, 0],
            [0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0],
            [0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1],
            [0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1],
            [0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0],
            [1, 0, 0, 0, 1, 1, 1, 1, 1, 1, 0, 0],
            [1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
        ],
    },

    # ── Huge (11×9) — CHRISTMAS TREE ─────────────────────────────────────
    # Two layered triangles (canopy) plus a 2-wide trunk.
    # The horizontal axis shows widening rows; the diagonal axis shows the
    # tree's silhouette in a triangular direction.
    "tree_huge": {
        "description": "Christmas tree (11x9)",
        "filename":    "from_image_tree_huge.json",
        "grid": [
            [0, 0, 0, 0, 1, 0, 0, 0, 0],
            [0, 0, 0, 1, 1, 1, 0, 0, 0],
            [0, 0, 1, 1, 1, 1, 1, 0, 0],
            [0, 1, 1, 1, 1, 1, 1, 1, 0],
            [1, 1, 1, 1, 1, 1, 1, 1, 1],
            [0, 0, 0, 1, 1, 1, 0, 0, 0],
            [0, 0, 1, 1, 1, 1, 1, 0, 0],
            [0, 1, 1, 1, 1, 1, 1, 1, 0],
            [1, 1, 1, 1, 1, 1, 1, 1, 1],
            [0, 0, 0, 0, 1, 1, 0, 0, 0],
            [0, 0, 0, 0, 1, 1, 0, 0, 0],
        ],
    },
}


def preview(P: Grid, name: str) -> None:
    """Print a compact ASCII preview of the pixel grid."""
    print(f"\n  {name}  ({len(P)}x{len(P[0])})")
    for row in P:
        print("    " + " ".join("#" if v else "." for v in row))


# ── CLI ────────────────────────────────────────────────────────────────────────

def main() -> None:
    ap = argparse.ArgumentParser(
        description="Convert a pixel grid or PNG image to a Triddler puzzle JSON.",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog=__doc__,
    )
    ap.add_argument("input",  nargs="?", help="PNG/JPEG path (omit for --examples)")
    ap.add_argument("output", nargs="?", help="JSON output path, or '-' for stdout")
    ap.add_argument("--rows",      type=int, default=10,  help="Target grid rows (PNG mode)")
    ap.add_argument("--cols",      type=int, default=10,  help="Target grid cols (PNG mode)")
    ap.add_argument("--threshold", type=int, default=128,
                    help="Grayscale threshold: pixels darker than this -> filled (PNG mode)")
    ap.add_argument("--examples",  action="store_true",
                    help="Generate all built-in example puzzles")
    ap.add_argument("--outdir", default=".",
                    help="Output directory for --examples  (default: current dir)")
    ap.add_argument("--preview",   action="store_true",
                    help="Print ASCII preview of each pixel grid")
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
            print(f"  OK  {ex['filename']:45s}  {R}x{C}  --  {ex['description']}")
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
    out = args.output or (os.path.splitext(args.input)[0] + ".json")
    save_puzzle(puzzle, out)
    if out != "-":
        print(f"Saved {args.rows}×{args.cols} puzzle → {out}")


if __name__ == "__main__":
    main()
