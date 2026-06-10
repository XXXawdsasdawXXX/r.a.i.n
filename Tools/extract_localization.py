#!/usr/bin/env python3
"""Extract localization strings from r.a.i.n project for Google Sheets / Unity Localization."""

import csv
import re
import codecs
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "Localization"
OUT.mkdir(exist_ok=True)

SKIP_TEXT = {
    "", "\u200b", "100\u200b", "100", "00", "000", "0000", "00:00:00", "1/1", "22",
    "text", "heroname", "Option A", "ip...", "\u200B",
}


def decode_value(raw: str) -> str:
    raw = raw.strip()
    if raw.startswith('"') and raw.endswith('"'):
        inner = raw[1:-1]
        try:
            return codecs.decode(inner, "unicode_escape")
        except Exception:
            return inner
    return raw


def extract_quoted_multiline(text: str, field: str) -> str:
    pattern = rf"<{field}>k__BackingField:\s*(.+?)(?=\n\s*<|\Z)"
    m = re.search(pattern, text, re.S)
    if not m:
        return ""
    block = m.group(1)
    parts = re.findall(r'"((?:\\.|[^"\\])*)"', block)
    if parts:
        joined = "".join(parts)
        try:
            return codecs.decode(joined, "unicode_escape")
        except Exception:
            return joined
    return block.replace("\n", " ").strip()


def extract_cards():
    rows = []
    for p in sorted((ROOT / "Assets/Data/Cards").rglob("*.asset")):
        if p.name == "Library_Cards.asset":
            continue
        text = p.read_text(encoding="utf-8", errors="replace")
        card_id = extract_quoted_multiline(text, "Id") or p.stem.replace("Card_", "")
        name = extract_quoted_multiline(text, "Name")
        desc = extract_quoted_multiline(text, "Description")
        if not name and not desc:
            continue
        rows.append({
            "Key": f"card.{card_id}.name" if name else f"card.{card_id}.description",
            "Id": card_id,
            "Field": "name" if name else "description",
            "Source": str(p.relative_to(ROOT)).replace("\\", "/"),
            "en": name if name else "",
            "ru": name if name else "",
            "Notes": "Card config",
        })
        if desc:
            rows.append({
                "Key": f"card.{card_id}.description",
                "Id": card_id,
                "Field": "description",
                "Source": str(p.relative_to(ROOT)).replace("\\", "/"),
                "en": desc,
                "ru": desc,
                "Notes": "Card config",
            })
        elif name:
            # fix first row field
            rows[-1]["Key"] = f"card.{card_id}.name"
            rows[-1]["Field"] = "name"
    return rows


def extract_prefab_texts(paths):
    rows = []
    seen = set()
    for prefab_path in paths:
        text = prefab_path.read_text(encoding="utf-8", errors="replace")
        # inline m_text
        for m in re.finditer(r"m_text:\s*(.+)", text):
            val = decode_value(m.group(1).strip())
            if val.lower() in SKIP_TEXT or val in SKIP_TEXT:
                continue
            key = f"ui.{prefab_path.stem}.{slug(val)}"
            if key in seen:
                continue
            seen.add(key)
            rows.append({
                "Key": key,
                "Source": str(prefab_path.relative_to(ROOT)).replace("\\", "/"),
                "en": val,
                "ru": "",
                "Notes": "Prefab TMP text",
            })
        # override values
        for m in re.finditer(
            r"propertyPath: m_text\n\s+value:\s*(.+?)\n\s+objectReference:",
            text,
            re.S,
        ):
            val = decode_value(m.group(1).strip())
            if not val or val.lower() in SKIP_TEXT or val in SKIP_TEXT:
                continue
            key = f"ui.{prefab_path.stem}.{slug(val)}"
            if key in seen:
                continue
            seen.add(key)
            rows.append({
                "Key": key,
                "Source": str(prefab_path.relative_to(ROOT)).replace("\\", "/"),
                "en": val,
                "ru": "",
                "Notes": "Prefab override",
            })
    return rows


def slug(s: str) -> str:
    s = s.lower().replace("\n", "_").replace(" ", "_")
    s = re.sub(r"[^a-z0-9_а-яё]", "", s)
    return s[:48] or "text"


MAIN_MENU_PREFABS = [
    ROOT / "Assets/Prefabs/UI/MainMenu/canvas_screen-main-menu.prefab",
    ROOT / "Assets/Prefabs/UI/MainMenu/window-connection.prefab",
    ROOT / "Assets/Prefabs/UI/MainMenu/infoCard-hero.prefab",
    ROOT / "Assets/Prefabs/UI/Components/inputField-.prefab",
    ROOT / "Assets/Prefabs/UI/Components/tabHeader-.prefab",
]

