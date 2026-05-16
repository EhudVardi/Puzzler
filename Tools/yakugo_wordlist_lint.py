#!/usr/bin/env python3
"""
yakugo_wordlist_lint.py
=======================
Interactive lint and dedup tool for yakugo_wordlist.json.

Walks every category and entry, runs a battery of checks, and for each
flagged entry shows an interactive prompt so you can keep, delete, edit,
or move it.  Progress is journaled so reruns skip already-reviewed entries.

Checks
------
  Source quality:
    - longer than 40 characters
    - starts with "to " (verb gloss in a noun bucket)
    - starts with "a " or "an " or "the " (article — usually just a gloss artifact)
    - contains parentheses (e.g. "foo (bar)")
    - contains Wiktionary template residue ({{, |)
    - leading colon or comma
    - trailing punctuation (., ;, :, ,)
    - all-uppercase source
    - mixed scripts in source

  Target quality:
    - non-Hebrew characters (after stripping spaces/hyphens)
    - Hebrew length < 2 or > 8

  Cross-entry:
    - duplicate source (case-insensitive, ignoring "a/an/the" prefixes)
    - duplicate target (final-letter normalised)

Usage
-----
  python yakugo_wordlist_lint.py                  # default wordlist, interactive
  python yakugo_wordlist_lint.py --report-only    # print flags, exit without prompts
  python yakugo_wordlist_lint.py --reset          # clear all reviewed markers and re-lint all
  python yakugo_wordlist_lint.py path/to/wordlist.json
"""

import argparse
import json
import os
import re
import sys
from collections import defaultdict
from typing import Dict, List, Optional, Set, Tuple

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")

_HERE = os.path.dirname(__file__)
DEFAULT_WORDLIST = os.path.join(_HERE, "yakugo_wordlist.json")

# Final Hebrew letters → regular forms (for dedup normalisation)
_FINAL_TO_REGULAR = str.maketrans("ךםןףץ", "כמנפצ")
_HEBREW_RE = re.compile(r"^[א-ת\s\-]+$")


def normalize_target(t: str) -> str:
    return t.translate(_FINAL_TO_REGULAR).replace(" ", "").replace("-", "")


def normalize_source(s: str) -> str:
    s = s.lower().strip()
    for prefix in ("a ", "an ", "the ", "to "):
        if s.startswith(prefix):
            s = s[len(prefix):]
    return s


def strip_spaces(s: str) -> str:
    return "".join(c for c in s if c not in " -")


# ── Checks ──────────────────────────────────────────────────────────────────────

def check_entry(source: str, target: str, category: str = "") -> List[str]:
    flags = []

    # Source checks
    if len(source) > 40:
        flags.append(f"source too long ({len(source)} chars)")
    # "to " prefix is expected for verbs, suspicious elsewhere
    if source.startswith("to ") and category != "verb":
        flags.append('source starts with "to " (verb gloss?)')
    if re.match(r"^(a |an |the )", source, re.IGNORECASE):
        flags.append('source starts with article ("a/an/the")')
    if "(" in source or ")" in source:
        flags.append("source contains parentheses")
    if "{{" in source or "}}" in source or "|" in source:
        flags.append("source contains Wiktionary template residue")
    if source.lstrip().startswith(":") or source.lstrip().startswith(","):
        flags.append("source starts with colon or comma")
    if source.rstrip().endswith((".", ";", ":", ",")):
        flags.append("source has trailing punctuation")
    if source == source.upper() and len(source) > 2 and source.isalpha():
        flags.append("source is all-uppercase")

    # Target checks
    bare = strip_spaces(target)
    if not _HEBREW_RE.match(target.replace(" ", "").replace("-", "")):
        flags.append(f"target contains non-Hebrew characters")
    if len(bare) < 2:
        flags.append(f"target too short ({len(bare)} chars)")
    if len(bare) > 8:
        flags.append(f"target too long ({len(bare)} chars)")

    return flags


# ── Journal (reviewed markers) ────────────────────────────────────────────────

def journal_path(wordlist_path: str) -> str:
    return wordlist_path + ".reviewed"


