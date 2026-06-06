using CoreGame.Card.Data;

namespace CoreGame.Card.Logic
{
    public static class CardSpendPolicy
    {
        public static void Spend(BattleSide side, BattleUnit actor, CardBattleState card)
        {
            if (side == null || actor == null || card == null)
            {
                return;
            }

            if (side.ContainsMandatoryCard(card))
            {
                // Mandatory-карты не должны попадать в deck/discard.
                // На следующем ходу создается новая обязательная копия через EnsureMandatoryCard().
                side.RemoveMandatoryCard(card);
                return;
            }

            actor.Hand.Remove(card);

            if (card.Config != null && card.Config.Charges > 0)
            {
                card.ChargesLeft--;
                if (card.ChargesLeft > 0)
                {
                    actor.Discard.Add(card);
                }

                return;
            }

            actor.Discard.Add(card);
        }
    }
}
