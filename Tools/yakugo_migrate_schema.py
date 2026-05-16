#!/usr/bin/env python3
"""
yakugo_migrate_schema.py
========================
One-shot migration: converts yakugo_wordlist.json from the legacy flat
{"Words": [...]} structure to the new category-grouped structure:

  {
    "SourceLanguage": "en",
    "TargetLanguage": "he",
    "Categories": {
      "noun":      [...],
      "verb":      [...],
      "adjective": [...],
      "adverb":    [...],
      "other":     [...]   <- entries with unknown/missing POS
    }
  }

Safe to run multiple times — detects an already-migrated file and exits
without touching it.

Usage
-----
  python yakugo_migrate_schema.py                           # default wordlist
  python yakugo_migrate_schema.py path/to/wordlist.json    # custom path
  python yakugo_migrate_schema.py --dry-run                # preview only
"""

import argparse
import json
import os
import sys
from collections import defaultdict

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")

_HERE = os.path.dirname(__file__)
DEFAULT_WORDLIST = os.path.join(_HERE, "yakugo_wordlist.json")

KNOWN_POS = {"noun", "verb", "adjective", "adverb"}


def migrate(path: str, dry_run: bool = False) -> None:
    with open(path, encoding="utf-8") as f:
        data = json.load(f)

    if "Categories" in data:
        print(f"Already migrated ({sum(len(v) for v in data['Categories'].values())} words). Nothing to do.")
        return

    src_lang = data.get("SourceLanguage", "en")
    tgt_lang = data.get("TargetLanguage", "he")
    words = data.get("Words", [])

    buckets: dict = defaultdict(list)
    for w in words:
        pos = w.get("PartOfSpeech", "").lower()
        if pos not in KNOWN_POS:
            pos = "noun"  # 53 hand-curated entries are all nouns
        entry = {"Source": w["Source"], "Target": w["Target"]}
        buckets[pos].append(entry)

    new_data = {
        "SourceLanguage": src_lang,
        "TargetLanguage": tgt_lang,
        "Categories": dict(buckets),
    }

    total = sum(len(v) for v in new_data["Categories"].values())
    print(f"Migration preview:")
    for cat, entries in new_data["Categories"].items():
        print(f"  {cat:12s}: {len(entries):4d} words")
    print(f"  {'TOTAL':12s}: {total:4d} words")

    if dry_run:
        print("\n[DRY RUN] No file written.")
        return

    tmp = path + ".tmp"
    with open(tmp, "w", encoding="utf-8") as f:
        json.dump(new_data, f, indent=2, ensure_ascii=False)
    os.replace(tmp, path)
    print(f"\nMigrated {total} words → {os.path.abspath(path)}")


def main() -> None:
    ap = argparse.ArgumentParser(
        description="Migrate yakugo_wordlist.json from flat Words[] to grouped Categories{}.",
    )
    ap.add_argument("wordlist", nargs="?", default=DEFAULT_WORDLIST)
    ap.add_argument("--dry-run", action="store_true")
    args = ap.parse_args()
    migrate(args.wordlist, args.dry_run)


if __name__ == "__main__":
    main()
