using Core.Save;
using CoreGame.Card.Data;

namespace CoreGame.Card.Logic.CardProcessors
{
    public class CardSummonCompanionProcessor : ICardProcessor
    {
        private readonly AllCardCollection _allCards;

        public CardSummonCompanionProcessor(AllCardCollection allCards)
        {
            _allCards = allCards;
        }

        public void Process(CardEffectConfiguration effect, BattleUnit actor, BattleUnit target, BattleModel battle)
        {
            BattleUnit companion = BattleUnit.FromCompanion(
                effect.CompanionConfiguration, 
                actor.OwnerId, 
                _allCards);

            BattleSide ownerSide = battle.SideA.Hero.OwnerId == actor.OwnerId
                ? battle.SideA
                : battle.SideB;

            // спутник встаёт во frontline по умолчанию
            companion.Line = EBattleLine.Frontline;

            if (effect.SummonDuration > 0)
            {
                companion.Statuses.Add(new StatusEffect
                {
                    Type = EStatusType.SummonDuration,
                    Duration = effect.SummonDuration
                });
            }

            ownerSide.Companions.Add(companion);
        }
    }
}