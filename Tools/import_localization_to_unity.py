#!/usr/bin/env python3
"""Fill Unity StringTable assets from project CSV files (en/ru columns)."""

from __future__ import annotations

import csv
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

TABLES = {
    "MainMenu": {
        "csv": ROOT / "Localization" / "main_menu.csv",
        "dir": ROOT / "Assets" / "Localization" / "StringTables" / "MainMenu",
    },
    "CoreGame": {
        "csv": ROOT / "Localization" / "core_game.csv",
        "dir": ROOT / "Assets" / "Localization" / "StringTables" / "CoreGame",
    },
    "Cards": {
        "csv": ROOT / "Localization" / "cards.csv",
        "dir": ROOT / "Assets" / "Localization" / "StringTables" / "Cards",
    },
}


def format_yaml_value(value: str) -> str:
    if value == "":
        return ""
    if any(ch in value for ch in "\n\r\t:#\"'{}[]&*!|>'%@`"):
        escaped = (
            value.replace("\\", "\\\\")
            .replace('"', '\\"')
            .replace("\r\n", "\\n")
            .replace("\n", "\\n")
            .replace("\r", "\\n")
            .replace("\t", "\\t")
        )
        return f'"{escaped}"'
    return value


def parse_shared_keys(shared_path: Path) -> dict[str, str]:
    text = shared_path.read_text(encoding="utf-8")
    key_to_id: dict[str, str] = {}
    for match in re.finditer(r"- m_Id: (\d+)\s+m_Key: (.+)", text):
        entry_id, key = match.groups()
        key_to_id[key.strip()] = entry_id
    return key_to_id


def read_csv_rows(csv_path: Path) -> dict[str, dict[str, str]]:
    rows: dict[str, dict[str, str]] = {}
    with csv_path.open(encoding="utf-8-sig", newline="") as handle:
        reader = csv.DictReader(handle)
        for row in reader:
            key = (row.get("Key") or "").strip()
            if not key:
                continue
            rows[key] = row
    return rows


def patch_locale_asset(asset_path: Path, key_to_id: dict[str, str], csv_rows: dict[str, dict[str, str]], locale: str) -> int:
    text = asset_path.read_text(encoding="utf-8")
    updated = 0

    for key, entry_id in key_to_id.items():
        row = csv_rows.get(key)
        if row is None:
            continue

        value = (row.get(locale) or "").strip()
        pattern = rf"(- m_Id: {entry_id}\s+m_Localized: ).*?(\s+m_Metadata:)"

        def repl(match: re.Match[str]) -> str:
            return f"{match.group(1)}{format_yaml_value(value)}{match.group(2)}"

        new_text, count = re.subn(pattern, repl, text, count=1, flags=re.S)
        if count:
            text = new_text
            updated += 1

    asset_path.write_text(text, encoding="utf-8")
    return updated


def patch_table(table_name: str, config: dict) -> None:
    csv_path: Path = config["csv"]
    table_dir: Path = config["dir"]
    shared_path = table_dir / f"{table_name} Shared Data.asset"

    if not csv_path.exists():
        print(f"[skip] CSV missing: {csv_path}")
        return
    if not shared_path.exists():
        print(f"[skip] Shared data missing: {shared_path}")
        return

    key_to_id = parse_shared_keys(shared_path)
    csv_rows = read_csv_rows(csv_path)

    for locale in ("en", "ru"):
        locale_path = table_dir / f"{table_name}_{locale}.asset"
        if not locale_path.exists():
            print(f"[skip] Locale table missing: {locale_path}")
            continue
        count = patch_locale_asset(locale_path, key_to_id, csv_rows, locale)
        print(f"[ok] {table_name}_{locale}: updated {count} entries")


def main() -> None:
    for table_name, config in TABLES.items():
        patch_table(table_name, config)


if __name__ == "__main__":
    main()
