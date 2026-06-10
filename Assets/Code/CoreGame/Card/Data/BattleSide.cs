using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CoreGame.Card.Data
{
    [Serializable]
    public class BattleSide
    {
        public BattleUnit Hero;
        public List<BattleUnit> Companions = new();
        
        [SerializeField] private List<CardBattleState> _mandatoryCards = new();
        
        
        public BattleSide(BattleUnit hero, List<CardBattleState> mandatoryCards = null)
        {
            Hero = hero;

            if (mandatoryCards != null)
            {
                _mandatoryCards.AddRange(mandatoryCards);
            }
        }
        
        public BattleUnit GetUnit(string id)
        {
            return Hero.UnitId.Equals(id) 
                ? Hero 
                : Companions.FirstOrDefault(c => c.UnitId.Equals(id));
        }
        
        public List<BattleUnit> GetAllUnits() 
        {
            List<BattleUnit> all = new() { Hero };
            all.AddRange(Companions);
            return all;
        }

        public List<CardBattleState> GetHand()
        {
            List<CardBattleState> hand = _mandatoryCards.ToList();
            
            hand.AddRange(Hero.Hand);

            foreach (BattleUnit companion in Companions)
            {
                hand.AddRange(companion.Hand);
            }

            return hand;
        }

        public void ApplyVisibleHand(List<CardBattleState> visibleHand)
        {
            if (Hero == null || visibleHand == null)
            {
                return;
            }

            HashSet<string> mandatoryConfigIds = new(
                _mandatoryCards.Where(card => card?.Config != null).Select(card => card.Config.Id));

            Hero.Hand.Clear();
            foreach (CardBattleState card in visibleHand)
            {
                if (card == null || card.OwnerId != Hero.UnitId)
                {
                    continue;
                }

                if (card.Config != null && mandatoryConfigIds.Contains(card.Config.Id))
                {
                    continue;
                }

                Hero.Hand.Add(card);
            }

            foreach (BattleUnit companion in Companions)
            {
                companion.Hand = visibleHand
                    .Where(card => card != null && card.OwnerId == companion.UnitId)
                    .ToList();
            }
        }

        /// <summary>
        /// Карты, видимые конкретному герою: его обязательные, рука героя и руки его компаньонов.
        /// </summary>
        public List<CardBattleState> GetVisibleHand(string viewerHeroUnitId)
        {
            List<CardBattleState> hand = new();

            if (string.IsNullOrEmpty(viewerHeroUnitId) || Hero == null)
            {
                return hand;
            }

            if (Hero.UnitId == viewerHeroUnitId)
            {
                hand.AddRange(_mandatoryCards);
                hand.AddRange(Hero.Hand);
            }

            foreach (BattleUnit companion in Companions)
            {
                if (companion != null && companion.OwnerId == viewerHeroUnitId)
                {
                    hand.AddRange(companion.Hand);
                }
            }

            return hand;
        }

        public bool ContainsMandatoryCard(CardBattleState card)
        {
            return card != null
                   && _mandatoryCards.Any(mandatory =>
                       mandatory == card
                       || (mandatory?.InstanceId != null && mandatory.InstanceId == card.InstanceId));
        }

        public void RemoveMandatoryCard(CardBattleState card)
        {
            if (card == null)
            {
                return;
            }

            _mandatoryCards.Remove(card);
        }

        public void EnsureMandatoryCard(CardConfiguration config, string ownerId)
        {
            if (config == null)
            {
                return;
            }

            string configId = config.Id;
            if (string.IsNullOrEmpty(configId))
            {
                return;
            }

            // Обязательная карта не должна "утекать" в обычные зоны.
            _removeCardByConfigId(Hero?.Hand, configId);
            _removeCardByConfigId(Hero?.Deck, configId);
            _removeCardByConfigId(Hero?.Discard, configId);

            List<CardBattleState> sameMandatoryCards = _mandatoryCards
                .Where(card => card?.Config != null && card.Config.Id == configId)
                .ToList();
            if (sameMandatoryCards.Count > 1)
            {
                CardBattleState keep = sameMandatoryCards[0];
                _mandatoryCards.RemoveAll(card => card?.Config != null && card.Config.Id == configId && !ReferenceEquals(card, keep));
            }

            CardBattleState existing = _mandatoryCards.FirstOrDefault(card => card?.Config != null && card.Config.Id == configId);
            if (existing != null)
            {
                return;
            }

            _mandatoryCards.Add(new CardBattleState
            {
                InstanceId = Guid.NewGuid().ToString(),
                OwnerId = ownerId,
                Config = config,
                ChargesLeft = config.Charges
            });
        }

        private static void _removeCardByConfigId(List<CardBattleState> cards, string configId)
        {
            if (cards == null || string.IsNullOrEmpty(configId))
            {
                return;
            }

            cards.RemoveAll(card => card?.Config != null && card.Config.Id == configId);
        }
    }
}