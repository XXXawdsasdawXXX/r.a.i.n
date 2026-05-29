using System;
using CoreGame.Card.Data;

namespace CoreGame.Card.Logic.CardProcessors
{
    public class CardDamageProcessor : ICardProcessor
    {
        public void Process(CardEffectConfiguration effect, BattleUnit actor, BattleUnit target, BattleModel battle)
        {
            // крит
            float value = effect.BaseValue.GetRandomValue();
            bool isCrit = UnityEngine.Random.value < actor.CritChance;

            if (isCrit)
            {
                value *= 1.5f;
            }

            // уклонение
            bool isDodge = UnityEngine.Random.value < target.DodgeChance;
            if (isDodge) return;

            // броня поглощает урон
            float absorbed = Math.Min(target.Armor, value);
            target.Armor -= absorbed;
            target.HP -= (value - absorbed);
        }
    }
}