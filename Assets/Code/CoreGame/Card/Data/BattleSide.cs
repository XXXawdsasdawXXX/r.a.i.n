using System.Collections.Generic;

namespace CoreGame.Card
{
    public class BattleSide
    {
        public BattleUnit Hero;
        public List<BattleUnit> Companions = new();
    
        public List<BattleUnit> GetAllUnits() 
        {
            var all = new List<BattleUnit> { Hero };
            all.AddRange(Companions);
            return all;
        }
    }
}