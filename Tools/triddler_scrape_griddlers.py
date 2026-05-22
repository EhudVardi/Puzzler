#!/usr/bin/env python3
"""
triddler_scrape_griddlers.py
============================
Scrape Triddler puzzles from griddlers.net into the local puzzle library.

Each puzzle on griddlers.net renders as an inline SVG inside
`<g id="zoomContainer">`. The SVG carries:

  * `c_<R>_<C>` polygons - one playable triangle at SVG row R, triangle
    column C. Even C => Left (Apex-up triangle), odd C => Right.
  * `t<A>_<L>_<K>` texts  - the integer in header slot K of line L on
    axis A (0 = horizontal row, 1 = vertical column, 2 = diagonal).

The on-disk JSON schema matches DataLayerTriddler / PuzzleTriddler:

  { "Type": "Triddler", "Source": "griddlers.net", "Name": "<author>-<id>",
    "DateCreated": "<ISO 8601>",
    "Horizontals": [...], "Verticals": [...], "Diagonals": [...],
    "BaseRowsCount": <int>, "BaseColumnCount": <int> }

`Diagonals` has `BaseRowsCount + BaseColumnCount` slots; dented corners
(any cell shape other than the full parallelogram) are encoded as
empty lists at the leading / trailing ends.

Usage
-----
  # Parse a saved fragment (no network):
  python triddler_scrape_griddlers.py --from-file ./triddlers --outdir /tmp/out

  # Fetch one or more puzzles by id (or full URL):
  python triddler_scrape_griddlers.py 3682 3683
  python triddler_scrape_griddlers.py "https://www.griddlers.net/iw_IL/triddlers?..._id=3682"

  # Send a session cookie if the AJAX endpoint requires it:
  python triddler_scrape_griddlers.py 3682 --cookie "JSESSIONID=abc..."
"""

from __future__ import annotations

import argparse
import json
import os
import re
import sys
import uuid
from datetime import datetime
from pathlib import Path
from typing import Dict, List, Set, Tuple


AJAX_URL_TEMPLATE = (
    "https://www.griddlers.net/iw_IL/triddlers"
    "?p_p_id=triddlers_WAR_puzzles"
    "&p_p_lifecycle=2"
    "&p_p_state=normal"
    "&p_p_mode=view"
    "&p_p_resource_id=html"
    "&p_p_cacheability=cacheLevelPage"
    "&p_p_col_id=column-1"
    "&p_p_col_count=2"
    "&_triddlers_WAR_puzzles_view=detail"
    "&_triddlers_WAR_puzzles_id={pid}"
)


# ── HTML fetch / parse ─────────────────────────────────────────────────────────


def _lazy_bs4():
    try:
        from bs4 import BeautifulSoup  # type: ignore
    except ImportError:
        sys.exit("error: this script needs beautifulsoup4 (pip install beautifulsoup4 lxml)")
    return BeautifulSoup


def _lazy_requests():
    try:
        import requests  # type: ignore
    except ImportError:
        sys.exit("error: this script needs requests (pip install requests)")
    return requests


def fetch_html(target: str, cookie: str | None) -> str:
    """`target` is either a numeric id or a full URL."""
    requests = _lazy_requests()
    url = AJAX_URL_TEMPLATE.format(pid=target) if target.isdigit() else target
    headers = {
        "User-Agent": "Mozilla/5.0 (Triddler scraper)",
        "Accept": "text/html,application/xhtml+xml",
    }
    if cookie:
        headers["Cookie"] = cookie
    r = requests.get(url, headers=headers, timeout=30)
    r.raise_for_status()
    return r.text


# ── SVG parsing ───────────────────────────────────────────────────────────────


CELL_RE = re.compile(r"^c_(\d+)_(\d+)$")
TEXT_RE = re.compile(r"^t(\d+)_(\d+)_(\d+)$")
META_AUTHOR_RE = re.compile(r"Author:\s*([^|<]+?)\s*(?:\||$)")
META_ID_RE = re.compile(r"Id:\s*(\d+)")


def parse_svg(html: str) -> Tuple[
    Set[Tuple[int, int]],
    Dict[int, Dict[int, List[Tuple[int, int]]]],
    Dict[str, str],
]:
    """Return (cells, clue_texts, meta).

    cells       : { (svg_row, svg_tri_col), ... }
    clue_texts  : axis -> line -> [(slot, value), ...]
    meta        : { "author": str, "id": str }
    """
    BeautifulSoup = _lazy_bs4()
    soup = BeautifulSoup(html, "lxml")

    zoom = soup.find(id="zoomContainer")
    if zoom is None:
        sys.exit("error: <g id='zoomContainer'> not found in page")

    cells: Set[Tuple[int, int]] = set()
    for poly in zoom.find_all("polygon"):
        m = CELL_RE.match(poly.get("id", ""))
        if m:
            cells.add((int(m.group(1)), int(m.group(2))))

    clue_texts: Dict[int, Dict[int, List[Tuple[int, int]]]] = {0: {}, 1: {}, 2: {}}
    for t in zoom.find_all("text"):
        m = TEXT_RE.match(t.get("id", ""))
        if not m:
            continue
        axis = int(m.group(1))
        if axis not in clue_texts:
            continue
        line = int(m.group(2))
        slot = int(m.group(3))
        raw = (t.text or "").strip()
        if not raw.lstrip("-").isdigit():
            continue
        clue_texts[axis].setdefault(line, []).append((slot, int(raw)))

    body = soup.get_text(" ", strip=False)
    meta = {}
    am = META_AUTHOR_RE.search(body)
    if am:
        meta["author"] = am.group(1).strip()
    im = META_ID_RE.search(body)
    if im:
        meta["id"] = im.group(1)

    return cells, clue_texts, meta


