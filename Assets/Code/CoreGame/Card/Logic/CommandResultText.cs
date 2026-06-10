using Core.Localization;

namespace CoreGame.Card.Logic
{
    public static class CommandResultText
    {
        public static string ToDebugText(CommandResult result)
        {
            LocalizationService localization = LocalizationService.TryGet();
            if (localization == null)
            {
                return _getFallback(result);
            }

            return result switch
            {
                CommandResult.Success => localization.GetCommandResultText(
                    LocalizationKeys.Cards.CommandSuccess,
                    "Success"),
                CommandResult.InvalidState => localization.GetCommandResultText(
                    LocalizationKeys.Cards.CommandInvalidState,
                    "Command is unavailable in current state"),
                CommandResult.InvalidPhase => localization.GetCommandResultText(
                    LocalizationKeys.Cards.CommandInvalidPhase,
                    "Command is unavailable in current phase"),
                CommandResult.UnitNotFound => localization.GetCommandResultText(
                    LocalizationKeys.Cards.CommandUnitNotFound,
                    "Unit not found"),
                CommandResult.NotYourSide => localization.GetCommandResultText(
                    LocalizationKeys.Cards.CommandNotYourSide,
                    "Target belongs to another side"),
                CommandResult.InvalidCell => localization.GetCommandResultText(
                    LocalizationKeys.Cards.CommandInvalidCell,
                    "Cell index is invalid"),
                CommandResult.TargetOccupied => localization.GetCommandResultText(
                    LocalizationKeys.Cards.CommandTargetOccupied,
                    "Target cell is occupied"),
                CommandResult.CardNotFound => localization.GetCommandResultText(
                    LocalizationKeys.Cards.CommandCardNotFound,
                    "Card not found in hand"),
                CommandResult.CardCannotBePlayed => localization.GetCommandResultText(
                    LocalizationKeys.Cards.CommandCannotPlay,
                    "Card cannot be played"),
                CommandResult.CardHasNoMoveEffect => localization.GetCommandResultText(
                    LocalizationKeys.Cards.CommandNoMoveEffect,
                    "Card has no move effect"),
                CommandResult.CardApplyRejected => localization.GetCommandResultText(
                    LocalizationKeys.Cards.CommandApplyRejected,
                    "Card application rejected by state"),
                CommandResult.TargetInvalid => localization.GetCommandResultText(
                    LocalizationKeys.Cards.CommandTargetInvalid,
                    "Selected target is invalid for this card"),
                CommandResult.MoveLineRejected => localization.GetCommandResultText(
                    LocalizationKeys.Cards.CommandMoveRejected,
                    "Line switch rejected"),
                CommandResult.MoveApplyFailed => localization.GetCommandResultText(
                    LocalizationKeys.Cards.CommandMoveFailed,
                    "Move apply failed"),
                CommandResult.NotEnoughEnergy => localization.GetCommandResultText(
                    LocalizationKeys.Cards.CommandNotEnoughEnergy,
                    "Not enough energy"),
                CommandResult.UnitStunned => localization.GetCommandResultText(
                    LocalizationKeys.Cards.CommandUnitStunned,
                    "Unit is stunned"),
                CommandResult.AttackBlockedByArmorStance => localization.GetCommandResultText(
                    LocalizationKeys.Cards.CommandArmorBlocked,
                    "Attack cards are blocked by armor stance"),
                _ => localization.GetCommandResultText(
                    LocalizationKeys.Cards.CommandUnknown,
                    "Unknown result: {0}",
                    result)
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
