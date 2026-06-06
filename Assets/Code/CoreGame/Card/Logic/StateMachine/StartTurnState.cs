using CoreGame.Card.Data;
using Cysharp.Threading.Tasks;
using GameKit.Dependencies.Utilities;

namespace CoreGame.Card.Logic.StateMachine
{
    public class StartTurnState : IBattleState
    {
        public EBattlePhase Phase => EBattlePhase.StartTurn;
        public bool IsInitialized { get; set; }

        private readonly BattleStateMachine _machine;
        private readonly CardLibrary _cardLibrary;

        
        public StartTurnState(BattleStateMachine machine, CardLibrary cardLibrary)
        {
            _machine = machine;
            _cardLibrary = cardLibrary;
        }

        public UniTask Initialize()
        {
            return UniTask.CompletedTask;
        }

        public UniTask Enter()
        {
            _machine.Model.TurnNumber++;

            _ensureMandatoryMovementCards();
            _startTurn(_machine.Model.SideA.Hero);
            _startTurn(_machine.Model.SideB.Hero);

            foreach (BattleUnit companion in _machine.Model.SideA.Companions)
            {
                _startCompanionTurn(companion);
            }

            foreach (BattleUnit companion in _machine.Model.SideB.Companions)
            {
                _startCompanionTurn(companion);
            }

            _machine.SwitchState(typeof(FirstSideTurnState));
            return UniTask.CompletedTask;
        }

        public UniTask Exit()
        {
            return UniTask.CompletedTask;
        }

        private void _ensureMandatoryMovementCards()
        {
            if (_cardLibrary.MandatoryCardsInHand == null || _cardLibrary.MandatoryCardsInHand.Length == 0)
            {
                // fallback для старых данных
                CardConfiguration legacyMoveCard = _cardLibrary.AllCards.Get("move_0") ?? _cardLibrary.AllCards.Get("energy_0");
                if (legacyMoveCard == null)
                {
                    return;
                }

                _machine.Model.SideA.EnsureMandatoryCard(legacyMoveCard, _machine.Model.SideA.Hero.UnitId);
                _machine.Model.SideB.EnsureMandatoryCard(legacyMoveCard, _machine.Model.SideB.Hero.UnitId);
                return;
            }

            foreach (string cardId in _cardLibrary.MandatoryCardsInHand)
            {
                CardConfiguration card = _cardLibrary.AllCards.Get(cardId);
                if (card == null)
                {
                    continue;
                }

                _machine.Model.SideA.EnsureMandatoryCard(card, _machine.Model.SideA.Hero.UnitId);
                _machine.Model.SideB.EnsureMandatoryCard(card, _machine.Model.SideB.Hero.UnitId);
            }
        }

        private static void _startTurn(BattleUnit unit)
        {
            unit.Energy = unit.MaxEnergy;
            _drawCardsToHandLimit(unit);
        }

        private static void _startCompanionTurn(BattleUnit companion)
        {
            if (companion == null || companion.HP <= 0 || companion.Hand == null || companion.Deck == null)
            {
                return;
            }

            int cardsPerTurn = UnityEngine.Mathf.Max(0, companion.CompanionCardsPerTurn);
            if (cardsPerTurn <= 0 || companion.Deck.Count == 0)
            {
                return;
            }

            int drawCount = UnityEngine.Mathf.Min(cardsPerTurn, companion.Deck.Count);
            for (int i = 0; i < drawCount; i++)
            {
                CardBattleState card = companion.Deck[0];
                companion.Deck.RemoveAt(0);
                companion.Hand.Add(card);
            }
        }

        private static void _drawCardsToHandLimit(BattleUnit unit)
        {
            int cardsToDraw = unit.HandLimit - unit.Hand.Count;

            if (cardsToDraw <= 0)
            {
                return;
            }

            for (int i = 0; i < cardsToDraw; i++)
            {
                if (unit.Deck.Count == 0)
                {
                    _reshuffleDeck(unit);
                }

                if (unit.Deck.Count == 0)
                {
                    break;
                }

                CardBattleState card = unit.Deck[0];
                unit.Deck.RemoveAt(0);
                unit.Hand.Add(card);
            }
        }

        private static void _reshuffleDeck(BattleUnit unit)
        {
            if (unit.Discard.Count == 0)
            {
                return;
            }

            unit.Deck.AddRange(unit.Discard);
            unit.Discard.Clear();
            unit.Deck.Shuffle();
        }

        
    }
}