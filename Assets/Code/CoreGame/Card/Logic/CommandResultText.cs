namespace CoreGame.Card.Logic
{
    public static class CommandResultText
    {
        public static string ToDebugText(CommandResult result)
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
                _ => $"Unknown result: {result}"
            };
        }
    }
}
