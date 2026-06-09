using System;
using CoreGame.Card.Data;

namespace CoreGame.Card.Logic.CardProcessors
{
    public class CardHealEffectProcessor : ICardProcessor
    {
        public void Process(CardEffectConfiguration effect, BattleUnit actor, BattleUnit target, BattleModel battle)
        {
            float healValue = BattleProcessor.CalculateEffectValue(effect, actor);
            target.HP = Math.Min(target.MaxHP, target.HP + healValue);
        }
    }
}