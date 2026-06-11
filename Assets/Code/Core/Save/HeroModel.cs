using System;
using System.Collections.Generic;
using FishNet.Object.Synchronizing;

namespace Core.Save
{
    [Serializable]
    public class HeroModel
    {
        public string HeroId;
        
        public string Name;
        public int Health;
        public int Armor;
        public int Gold;
        public bool InBattle;
        
        public TimeSpan GameTime;
        public TimeSpan ExitTime;

        public HeroStats Stats;
    
        public List<string> CardCollection; // все карты которые есть у игрока (id)
        public List<string> Deck;           // собранная колода (id), макс 30
        public List<SavedDeckDefinition> Decks;
        public string SelectedDeckId;
        
        
        public HeroModel()
        {
            Name = "name";
            Health = 100;
            Gold = 100;

            GameTime = new TimeSpan();
            ExitTime = new TimeSpan();

            Stats = new HeroStats();

            CardCollection = new List<string>();
            Deck = new List<string>();
            Decks = new List<SavedDeckDefinition>();
            SelectedDeckId = string.Empty;
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