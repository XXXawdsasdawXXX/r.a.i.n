using System;
using Core.Save;
using UnityEngine;

namespace CoreGame.Card.Data
{
    [Serializable]
    public class CardBattleState
    {
        public CardConfiguration Config;

        public string OwnerId;
        public int ChargesLeft;    // копия из Config.Charges при входе в бой
        public bool IsParasite;    // замешана паразитом, при разыгрывании - особый эффект
    
        public int GetEnergyCost(HeroStats stats)
        {
            int cost = Config.BaseEnergyCost;
        
            if (Config.Type.HasFlag(ECardType.Attack))
                cost -= Mathf.FloorToInt(stats.Agility * 0.1f);
            
            if (Config.Type.HasFlag(ECardType.Spell) || 
                Config.Type.HasFlag(ECardType.Buff)  || 
                Config.Type.HasFlag(ECardType.Summon))
                cost -= Mathf.FloorToInt(stats.Intellect * 0.1f);
            
            return Mathf.Max(0, cost);
        }
    }
}