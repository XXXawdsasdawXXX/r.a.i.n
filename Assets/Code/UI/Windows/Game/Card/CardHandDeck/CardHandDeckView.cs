using System.Collections.Generic;
using Core.Save;
using CoreGame.Card.Data;
using UI.Components;
using UI.Windows.Base;
using UnityEngine;

namespace UI.Windows.Card.CardDeck
{
    public class CardHandDeckView : UIWindowView
    {
        public List<CardView> CurrentCards => _cardsPool.Enabled;

        [SerializeField] private UIElementPool<CardView> _cardsPool;

        private HeroStats _heroStats;

        public void InitializePool()
        {
            _cardsPool.Initialize();
        }
        
        public void SetHeroStats(HeroStats heroStats)
        {
            _heroStats = heroStats;
        }
        
        public List<CardView> SetCards(List<CardBattleState> cards)
        {
            _cardsPool.DisableAll();

            foreach (CardBattleState cardBattleState in cards)
            {
                CardView cardView = _cardsPool.GetNext();
                
                cardView.SetModel(new CardView.Model
                {
                    Id = cardBattleState.Config.Id,
                    Name = cardBattleState.Config.Name,
                    Type = cardBattleState.Config.Type,
                    Description = "", //todo create description
                    EnergyPrice = cardBattleState.GetEnergyCost(_heroStats),
                    CurrentCharge = cardBattleState.ChargesLeft,
                    MaxCharge = cardBattleState.Config.Charges
                });

                cardView.UsedCard += id =>
                {
                    _cardsPool.Disable(cardView);
                };
            }

            return _cardsPool.Enabled;
        }

        public void SetInteractable(bool isMyTurn)
        {
            foreach (CardView cardView in _cardsPool.Enabled)
            {
                cardView.SetInteractable(isMyTurn);
            }
        }
    }
}