using System;
using System.Collections.Generic;
using System.Linq;
using Core.Data;
using Core.Save;
using CoreGame.Card;
using CoreGame.Card.Data;
using Newtonsoft.Json;

namespace CoreGame.Card.Logic.Network
{
    public static class BattleSnapshotSerializer
    {
        public static string Serialize(BattleModel model)
        {
            if (model == null)
            {
                return string.Empty;
            }

            BattleSnapshotDto dto = _toDto(model);
            return JsonConvert.SerializeObject(dto);
        }

        public static BattleModel Deserialize(string json, CardLibrary cardLibrary)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            BattleSnapshotDto dto = JsonConvert.DeserializeObject<BattleSnapshotDto>(json);
            return _fromDto(dto, cardLibrary);
        }

        public static string SerializeHand(IReadOnlyList<CardBattleState> hand)
        {
            if (hand == null || hand.Count == 0)
            {
                return "[]";
            }

            List<CardSnapshotDto> cards = hand.Select(_toCardDto).ToList();
            return JsonConvert.SerializeObject(cards);
        }

        public static List<CardBattleState> DeserializeHand(string json, CardLibrary cardLibrary)
        {
            if (string.IsNullOrEmpty(json))
            {
                return new List<CardBattleState>();
            }

            List<CardSnapshotDto> cards = JsonConvert.DeserializeObject<List<CardSnapshotDto>>(json) ?? new List<CardSnapshotDto>();
            return cards.Select(card => _fromCardDto(card, cardLibrary)).Where(card => card != null).ToList();
        }

        public static void ApplyPublicSnapshot(BattleModel target, BattleModel source)
        {
            if (target == null || source == null)
            {
                return;
            }

            target.BattleId = source.BattleId;
            target.Mode = source.Mode;
            target.TurnNumber = source.TurnNumber;
            target.Phase.Value = source.Phase.Value;
            target.TurnTimeRemaining.Value = source.TurnTimeRemaining.Value;
            _copySideUnits(target.SideA, source.SideA);
            _copySideUnits(target.SideB, source.SideB);

            if (source.EnemySide != null)
            {
                target.EnemySide ??= new BattleSide(source.EnemySide.Hero);
                _copySideUnits(target.EnemySide, source.EnemySide);
            }
        }

        private static void _copySideUnits(BattleSide target, BattleSide source)
        {
            if (source == null)
            {
                return;
            }

            if (target == null)
            {
                return;
            }

            if (target.Hero == null && source.Hero != null)
            {
                target.Hero = source.Hero;
            }
            else
            {
                _copyUnit(target.Hero, source.Hero);
            }

            _syncCompanions(target, source);
        }

        private static void _syncCompanions(BattleSide target, BattleSide source)
        {
            if (target.Companions == null || source.Companions == null)
            {
                return;
            }

            while (target.Companions.Count < source.Companions.Count)
            {
                target.Companions.Add(new BattleUnit());
            }

            while (target.Companions.Count > source.Companions.Count)
            {
                target.Companions.RemoveAt(target.Companions.Count - 1);
            }

            for (int i = 0; i < source.Companions.Count; i++)
            {
                _copyUnit(target.Companions[i], source.Companions[i]);
            }
        }

        private static void _copyUnit(BattleUnit target, BattleUnit source)
        {
            if (target == null || source == null)
            {
                return;
            }

            target.UnitId = source.UnitId;
            target.OwnerId = source.OwnerId;
            target.IsCompanion = source.IsCompanion;
            target.HP = source.HP;
            target.MaxHP = source.MaxHP;
            target.Armor = source.Armor;
            target.Energy = source.Energy;
            target.MaxEnergy = source.MaxEnergy;
            target.HandLimit = source.HandLimit;
            target.Stats = source.Stats;
            target.IsInArmorStance = source.IsInArmorStance;
            target.ArmorStanceTurnsLeft = source.ArmorStanceTurnsLeft;
            target.Statuses = source.Statuses;
            target.MoveLineCost = source.MoveLineCost;
            target.Line = source.Line;
            target.LineCellIndex = source.LineCellIndex;
            target.AutoActionType = source.AutoActionType;
            target.AutoActionValue = source.AutoActionValue;
            target.CompanionCardsPerTurn = source.CompanionCardsPerTurn;
        }

