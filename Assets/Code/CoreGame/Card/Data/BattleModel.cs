using System.Collections.Generic;

namespace CoreGame.Card.Data
{
    public class BattleModel
    {
        public string BattleId;
        public EBattleMode Mode;
    
        public BattleSide SideA;
        public BattleSide SideB;
    
        public int TurnNumber;
        public float TurnTimeRemaining;
    }
}