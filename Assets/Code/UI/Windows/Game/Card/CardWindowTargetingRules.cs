using System;
using System.Linq;
using CoreGame.Card.Data;
using CoreGame.Card.Logic;

namespace UI.Windows.Game.Card
{
    public class CardWindowTargetingRules
    {
        private readonly Func<BattleUnit, BattleSide> _resolveOwnerSide;
        private readonly Func<string, BattleSide> _findSideByHeroUnitId;

        public CardWindowTargetingRules(
            Func<BattleUnit, BattleSide> resolveOwnerSide,
            Func<string, BattleSide> findSideByHeroUnitId)
        {
            _resolveOwnerSide = resolveOwnerSide;
            _findSideByHeroUnitId = findSideByHeroUnitId;
        }

        public bool RequiresManualTargetSelection(CardBattleState card)
        {
            return card?.Config?.Effects != null && card.Config.Effects.Any(_requiresManualTarget);
        }

        public bool IsValidTargetForPendingCard(
            BattleModel battleModel,
            CardBattleState pendingTargetCard,
            string actorId,
            string actorSideHeroId,
            BattleUnit target)
        {
            if (pendingTargetCard?.Config?.Effects == null || target == null || battleModel == null)
            {
                return false;
            }

            BattleSide actorSide = _findSideByHeroUnitId(actorSideHeroId);
            BattleSide targetSide = _resolveOwnerSide(target);
            if (actorSide == null || targetSide == null)
            {
                return false;
            }

            bool isEnemy = !ReferenceEquals(actorSide, targetSide);
            bool isSelf = target.UnitId == actorId;
            bool isAlly = ReferenceEquals(actorSide, targetSide);
            bool isCompanion = target.IsCompanion;

            bool hasManualTargetEffect = false;
            bool hasEnemyManualTarget = false;
            foreach (CardEffectConfiguration effect in pendingTargetCard.Config.Effects)
            {
                if (!_requiresManualTarget(effect))
                {
                    continue;
                }

                hasManualTargetEffect = true;
                if (effect.Target == EEffectTarget.SelectedEnemy || effect.Target == EEffectTarget.EnemyCompanions)
                {
                    hasEnemyManualTarget = true;
                }

                bool valid = effect.Target switch
                {
                    EEffectTarget.Self => isSelf,
                    EEffectTarget.SelectedAlly => isAlly,
                    EEffectTarget.SelectedAnyAllyUnit => isAlly,
                    EEffectTarget.SelectedEnemy => isEnemy,
                    EEffectTarget.EnemyCompanions => isEnemy && isCompanion,
                    _ => false
                };

                if (valid)
                {
                    return true;
                }
            }

            if (hasEnemyManualTarget)
            {
                return isEnemy;
            }

            return !hasManualTargetEffect;
        }

        private static bool _requiresManualTarget(CardEffectConfiguration effect)
        {
            if (effect == null)
            {
                return false;
            }

            if (effect.Type == EEffectType.SummonCompanion || effect.Type == EEffectType.MoveLine)
            {
                return false;
            }

            EEffectTarget target = effect.Target;
            return target == EEffectTarget.SelectedEnemy
                   || target == EEffectTarget.SelectedAlly
                   || target == EEffectTarget.SelectedAnyAllyUnit
                   || target == EEffectTarget.Self
                   || target == EEffectTarget.EnemyCompanions;
        }
    }
}