CORE_GAME_PREFABS = [
    ROOT / "Assets/Prefabs/UI/CoreGame/canvas_screen-core-game.prefab",
    ROOT / "Assets/Prefabs/UI/CoreGame/card/window-card-battle.prefab",
    ROOT / "Assets/Prefabs/UI/CoreGame/card/battleUnit.prefab",
    ROOT / "Assets/Prefabs/UI/CoreGame/IntoGame/text_small_game.prefab",
    ROOT / "Assets/Prefabs/UI/CoreGame/HUD/window-game-HUD.prefab",
    ROOT / "Assets/Prefabs/UI/Components/contentBox_resource.prefab",
]

CARDS_UI_PREFABS = [
    ROOT / "Assets/Prefabs/UI/CoreGame/card/card.prefab",
]

CODE_STRINGS = {
    "main_menu": [
        ("ui.main_menu.ip_label", "Assets/Code/UI/Windows/MainMenu/Game/GameWindowController.cs", "IP: {0}", "IP: {0}"),
        ("ui.main_menu.connection.your_ip", "Assets/Code/UI/Windows/MainMenu/Connection/ConnectionWindowController.cs", "your ip: {0}", "ваш ip: {0}"),
        ("ui.main_menu.delete.confirm", "Assets/Code/UI/Windows/MainMenu/Delete/DeleteWindowController.cs", "Delete '{0}'?", "Удалить «{0}»?"),
    ],
    "core_game": [
        ("ui.core_game.battle_lobby.status", "Assets/Code/CoreGame/Card/Logic/Network/BattleLobbyState.cs", "Waiting for players: {0}/{1}", "Ожидание игроков: {0}/{1}"),
        ("ui.core_game.battle_lobby.ready", "Assets/Code/CoreGame/Card/Logic/Network/BattleLobbyState.cs", "Team is ready — you can start the battle.", "Команда собрана — можно начинать бой."),
        ("ui.core_game.battle_lobby.wait_partner", "Assets/Code/CoreGame/Card/Logic/Network/BattleLobbyState.cs", "Wait for a partner or connect a second player to this activator.", "Дождитесь напарника или подключите второго игрока к этому активатору."),
        ("ui.core_game.battle_lobby.solo_pve", "Assets/Code/CoreGame/Card/Logic/Network/BattleLobbyState.cs", "You can start solo — the battle will run as PvE.", "Можно начать в одиночку — бой пройдёт как PvE."),
        ("ui.core_game.battle_lobby.not_enough_pvp", "Assets/Code/CoreGame/Card/Logic/Network/BattleLobbyState.cs", "Not enough players for PvP.", "Недостаточно игроков для PvP."),
        ("ui.core_game.battle_lobby.start_current", "Assets/Code/CoreGame/Card/Logic/Network/BattleLobbyState.cs", "You can start with the current lineup.", "Можно начать с текущим составом."),
        ("ui.core_game.companion.temporary", "Assets/Code/UI/Windows/Game/Card/Unit/BattleUnitView.cs", "Temporary: {0} turn(s)", "Временный: {0} ход(ов)"),
        ("ui.core_game.companion.lifetime", "Assets/Code/UI/Windows/Game/Card/Unit/BattleUnitView.cs", "Lifetime: until death", "Время жизни: до смерти"),
        ("ui.core_game.companion.cards_per_turn", "Assets/Code/UI/Windows/Game/Card/Unit/BattleUnitView.cs", "Cards/turn: {0}", "Карт/ход: {0}"),
    ],
    "cards": [
        ("ui.cards.hover.temporary_unit", "Assets/Code/UI/Windows/Game/Card/Hover/CardUnitHoverView.cs", "Temporary unit", "Временный юнит"),
        ("ui.cards.hover.auto_action", "Assets/Code/UI/Windows/Game/Card/Hover/CardUnitHoverView.cs", "Auto-action: ", "Авто-действие: "),
        ("ui.cards.hover.effects", "Assets/Code/UI/Windows/Game/Card/Hover/CardUnitHoverView.cs", "Effects:", "Эффекты:"),
        ("ui.cards.hover.turns_left", "Assets/Code/UI/Windows/Game/Card/Hover/CardUnitHoverView.cs", "Turns left: ", "Осталось ходов: "),
        ("ui.cards.hover.no_effects", "Assets/Code/UI/Windows/Game/Card/Hover/CardUnitHoverView.cs", "No active effects", "Нет активных эффектов"),
        ("ui.cards.hover.attack_enemy_hero", "Assets/Code/UI/Windows/Game/Card/Hover/CardUnitHoverView.cs", "Attack enemy hero", "Атака вражеского героя"),
        ("ui.cards.hover.shield_owner", "Assets/Code/UI/Windows/Game/Card/Hover/CardUnitHoverView.cs", "Shield to allied hero", "Щит союзному герою"),
        ("ui.cards.hover.none", "Assets/Code/UI/Windows/Game/Card/Hover/CardUnitHoverView.cs", "None", "Нет"),
        ("ui.cards.hover.turn_short", "Assets/Code/UI/Windows/Game/Card/Hover/CardUnitHoverView.cs", " turn.", " ход."),
        ("ui.cards.status.bleed", "Assets/Code/UI/Windows/Game/Card/Hover/CardUnitHoverView.cs", "Bleeding", "Кровотечение"),
        ("ui.cards.status.poison", "Assets/Code/UI/Windows/Game/Card/Hover/CardUnitHoverView.cs", "Poison", "Яд"),
        ("ui.cards.status.burn", "Assets/Code/UI/Windows/Game/Card/Hover/CardUnitHoverView.cs", "Burn", "Горение"),
        ("ui.cards.status.electro", "Assets/Code/UI/Windows/Game/Card/Hover/CardUnitHoverView.cs", "Electro", "Электро"),
        ("ui.cards.status.stun", "Assets/Code/UI/Windows/Game/Card/Hover/CardUnitHoverView.cs", "Stun", "Оглушение"),
        ("ui.cards.status.weak", "Assets/Code/UI/Windows/Game/Card/Hover/CardUnitHoverView.cs", "Weakness", "Слабость"),
        ("ui.cards.status.regen", "Assets/Code/UI/Windows/Game/Card/Hover/CardUnitHoverView.cs", "Regeneration", "Регенерация"),
        ("ui.cards.status.cost_reduction", "Assets/Code/UI/Windows/Game/Card/Hover/CardUnitHoverView.cs", "Cost reduction", "Снижение стоимости"),
        ("ui.cards.status.crit_boost", "Assets/Code/UI/Windows/Game/Card/Hover/CardUnitHoverView.cs", "Crit boost", "Крит-усиление"),
        ("ui.cards.status.armor_stance", "Assets/Code/UI/Windows/Game/Card/Hover/CardUnitHoverView.cs", "Armor stance", "Стойка брони"),
        ("ui.cards.type.attack", "Assets/Code/CoreGame/Card/Data/ECardType.cs", "Attack", "Атака"),
        ("ui.cards.type.spell", "Assets/Code/CoreGame/Card/Data/ECardType.cs", "Spell", "Заклинание"),
        ("ui.cards.type.armor", "Assets/Code/CoreGame/Card/Data/ECardType.cs", "Armor", "Броня"),
        ("ui.cards.type.summon", "Assets/Code/CoreGame/Card/Data/ECardType.cs", "Summon", "Призыв"),
        ("ui.cards.type.buff", "Assets/Code/CoreGame/Card/Data/ECardType.cs", "Buff", "Бафф"),
        ("ui.cards.type.debuff", "Assets/Code/CoreGame/Card/Data/ECardType.cs", "Debuff", "Дебафф"),
        ("ui.cards.type.parasite", "Assets/Code/CoreGame/Card/Data/ECardType.cs", "Parasite", "Паразит"),
        ("ui.cards.command.success", "Assets/Code/CoreGame/Card/Logic/CommandResultText.cs", "Success", "Успех"),
        ("ui.cards.command.invalid_state", "Assets/Code/CoreGame/Card/Logic/CommandResultText.cs", "Command is unavailable in current state", "Команда недоступна в текущем состоянии"),
        ("ui.cards.command.invalid_phase", "Assets/Code/CoreGame/Card/Logic/CommandResultText.cs", "Command is unavailable in current phase", "Команда недоступна в текущей фазе"),
        ("ui.cards.command.unit_not_found", "Assets/Code/CoreGame/Card/Logic/CommandResultText.cs", "Unit not found", "Юнит не найден"),
        ("ui.cards.command.not_your_side", "Assets/Code/CoreGame/Card/Logic/CommandResultText.cs", "Target belongs to another side", "Цель принадлежит другой стороне"),
        ("ui.cards.command.invalid_cell", "Assets/Code/CoreGame/Card/Logic/CommandResultText.cs", "Cell index is invalid", "Некорректный индекс клетки"),
        ("ui.cards.command.target_occupied", "Assets/Code/CoreGame/Card/Logic/CommandResultText.cs", "Target cell is occupied", "Целевая клетка занята"),
        ("ui.cards.command.card_not_found", "Assets/Code/CoreGame/Card/Logic/CommandResultText.cs", "Card not found in hand", "Карта не найдена в руке"),
        ("ui.cards.command.cannot_play", "Assets/Code/CoreGame/Card/Logic/CommandResultText.cs", "Card cannot be played", "Карту нельзя разыграть"),
        ("ui.cards.command.no_move_effect", "Assets/Code/CoreGame/Card/Logic/CommandResultText.cs", "Card has no move effect", "У карты нет эффекта перемещения"),
        ("ui.cards.command.apply_rejected", "Assets/Code/CoreGame/Card/Logic/CommandResultText.cs", "Card application rejected by state", "Применение карты отклонено состоянием"),
        ("ui.cards.command.target_invalid", "Assets/Code/CoreGame/Card/Logic/CommandResultText.cs", "Selected target is invalid for this card", "Выбранная цель недопустима для этой карты"),
        ("ui.cards.command.move_rejected", "Assets/Code/CoreGame/Card/Logic/CommandResultText.cs", "Line switch rejected", "Смена линии отклонена"),
        ("ui.cards.command.move_failed", "Assets/Code/CoreGame/Card/Logic/CommandResultText.cs", "Move apply failed", "Не удалось применить перемещение"),
        ("ui.cards.command.not_enough_energy", "Assets/Code/CoreGame/Card/Logic/CommandResultText.cs", "Not enough energy", "Недостаточно энергии"),
        ("ui.cards.command.unit_stunned", "Assets/Code/CoreGame/Card/Logic/CommandResultText.cs", "Unit is stunned", "Юнит оглушён"),
        ("ui.cards.command.armor_blocked", "Assets/Code/CoreGame/Card/Logic/CommandResultText.cs", "Attack cards are blocked by armor stance", "Атакующие карты заблокированы стойкой брони"),
        ("ui.cards.command.unknown", "Assets/Code/CoreGame/Card/Logic/CommandResultText.cs", "Unknown result: {0}", "Неизвестный результат: {0}"),
    ],
}


