using System;
using Random = UnityEngine.Random;

namespace Core.Data.RangeFloat
{
    [Serializable]
    public struct RangedFloat
    {
        public float MinValue;
        public float MaxValue;

        public RangedFloat(float min, float max)
        {
            MinValue = min;
            MaxValue = max;
        }

        public readonly float GetRandomValue()
        {
            return Random.Range(MinValue, MaxValue);
        }

        public bool Contains(float value)
        {
            return MinValue <= value && MaxValue >= value;
        }
    }
}