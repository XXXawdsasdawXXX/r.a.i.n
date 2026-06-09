using System;
using Core.Data;

namespace CoreGame.Card.Data
{
    [Serializable]
    public class BattleModel
    {
        public const float MAX_TURN_TIME = 60;
        
        public string BattleId;
        public EBattleMode Mode;
        
        public BattleSide SideA;
        public BattleSide SideB;
        /// <summary>Второй игрок в режиме <see cref="EBattleMode.CoOpPvE"/>.</summary>
        public BattleSide AllySide;
    
        public int TurnNumber;
        public ReactiveProperty<float> TurnTimeRemaining;
        public ReactiveProperty<EBattlePhase> Phase;

        public bool IsMultiplayer => Mode is EBattleMode.PvP or EBattleMode.CoOpPvE or EBattleMode.Duel;

        public bool HasAllySide => Mode == EBattleMode.CoOpPvE && AllySide != null;
    }
}