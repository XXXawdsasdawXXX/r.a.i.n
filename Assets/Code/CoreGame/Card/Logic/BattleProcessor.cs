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
                if (_cardProcessors.TryGetValue(effect.Type, out ICardProcessor processor))
                {
                    processor.Process(effect, actor, primaryTarget, battle);
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
    }
}