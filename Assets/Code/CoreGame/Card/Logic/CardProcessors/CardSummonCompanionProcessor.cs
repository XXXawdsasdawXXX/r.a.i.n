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
            if (effect?.CompanionConfiguration == null || actor == null || battle == null)
            {
                return;
            }

            BattleUnit companion = BattleUnit.FromCompanion(
                effect.CompanionConfiguration, 
                actor.UnitId, 
                _allCards);

            BattleSide ownerSide = BattleGridRules.GetOwnerSide(battle, actor);
            if (ownerSide == null)
            {
                return;
            }

            // спутник встаёт во frontline по умолчанию
            companion.Line = EBattleLine.Frontline;

            int summonDuration = effect.SummonDuration > 0
                ? effect.SummonDuration
                : effect.CompanionConfiguration.LifetimeTurns;
            if (summonDuration > 0)
            {
                companion.Statuses.Add(new StatusEffect
                {
                    Type = EStatusType.SummonDuration,
                    Duration = summonDuration
                });
            }

            ownerSide.Companions.Add(companion);
        }
    }
}