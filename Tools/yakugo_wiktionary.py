#!/usr/bin/env python3
"""
yakugo_wiktionary.py
====================
Fetches English-Hebrew word pairs from the English Wiktionary API and
accumulates them into a Yakugo vocabulary JSON file.

Words are fetched by part-of-speech category (noun, verb, adjective, adverb),
tagged with their POS, and merged into the existing wordlist without removing
or overwriting any hand-curated entries.

Usage
-----
  # Add up to 200 new words (nouns + verbs + adjectives, the default mix):
  python yakugo_wiktionary.py

  # Fetch only nouns, preview without writing:
  python yakugo_wiktionary.py --pos noun --limit 500 --dry-run

  # Custom wordlist path and stricter length limits:
  python yakugo_wiktionary.py --wordlist my_words.json --min-len 2 --max-len 5
"""

import argparse
import json
import os
import re
import sys
import time
import urllib.parse
import urllib.request
from typing import Dict, List, Optional, Set, Tuple

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")

# ── Configuration ──────────────────────────────────────────────────────────────

_HERE = os.path.dirname(__file__)
DEFAULT_WORDLIST = os.path.join(_HERE, "yakugo_wordlist.json")

WIKTIONARY_API = "https://en.wiktionary.org/w/api.php"

# Wiktionary category name and section header per POS
POS_CATEGORIES: Dict[str, Tuple[str, str]] = {
    "noun":      ("Hebrew_nouns",      "Noun"),
    "verb":      ("Hebrew_verbs",      "Verb"),
    "adjective": ("Hebrew_adjectives", "Adjective"),
    "adverb":    ("Hebrew_adverbs",    "Adverb"),
}

# Final Hebrew letters → regular forms (so crossword intersections work on any position)
_FINAL_TO_REGULAR = str.maketrans("ךםןףץ", "כמנפצ")

DEFAULT_POS    = ["noun", "verb", "adjective"]
DEFAULT_LIMIT  = 200
DEFAULT_MIN    = 2
DEFAULT_MAX    = 7
API_DELAY      = 0.5   # seconds between API calls — be a polite client
BATCH_SIZE     = 50    # max titles per revisions request


# ── Hebrew helpers ─────────────────────────────────────────────────────────────

def normalize_hebrew(word: str) -> str:
    return word.translate(_FINAL_TO_REGULAR)


_HEBREW_RE = re.compile(r'^[א-ת]+$')

def is_pure_hebrew(word: str) -> bool:
    return bool(_HEBREW_RE.match(word))


# ── Wikitext parsing ───────────────────────────────────────────────────────────

def _strip_markup(text: str) -> str:
    text = re.sub(r'\{\{[^}]*\}\}', '', text)                      # {{templates}}
    text = re.sub(r'\[\[(?:[^\]|]+\|)?([^\]]+)\]\]', r'\1', text)  # [[link|label]]
    text = re.sub(r"['\[\]{}|<>]", '', text)
    return re.sub(r'\s+', ' ', text).strip().rstrip('.')


def parse_definition(wikitext: str, pos_header: str) -> Optional[str]:
    """Extract the first English definition from the Hebrew section of a Wiktionary page."""
    he = re.search(r'==Hebrew==\s*(.*?)(?=\n==[^=]|\Z)', wikitext, re.DOTALL)
    if not he:
        return None

    pos = re.search(
        rf'==={re.escape(pos_header)}===\s*(.*?)(?=\n==|\Z)',
        he.group(1), re.DOTALL,
    )
    if not pos:
        return None

    for line in pos.group(1).splitlines():
        line = line.strip()
        if line.startswith('#') and not line.startswith(('#:', '#*')):
            definition = _strip_markup(line[1:])
            if definition:
                return definition
    return None


# ── Wiktionary API ─────────────────────────────────────────────────────────────

_HEADERS = {
    "User-Agent": "yakugo-wordlist-builder/1.0 (Puzzler project; educational use)"
}

def _api(params: Dict, _retries: int = 5) -> Dict:
    params = dict(params, format="json", formatversion="2")
    url = WIKTIONARY_API + "?" + urllib.parse.urlencode(params)
    req = urllib.request.Request(url, headers=_HEADERS)
    for attempt in range(_retries):
        try:
            with urllib.request.urlopen(req, timeout=20) as r:
                return json.loads(r.read().decode("utf-8"))
        except urllib.error.HTTPError as e:
            if e.code == 429 and attempt < _retries - 1:
                wait = 2 ** (attempt + 2)   # 4s, 8s, 16s, 32s …
                print(f"  [rate-limited] waiting {wait}s …", flush=True)
                time.sleep(wait)
            else:
                raise


def fetch_category_members(category: str) -> List[str]:
    """Return every page title in a Wiktionary category, following continuation."""
    titles: List[str] = []
    params: Dict = {
        "action":  "query",
        "list":    "categorymembers",
        "cmtitle": f"Category:{category}",
        "cmlimit": "500",
        "cmtype":  "page",
    }
    while True:
        data = _api(params)
        for m in data.get("query", {}).get("categorymembers", []):
            titles.append(m["title"])
        cont = data.get("continue", {}).get("cmcontinue")
        if not cont:
            break
        params["cmcontinue"] = cont
        time.sleep(API_DELAY)
    return titles


