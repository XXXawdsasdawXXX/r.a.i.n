using System.Collections.Generic;
using System.Linq;
using Core.Save;

namespace CoreGame.Card.Data
{
    public class BattleSide
    {
        public BattleUnit Hero;
        public List<BattleUnit> Companions = new();


        public BattleSide(BattleUnit hero)
        {
            Hero = hero;
        }
        
        public BattleUnit GetUnit(string id)
        {
            if (Hero.UnitId.Equals(id))
            {
                return Hero;
            }

            return Companions.FirstOrDefault(c => c.UnitId.Equals(id));
        }

        public void AddCompanion(BattleUnit unit)
        {
            Companions.Add(unit);
            Deck.AddRange(unit.Deck);
        }

        public void RemoveCompanion(BattleUnit unit)
        {
            Hand.RemoveAll(c => c.OwnerId == unit.UnitId);
            Deck.RemoveAll(c => c.OwnerId == unit.UnitId);
            Discard.RemoveAll(c => c.OwnerId == unit.UnitId);
            Companions.Remove(unit);
        }

        public List<BattleUnit> GetAllUnits() 
        {
            var all = new List<BattleUnit> { Hero };
            all.AddRange(Companions);
            return all;
        }
    }
}