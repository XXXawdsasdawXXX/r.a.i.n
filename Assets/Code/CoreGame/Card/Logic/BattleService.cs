using System;
using Core.GameLoop;
using Core.Save;
using Core.ServiceLocator;
using CoreGame.Card.Data;
using CoreGame.Card.Logic.StateMachine;
using Cysharp.Threading.Tasks;
using Essential;
using UnityEngine;

namespace CoreGame.Card.Logic
{
    public class BattleService : IService, IInitializeListener
    {
        public bool IsInitialized { get; set; }
        public event Action<BattleModel> BattleStarted;
        public event Action<BattleModel> TurnStarted;
        public event Action<BattleModel> BattleFinished;
        public event Action<BattleModel> CardPlayed;
        
        private BattleStateMachine _machine;

        
        public UniTask Initialize()
        {
            _machine = Container.Instance.GetService<BattleStateMachine>();
            
            return UniTask.CompletedTask;
        }

        public void StartBattle(HeroModel attacker, HeroModel defender, EBattleMode mode = EBattleMode.PvE)
        {
            _machine.StartBattle(attacker, defender, mode);
            _machine.Model.Phase.SubscribeProperty(_onPhaseChanged);
            BattleStarted?.Invoke(_machine.Model);
            Log.Info(this, "Start battle");            
        }

        public bool TryPlayCard(int cardIndex, string targetId)
        {
            if (_machine.CurrentState is IAcceptPlayerInput acceptPlayerInput)
            {
                if (acceptPlayerInput.TryPlayCard(cardIndex, targetId))
                {
                    CardPlayed?.Invoke(_machine.Model);
                    return true;
                }
            }
            
            return false;
        }

        public bool TryMoveLine(string unitId)
        {
            return (_machine.CurrentState as IAcceptPlayerInput)?.TryMoveLine(unitId) ?? false;
        }

        public void EndTurn()
        {
            (_machine.CurrentState as IAcceptPlayerInput)?.EndTurn();
        }

        public BattleUnit FindUnit(string unitId)
        {
            return _machine.FindUnit(unitId);
        }

        private void _onPhaseChanged(EBattlePhase phase)
        {
            switch (phase)
            {
                case EBattlePhase.WaitingBattle:
                    break;
                case EBattlePhase.StartBattle:
                    BattleStarted?.Invoke(_machine.Model);
                    break;
                case EBattlePhase.StartTurn:
                    break;
                case EBattlePhase.FirstSideTurn:
                    TurnStarted?.Invoke(_machine.Model);
                    break;
                case EBattlePhase.SecondSideTurn:
                    TurnStarted?.Invoke(_machine.Model);
                    break;
                case EBattlePhase.Resolution:
                    break;
                case EBattlePhase.Finished:
                    _machine.Model.Phase.UnsubscribeProperty(_onPhaseChanged);
                    BattleFinished?.Invoke(_machine.Model);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(phase), phase, null);
            }
        }
    }
}