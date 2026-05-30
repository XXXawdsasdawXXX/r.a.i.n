using System.Collections.Generic;

namespace CoreGame.Card.Data
{
    public class BattleSide
    {
        public BattleUnit Hero;
        public List<BattleUnit> Companions = new();

        public readonly List<CardBattleState> Hand = new();
        public readonly List<CardBattleState> Deck = new();
        public readonly List<CardBattleState> Discard = new();

        public BattleSide(BattleUnit hero)
        {
            foreach (CardBattleState VARIABLE in hero.Deck)
            {
                
            }
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