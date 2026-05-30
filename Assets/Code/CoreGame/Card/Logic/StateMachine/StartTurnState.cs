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

        
        public StartTurnState(BattleStateMachine machine)
        {
            _machine = machine;
        }

        public UniTask Initialize()
        {
            return UniTask.CompletedTask;
        }

        public UniTask Enter()
        {
            _drawCards(_machine.Model.SideA, _machine.Model.SideA.Hero.HandLimit);
            _drawCards(_machine.Model.SideB, _machine.Model.SideB.Hero.HandLimit);

            return UniTask.CompletedTask;
            // количество карт пока оставляем так, можем потом переделать правила
        }

        public UniTask Exit()
        {
            return UniTask.CompletedTask;
        }

        private void _drawCards(BattleSide side, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (side.Deck.Count == 0)
                {
                    _reshuffleDeck(side);
                }

                if (side.Deck.Count == 0)
                {
                    break;
                }

                CardBattleState card = side.Deck[0];
                side.Deck.RemoveAt(0);
                side.Hand.Add(card);
            }
        }

        private void _reshuffleDeck(BattleSide unit)
        {
            unit.Deck.AddRange(unit.Discard);
            unit.Discard.Clear();
            unit.Deck.Shuffle();
        }
    }
}