def fetch_wikitext_batch(titles: List[str]) -> Dict[str, str]:
    """Fetch wikitext for up to BATCH_SIZE titles in a single API call."""
    params = {
        "action":  "query",
        "prop":    "revisions",
        "rvprop":  "content",
        "rvslots": "main",
        "titles":  "|".join(titles),
    }
    result: Dict[str, str] = {}
    for page in _api(params).get("query", {}).get("pages", []):
        title = page.get("title", "")
        revs  = page.get("revisions", [])
        if revs:
            content = revs[0].get("slots", {}).get("main", {}).get("content", "")
            if content:
                result[title] = content
    return result


# ── Scrape ─────────────────────────────────────────────────────────────────────

def scrape(
    pos_list: List[str],
    limit: int,
    min_len: int,
    max_len: int,
    existing_words: List[Dict],
) -> List[Dict]:
    existing_targets: Set[str] = {normalize_hebrew(w["Target"]) for w in existing_words}
    existing_sources: Set[str] = {w["Source"].lower()           for w in existing_words}
    new_words: List[Dict] = []
    added = 0

    for pos in pos_list:
        if added >= limit:
            break

        category, pos_header = POS_CATEGORIES[pos]
        print(f"\n── {pos}s  (Category:{category}) ──")
        titles = fetch_category_members(category)
        print(f"  {len(titles)} pages in category")

        for batch_start in range(0, len(titles), BATCH_SIZE):
            if added >= limit:
                break

            batch = titles[batch_start:batch_start + BATCH_SIZE]
            wikitexts = fetch_wikitext_batch(batch)
            time.sleep(API_DELAY)

            for title in batch:
                if added >= limit:
                    break

                wikitext = wikitexts.get(title, "")
                if not wikitext:
                    continue

                # The page title is the Hebrew word as written (may use final forms)
                target_raw = title.strip()
                if not is_pure_hebrew(target_raw):
                    continue

                target_norm = normalize_hebrew(target_raw)
                if not (min_len <= len(target_norm) <= max_len):
                    continue
                if target_norm in existing_targets:
                    continue

                source = parse_definition(wikitext, pos_header)
                if not source or len(source) > 40:
                    continue
                if source.lower() in existing_sources:
                    continue

                new_words.append({"Source": source, "Target": target_raw, "PartOfSpeech": pos})
                existing_targets.add(target_norm)
                existing_sources.add(source.lower())
                added += 1

        print(f"  → {added} new word(s) accumulated so far")

    return new_words


# ── I/O ────────────────────────────────────────────────────────────────────────

def load_wordlist(path: str) -> Tuple[List[Dict], str, str]:
    if not os.path.exists(path):
        return [], "en", "he"
    with open(path, encoding="utf-8") as f:
        data = json.load(f)
    return data["Words"], data.get("SourceLanguage", "en"), data.get("TargetLanguage", "he")


def save_wordlist(path: str, words: List[Dict], src: str, tgt: str) -> None:
    with open(path, "w", encoding="utf-8") as f:
        json.dump({"SourceLanguage": src, "TargetLanguage": tgt, "Words": words},
                  f, indent=2, ensure_ascii=False)


# ── CLI ────────────────────────────────────────────────────────────────────────

def main() -> None:
    ap = argparse.ArgumentParser(
        description="Accumulate English-Hebrew word pairs from Wiktionary into a Yakugo wordlist.",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog=__doc__,
    )
    ap.add_argument("--wordlist", default=DEFAULT_WORDLIST,
                    help=f"Wordlist JSON to accumulate into  (default: yakugo_wordlist.json)")
    ap.add_argument("--pos", nargs="+", choices=list(POS_CATEGORIES), default=DEFAULT_POS,
                    metavar="POS",
                    help=f"Parts of speech to fetch  (choices: {', '.join(POS_CATEGORIES)}; "
                         f"default: {' '.join(DEFAULT_POS)})")
    ap.add_argument("--limit",   type=int, default=DEFAULT_LIMIT,
                    help=f"Maximum new words to add per run  (default: {DEFAULT_LIMIT})")
    ap.add_argument("--min-len", type=int, default=DEFAULT_MIN,
                    help=f"Minimum Hebrew target length  (default: {DEFAULT_MIN})")
    ap.add_argument("--max-len", type=int, default=DEFAULT_MAX,
                    help=f"Maximum Hebrew target length  (default: {DEFAULT_MAX})")
    ap.add_argument("--dry-run", action="store_true",
                    help="Print what would be added without writing the file")
    args = ap.parse_args()

    existing, src_lang, tgt_lang = load_wordlist(args.wordlist)
    print(f"Wordlist : {os.path.abspath(args.wordlist)}")
    print(f"           {len(existing)} existing word(s)")
    print(f"POS      : {', '.join(args.pos)}")
    print(f"Limit    : {args.limit}  |  length {args.min_len}–{args.max_len}")

    new_words = scrape(
        pos_list=args.pos,
        limit=args.limit,
        min_len=args.min_len,
        max_len=args.max_len,
        existing_words=existing,
    )

    print(f"\n{'[DRY RUN] ' if args.dry_run else ''}Found {len(new_words)} new word(s):")
    for w in new_words:
        print(f"  {w['Source']:30s} → {w['Target']:10s}  ({w['PartOfSpeech']})")

    if args.dry_run or not new_words:
        if not new_words:
            print("Nothing new to add.")
        return

    merged = existing + new_words
    save_wordlist(args.wordlist, merged, src_lang, tgt_lang)
    print(f"\nSaved {len(merged)} word(s) → {os.path.abspath(args.wordlist)}")


if __name__ == "__main__":
    main()
