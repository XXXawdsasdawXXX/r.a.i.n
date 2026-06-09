using System.Collections.Generic;
using System.Linq;
using Core.Data;
using Core.Save;
using CoreGame.Card.Data;

namespace CoreGame.Card.Logic
{
    public static class BattleSyncSerializer
    {
        public static BattleSyncData CreateSnapshot(BattleModel model, string viewerHeroId, CardLibrary library)
        {
            if (model == null)
            {
                return default;
            }

            return new BattleSyncData
            {
                BattleId = model.BattleId,
                Mode = model.Mode,
                Phase = model.Phase?.Value ?? EBattlePhase.WaitingBattle,
                TurnNumber = model.TurnNumber,
                TurnTimeRemaining = model.TurnTimeRemaining?.Value ?? 0f,
                SideA = _createSideSnapshot(model.SideA, viewerHeroId, library),
                SideB = _createSideSnapshot(model.SideB, viewerHeroId, library),
                HasAllySide = model.HasAllySide,
                AllySide = model.HasAllySide
                    ? _createSideSnapshot(model.AllySide, viewerHeroId, library)
                    : default
            };
        }

        public static void ApplySnapshot(BattleModel model, BattleSyncData data, string viewerHeroId, CardLibrary library)
        {
            if (model == null)
            {
                return;
            }

            model.BattleId = data.BattleId;
            model.Mode = data.Mode;
            model.TurnNumber = data.TurnNumber;

            if (model.Phase == null)
            {
                model.Phase = new ReactiveProperty<EBattlePhase>(data.Phase);
            }
            else
            {
                model.Phase.Value = data.Phase;
            }

            if (model.TurnTimeRemaining == null)
            {
                model.TurnTimeRemaining = new ReactiveProperty<float>(data.TurnTimeRemaining);
            }
            else
            {
                model.TurnTimeRemaining.Value = data.TurnTimeRemaining;
            }

            model.SideA = _applySide(model.SideA, data.SideA, viewerHeroId, library);
            model.SideB = _applySide(model.SideB, data.SideB, viewerHeroId, library);

            if (data.HasAllySide)
            {
                model.AllySide = _applySide(model.AllySide, data.AllySide, viewerHeroId, library);
            }
            else
            {
                model.AllySide = null;
            }
        }

        private static BattleSideSyncData _createSideSnapshot(
            BattleSide side,
            string viewerHeroId,
            CardLibrary library)
        {
            if (side?.Hero == null)
            {
                return default;
            }

            return new BattleSideSyncData
            {
                Hero = _createUnitSnapshot(side.Hero, viewerHeroId, side.Hero.UnitId, library),
                Companions = side.Companions
                    .Where(c => c != null)
                    .Select(c => _createUnitSnapshot(c, viewerHeroId, side.Hero.UnitId, library))
                    .ToList(),
                MandatoryCards = _canViewHeroCards(side.Hero.UnitId, viewerHeroId)
                    ? _mapCards(side.GetHandForOwner(side.Hero.UnitId)
                        .Where(card => side.ContainsMandatoryCard(card)))
                    : new List<CardSyncData>()
            };
        }

        private static BattleUnitSyncData _createUnitSnapshot(
            BattleUnit unit,
            string viewerHeroId,
            string sideHeroId,
            CardLibrary library)
        {
            bool canViewCards = unit.IsCompanion
                ? _canViewCompanionCards(unit.OwnerId, viewerHeroId)
                : _canViewHeroCards(unit.UnitId, viewerHeroId);

            List<CardSyncData> visibleHand = canViewCards
                ? _mapCards(unit.Hand)
                : new List<CardSyncData>();

            return new BattleUnitSyncData
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
                IsInArmorStance = unit.IsInArmorStance,
                ArmorStanceTurnsLeft = unit.ArmorStanceTurnsLeft,
                CritChance = unit.CritChance,
                DodgeChance = unit.DodgeChance,
                StunChance = unit.StunChance,
                MoveLineCost = unit.MoveLineCost,
                Line = unit.Line,
                LineCellIndex = unit.LineCellIndex,
                AutoActionType = unit.AutoActionType,
                AutoActionValue = unit.AutoActionValue,
                CompanionCardsPerTurn = unit.CompanionCardsPerTurn,
                HasAI = unit.AI != null,
                HasStats = unit.Stats != null,
                StatAgility = unit.Stats?.Agility ?? 0,
                StatStrength = unit.Stats?.Strength ?? 0,
                StatEndurance = unit.Stats?.Endurance ?? 0,
                StatIntellect = unit.Stats?.Intellect ?? 0,
                Hand = visibleHand,
                HiddenHandCount = canViewCards ? 0 : unit.Hand?.Count ?? 0,
                DeckCount = unit.Deck?.Count ?? 0,
                DiscardCount = unit.Discard?.Count ?? 0,
                Statuses = unit.Statuses?
                    .Select(status => new StatusSyncData
                    {
                        Type = status.Type,
                        Value = status.Value,
                        Duration = status.Duration
                    })
                    .ToList() ?? new List<StatusSyncData>()
            };
        }

