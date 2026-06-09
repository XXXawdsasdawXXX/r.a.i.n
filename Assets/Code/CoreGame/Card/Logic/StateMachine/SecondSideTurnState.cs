using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using CoreGame.Card.Data;
using CoreGame.Card.Logic.AI;
using Cysharp.Threading.Tasks;

namespace CoreGame.Card.Logic.StateMachine
{
    public class SecondSideTurnState : IBattleState, IAcceptPlayerInput
    {
        private readonly BattleStateMachine _machine;
        public EBattlePhase Phase => EBattlePhase.SecondSideTurn;
        public bool IsInitialized { get; set; }
        
        private const int AI_ACTION_DELAY_MS = 1000;

        private CancellationTokenSource _cts;


        public SecondSideTurnState(BattleStateMachine machine)
        {
            _machine = machine;
        }

        public UniTask Initialize()
        {
            return UniTask.CompletedTask;
        }

        public UniTask Enter()
        {
            _machine.Model.TurnTimeRemaining.Value = BattleModel.MAX_TURN_TIME;

            if (_machine.Model.SideB.Hero.AI != null)
            {
                _processAI();
            }
            else
            {
                _startTurnTimer().Forget();
            }

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
                _machine.Model.SideB,
                _machine.Model.SideB.Hero,
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

            if (unit == null)
            {
                return false;
            }
            
            if (!ReferenceEquals(BattleGridRules.GetOwnerSide(_machine.Model, unit), _machine.Model.SideB))
            {
                return false;
            }

            if (unit.Energy < unit.MoveLineCost)
            {
                return false;
            }

            if (unit.Statuses.Any(s => s.Type == EStatusType.Stun))
            {
                return false;
            }

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

            _machine.SwitchState(typeof(TurnResolutionState));
        }

        private void _processAI()
        {
            _processAIAsync().Forget();
        }

        private async UniTaskVoid _processAIAsync()
        {
            if (_machine.Model.Mode is not (EBattleMode.PvE or EBattleMode.CoOpPvE))
            {
                _startTurnTimer().Forget();
                return;
            }

            BattleUnit ai = _machine.Model.SideB.Hero;
            BattleSide aiSide = _machine.Model.SideB;

            while (true)
            {
                AIAction action = ai.AI.SelectAction(aiSide, ai, _machine.Model);
                if (action?.Card == null)
                {
                    break;
                }

                BattleUnit target = action.Target;
                if (target == null)
                {
                    break;
                }

                if (!CardPlayRules.TryPlay(
                        aiSide,
                        ai,
                        action.Card.InstanceId,
                        target.UnitId,
                        _machine.Model,
                        _machine.Processor,
                        _machine.FindUnit,
                        CardSpendPolicy.Spend))
                {
                    break;
                }

                _machine.NotifyCardPlayed(new BattleCardPlayedEvent
                {
                    ActorUnitId = ai.UnitId,
                    TargetUnitId = target.UnitId,
                    Card = action.Card,
                    EffectTypes = _collectEffectTypes(action.Card)
                });

                await UniTask.Delay(AI_ACTION_DELAY_MS);
            }

            EndTurn();
        }

        private static List<EEffectType> _collectEffectTypes(CardBattleState card)
        {
            if (card?.Config?.Effects == null || card.Config.Effects.Count == 0)
            {
                return new List<EEffectType>();
            }

            return card.Config.Effects
                .Where(effect => effect != null)
                .Select(effect => effect.Type)
                .Distinct()
                .ToList();
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