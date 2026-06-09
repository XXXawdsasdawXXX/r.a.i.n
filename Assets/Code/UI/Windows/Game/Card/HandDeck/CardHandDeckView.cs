using System;
using System.Collections.Generic;
using Core.Save;
using CoreGame.Card.Data;
using UI.Components;
using UI.Windows.Base;
using UnityEngine;

namespace UI.Windows.Game.Card.HandDeck
{
    public class CardHandDeckView : UIWindowView
    {
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

        /// <summary>Показывает руку героя. Пул остаётся во view; клики уходят через <paramref name="onCardClicked"/>.</summary>
        public void DisplayHand(IReadOnlyList<CardBattleState> hand, Action<string> onCardClicked)
        {
            _cardsPool.DisableAll();

            foreach (CardBattleState cardBattleState in hand)
            {
                CardView cardView = _cardsPool.GetNext();

                cardView.SetModel(new CardView.Model
                {
                    Id = cardBattleState.Config.Id,
                    InstanceId = cardBattleState.InstanceId,
                    Name = cardBattleState.Config.Name,
                    Type = cardBattleState.Config.Type,
                    Icon = cardBattleState.Config.Icon,
                    Description = cardBattleState.Config.Description,
                    EnergyPrice = cardBattleState.GetEnergyCost(_heroStats),
                    CurrentCharge = cardBattleState.ChargesLeft,
                    MaxCharge = cardBattleState.Config.Charges
                });

                if (onCardClicked != null)
                {
                    cardView.CardClicked += onCardClicked;
                }
            }
        }

        public void SetInteractable(bool isInteractable)
        {
            foreach (CardView cardView in _cardsPool.Enabled)
            {
                cardView.SetInteractable(isInteractable);
            }
        }

    }
}
