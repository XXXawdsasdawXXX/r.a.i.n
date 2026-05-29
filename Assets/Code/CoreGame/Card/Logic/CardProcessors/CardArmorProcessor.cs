using CoreGame.Card.Data;

namespace CoreGame.Card.Logic.CardProcessors
{
    public class CardArmorProcessor : ICardProcessor
    {
        public void Process(CardEffectConfiguration effect, BattleUnit actor, BattleUnit target, BattleModel battle)
        {
            target.Armor += effect.BaseValue.GetRandomValue();
        }
    }
}