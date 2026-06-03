using System;
using System.Linq;
using System.Threading;
using CoreGame.Card.Data;
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
            
            _startTurnTimer().Forget();

            return UniTask.CompletedTask;
        }

        public UniTask Exit()
        {
            return UniTask.CompletedTask;
        }

        public bool TryPlayCard(int cardIndex, string targetId)
        {
            BattleUnit actor = _machine.Model.SideA.Hero;

            if (cardIndex < 0 || cardIndex >= actor.Hand.Count)
            {
                return false;
            }

            CardBattleState card = actor.Hand[cardIndex];
            int cost = card.GetEnergyCost(actor.Stats);

            if (actor.Energy < cost)
            {
                return false;
            }

            if (actor.Statuses.Any(s => s.Type == EStatusType.Stun))
            {
                return false;
            }

            if (actor.IsInArmorStance && card.Config.Type.HasFlag(ECardType.Attack))
            {
                return false;
            }

            BattleUnit target = _machine.FindUnit(targetId);
            _machine.Processor.ApplyCard(actor, card, target, _machine.Model);
            _spendCard(actor, card);

            return true;
        }

        public bool TryMoveLine(string unitId)
        {
            BattleUnit unit = _machine.FindUnit(unitId);
            if (unit == null) return false;
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
        
        private void _spendCard(BattleUnit actor, CardBattleState card)
        {
            if (card.Config.Charges > 0)
            {
                card.ChargesLeft--;
                actor.Hand.Remove(card);
                
                if (card.ChargesLeft > 0)
                {
                    actor.Discard.Add(card);
                }
            }
            else
            {
                actor.Hand.Remove(card);
                actor.Discard.Add(card);
            }
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
                    await UniTask.Delay(TimeSpan.FromSeconds(1));
                    
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