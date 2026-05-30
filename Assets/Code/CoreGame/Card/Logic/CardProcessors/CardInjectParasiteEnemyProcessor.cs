using CoreGame.Card.Data;

namespace CoreGame.Card.Logic.CardProcessors
{
    public class CardInjectParasiteEnemyProcessor : ICardProcessor
    {
        public void Process(CardEffectConfiguration effect, BattleUnit actor, BattleUnit target, BattleModel battle)
        {
            BattleSide targetSide = battle.SideA.Hero.OwnerId == actor.OwnerId
                ? battle.SideB
                : battle.SideA;
            
            for (int i = 0; i < effect.ParasiteCount; i++)
            {
                CardBattleState parasite = new CardBattleState
                {
                    Config = effect.ParasiteCard,
                    ChargesLeft = effect.ParasiteCard.Charges,
                    IsParasite = true
                };

                // вставляем в случайное место колоды
                int insertIndex = UnityEngine.Random.Range(
                    0, targetSide.Hero.Deck.Count + 1);
                
                targetSide.Hero.Deck.Insert(insertIndex, parasite);
            }
        }
    }
}