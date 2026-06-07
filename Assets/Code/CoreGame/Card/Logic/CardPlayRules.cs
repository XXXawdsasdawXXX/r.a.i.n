using System;
using System.Collections.Generic;
using System.Linq;
using CoreGame.Card.Data;

namespace CoreGame.Card.Logic
{
    public static class CardPlayRules
    {
        public static bool TryPlay(
            BattleSide side,
            BattleUnit actor,
            string cardId,
            string targetUnitId,
            BattleModel battle,
            BattleProcessor processor,
            Func<string, BattleUnit> findUnit,
            Action<BattleSide, BattleUnit, CardBattleState> spendCard)
        {
            CardBattleState card = FindCardInHand(side.GetHand(), cardId);

            if (card == null || !CanPlayCard(actor, card))
            {
                return false;
            }

            BattleUnit target = findUnit(targetUnitId);
            processor.ApplyCard(actor, card, target, battle);
            spendCard(side, actor, card);

            return true;
        }

        public static bool CanPlayCard(BattleUnit actor, CardBattleState card)
        {
            return GetPlayRejectionReason(actor, card) == CommandResult.Success;
        }

        public static CommandResult GetPlayRejectionReason(BattleUnit actor, CardBattleState card)
        {
            if (actor == null || card?.Config == null)
            {
                return CommandResult.CardCannotBePlayed;
            }

            int cost = card.GetEnergyCost(actor.Stats);

            if (actor.Energy < cost)
            {
                return CommandResult.NotEnoughEnergy;
            }

            if (actor.Statuses.Any(s => s.Type == EStatusType.Stun))
            {
                return CommandResult.UnitStunned;
            }

            if (actor.IsInArmorStance && card.Config.Type.HasFlag(ECardType.Attack))
            {
                return CommandResult.AttackBlockedByArmorStance;
            }

            return CommandResult.Success;
        }

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
    }
}
