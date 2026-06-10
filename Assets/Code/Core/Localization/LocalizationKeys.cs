namespace Core.Localization
{
    public static class LocalizationKeys
    {
        public static class MainMenu
        {
            public const string IpLabel = "ui.main_menu.ip_label";
            public const string ConnectionYourIp = "ui.main_menu.connection.your_ip";
            public const string DeleteConfirm = "ui.main_menu.delete.confirm";
            public const string PlaceholderIp = "ui.main_menu.placeholder.ip";
        }

        public static class Common
        {
            public const string LocaleEnglish = "ui.common.locale.en";
            public const string LocaleRussian = "ui.common.locale.ru";
        }

        public static class CoreGame
        {
            public const string BattleLobbyStatus = "ui.core_game.battle_lobby.status";
            public const string BattleLobbyReady = "ui.core_game.battle_lobby.ready";
            public const string BattleLobbyWaitPartner = "ui.core_game.battle_lobby.wait_partner";
            public const string BattleLobbySoloPve = "ui.core_game.battle_lobby.solo_pve";
            public const string BattleLobbyNotEnoughPvp = "ui.core_game.battle_lobby.not_enough_pvp";
            public const string BattleLobbyStartCurrent = "ui.core_game.battle_lobby.start_current";
            public const string BattleLobbyCancel = "ui.core_game.battle_lobby.cancel";
            public const string CompanionTemporary = "ui.core_game.companion.temporary";
            public const string CompanionLifetime = "ui.core_game.companion.lifetime";
            public const string CompanionCardsPerTurn = "ui.core_game.companion.cards_per_turn";
            public const string PauseLanguage = "ui.core_game.pause.language";
        }

        public static class Cards
        {
            public const string BattleTime = "ui.cards.battle.time";
            public const string BattleStep = "ui.cards.battle.step";
            public const string BattleEndStep = "ui.cards.battle.end_step";
            public const string HoverTemporaryUnit = "ui.cards.hover.temporary_unit";
            public const string HoverAutoAction = "ui.cards.hover.auto_action";
            public const string HoverEffects = "ui.cards.hover.effects";
            public const string HoverTurnsLeft = "ui.cards.hover.turns_left";
            public const string HoverNoEffects = "ui.cards.hover.no_effects";
            public const string HoverAttackEnemyHero = "ui.cards.hover.attack_enemy_hero";
            public const string HoverShieldOwner = "ui.cards.hover.shield_owner";
            public const string HoverNone = "ui.cards.hover.none";
            public const string HoverTurnShort = "ui.cards.hover.turn_short";

            public const string CommandSuccess = "ui.cards.command.success";
            public const string CommandInvalidState = "ui.cards.command.invalid_state";
            public const string CommandInvalidPhase = "ui.cards.command.invalid_phase";
            public const string CommandUnitNotFound = "ui.cards.command.unit_not_found";
            public const string CommandNotYourSide = "ui.cards.command.not_your_side";
            public const string CommandInvalidCell = "ui.cards.command.invalid_cell";
            public const string CommandTargetOccupied = "ui.cards.command.target_occupied";
            public const string CommandCardNotFound = "ui.cards.command.card_not_found";
            public const string CommandCannotPlay = "ui.cards.command.cannot_play";
            public const string CommandNoMoveEffect = "ui.cards.command.no_move_effect";
            public const string CommandApplyRejected = "ui.cards.command.apply_rejected";
            public const string CommandTargetInvalid = "ui.cards.command.target_invalid";
            public const string CommandMoveRejected = "ui.cards.command.move_rejected";
            public const string CommandMoveFailed = "ui.cards.command.move_failed";
            public const string CommandNotEnoughEnergy = "ui.cards.command.not_enough_energy";
            public const string CommandUnitStunned = "ui.cards.command.unit_stunned";
            public const string CommandArmorBlocked = "ui.cards.command.armor_blocked";
            public const string CommandUnknown = "ui.cards.command.unknown";

            public static string CardName(string cardId) => $"card.{cardId}.name";
            public static string CardDescription(string cardId) => $"card.{cardId}.description";
            public static string CompanionName(string companionId) => $"companion.{companionId}.name";
            public static string CardType(string typeName) => $"ui.cards.type.{typeName.ToLowerInvariant()}";
            public static string Status(string statusName) => $"ui.cards.status.{statusName}";
        }
    }
}
