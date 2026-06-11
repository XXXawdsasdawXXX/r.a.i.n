namespace CoreGame.Card.Logic
{
    public enum CommandResult
    {
        Success = 0,
        
        InvalidState = 1,
        InvalidPhase = 2,

        UnitNotFound = 3,
        NotYourSide = 4,
        InvalidCell = 5,
        TargetOccupied = 6,
        
        CardNotFound = 7,
        CardCannotBePlayed = 8,
        CardHasNoMoveEffect = 9,
        CardApplyRejected = 10,
        TargetInvalid = 11,
        
        MoveLineRejected = 12,
        MoveApplyFailed = 13,
        
        NotEnoughEnergy = 14,
        UnitStunned = 15,
        AttackBlockedByArmorStance = 16,
    }
}
