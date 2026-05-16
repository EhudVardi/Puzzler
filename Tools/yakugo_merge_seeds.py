#!/usr/bin/env python3
"""
yakugo_merge_seeds.py
=====================
Merges a seeds JSON file (same grouped Categories structure) into the main
yakugo_wordlist.json, deduplicating by normalized source and target.

Usage
-----
  # Merge default seeds into default wordlist:
  python yakugo_merge_seeds.py

  # Custom paths:
  python yakugo_merge_seeds.py --seeds my_seeds.json --wordlist my_words.json

  # Preview only:
  python yakugo_merge_seeds.py --dry-run
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
DEFAULT_SEEDS    = os.path.join(_HERE, "yakugo_manual_seeds.json")

_FINAL = str.maketrans("ךםןףץ", "כמנפצ")


def norm_tgt(t: str) -> str:
    return t.translate(_FINAL).replace(" ", "").replace("-", "").lower()


def norm_src(s: str) -> str:
    return s.strip().lower()


def load(path: str):
    with open(path, encoding="utf-8") as f:
        data = json.load(f)
    src_lang = data.get("SourceLanguage", "en")
    tgt_lang = data.get("TargetLanguage", "he")
    cats = data.get("Categories", {})
    return cats, src_lang, tgt_lang


def save(path: str, categories: dict, src_lang: str, tgt_lang: str) -> None:
    KNOWN_ORDER = ["noun", "verb", "adjective", "adverb"]
    ordered = {}
    for key in KNOWN_ORDER:
        if key in categories:
            ordered[key] = categories[key]
    for key in sorted(categories):
        if key not in ordered:
            ordered[key] = categories[key]
    tmp = path + ".tmp"
    with open(tmp, "w", encoding="utf-8") as f:
        json.dump({"SourceLanguage": src_lang, "TargetLanguage": tgt_lang,
                   "Categories": ordered}, f, indent=2, ensure_ascii=False)
    os.replace(tmp, path)


def merge(wordlist_path: str, seeds_path: str, dry_run: bool) -> None:
    main_cats, src_lang, tgt_lang = load(wordlist_path)
    seed_cats, _, _ = load(seeds_path)

    # Build dedup sets from existing entries
    existing_srcs: set = set()
    existing_tgts: set = set()
    for entries in main_cats.values():
        for e in entries:
            existing_srcs.add(norm_src(e["Source"]))
            existing_tgts.add(norm_tgt(e["Target"]))

    added_by_cat: dict = defaultdict(int)
    skipped = 0

    for cat, entries in seed_cats.items():
        bucket = main_cats.setdefault(cat, [])
        for e in entries:
            ns = norm_src(e["Source"])
            nt = norm_tgt(e["Target"])
            if ns in existing_srcs or nt in existing_tgts:
                skipped += 1
                continue
            if not dry_run:
                bucket.append({"Source": e["Source"], "Target": e["Target"]})
            existing_srcs.add(ns)
            existing_tgts.add(nt)
            added_by_cat[cat] += 1

    total_added = sum(added_by_cat.values())
    print(f"{'[DRY RUN] ' if dry_run else ''}Merge results:")
    for cat, n in sorted(added_by_cat.items()):
        print(f"  +{n:4d} {cat}")
    print(f"  {skipped:4d} skipped (already present)")
    print(f"  {total_added:4d} total new entries")

    if not dry_run and total_added:
        save(wordlist_path, main_cats, src_lang, tgt_lang)
        total = sum(len(v) for v in main_cats.values())
        print(f"\nSaved {total} total words → {os.path.abspath(wordlist_path)}")


def main() -> None:
    ap = argparse.ArgumentParser(
        description="Merge a Yakugo seed file into the main wordlist."
    )
    ap.add_argument("--wordlist", default=DEFAULT_WORDLIST)
    ap.add_argument("--seeds",    default=DEFAULT_SEEDS)
    ap.add_argument("--dry-run",  action="store_true")
    args = ap.parse_args()
    merge(args.wordlist, args.seeds, args.dry_run)


if __name__ == "__main__":
    main()
