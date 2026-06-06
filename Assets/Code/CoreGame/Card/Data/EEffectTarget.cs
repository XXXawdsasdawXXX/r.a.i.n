namespace CoreGame.Card.Data
{
    public enum EEffectTarget
    {
        None,
        Self,
        SelectedEnemy,        // игрок выбирает цель
        AllEnemies,           // все враги обеих зон
        EnemyFrontline,       // только передняя линия врага
        EnemyBackline,        // только задняя (заклинание/дебаф)
        AllAllies,
        AllCompanions, 
        EnemyCompanions,
        SelectedAlly          // игрок выбирает союзника (для бафов)
    }
}