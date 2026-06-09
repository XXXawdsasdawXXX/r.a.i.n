using System;
using CoreGame.Card.Data;

namespace CoreGame.Card.Logic.CardProcessors
{
    public class CardEnergyProcessor : ICardProcessor
    {
        public void Process(CardEffectConfiguration effect, BattleUnit actor, BattleUnit target, BattleModel battle)
        {
            float energyValue = BattleProcessor.CalculateEffectValue(effect, actor);
            actor.Energy = Math.Min(actor.MaxEnergy, actor.Energy + (int)energyValue);
        }
    }
}