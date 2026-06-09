using System;
using System.Collections.Generic;
using CoreGame.Card.Data;

namespace CoreGame.Card.Logic
{
    [Serializable]
    public struct BattleSyncData
    {
        public string BattleId;
        public EBattleMode Mode;
        public EBattlePhase Phase;
        public int TurnNumber;
        public float TurnTimeRemaining;
        public BattleSideSyncData SideA;
        public BattleSideSyncData SideB;
        public bool HasAllySide;
        public BattleSideSyncData AllySide;
    }

    [Serializable]
    public struct BattleSideSyncData
    {
        public BattleUnitSyncData Hero;
        public List<BattleUnitSyncData> Companions;
        public List<CardSyncData> MandatoryCards;
    }

    [Serializable]
    public struct BattleUnitSyncData
    {
        public string UnitId;
        public string OwnerId;
        public bool IsCompanion;
        public float HP;
        public float MaxHP;
        public float Armor;
        public int Energy;
        public int MaxEnergy;
        public int HandLimit;
        public bool IsInArmorStance;
        public int ArmorStanceTurnsLeft;
        public float CritChance;
        public float DodgeChance;
        public float StunChance;
        public int MoveLineCost;
        public EBattleLine Line;
        public int LineCellIndex;
        public EAutoActionType AutoActionType;
        public float AutoActionValue;
        public int CompanionCardsPerTurn;
        public bool HasAI;
        public bool HasStats;
        public int StatAgility;
        public int StatStrength;
        public int StatEndurance;
        public int StatIntellect;

        public List<CardSyncData> Hand;
        public int HiddenHandCount;
        public int DeckCount;
        public int DiscardCount;
        public List<StatusSyncData> Statuses;
    }

    [Serializable]
    public struct CardSyncData
    {
        public string InstanceId;
        public string ConfigId;
        public string OwnerId;
        public int ChargesLeft;
        public bool IsParasite;
    }

    [Serializable]
    public struct StatusSyncData
    {
        public EStatusType Type;
        public float Value;
        public int Duration;
    }
}
