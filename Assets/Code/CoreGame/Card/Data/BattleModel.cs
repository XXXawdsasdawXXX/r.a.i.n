using System.Collections.Generic;

namespace CoreGame.Card
{
    public class BattleModel
    {
        public string BattleId;
        public EBattleMode Mode;
        public EBattlePhase Phase;
    
        public BattleSide ActiveSide;
        public BattleSide WaitingSide;
    
        public int TurnNumber;
        public float TurnTimeRemaining;
    
        // для дуэли
        public Dictionary<string, int> Stakes = new();
    }
}