# ── Model assembly ────────────────────────────────────────────────────────────


def build_puzzle(
    cells: Set[Tuple[int, int]],
    clue_texts: Dict[int, Dict[int, List[Tuple[int, int]]]],
) -> Dict:
    if not cells:
        sys.exit("error: no cell polygons (c_R_C) found")

    max_row = max(r for r, _ in cells)
    max_tri = max(c for _, c in cells)
    base_rows = max_row + 1
    base_cols = max_tri // 2 + 1

    def ordered(seq: List[Tuple[int, int]]) -> List[int]:
        return [v for _, v in sorted(seq, key=lambda kv: kv[0])]

    horizontals = [ordered(clue_texts[0].get(r, [])) for r in range(base_rows)]
    verticals   = [ordered(clue_texts[1].get(c, [])) for c in range(base_cols)]

    # Compute which model diagonal slots are "active" (their anchor triangle
    # exists in the SVG). The factory order is:
    #   d in [0, base_rows): anchor (d, base_cols-1, Right)  -> tri_col = 2*base_cols - 1
    #   d in [base_rows, base_rows+base_cols): anchor (base_rows-1, base_rows+base_cols-1-d, Left)
    #                                          -> tri_col = 2 * (base_rows+base_cols-1-d)
    total_diag = base_rows + base_cols
    active_model_ds: List[int] = []
    for d in range(base_rows):
        if (d, 2 * base_cols - 1) in cells:
            active_model_ds.append(d)
    for d in range(base_rows, total_diag):
        col = base_rows + base_cols - 1 - d
        if (base_rows - 1, 2 * col) in cells:
            active_model_ds.append(d)

    html_lines = sorted(clue_texts[2].keys())
    if len(active_model_ds) != len(html_lines):
        sys.exit(
            f"error: diagonal mismatch — anchor-derived active count "
            f"{len(active_model_ds)} != SVG t2 lines {len(html_lines)}"
        )

    diagonals: List[List[int]] = [[] for _ in range(total_diag)]
    # HTML t2 indexes from bottom-left → top-right of the figure; the model
    # walks top-right → bottom-right → bottom-left. Reverse-map.
    # Within each diagonal line, the SVG slots are numbered innermost-first
    # whereas the on-disk schema stores clues outermost-first, so reverse.
    for k, html_line in enumerate(html_lines):
        model_d = active_model_ds[len(active_model_ds) - 1 - k]
        diagonals[model_d] = list(reversed(ordered(clue_texts[2][html_line])))

    return {
        "Horizontals":      horizontals,
        "Verticals":        verticals,
        "Diagonals":        diagonals,
        "BaseRowsCount":    base_rows,
        "BaseColumnCount":  base_cols,
    }


def add_metadata(puzzle: Dict, meta: Dict[str, str]) -> Dict:
    pid = meta.get("id", "")
    author = meta.get("author", "")
    if author and pid:
        name = f"{author}-{pid}"
    elif pid:
        name = f"griddlers-{pid}"
    else:
        name = f"griddlers-{uuid.uuid4().hex[:8]}"

    stamped = {
        "Type":         "Triddler",
        "Source":       "griddlers.net",
        "Name":         name,
        "DateCreated":  datetime.now().astimezone().isoformat(),
    }
    stamped.update(puzzle)
    return stamped


# ── CLI ───────────────────────────────────────────────────────────────────────


def repo_root_default_outdir() -> Path:
    here = Path(__file__).resolve().parent           # Tools/
    return here.parent / "Documents" / "Triddler" / "puzzles"


def write_puzzle(puzzle: Dict, outdir: Path) -> Path:
    outdir.mkdir(parents=True, exist_ok=True)
    path = outdir / f"{uuid.uuid4()}.json"
    path.write_text(json.dumps(puzzle, indent=2, ensure_ascii=False), encoding="utf-8")
    return path


def process_html(html: str, outdir: Path) -> Path:
    cells, clue_texts, meta = parse_svg(html)
    puzzle = build_puzzle(cells, clue_texts)
    puzzle = add_metadata(puzzle, meta)
    return write_puzzle(puzzle, outdir)


def main() -> None:
    ap = argparse.ArgumentParser(description="Scrape Triddler puzzles from griddlers.net")
    ap.add_argument("targets", nargs="*", help="puzzle id (e.g. 3682) or full URL")
    ap.add_argument("--from-file", action="append", default=[],
                    help="parse a local HTML file instead of fetching (repeatable)")
    ap.add_argument("--outdir", type=Path, default=repo_root_default_outdir(),
                    help="output directory (default: <repo>/Documents/Triddler/puzzles)")
    ap.add_argument("--cookie", default=None,
                    help="optional Cookie header to send with requests")
    args = ap.parse_args()

    if not args.targets and not args.from_file:
        ap.error("provide at least one puzzle id/URL or --from-file PATH")

    written = []

    for path in args.from_file:
        html = Path(path).read_text(encoding="utf-8", errors="replace")
        out = process_html(html, args.outdir)
        written.append((path, out))

    for target in args.targets:
        html = fetch_html(target, args.cookie)
        out = process_html(html, args.outdir)
        written.append((target, out))

    for src, dst in written:
        print(f"{src} -> {dst}")


if __name__ == "__main__":
    main()
