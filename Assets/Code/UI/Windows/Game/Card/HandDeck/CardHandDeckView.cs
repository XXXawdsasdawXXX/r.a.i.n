using System;
using System.Collections.Generic;
using Core.Save;
using CoreGame.Card.Data;
using UI.Components;
using UI.Windows.Base;
using UnityEngine;
using System.Linq;
using System.Text;

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
                    Description = _buildDescription(cardBattleState),
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

        private static string _buildDescription(CardBattleState cardState)
        {
            if (cardState?.Config?.Effects == null || cardState.Config.Effects.Count == 0)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            foreach (CardEffectConfiguration effect in cardState.Config.Effects.Where(e => e != null))
            {
                if (sb.Length > 0)
                {
                    sb.Append('\n');
                }

                if (effect.Type == EEffectType.SummonCompanion)
                {
                    int cardsPerTurn = Mathf.Max(0, effect.CompanionConfiguration?.CardsPerTurn ?? 0);
                    int lifetimeTurns = effect.SummonDuration > 0
                        ? effect.SummonDuration
                        : Mathf.Max(0, effect.CompanionConfiguration?.LifetimeTurns ?? 0);

                    if (lifetimeTurns > 0)
                    {
                        sb.Append($"Summon temporary companion ({lifetimeTurns} turn(s)), cards/turn: {cardsPerTurn}");
                    }
                    else
                    {
                        sb.Append($"Summon lifetime companion, cards/turn: {cardsPerTurn}");
                    }

                    continue;
                }

                sb.Append(effect.Type);
                if (effect.Target != EEffectTarget.None)
                {
                    sb.Append(" -> ");
                    sb.Append(effect.Target);
                }
            }

            return sb.ToString();
        }
    }
}
