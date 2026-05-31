using Core.Data;

namespace CoreGame.Card.Data
{
    public class BattleModel
    {
        public const float MAX_TURN_TIME = 60;
        
        public string BattleId;
        public EBattleMode Mode;
        
        public BattleSide SideA;
        public BattleSide SideB;
    
        public int TurnNumber;
        public float TurnTimeRemaining;
        public ReactiveProperty<EBattlePhase> Phase;
    }
}