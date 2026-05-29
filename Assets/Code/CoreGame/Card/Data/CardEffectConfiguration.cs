using System;
using Core.Data.RangeInt;
using UnityEngine;

namespace CoreGame.Card.Data
{
    [Serializable]
    public class CardEffectConfiguration
    {
        [field: SerializeField] public EEffectType Type { get; private set; }
        [field: SerializeField] public EEffectTarget Target { get; private set; }
        [field: SerializeField,MinMaxRangeInt(0,20)] public RangedInt BaseValue { get; private set; }
        [field: SerializeField] public EStatScaling Scaling { get; private set; }
        [field: SerializeField] public float ScalingFactor { get; private set; }
    
        // для паразитов
        [field: SerializeField] public CardConfiguration ParasiteCard { get; private set; }
        [field: SerializeField] public int ParasiteCount { get; private set; }
    
        // для призыва
        [field: SerializeField] public CompanionConfiguration CompanionConfiguration { get; private set; }
        [field: SerializeField] public int SummonDuration { get; private set; } // 0 = до смерти
    
        // для брони/стойки
        [field: SerializeField] public int StanceDuration { get; private set; }
        
        // для статусов
        [field: SerializeField] public EStatusType StatusType { get; private set; }
        [field: SerializeField] public int StatusDuration { get; private set; }
    }
}