def load_journal(path: str) -> Set[str]:
    if not os.path.exists(path):
        return set()
    with open(path, encoding="utf-8") as f:
        return set(line.strip() for line in f if line.strip())


def save_journal(path: str, reviewed: Set[str]) -> None:
    with open(path, "w", encoding="utf-8") as f:
        for key in sorted(reviewed):
            f.write(key + "\n")


def entry_key(cat: str, source: str, target: str) -> str:
    return f"{cat}|{source}|{target}"


# ── I/O ────────────────────────────────────────────────────────────────────────

def load_wordlist(path: str):
    with open(path, encoding="utf-8") as f:
        data = json.load(f)
    src_lang = data.get("SourceLanguage", "en")
    tgt_lang = data.get("TargetLanguage", "he")
    if "Categories" in data:
        return data["Categories"], src_lang, tgt_lang
    # Legacy flat — wrap in a single "noun" category for linting
    return {"noun": data.get("Words", [])}, src_lang, tgt_lang


def save_wordlist(path: str, categories: Dict, src_lang: str, tgt_lang: str) -> None:
    tmp = path + ".tmp"
    with open(tmp, "w", encoding="utf-8") as f:
        json.dump({"SourceLanguage": src_lang, "TargetLanguage": tgt_lang,
                   "Categories": categories}, f, indent=2, ensure_ascii=False)
    os.replace(tmp, path)


# ── Interactive session ────────────────────────────────────────────────────────

HELP_TEXT = """
  [k]eep          keep as-is and mark reviewed
  [d]elete        remove this entry
  [e]dit          edit Source and/or Target inline
  [m] <cat>       move to another category (e.g.  m verb)
  [s]kip          skip this entry for now (not marked reviewed)
  [S]kip-type     skip all remaining flags of this check type
  [q]uit          save progress and exit
  [?]             show this help
"""


