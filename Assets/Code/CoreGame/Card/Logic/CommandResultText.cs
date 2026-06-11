using Core.Localization;

namespace CoreGame.Card.Logic
{
    public static class CommandResultText
    {
        private const string Table = LocalizationTables.Cards;

        public static string ToDebugText(CommandResult result)
        {
            LocalizationService localization = LocalizationService.TryGet();
            if (localization == null)
            {
                return _getFallback(result);
            }

            return result switch
            {
                CommandResult.Success => localization.Get(Table, "ui.cards.command.success", "Success"),
                CommandResult.InvalidState => localization.Get(Table, "ui.cards.command.invalid_state", "Command is unavailable in current state"),
                CommandResult.InvalidPhase => localization.Get(Table, "ui.cards.command.invalid_phase", "Command is unavailable in current phase"),
                CommandResult.UnitNotFound => localization.Get(Table, "ui.cards.command.unit_not_found", "Unit not found"),
                CommandResult.NotYourSide => localization.Get(Table, "ui.cards.command.not_your_side", "Target belongs to another side"),
                CommandResult.InvalidCell => localization.Get(Table, "ui.cards.command.invalid_cell", "Cell index is invalid"),
                CommandResult.TargetOccupied => localization.Get(Table, "ui.cards.command.target_occupied", "Target cell is occupied"),
                CommandResult.CardNotFound => localization.Get(Table, "ui.cards.command.card_not_found", "Card not found in hand"),
                CommandResult.CardCannotBePlayed => localization.Get(Table, "ui.cards.command.cannot_play", "Card cannot be played"),
                CommandResult.CardHasNoMoveEffect => localization.Get(Table, "ui.cards.command.no_move_effect", "Card has no move effect"),
                CommandResult.CardApplyRejected => localization.Get(Table, "ui.cards.command.apply_rejected", "Card application rejected by state"),
                CommandResult.TargetInvalid => localization.Get(Table, "ui.cards.command.target_invalid", "Selected target is invalid for this card"),
                CommandResult.MoveLineRejected => localization.Get(Table, "ui.cards.command.move_rejected", "Line switch rejected"),
                CommandResult.MoveApplyFailed => localization.Get(Table, "ui.cards.command.move_failed", "Move apply failed"),
                CommandResult.NotEnoughEnergy => localization.Get(Table, "ui.cards.command.not_enough_energy", "Not enough energy"),
                CommandResult.UnitStunned => localization.Get(Table, "ui.cards.command.unit_stunned", "Unit is stunned"),
                CommandResult.AttackBlockedByArmorStance => localization.Get(Table, "ui.cards.command.armor_blocked", "Attack cards are blocked by armor stance"),
                _ => localization.Format(Table, "ui.cards.command.unknown", result)
            };
        }

        private static string _getFallback(CommandResult result)
        {
            return result switch
            {
                CommandResult.Success => "Success",
                CommandResult.InvalidState => "Command is unavailable in current state",
                CommandResult.InvalidPhase => "Command is unavailable in current phase",
                CommandResult.UnitNotFound => "Unit not found",
                CommandResult.NotYourSide => "Target belongs to another side",
                CommandResult.InvalidCell => "Cell index is invalid",
                CommandResult.TargetOccupied => "Target cell is occupied",
                CommandResult.CardNotFound => "Card not found in hand",
                CommandResult.CardCannotBePlayed => "Card cannot be played",
                CommandResult.CardHasNoMoveEffect => "Card has no move effect",
                CommandResult.CardApplyRejected => "Card application rejected by state",
                CommandResult.TargetInvalid => "Selected target is invalid for this card",
                CommandResult.MoveLineRejected => "Line switch rejected",
                CommandResult.MoveApplyFailed => "Move apply failed",
                CommandResult.NotEnoughEnergy => "Not enough energy",
                CommandResult.UnitStunned => "Unit is stunned",
                CommandResult.AttackBlockedByArmorStance => "Attack cards are blocked by armor stance",
                _ => $"Unknown result: {result}"
            };
        }
    }
}
