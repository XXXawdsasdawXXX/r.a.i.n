using System.Collections.Generic;
using System.Linq;
using Core.Save;
using CoreGame.Card.Logic.AI;
using GameKit.Dependencies.Utilities;

namespace CoreGame.Card.Data
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
        public HeroStats Stats;

        public IEnemyAI AI;
        
        public bool IsInArmorStance;
        public int ArmorStanceTurnsLeft;
    
        public List<StatusEffect> Statuses = new();

        // считаем из HeroStats при входе в бой
        public float CritChance;
        public float DodgeChance;
        public float StunChance;
        
        public int MoveLineCost;
        public EBattleLine Line;


        public List<CardBattleState> Hand = new();
        public List<CardBattleState> Deck = new();
        public List<CardBattleState> Discard = new();
        
        public static BattleUnit FromHero(HeroModel hero, AllCardCollection library)
        {
            BattleUnit unit = new()
            {
                UnitId = hero.Name,
                MaxHP = 100 + hero.Stats.Endurance * 10,
                HP = hero.Health,
                MaxEnergy = 100 + hero.Stats.Endurance * 5,
                Stats = hero.Stats
            };
            
            unit.Energy = unit.MaxEnergy;
            unit.HandLimit = 7; // TODO: + прокачка колоды
            unit.CritChance = hero.Stats.Agility * 0.02f;
            unit.DodgeChance = hero.Stats.Agility * 0.015f;
            unit.StunChance = hero.Stats.Strength * 0.02f;

            unit.Deck = _createDeck(unit.UnitId, hero.Deck, library);
            unit.Deck.Shuffle();
        
            return unit;
        }

        public static BattleUnit FromCompanion(CompanionConfiguration companion, string ownerId)
        {
            return new BattleUnit
            {
                UnitId = null,
                OwnerId = ownerId,
                IsCompanion = false,
                HP = 50,
                MaxHP = 50,
                Armor = 0,
                Energy = 0,
                MaxEnergy = 0,
                HandLimit = 0,
                IsInArmorStance = false,
                ArmorStanceTurnsLeft = 0,
                Statuses = null,
                Hand = null,
                Deck = _createDeck(companion.),
                Discard = null,
                CritChance = 0,
                DodgeChance = 0,
                StunChance = 0,
                MoveLineCost = 0,
                Stats = null
            };
        }

        private static List<CardBattleState> _createDeck(
            string ownerId, 
            IEnumerable<string> cardsId, 
            AllCardCollection library)
        {
            return cardsId
                .Select(id => new CardBattleState 
                { 
                    OwnerId = ownerId,
                    Config = library.Get(id),
                    ChargesLeft = library.Get(id).Charges
                })
                .ToList();
        }
    }
}