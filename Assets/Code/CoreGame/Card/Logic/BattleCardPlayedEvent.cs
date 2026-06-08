using CoreGame.Card.Data;
using System.Collections.Generic;

namespace CoreGame.Card.Logic
{
    public class BattleCardPlayedEvent
    {
        public string ActorUnitId;
        public string TargetUnitId;
        public CardBattleState Card;
        public List<EEffectType> EffectTypes;
    }
}
