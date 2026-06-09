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