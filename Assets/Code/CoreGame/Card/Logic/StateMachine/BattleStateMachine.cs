using System;
using System.Collections.Generic;
using Core.Data;
using Core.GameLoop;
using Core.Save;
using Core.ServiceLocator;
using Core.StateMachine;
using CoreGame.Card.Data;
using Cysharp.Threading.Tasks;

namespace CoreGame.Card.Logic.StateMachine
{
    public sealed class BattleStateMachine : StateMachine<IBattleState>, IInitializeListener, ISubscriber
    {
        public bool IsInitialized { get; set; }
        public ReactiveProperty<EBattlePhase> Phase { get; } = new(EBattlePhase.WaitingBattle);
  
        
        
        public BattleStateMachine()
        {
            states = new Dictionary<Type, IBattleState>()
            {
                { typeof(StartBattleState), new StartBattleState(this) },
                { typeof(StartTurnState), new StartTurnState(this) },
                { typeof(StartTurnState), new StartTurnState(this) },
                { typeof(FirstSideTurnState), new FirstSideTurnState(this) },
                { typeof(SecondSideTurnState), new SecondSideTurnState(this) },
                { typeof(TurnResolutionState), new TurnResolutionState(this) },
                { typeof(EndBattleState), new EndBattleState(this) },
            };
        }

        public UniTask Initialize()
        {
         
            return UniTask.CompletedTask;
        }

        public void Subscribe()
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe()
        {
            throw new NotImplementedException();
        }

        protected override UniTask setState(Type type)
        {
            Phase.Value = states[type].Phase;
            
            return base.setState(type);
        }

      
    }
}