#!/usr/bin/env python3
"""
Format Griddler and Triddler JSON puzzle files so that each inner clue array
sits on a single line:  [ 1, 2, 3 ]

Usage:
    python format_puzzles.py                  # reformat all puzzle JSONs in Documents/ and TestData/
    python format_puzzles.py path/to/file.json [...]
    python format_puzzles.py --dry-run        # show what would change, write nothing
"""

import json
import re
import sys
from pathlib import Path

_INNER_ARRAY_RE = re.compile(
    r'\[\s*\n\s+\d+(?:,\s*\n\s+\d+)*\s*\n\s+\]'
)

PUZZLE_KEYS = {'Horizontals', 'Verticals', 'Diagonals', 'Rows', 'Columns'}


def _collapse(match: re.Match) -> str:
    nums = re.findall(r'\d+', match.group(0))
    return '[ ' + ', '.join(nums) + ' ]'


def format_puzzle_json(data: dict) -> str:
    raw = json.dumps(data, indent=2)
    return _INNER_ARRAY_RE.sub(_collapse, raw) + '\n'


def process_file(path: Path, dry_run: bool = False) -> bool:
    try:
        text = path.read_text(encoding='utf-8')
        data = json.loads(text)
    except (json.JSONDecodeError, OSError) as e:
        print(f'  SKIP  {path}  ({e})')
        return False

    if not PUZZLE_KEYS.intersection(data.keys()):
        print(f'  SKIP  {path}  (unrecognised format)')
        return False

    formatted = format_puzzle_json(data)
    if formatted == text:
        print(f'  OK    {path.name}')
        return False

    if not dry_run:
        path.write_text(formatted, encoding='utf-8')
        print(f'  FMT   {path.name}')
    else:
        print(f'  DIFF  {path.name}')
    return True


def collect_default_targets(repo_root: Path) -> list[Path]:
    patterns = [
        'Documents/Griddler/**/*.json',
        'Documents/Triddler/**/*.json',
        'Source/Tests/TestData/griddler*.json',
        'Source/Tests/TestData/triddler*.json',
    ]
    files: list[Path] = []
    for p in patterns:
        files.extend(repo_root.glob(p))
    return sorted(set(files))


def main() -> None:
    args = sys.argv[1:]
    dry_run = '--dry-run' in args
    path_args = [a for a in args if not a.startswith('--')]

    if path_args:
        targets = [Path(p) for p in path_args]
    else:
        repo_root = Path(__file__).resolve().parent.parent
        targets = collect_default_targets(repo_root)

    changed = 0
    for t in targets:
        if t.is_file():
            if process_file(t, dry_run):
                changed += 1

    label = 'would be reformatted' if dry_run else 'reformatted'
    print(f'\n{changed} file(s) {label}.')


if __name__ == '__main__':
    main()
