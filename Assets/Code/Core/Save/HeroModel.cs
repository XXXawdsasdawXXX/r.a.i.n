using System;
using System.Collections.Generic;

namespace Core.Save
{
    [Serializable]
    public class HeroModel
    {
        public string Id;
        
        public string Name;
        public int Health;
        public int Armor;
        
        public TimeSpan GameTime;
        public TimeSpan ExitTime;

        public HeroStats Stats;
    
        public List<string> CardCollection; // все карты которые есть у игрока (id)
        public List<string> Deck;           // собранная колода (id), макс 30
        
        public HeroModel()
        {
            Name = "name";
            Health = 100;
            
            GameTime = new TimeSpan();
            ExitTime = new TimeSpan();

            Stats = new HeroStats();

            CardCollection = new List<string>();
            Deck = new List<string>();
        }
    }
    
    [Serializable]
    public class HeroStats
    {
        public int Agility;    
        public int Strength;   
        public int Endurance;  
        public int Intellect;  
    }
}