        public static void ApplyPrivateHand(BattleModel model, string viewerUnitId, List<CardBattleState> visibleHand)
        {
            BattleSide side = BattleParticipantHelper.GetMySide(model, viewerUnitId);
            side?.ApplyVisibleHand(visibleHand);
        }

        private static BattleSnapshotDto _toDto(BattleModel model)
        {
            return new BattleSnapshotDto
            {
                BattleId = model.BattleId,
                Mode = model.Mode,
                Phase = model.Phase?.Value ?? EBattlePhase.WaitingBattle,
                TurnNumber = model.TurnNumber,
                TurnTimeRemaining = model.TurnTimeRemaining?.Value ?? 0f,
                SideA = _toSideDto(model.SideA, includePrivateCards: false),
                SideB = _toSideDto(model.SideB, includePrivateCards: false),
                EnemySide = _toSideDto(model.EnemySide, includePrivateCards: false)
            };
        }

        private static BattleModel _fromDto(BattleSnapshotDto dto, CardLibrary cardLibrary)
        {
            if (dto == null)
            {
                return null;
            }

            return new BattleModel
            {
                BattleId = dto.BattleId,
                Mode = dto.Mode,
                Phase = new ReactiveProperty<EBattlePhase>(dto.Phase),
                TurnNumber = dto.TurnNumber,
                TurnTimeRemaining = new ReactiveProperty<float>(dto.TurnTimeRemaining),
                SideA = _fromSideDto(dto.SideA, cardLibrary),
                SideB = _fromSideDto(dto.SideB, cardLibrary),
                EnemySide = _fromSideDto(dto.EnemySide, cardLibrary)
            };
        }

        private static SideSnapshotDto _toSideDto(BattleSide side, bool includePrivateCards)
        {
            if (side == null)
            {
                return null;
            }

            return new SideSnapshotDto
            {
                Hero = _toUnitDto(side.Hero, includePrivateCards),
                Companions = side.Companions?.Select(unit => _toUnitDto(unit, includePrivateCards)).ToList()
            };
        }

        private static BattleSide _fromSideDto(SideSnapshotDto dto, CardLibrary cardLibrary)
        {
            if (dto?.Hero == null)
            {
                return null;
            }

            BattleUnit hero = _fromUnitDto(dto.Hero, cardLibrary);
            BattleSide side = new BattleSide(hero);

            if (dto.Companions != null)
            {
                side.Companions.AddRange(dto.Companions.Select(unit => _fromUnitDto(unit, cardLibrary)));
            }

            return side;
        }

        private static UnitSnapshotDto _toUnitDto(BattleUnit unit, bool includePrivateCards)
        {
            if (unit == null)
            {
                return null;
            }

            return new UnitSnapshotDto
            {
                UnitId = unit.UnitId,
                OwnerId = unit.OwnerId,
                IsCompanion = unit.IsCompanion,
                HP = unit.HP,
                MaxHP = unit.MaxHP,
                Armor = unit.Armor,
                Energy = unit.Energy,
                MaxEnergy = unit.MaxEnergy,
                HandLimit = unit.HandLimit,
                Stats = unit.Stats,
                IsInArmorStance = unit.IsInArmorStance,
                ArmorStanceTurnsLeft = unit.ArmorStanceTurnsLeft,
                Statuses = unit.Statuses?.Select(status => new StatusSnapshotDto
                {
                    Type = status.Type,
                    Duration = status.Duration,
                    Value = status.Value
                }).ToList(),
                MoveLineCost = unit.MoveLineCost,
                Line = unit.Line,
                LineCellIndex = unit.LineCellIndex,
                AutoActionType = unit.AutoActionType,
                AutoActionValue = unit.AutoActionValue,
                CompanionCardsPerTurn = unit.CompanionCardsPerTurn,
                Hand = includePrivateCards ? unit.Hand?.Select(_toCardDto).ToList() : null,
                DeckCount = unit.Deck?.Count ?? 0,
                DiscardCount = unit.Discard?.Count ?? 0
            };
        }

