using System;

namespace CoreGame.Card.Data
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