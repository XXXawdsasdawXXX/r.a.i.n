using System;

namespace CoreGame.Card
{
    [Flags]
    public enum ECardType
    {
        Attack   = 1 << 0,
        Spell    = 1 << 1,
        Armor    = 1 << 2,
        Summon   = 1 << 3,
        Buff     = 1 << 4,
        Debuff   = 1 << 5,
        Parasite = 1 << 6
    }
}