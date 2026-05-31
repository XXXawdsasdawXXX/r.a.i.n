using System;
using System.Collections.Generic;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;

namespace Core.StateMachine
{
    public abstract class StateMachine<TState> : IService where TState : IState
    {
        protected Dictionary<Type, TState> states;
        
        protected TState currentState;
        
        public async void SwitchState(Type type)
        {
            if (currentState != null)
            {
                await currentState.Exit();
            }

            await setState(type);
        }

        protected virtual async UniTask setState(Type type)
        {
            currentState = states[type];

            if (!currentState.IsInitialized)
            {
                await currentState.Initialize();

                currentState.IsInitialized = true;
            }
            
            await states[type].Enter();
        }
    }
}