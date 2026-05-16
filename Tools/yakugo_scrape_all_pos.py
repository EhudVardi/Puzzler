#!/usr/bin/env python3
"""
yakugo_scrape_all_pos.py
========================
Convenience wrapper: runs yakugo_wiktionary.py for each POS in sequence,
pausing politely between runs.  Designed to be left running unattended —
each POS scrape saves progress immediately so a Ctrl-C between phases
doesn't lose work.

Usage
-----
  # Full run targeting ~5000 total words:
  python yakugo_scrape_all_pos.py

  # Custom limits:
  python yakugo_scrape_all_pos.py --noun-limit 2500 --verb-limit 700 --adj-limit 700 --adv-limit 400

  # Dry run (preview only, no writes):
  python yakugo_scrape_all_pos.py --dry-run
"""

import argparse
import os
import subprocess
import sys
import time

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")

_HERE = os.path.dirname(__file__)
WIKTIONARY_SCRIPT = os.path.join(_HERE, "yakugo_wiktionary.py")
DEFAULT_WORDLIST   = os.path.join(_HERE, "yakugo_wordlist.json")


def run_pos(wordlist: str, pos: str, limit: int, max_len: int, dry_run: bool) -> None:
    print(f"\n{'='*60}")
    print(f"  Phase: {pos}s   (limit {limit})")
    print(f"{'='*60}\n")
    cmd = [
        sys.executable, WIKTIONARY_SCRIPT,
        "--wordlist", wordlist,
        "--pos",      pos,
        "--limit",    str(limit),
        "--max-len",  str(max_len),
    ]
    if dry_run:
        cmd.append("--dry-run")
    subprocess.run(cmd, check=False)


def main() -> None:
    ap = argparse.ArgumentParser(
        description="Batch-scrape all POS categories into yakugo_wordlist.json.",
    )
    ap.add_argument("--wordlist",    default=DEFAULT_WORDLIST)
    ap.add_argument("--noun-limit",  type=int, default=2500,
                    help="Max new nouns to add  (default: 2500)")
    ap.add_argument("--verb-limit",  type=int, default=700,
                    help="Max new verbs to add  (default: 700)")
    ap.add_argument("--adj-limit",   type=int, default=700,
                    help="Max new adjectives to add  (default: 700)")
    ap.add_argument("--adv-limit",   type=int, default=400,
                    help="Max new adverbs to add  (default: 400)")
    ap.add_argument("--max-len",     type=int, default=8,
                    help="Max Hebrew target length  (default: 8)")
    ap.add_argument("--dry-run",     action="store_true")
    args = ap.parse_args()

    phases = [
        ("noun",      args.noun_limit),
        ("verb",      args.verb_limit),
        ("adjective", args.adj_limit),
        ("adverb",    args.adv_limit),
    ]

    for pos, limit in phases:
        run_pos(args.wordlist, pos, limit, args.max_len, args.dry_run)
        if pos != phases[-1][0]:
            print(f"\nPausing 10s before next phase …")
            time.sleep(10)

    print(f"\n✓ All phases complete.")


if __name__ == "__main__":
    main()
