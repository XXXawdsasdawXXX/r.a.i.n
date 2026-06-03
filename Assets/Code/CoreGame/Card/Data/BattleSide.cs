using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CoreGame.Card.Data
{
    [Serializable]
    public class BattleSide
    {
        public BattleUnit Hero;
        public List<BattleUnit> Companions = new();
        
        [SerializeField] private List<CardBattleState> _mandatoryCards = new();
        
        
        public BattleSide(BattleUnit hero, List<CardBattleState> mandatoryCards = null)
        {
            Hero = hero;

            if (mandatoryCards != null)
            {
                _mandatoryCards.AddRange(mandatoryCards);
            }
        }
        
        public BattleUnit GetUnit(string id)
        {
            return Hero.UnitId.Equals(id) 
                ? Hero 
                : Companions.FirstOrDefault(c => c.UnitId.Equals(id));
        }
        
        public List<BattleUnit> GetAllUnits() 
        {
            List<BattleUnit> all = new() { Hero };
            all.AddRange(Companions);
            return all;
        }

        public List<CardBattleState> GetHand()
        {
            List<CardBattleState> hand = _mandatoryCards.ToList();
            
            hand.AddRange(Hero.Hand);

            foreach (BattleUnit companion in Companions)
            {
                hand.AddRange(companion.Hand);
            }

            return hand;
        }
    }
}