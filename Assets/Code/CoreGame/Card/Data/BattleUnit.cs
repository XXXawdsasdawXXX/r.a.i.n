using System;
using System.Collections.Generic;
using System.Linq;
using Core.Save;
using CoreGame.Card.Logic.AI;
using GameKit.Dependencies.Utilities;

namespace CoreGame.Card.Data
{
    [Serializable]
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
        public int HandLimit;       // базово 5
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
        public int LineCellIndex;
        public EAutoActionType AutoActionType;
        public float AutoActionValue;
        public int CompanionCardsPerTurn = 1;


        public List<CardBattleState> Hand = new();
        public List<CardBattleState> Deck = new();
        public List<CardBattleState> Discard = new();
        
        public static BattleUnit FromHero(HeroModel hero, AllCardCollection library)
        {
            BattleUnit unit = new()
            {
                UnitId = hero.HeroId,
                OwnerId = string.Empty,
                MaxHP = 100 + hero.Stats.Endurance * 10,
                HP = hero.Health,
                MaxEnergy = 100 + hero.Stats.Endurance * 5,
                Stats = hero.Stats,
                AutoActionType = EAutoActionType.AttackEnemyHero,
                AutoActionValue = 7,
                Line = EBattleLine.Frontline,
                LineCellIndex = 1
            };
            
            unit.Energy = unit.MaxEnergy;
            unit.HandLimit = 5; // TODO: + прокачка колоды
            unit.CritChance = hero.Stats.Agility * 0.02f;
            unit.DodgeChance = hero.Stats.Agility * 0.015f;
            unit.StunChance = hero.Stats.Strength * 0.02f;

            unit.Deck = _createDeck(unit.UnitId, hero.Deck, library);
            unit.Deck.Shuffle();
        
            return unit;
        }

        public static BattleUnit FromCompanion(
            CompanionConfiguration companion, 
            string ownerId, 
            AllCardCollection library)
        {
            string unitId = Guid.NewGuid().ToString();
            List<CardBattleState> companionDeck = _createDeck(unitId, companion.Cards, library);

            return new BattleUnit
            {
                UnitId = unitId,
                OwnerId = ownerId,
                IsCompanion = true,
                HP = 50,
                MaxHP = 50,
                Armor = 0,
                Energy = 0,
                MaxEnergy = 0,
                HandLimit = 0,
                IsInArmorStance = false,
                ArmorStanceTurnsLeft = 0,
                Statuses = new List<StatusEffect>(),
                Hand = new List<CardBattleState>(),
                Deck = companionDeck,
                Discard = new List<CardBattleState>(),
                CritChance = 0,
                DodgeChance = 0,
                StunChance = 0,
                MoveLineCost = 0,
                Stats = null,
                AutoActionType = EAutoActionType.GiveShieldToOwnerHero,
                AutoActionValue = 4,
                CompanionCardsPerTurn = UnityEngine.Mathf.Max(0, companion.CardsPerTurn),
                Line = EBattleLine.Backline,
                LineCellIndex = 1
            };
        }

        private static List<CardBattleState> _createDeck(
            string ownerId, 
            IEnumerable<string> cardsId, 
            AllCardCollection library)
        {
            if (cardsId == null)
            {
                return new List<CardBattleState>();
            }

            return cardsId
                .Where(id => !string.IsNullOrEmpty(id))
                .Where(id => library.Get(id) != null)
                .Select(id => new CardBattleState 
                { 
                    InstanceId = Guid.NewGuid().ToString(),
                    OwnerId = ownerId,
                    Config = library.Get(id),
                    ChargesLeft = library.Get(id).Charges
                })
                .ToList();
        }
    }
}