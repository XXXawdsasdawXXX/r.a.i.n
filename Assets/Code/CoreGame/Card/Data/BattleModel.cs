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
        public BattleSide EnemySide;

        public bool IsCoOp => Mode == EBattleMode.CoOpPvE;

        public int TurnNumber;
        public ReactiveProperty<float> TurnTimeRemaining;
        public ReactiveProperty<EBattlePhase> Phase;
    }
}