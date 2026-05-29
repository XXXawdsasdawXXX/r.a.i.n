namespace CoreGame.Card.Data
{
    public enum EEffectType
    {
        Damage,
        Heal,
        AddEnergy,
        AddArmor,
        ApplyStatus,
        SummonCompanion,
        InjectParasite, // в свою колоду
        InjectParasiteEnemy, // в колоду врага
        DrawCards
    }
}