        private static BattleUnit _fromUnitDto(UnitSnapshotDto dto, CardLibrary cardLibrary)
        {
            if (dto == null)
            {
                return null;
            }

            return new BattleUnit
            {
                UnitId = dto.UnitId,
                OwnerId = dto.OwnerId,
                IsCompanion = dto.IsCompanion,
                HP = dto.HP,
                MaxHP = dto.MaxHP,
                Armor = dto.Armor,
                Energy = dto.Energy,
                MaxEnergy = dto.MaxEnergy,
                HandLimit = dto.HandLimit,
                Stats = dto.Stats,
                IsInArmorStance = dto.IsInArmorStance,
                ArmorStanceTurnsLeft = dto.ArmorStanceTurnsLeft,
                Statuses = dto.Statuses?.Select(status => new StatusEffect
                {
                    Type = status.Type,
                    Duration = status.Duration,
                    Value = status.Value
                }).ToList() ?? new List<StatusEffect>(),
                MoveLineCost = dto.MoveLineCost,
                Line = dto.Line,
                LineCellIndex = dto.LineCellIndex,
                AutoActionType = dto.AutoActionType,
                AutoActionValue = dto.AutoActionValue,
                CompanionCardsPerTurn = dto.CompanionCardsPerTurn,
                Hand = dto.Hand?.Select(card => _fromCardDto(card, cardLibrary)).Where(card => card != null).ToList()
                       ?? new List<CardBattleState>(),
                Deck = _createPlaceholderDeck(dto.DeckCount),
                Discard = _createPlaceholderDeck(dto.DiscardCount)
            };
        }

        private static List<CardBattleState> _createPlaceholderDeck(int count)
        {
            List<CardBattleState> deck = new();
            for (int i = 0; i < count; i++)
            {
                deck.Add(new CardBattleState { InstanceId = Guid.NewGuid().ToString() });
            }

            return deck;
        }

        private static CardSnapshotDto _toCardDto(CardBattleState card)
        {
            if (card == null)
            {
                return null;
            }

            return new CardSnapshotDto
            {
                InstanceId = card.InstanceId,
                OwnerId = card.OwnerId,
                ConfigId = card.Config?.Id,
                ChargesLeft = card.ChargesLeft,
                IsParasite = card.IsParasite
            };
        }

        private static CardBattleState _fromCardDto(CardSnapshotDto dto, CardLibrary cardLibrary)
        {
            if (dto == null || string.IsNullOrEmpty(dto.ConfigId))
            {
                return null;
            }

            CardConfiguration config = cardLibrary?.AllCards?.Get(dto.ConfigId);
            if (config == null)
            {
                return null;
            }

            return new CardBattleState
            {
                InstanceId = dto.InstanceId,
                OwnerId = dto.OwnerId,
                Config = config,
                ChargesLeft = dto.ChargesLeft,
                IsParasite = dto.IsParasite
            };
        }

        [Serializable]
        private class BattleSnapshotDto
        {
            public string BattleId;
            public EBattleMode Mode;
            public EBattlePhase Phase;
            public int TurnNumber;
            public float TurnTimeRemaining;
            public SideSnapshotDto SideA;
            public SideSnapshotDto SideB;
            public SideSnapshotDto EnemySide;
        }

        [Serializable]
        private class SideSnapshotDto
        {
            public UnitSnapshotDto Hero;
            public List<UnitSnapshotDto> Companions;
        }

        [Serializable]
        private class UnitSnapshotDto
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
            public HeroStats Stats;
            public bool IsInArmorStance;
            public int ArmorStanceTurnsLeft;
            public List<StatusSnapshotDto> Statuses;
            public int MoveLineCost;
            public EBattleLine Line;
            public int LineCellIndex;
            public EAutoActionType AutoActionType;
            public float AutoActionValue;
            public int CompanionCardsPerTurn;
            public List<CardSnapshotDto> Hand;
            public int DeckCount;
            public int DiscardCount;
        }

        [Serializable]
        private class StatusSnapshotDto
        {
            public EStatusType Type;
            public int Duration;
            public float Value;
        }

        [Serializable]
        private class CardSnapshotDto
        {
            public string InstanceId;
            public string OwnerId;
            public string ConfigId;
            public int ChargesLeft;
            public bool IsParasite;
        }
    }
}