def code_rows(sheet_key):
    return [
        {
            "Key": key,
            "Source": source,
            "en": en,
            "ru": ru,
            "Notes": "Code string",
        }
        for key, source, en, ru in CODE_STRINGS[sheet_key]
    ]


def write_sheet(name, rows, extra_fields=None):
    path = OUT / f"{name}.csv"
    fields = ["Key", "Source", "en", "ru", "Notes"]
    if extra_fields:
        fields = ["Key"] + extra_fields + ["Source", "en", "ru", "Notes"]
    with path.open("w", encoding="utf-8-sig", newline="") as f:
        w = csv.DictWriter(f, fieldnames=fields, extrasaction="ignore")
        w.writeheader()
        for row in rows:
            w.writerow(row)
    print(f"Wrote {path} ({len(rows)} rows)")


def main():
    main_menu = extract_prefab_texts(MAIN_MENU_PREFABS) + code_rows("main_menu")
    core_game = extract_prefab_texts(CORE_GAME_PREFABS) + code_rows("core_game")
    cards = extract_cards() + extract_prefab_texts(CARDS_UI_PREFABS) + code_rows("cards")

    # dedupe by Key within each sheet
    for name, rows in [("main_menu", main_menu), ("core_game", core_game), ("cards", cards)]:
        seen = {}
        deduped = []
        for r in rows:
            k = r["Key"]
            if k not in seen:
                seen[k] = r
                deduped.append(r)
        write_sheet(name, deduped, extra_fields=["Id", "Field"] if name == "cards" else None)

    # Unity Localization combined export format
    combined = OUT / "unity_string_tables.csv"
    with combined.open("w", encoding="utf-8-sig", newline="") as f:
        w = csv.writer(f)
        w.writerow(["Table", "Key", "en", "ru"])
        for table, rows in [("MainMenu", main_menu), ("CoreGame", core_game), ("Cards", cards)]:
            seen = set()
            for r in rows:
                if r["Key"] in seen:
                    continue
                seen.add(r["Key"])
                w.writerow([table, r["Key"], r.get("en", ""), r.get("ru", "")])
    print(f"Wrote {combined}")


if __name__ == "__main__":
    main()