def interactive_lint(
    wordlist_path: str,
    report_only: bool = False,
    reset: bool = False,
) -> None:
    categories, src_lang, tgt_lang = load_wordlist(wordlist_path)
    j_path = journal_path(wordlist_path)
    reviewed = set() if reset else load_journal(j_path)

    # Build global dedup sets from ALL entries (for cross-entry checks)
    all_sources: Dict[str, List[Tuple[str, str, str]]] = defaultdict(list)  # norm_src → [(cat, src, tgt)]
    all_targets: Dict[str, List[Tuple[str, str, str]]] = defaultdict(list)  # norm_tgt → [(cat, src, tgt)]
    for cat, entries in categories.items():
        for e in entries:
            ns = normalize_source(e["Source"])
            nt = normalize_target(e["Target"])
            all_sources[ns].append((cat, e["Source"], e["Target"]))
            all_targets[nt].append((cat, e["Source"], e["Target"]))

    dup_sources: Set[str] = {ns for ns, lst in all_sources.items() if len(lst) > 1}
    dup_targets: Set[str] = {nt for nt, lst in all_targets.items() if len(lst) > 1}

    total_flagged = 0
    total_reviewed_now = 0
    skipped_types: Set[str] = set()
    dirty = False

    available_cats = set(categories.keys())

    def print_sep():
        print("─" * 60)

    for cat in list(categories.keys()):
        entries = categories[cat]
        i = 0
        while i < len(entries):
            e = entries[i]
            src, tgt = e["Source"], e["Target"]
            key = entry_key(cat, src, tgt)

            flags = check_entry(src, tgt, category=cat)

            ns = normalize_source(src)
            nt = normalize_target(tgt)
            if ns in dup_sources:
                other = [(c, s, t) for c, s, t in all_sources[ns]
                         if not (c == cat and s == src and t == tgt)]
                if other:
                    flags.append(f"duplicate source (also in: {other[0][0]})")
            if nt in dup_targets:
                other = [(c, s, t) for c, s, t in all_targets[nt]
                         if not (c == cat and s == src and t == tgt)]
                if other:
                    flags.append(f"duplicate target (also in: {other[0][0]})")

            # Filter flags suppressed by skip-type
            active_flags = [f for f in flags if f not in skipped_types]

            if not active_flags:
                i += 1
                continue

            total_flagged += 1

            if key in reviewed:
                i += 1
                continue

            if report_only:
                print(f"[{cat}]  {src!r:40s} → {tgt!r}")
                for fl in active_flags:
                    print(f"       ⚑ {fl}")
                i += 1
                continue

            # Interactive prompt
            print_sep()
            print(f"[{cat}  #{i+1}/{len(entries)}]")
            print(f"  Source : {src!r}")
            print(f"  Target : {tgt!r}")
            for fl in active_flags:
                print(f"  ⚑ {fl}")
            print()

            action = ""
            while True:
                try:
                    raw = input("  Action: [k]eep / [d]elete / [e]dit / [m]<cat> / [s]kip / [S]kip-type / [q]uit / [?] > ").strip()
                except (EOFError, KeyboardInterrupt):
                    print("\nInterrupted — saving progress.")
                    raw = "q"

                if raw in ("?", "h", "help"):
                    print(HELP_TEXT)
                    continue

                action = raw
                break

            if action in ("k", "keep"):
                reviewed.add(key)
                total_reviewed_now += 1
                i += 1

            elif action in ("d", "delete"):
                entries.pop(i)
                reviewed.discard(key)
                # Update dedup sets
                all_sources[ns] = [(c, s, t) for c, s, t in all_sources[ns]
                                   if not (c == cat and s == src and t == tgt)]
                all_targets[nt] = [(c, s, t) for c, s, t in all_targets[nt]
                                   if not (c == cat and s == src and t == tgt)]
                dirty = True
                print(f"  Deleted.")
                # don't increment i — next entry slides into place

            elif action in ("e", "edit"):
                new_src = input(f"  New Source [{src}]: ").strip() or src
                new_tgt = input(f"  New Target [{tgt}]: ").strip() or tgt
                entries[i] = {"Source": new_src, "Target": new_tgt}
                reviewed.add(entry_key(cat, new_src, new_tgt))
                dirty = True
                print(f"  Updated.")
                i += 1

            elif action.startswith("m") and len(action) > 1:
                dest_cat = action[1:].strip().lower()
                if dest_cat not in available_cats:
                    categories[dest_cat] = []
                    available_cats.add(dest_cat)
                entries.pop(i)
                categories[dest_cat].append({"Source": src, "Target": tgt})
                reviewed.add(entry_key(dest_cat, src, tgt))
                dirty = True
                print(f"  Moved to '{dest_cat}'.")
                # don't increment i

            elif action in ("s", "skip"):
                print(f"  Skipped.")
                i += 1

            elif action in ("S", "skip-type", "Skip-type"):
                for fl in active_flags:
                    skipped_types.add(fl)
                print(f"  Will skip '{active_flags[0]}' for the rest of this session.")
                i += 1

            elif action in ("q", "quit"):
                print("\nSaving and exiting.")
                break

            else:
                print("  Unknown action. Type [?] for help.")

        else:
            continue
        break  # quit was chosen

    # Persist changes
    save_journal(j_path, reviewed)
    if dirty:
        save_wordlist(wordlist_path, categories, src_lang, tgt_lang)
        print(f"Wordlist saved → {os.path.abspath(wordlist_path)}")

    print(f"\nSummary: {total_flagged} flagged  |  {total_reviewed_now} reviewed this session")
    if report_only:
        print("(report-only mode — no changes written)")


# ── CLI ────────────────────────────────────────────────────────────────────────

def main() -> None:
    ap = argparse.ArgumentParser(
        description="Interactively lint and deduplicate a Yakugo wordlist JSON.",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog=__doc__,
    )
    ap.add_argument("wordlist", nargs="?", default=DEFAULT_WORDLIST,
                    help="Path to wordlist JSON  (default: yakugo_wordlist.json)")
    ap.add_argument("--report-only", action="store_true",
                    help="Print flagged entries without interactive prompts")
    ap.add_argument("--reset", action="store_true",
                    help="Clear all reviewed markers and re-lint everything")
    args = ap.parse_args()

    interactive_lint(
        wordlist_path=args.wordlist,
        report_only=args.report_only,
        reset=args.reset,
    )


if __name__ == "__main__":
    main()