        private static BattleSide _applySide(
            BattleSide existing,
            BattleSideSyncData data,
            string viewerHeroId,
            CardLibrary library)
        {
            BattleUnit hero = _applyUnit(existing?.Hero, data.Hero, viewerHeroId, library, preserveAi: true);
            BattleSide side = new BattleSide(hero);

            if (data.Companions != null)
            {
                foreach (BattleUnitSyncData companionData in data.Companions)
                {
                    BattleUnit existingCompanion = existing?.Companions
                        ?.FirstOrDefault(c => c.UnitId == companionData.UnitId);
                    side.Companions.Add(_applyUnit(existingCompanion, companionData, viewerHeroId, library, preserveAi: true));
                }
            }

            if (data.MandatoryCards != null && hero != null)
            {
                foreach (CardSyncData cardData in data.MandatoryCards)
                {
                    CardConfiguration config = library.AllCards.Get(cardData.ConfigId);
                    if (config != null)
                    {
                        side.EnsureMandatoryCard(config, hero.UnitId);
                    }
                }
            }

            return side;
        }

        private static BattleUnit _applyUnit(
            BattleUnit existing,
            BattleUnitSyncData data,
            string viewerHeroId,
            CardLibrary library,
            bool preserveAi)
        {
            BattleUnit unit = existing ?? new BattleUnit();
            unit.UnitId = data.UnitId;
            unit.OwnerId = data.OwnerId;
            unit.IsCompanion = data.IsCompanion;
            unit.HP = data.HP;
            unit.MaxHP = data.MaxHP;
            unit.Armor = data.Armor;
            unit.Energy = data.Energy;
            unit.MaxEnergy = data.MaxEnergy;
            unit.HandLimit = data.HandLimit;
            unit.IsInArmorStance = data.IsInArmorStance;
            unit.ArmorStanceTurnsLeft = data.ArmorStanceTurnsLeft;
            unit.CritChance = data.CritChance;
            unit.DodgeChance = data.DodgeChance;
            unit.StunChance = data.StunChance;
            unit.MoveLineCost = data.MoveLineCost;
            unit.Line = data.Line;
            unit.LineCellIndex = data.LineCellIndex;
            unit.AutoActionType = data.AutoActionType;
            unit.AutoActionValue = data.AutoActionValue;
            unit.CompanionCardsPerTurn = data.CompanionCardsPerTurn;

            if (!preserveAi || unit.AI == null)
            {
                unit.AI = data.HasAI ? existing?.AI : null;
            }

            if (data.HasStats)
            {
                unit.Stats = new HeroStats
                {
                    Agility = data.StatAgility,
                    Strength = data.StatStrength,
                    Endurance = data.StatEndurance,
                    Intellect = data.StatIntellect
                };
            }
            else if (existing?.Stats != null)
            {
                unit.Stats = existing.Stats;
            }

            unit.Hand = _applyCards(data.Hand, library);
            unit.Deck = _createHiddenZone(data.DeckCount);
            unit.Discard = _createHiddenZone(data.DiscardCount);

            unit.Statuses = data.Statuses?
                .Select(status => new StatusEffect
                {
                    Type = status.Type,
                    Value = status.Value,
                    Duration = status.Duration
                })
                .ToList() ?? new List<StatusEffect>();

            return unit;
        }

        private static List<CardBattleState> _applyCards(List<CardSyncData> cards, CardLibrary library)
        {
            List<CardBattleState> result = new();

            if (cards == null)
            {
                return result;
            }

            foreach (CardSyncData cardData in cards)
            {
                CardConfiguration config = library.AllCards.Get(cardData.ConfigId);
                if (config == null)
                {
                    continue;
                }

                result.Add(new CardBattleState
                {
                    InstanceId = cardData.InstanceId,
                    Config = config,
                    OwnerId = cardData.OwnerId,
                    ChargesLeft = cardData.ChargesLeft,
                    IsParasite = cardData.IsParasite
                });
            }

            return result;
        }

        private static List<CardSyncData> _mapCards(IEnumerable<CardBattleState> cards)
        {
            return cards?
                .Where(card => card?.Config != null)
                .Select(card => new CardSyncData
                {
                    InstanceId = card.InstanceId,
                    ConfigId = card.Config.Id,
                    OwnerId = card.OwnerId,
                    ChargesLeft = card.ChargesLeft,
                    IsParasite = card.IsParasite
                })
                .ToList() ?? new List<CardSyncData>();
        }

        private static List<CardBattleState> _createHiddenZone(int count)
        {
            List<CardBattleState> zone = new();
            for (int i = 0; i < count; i++)
            {
                zone.Add(new CardBattleState { InstanceId = $"hidden_{i}" });
            }

            return zone;
        }

        private static bool _canViewHeroCards(string heroUnitId, string viewerHeroId)
        {
            return !string.IsNullOrEmpty(viewerHeroId) && heroUnitId == viewerHeroId;
        }

        private static bool _canViewCompanionCards(string companionOwnerId, string viewerHeroId)
        {
            return !string.IsNullOrEmpty(viewerHeroId) && companionOwnerId == viewerHeroId;
        }
    }
}
