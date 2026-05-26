using System;
using Core.Data.RangeFloat;
using UnityEngine;

namespace CoreGame.Card
{
    [Serializable]
    public class CardEffectConfiguration
    {
        [field: SerializeField] public EEffectType Type { get; private set; }
        [field: SerializeField] public EEffectTarget Target { get; private set; }
        [field: SerializeField,MinMaxRangeFloat(0,20)] public RangedFloat BaseValue { get; private set; }
        [field: SerializeField] public EStatScaling Scaling { get; private set; }
        [field: SerializeField] public float ScalingFactor { get; private set; }
    
        // для паразитов
        [field: SerializeField] public CardConfiguration ParasiteCard { get; private set; }
        [field: SerializeField] public int ParasiteCount { get; private set; }
    
        // для призыва
        [field: SerializeField] public CompanionConfiguration CompanionPrefab { get; private set; }
        [field: SerializeField] public int SummonDuration { get; private set; } // 0 = до смерти
    
        // для брони/стойки
        [field: SerializeField] public int StanceDuration { get; private set; }
    }
}