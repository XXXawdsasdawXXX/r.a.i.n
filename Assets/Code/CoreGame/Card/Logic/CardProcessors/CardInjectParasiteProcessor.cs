using CoreGame.Card.Data;

namespace CoreGame.Card.Logic.CardProcessors
{
    public class CardInjectParasiteProcessor : ICardProcessor
    {
        public void Process(CardEffectConfiguration effect, BattleUnit actor, BattleUnit target, BattleModel battle)
        {
            BattleSide targetSide = battle.SideA.Hero.OwnerId == actor.OwnerId 
                ? battle.SideA 
                : battle.SideB;

            for (int i = 0; i < effect.ParasiteCount; i++)
            {
                CardBattleState parasite = new CardBattleState
                {
                    Config = effect.ParasiteCard,
                    ChargesLeft = effect.ParasiteCard.Charges,
                    IsParasite = true
                };
                
                int insertIndex = UnityEngine.Random.Range(
                    0, targetSide.Hero.Deck.Count + 1);
                
                targetSide.Hero.Deck.Insert(insertIndex, parasite);
            }
        }
    }
}