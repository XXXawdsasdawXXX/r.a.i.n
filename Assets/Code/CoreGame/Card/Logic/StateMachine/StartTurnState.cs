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
            _machine.Model.TurnNumber++;
            
            // количество карт пока оставляем так, можем потом переделать правила
            // todo уже добавляем карты
            // +1 карту перемещения каждый ход
            // +1-2 карты спутников
            _drawCards(_machine.Model.SideA.Hero, _machine.Model.SideA.Hero.HandLimit);
            _drawCards(_machine.Model.SideB.Hero, _machine.Model.SideB.Hero.HandLimit);

            _machine.SwitchState(typeof(FirstSideTurnState));
            return UniTask.CompletedTask;
        }

        public UniTask Exit()
        {
            return UniTask.CompletedTask;
        }

        private void _drawCards(BattleUnit unit, int count)
        {
            for (int i = 0; i < count; i++)
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

        private void _reshuffleDeck(BattleUnit unit)
        {
            unit.Deck.AddRange(unit.Discard);
            unit.Discard.Clear();
            unit.Deck.Shuffle();
        }
    }
}