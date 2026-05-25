using System.Collections.Generic;
using System.Linq;
using Core.Save;
using GameKit.Dependencies.Utilities;

namespace CoreGame.Card
{
    public class BattleUnit
    {
        public string UnitId;
        public string OwnerId;      // кто владелец (для спутников)
        public bool IsCompanion;
    
        public float HP;
        public float MaxHP;
        public float Armor;
        public int Energy;
        public int MaxEnergy;
        public int HandLimit;       // базово 7
    
        public bool IsInArmorStance;
        public int ArmorStanceTurnsLeft;
    
        public List<StatusEffect> Statuses = new();
        public List<CardBattleState> Hand = new();
        public List<CardBattleState> Deck = new();
        public List<CardBattleState> Discard = new();
    
        // считаем из HeroStats при входе в бой
        public float CritChance;
        public float DodgeChance;
        public float StunChance;
    
        public static BattleUnit FromHero(HeroModel hero, CardLibrary library)
        {
            var unit = new BattleUnit();
            unit.UnitId = hero.Name;
            unit.MaxHP = 100 + hero.Stats.Endurance * 10;
            unit.HP = hero.Health;
            unit.MaxEnergy = 100 + hero.Stats.Endurance * 5;
            unit.Energy = unit.MaxEnergy;
            unit.HandLimit = 7; // TODO: + прокачка колоды
            unit.CritChance = hero.Stats.Agility * 0.02f;
            unit.DodgeChance = hero.Stats.Agility * 0.015f;
            unit.StunChance = hero.Stats.Strength * 0.02f;
        
            // собираем колоду из id
            unit.Deck = hero.Deck
                .Select(id => new CardBattleState 
                { 
                    Config = library.Get(id),
                    ChargesLeft = library.Get(id).Charges
                })
                .ToList();
            
            unit.Deck.Shuffle();
        
            return unit;
        }
    }
}