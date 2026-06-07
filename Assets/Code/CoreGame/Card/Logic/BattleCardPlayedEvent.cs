using CoreGame.Card.Data;

namespace CoreGame.Card.Logic
{
    public class BattleCardPlayedEvent
    {
        public string ActorUnitId;
        public string TargetUnitId;
        public CardBattleState Card;
    }
}
