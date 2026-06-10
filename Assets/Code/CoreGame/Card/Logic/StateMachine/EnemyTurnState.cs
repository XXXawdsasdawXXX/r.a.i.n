using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using CoreGame.Card.Data;
using CoreGame.Card.Logic.AI;
using Cysharp.Threading.Tasks;

namespace CoreGame.Card.Logic.StateMachine
{
    public class EnemyTurnState : IBattleState
    {
        private readonly BattleStateMachine _machine;
        public EBattlePhase Phase => EBattlePhase.EnemyTurn;
        public bool IsInitialized { get; set; }

        private const int AI_ACTION_DELAY_MS = 1000;
        private CancellationTokenSource _cts;

        public EnemyTurnState(BattleStateMachine machine)
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
            _processAI();
            return UniTask.CompletedTask;
        }

        public UniTask Exit()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            return UniTask.CompletedTask;
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
            BattleSide enemySide = _machine.Model.EnemySide;
            BattleUnit ai = enemySide?.Hero;
            if (ai?.AI == null)
            {
                EndTurn();
                return;
            }

            while (true)
            {
                AIAction action = ai.AI.SelectAction(enemySide, ai, _machine.Model);
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
                        enemySide,
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
    }
}
