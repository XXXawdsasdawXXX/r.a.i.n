namespace CoreGame.Card.Data
{
    public enum EBattlePhase
    {
        WaitingBattle,
        StartBattle,
        StartTurn,
        FirstSideTurn,
        /// <summary>Ход второго игрока в кооперативе против AI.</summary>
        AllySideTurn,
        SecondSideTurn,
        Resolution,
        Finished,
    }
}