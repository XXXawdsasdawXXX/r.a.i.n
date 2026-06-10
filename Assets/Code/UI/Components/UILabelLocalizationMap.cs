using System;
using System.Collections.Generic;
using Core.Localization;
using UnityEngine;

namespace UI.Components
{
    public static class UILabelLocalizationMap
    {
        private static readonly Dictionary<string, (string table, string key)> TextToEntry =
            new Dictionary<string, (string table, string key)>(StringComparer.OrdinalIgnoreCase)
            {
                ["game"] = (LocalizationTables.MainMenu, "ui.main_menu.tab.game"),
                ["hero"] = (LocalizationTables.MainMenu, "ui.main_menu.tab.hero"),
                ["continue"] = (LocalizationTables.MainMenu, "ui.main_menu.button.continue"),
                ["new"] = (LocalizationTables.MainMenu, "ui.main_menu.button.new"),
                ["new game"] = (LocalizationTables.MainMenu, "ui.main_menu.button.new_game"),
                ["create"] = (LocalizationTables.MainMenu, "ui.main_menu.button.create"),
                ["delete"] = (LocalizationTables.MainMenu, "ui.main_menu.button.delete"),
                ["delete hero"] = (LocalizationTables.MainMenu, "ui.main_menu.button.delete_hero"),
                ["settings"] = (LocalizationTables.MainMenu, "ui.main_menu.button.settings"),
                ["join"] = (LocalizationTables.MainMenu, "ui.main_menu.button.join"),
                ["join to game"] = (LocalizationTables.MainMenu, "ui.main_menu.button.join_to_game"),
                ["host"] = (LocalizationTables.MainMenu, "ui.main_menu.button.host"),
                ["server"] = (LocalizationTables.MainMenu, "ui.main_menu.button.server"),
                ["copy"] = (LocalizationTables.MainMenu, "ui.main_menu.button.copy"),
                ["yes"] = (LocalizationTables.MainMenu, "ui.main_menu.button.yes"),
                ["no"] = (LocalizationTables.MainMenu, "ui.main_menu.button.no"),
                ["user ip"] = (LocalizationTables.MainMenu, "ui.main_menu.label.user_ip"),
                ["new hero"] = (LocalizationTables.MainMenu, "ui.main_menu.label.new_hero"),
                ["game name.."] = (LocalizationTables.MainMenu, "ui.main_menu.placeholder.game_name"),
                ["enter text..."] = (LocalizationTables.MainMenu, "ui.main_menu.placeholder.enter_text"),
                ["ip..."] = (LocalizationTables.MainMenu, "ui.main_menu.placeholder.ip"),
                ["do you realy want delete this hero?"] = (LocalizationTables.MainMenu, "ui.main_menu.delete_hero.confirm"),
                ["pause"] = (LocalizationTables.CoreGame, "ui.core_game.pause.title"),
                ["main menu"] = (LocalizationTables.CoreGame, "ui.core_game.pause.main_menu"),
                ["language"] = (LocalizationTables.CoreGame, "ui.core_game.pause.language"),
                ["battle lobby"] = (LocalizationTables.CoreGame, "ui.core_game.battle_lobby.title"),
                ["status"] = (LocalizationTables.CoreGame, "ui.core_game.battle_lobby.status_label"),
                ["hint"] = (LocalizationTables.CoreGame, "ui.core_game.battle_lobby.hint_label"),
                ["start"] = (LocalizationTables.CoreGame, "ui.core_game.battle_lobby.start"),
                ["cancel"] = (LocalizationTables.CoreGame, "ui.core_game.battle_lobby.cancel"),
                ["name"] = (LocalizationTables.CoreGame, "ui.core_game.hud.name"),
                ["qa"] = (LocalizationTables.CoreGame, "ui.core_game.qa.title"),
                ["add hp"] = (LocalizationTables.CoreGame, "ui.core_game.qa.add_hp"),
                ["remove hp"] = (LocalizationTables.CoreGame, "ui.core_game.qa.remove_hp"),
                ["add resource"] = (LocalizationTables.CoreGame, "ui.core_game.qa.add_resource"),
                ["end step"] = (LocalizationTables.Cards, "ui.cards.battle.end_step"),
                ["time"] = (LocalizationTables.Cards, "ui.cards.battle.time"),
                ["step"] = (LocalizationTables.Cards, "ui.cards.battle.step"),
            };

        private static readonly HashSet<string> ExcludedObjectNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "TextUserIP",
            "TextStatus",
            "TextHint",
            "_textCommandMessage",
            "text_command_message",
        };

        public static bool ShouldSkip(GameObject gameObject)
        {
            return gameObject != null && ExcludedObjectNames.Contains(gameObject.name);
        }

        public static bool TryResolve(string sourceText, out string table, out string key)
        {
            table = null;
            key = null;

            string normalized = _normalize(sourceText);
            if (string.IsNullOrEmpty(normalized))
            {
                return false;
            }

            return TextToEntry.TryGetValue(normalized, out var entry)
                && !string.IsNullOrEmpty(entry.table)
                && !string.IsNullOrEmpty(entry.key)
                && _assign(entry, out table, out key);
        }

        private static bool _assign((string table, string key) entry, out string table, out string key)
        {
            table = entry.table;
            key = entry.key;
            return true;
        }

        private static string _normalize(string sourceText)
        {
            if (string.IsNullOrWhiteSpace(sourceText))
            {
                return string.Empty;
            }

            string[] parts = sourceText.Split(new[] { '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            return string.Join(" ", parts).Trim();
        }
    }
}
