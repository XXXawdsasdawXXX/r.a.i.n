using System;
using System.Collections.Generic;
using System.Linq;
using Core.Save;
using CoreGame.Card.Data;
using CoreGame.Card.Logic.CardProcessors;

namespace CoreGame.Card.Logic
{
    public class BattleProcessor
    {
        private readonly Dictionary<EEffectType, ICardProcessor> _cardProcessors;
        
        public BattleProcessor(AllCardCollection allCards)
        {
            _cardProcessors = new Dictionary<EEffectType, ICardProcessor>
            {
                { EEffectType.Damage, new CardDamageProcessor() },
                { EEffectType.Heal, new CardHealEffectProcessor() },
                { EEffectType.AddEnergy, new CardEnergyProcessor() },
                { EEffectType.AddArmor, new CardArmorProcessor() },
                { EEffectType.MoveLine, new CardMoveLineProcessor() },
                { EEffectType.ApplyStatus, new CardApplyStatusProcessor() },
                { EEffectType.SummonCompanion, new CardSummonCompanionProcessor(allCards) },
                { EEffectType.InjectParasite, new CardInjectParasiteProcessor() },
                { EEffectType.InjectParasiteEnemy, new CardInjectParasiteEnemyProcessor() },
            };
        }
        
        public static float CalculateScaling(CardEffectConfiguration effect, HeroStats stats)
        {
            return effect.Scaling switch
            {
                EStatScaling.Strength  => stats.Strength  * effect.ScalingFactor,
                EStatScaling.Agility   => stats.Agility   * effect.ScalingFactor,
                EStatScaling.Intellect => stats.Intellect * effect.ScalingFactor,
                EStatScaling.Endurance => stats.Endurance * effect.ScalingFactor,
                _                      => 0f
            };
        }
        
        public void ApplyCard(BattleUnit actor, CardBattleState card, BattleUnit primaryTarget, BattleModel battle)
        {
            actor.Energy -= card.GetEnergyCost(actor.Stats);

            foreach (CardEffectConfiguration effect in card.Config.Effects)
            {
                if (!_cardProcessors.TryGetValue(effect.Type, out ICardProcessor processor))
                {
                    continue;
                }

                foreach (BattleUnit target in _resolveTargets(effect, actor, primaryTarget, battle))
                {
                    if (target == null || target.HP <= 0)
                    {
                        continue;
                    }

                    processor.Process(effect, actor, target, battle);
                }
            }
        }
        
        public void TickStatuses(BattleUnit unit, BattleModel battle)
        {
            foreach (StatusEffect status in unit.Statuses.ToList())
            {
                _tickStatus(status, unit);

                status.Duration--;

                // SummonDuration нужен с duration<=0 до конца резолва:
                // TurnResolutionState удаляет компаньона и чистит его карты по этому флагу.
                if (status.Type == EStatusType.SummonDuration)
                {
                    continue;
                }
               
                if (status.Duration <= 0)
                    unit.Statuses.Remove(status);
            }
        }

        private void _tickStatus(StatusEffect status, BattleUnit unit)
        {
            switch (status.Type)
            {
                case EStatusType.Bleed:
                case EStatusType.Poison:
                case EStatusType.Burn:
                    unit.HP -= status.Value;
                    break;

                case EStatusType.Regeneration:
                    unit.HP = Math.Min(unit.MaxHP, unit.HP + status.Value);
                    break;

                case EStatusType.Stun:
                    // обрабатывается в BattleValidator
                    break;

                case EStatusType.ArmorStance:
                    // обрабатывается в BattleValidator
                    break;
            }
        }

        private static List<BattleUnit> _resolveTargets(CardEffectConfiguration effect, BattleUnit actor, BattleUnit primaryTarget, BattleModel battle)
        {
            List<BattleUnit> fallback = new List<BattleUnit>();
            if (primaryTarget != null)
            {
                fallback.Add(primaryTarget);
            }

            if (effect == null || actor == null || battle == null)
            {
                return fallback;
            }

            BattleSide actorSide = BattleGridRules.GetOwnerSide(battle, actor);
            BattleSide enemySide = null;
            if (actorSide != null)
            {
                enemySide = ReferenceEquals(actorSide, battle.SideA) ? battle.SideB : battle.SideA;
            }

            switch (effect.Target)
            {
                case EEffectTarget.Self:
                    return new List<BattleUnit> { actor };
                case EEffectTarget.AllAllies:
                    return actorSide?.GetAllUnits()
                        .Where(unit => unit != null && unit.HP > 0)
                        .ToList() ?? fallback;
                case EEffectTarget.AllEnemies:
                    return enemySide?.GetAllUnits()
                        .Where(unit => unit != null && unit.HP > 0)
                        .ToList() ?? fallback;
                case EEffectTarget.EnemyFrontline:
                    return enemySide?.GetAllUnits()
                        .Where(unit => unit != null && unit.HP > 0 && unit.Line == EBattleLine.Frontline)
                        .ToList() ?? fallback;
                case EEffectTarget.EnemyBackline:
                    return enemySide?.GetAllUnits()
                        .Where(unit => unit != null && unit.HP > 0 && unit.Line == EBattleLine.Backline)
                        .ToList() ?? fallback;
                case EEffectTarget.AllCompanions:
                    return actorSide?.Companions
                        .Where(unit => unit != null && unit.HP > 0)
                        .ToList() ?? fallback;
                case EEffectTarget.EnemyCompanions:
                    return enemySide?.Companions
                        .Where(unit => unit != null && unit.HP > 0)
                        .ToList() ?? fallback;
                case EEffectTarget.SelectedAnyAllyUnit:
                    return actorSide?.GetAllUnits()
                        .Where(unit => unit != null && unit.HP > 0 && primaryTarget != null && unit.UnitId == primaryTarget.UnitId)
                        .ToList() ?? fallback;
                default:
                    return fallback;
            }
        }
    }
}