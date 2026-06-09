using System;
using System.Linq;
using System.Threading;
using CoreGame.Card.Data;
using CoreGame.Card.Logic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CoreGame.Card.Logic.StateMachine
{
    public class FirstSideTurnState : IBattleState, IAcceptPlayerInput
    {
        private readonly BattleStateMachine _machine;
        public EBattlePhase Phase => EBattlePhase.FirstSideTurn;
        public bool IsInitialized { get; set; }

        private CancellationTokenSource _cts;
        
        public FirstSideTurnState(BattleStateMachine machine)
        {
            _machine = machine;
        }
        
        public UniTask Initialize()
        {
            return UniTask.CompletedTask;
        }

        public UniTask Enter()
        {
            Debug.Log("enter to first side step");

            _machine.Model.SideA.Hero.Energy = _machine.Model.SideA.Hero.MaxEnergy;
            _machine.Model.TurnTimeRemaining.Value = BattleModel.MAX_TURN_TIME;
            
            _startTurnTimer().Forget();

            return UniTask.CompletedTask;
        }

        public UniTask Exit()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;

            return UniTask.CompletedTask;
        }

        public bool TryPlayCard(string cardId, string targetId)
        {
            return CardPlayRules.TryPlay(
                _machine.Model.SideA,
                _machine.Model.SideA.Hero,
                cardId,
                targetId,
                _machine.Model,
                _machine.Processor,
                _machine.FindUnit,
                CardSpendPolicy.Spend);
        }

        public bool TryMoveLine(string unitId)
        {
            BattleUnit unit = _machine.FindUnit(unitId);
            if (unit == null) return false;
            if (!ReferenceEquals(BattleGridRules.GetOwnerSide(_machine.Model, unit), _machine.Model.SideA)) return false;
            if (unit.Energy < unit.MoveLineCost) return false;
            if (unit.Statuses.Any(s => s.Type == EStatusType.Stun)) return false;

            unit.Energy -= unit.MoveLineCost;
            unit.Line = unit.Line == EBattleLine.Frontline
                ? EBattleLine.Backline
                : EBattleLine.Frontline;

            return true;
        }

        public void EndTurn()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            
            _machine.SwitchState(typeof(SecondSideTurnState));
        }
        
        private async UniTaskVoid _startTurnTimer()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            float endTime = UnityEngine.Time.time + BattleModel.MAX_TURN_TIME;

            try
            {
                while (true)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: _cts.Token);
                    
                    float remaining = endTime - UnityEngine.Time.time;

                    if (remaining <= 0)
                    {
                        _machine.Model.TurnTimeRemaining.Value = 0;
                        EndTurn();
                        return;
                    }

                    _machine.Model.TurnTimeRemaining.Value = remaining;
                }
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}