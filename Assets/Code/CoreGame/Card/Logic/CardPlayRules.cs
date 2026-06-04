using System;
using System.Collections.Generic;
using System.Linq;
using CoreGame.Card.Data;

namespace CoreGame.Card.Logic
{
    public static class CardPlayRules
    {
        /// <summary>
        /// Ищет карту в руке по <see cref="CardBattleState.InstanceId"/> или <see cref="CardConfiguration.Id"/>.
        /// При нескольких картах с одним Config.Id без InstanceId — null (неоднозначно).
        /// </summary>
        public static CardBattleState FindCardInHand(IReadOnlyList<CardBattleState> hand, string cardId)
        {
            if (string.IsNullOrEmpty(cardId) || hand == null)
            {
                return null;
            }

            foreach (CardBattleState card in hand)
            {
                if (card.InstanceId == cardId)
                {
                    return card;
                }
            }

            CardBattleState byConfigId = null;

            foreach (CardBattleState card in hand)
            {
                if (card.Config == null || card.Config.Id != cardId)
                {
                    continue;
                }

                if (byConfigId != null)
                {
                    return null;
                }

                byConfigId = card;
            }

            return byConfigId;
        }

        public static bool CanPlayCard(BattleUnit actor, CardBattleState card)
        {
            if (actor == null || card?.Config == null)
            {
                return false;
            }

            int cost = card.GetEnergyCost(actor.Stats);

            if (actor.Energy < cost)
            {
                return false;
            }

            if (actor.Statuses.Any(s => s.Type == EStatusType.Stun))
            {
                return false;
            }

            if (actor.IsInArmorStance && card.Config.Type.HasFlag(ECardType.Attack))
            {
                return false;
            }

            return true;
        }

        public static bool TryPlay(
            BattleUnit actor,
            string cardId,
            string targetUnitId,
            BattleModel battle,
            BattleProcessor processor,
            Func<string, BattleUnit> findUnit,
            Action<BattleUnit, CardBattleState> spendCard)
        {
            CardBattleState card = FindCardInHand(actor.Hand, cardId);

            if (card == null || !CanPlayCard(actor, card))
            {
                return false;
            }

            BattleUnit target = findUnit(targetUnitId);
            processor.ApplyCard(actor, card, target, battle);
            spendCard(actor, card);

            return true;
        }
    }
}
