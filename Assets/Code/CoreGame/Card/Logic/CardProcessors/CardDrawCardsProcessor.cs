using GameKit.Dependencies.Utilities;
using CoreGame.Card.Data;

namespace CoreGame.Card.Logic.CardProcessors
{
    public class CardDrawCardsProcessor : ICardProcessor
    {
        public void Process(CardEffectConfiguration effect, BattleUnit actor, BattleUnit target, BattleModel battle)
        {
            if (actor == null)
            {
                return;
            }

            int drawCount = UnityEngine.Mathf.Max(0, (int)BattleProcessor.CalculateEffectValue(effect, actor));
            if (drawCount <= 0 || actor.Hand == null || actor.Deck == null)
            {
                return;
            }

            for (int i = 0; i < drawCount; i++)
            {
                if (actor.Deck.Count == 0)
                {
                    _reshuffleDiscardToDeck(actor);
                }

                if (actor.Deck.Count == 0)
                {
                    break;
                }

                CardBattleState card = actor.Deck[0];
                actor.Deck.RemoveAt(0);
                actor.Hand.Add(card);
            }
        }

        private static void _reshuffleDiscardToDeck(BattleUnit actor)
        {
            if (actor.Discard == null || actor.Discard.Count == 0 || actor.Deck == null)
            {
                return;
            }

            actor.Deck.AddRange(actor.Discard);
            actor.Discard.Clear();
            actor.Deck.Shuffle();
        }
    }
}
