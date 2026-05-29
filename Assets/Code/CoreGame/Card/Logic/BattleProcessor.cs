using System.Collections.Generic;
using Core.Save;
using CoreGame.Card.Data;
using CoreGame.Card.Logic.CardProcessors;

namespace CoreGame.Card.Logic
{
    public class BattleProcessor
    {
        private Dictionary<EEffectType, ICardProcessor> _cardProcessors = new()
        {
            { EEffectType.Damage, new CardDamageProcessor()},
            { EEffectType.Heal, new CardHealEffectProcessor()},
            { EEffectType.AddEnergy, new CardEnergyProcessor()},
            { EEffectType.AddArmor, new CardArmorProcessor()},
            { EEffectType.ApplyStatus, new CardApplyStatusProcessor()},
            { EEffectType.SummonCompanion, new CardSummonCompanionProcessor()},
            { EEffectType.InjectParasite, new CardInjectParasiteProcessor()},
            { EEffectType.InjectParasiteEnemy, new CardInjectParasiteEnemyProcessor()},
        };
        
        
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
    }
}