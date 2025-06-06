using System;
using Random = UnityEngine.Random;

namespace Core.Data.RangeInt
{
    [Serializable]
    public struct RangedInt
    {
        public int MinValue;
        public int MaxValue;

        public RangedInt(int min, int max)
        {
            MinValue = min;
            MaxValue = max;
        }
        
        public readonly int GetRandomValue()
        {
            return Random.Range(MinValue, MaxValue);
        }

        public bool Contains(int value)
        {
            return MinValue <= value && MaxValue >= value;
        }
    }
}