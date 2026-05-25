using System;

namespace CoreGame.Card
{
    [Serializable]
    public class StatusEffect
    {
        public EStatusType Type;
        public int Duration;        // ходов
        public float Value;         // сила
        public string SourceCardId;
    }
}