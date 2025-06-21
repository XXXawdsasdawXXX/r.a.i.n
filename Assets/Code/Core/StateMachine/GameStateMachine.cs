using System;
using System.Collections.Generic;
using Core.GameLoop;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;

namespace Core.StateMachine
{
    public class GameStateMachine : IService
    {
        private readonly Dictionary<Type, IState> _states;
        
        private IState _currentState;

        public GameStateMachine()
        {
            _states = new Dictionary<Type, IState>()
            {
                { typeof(BootstrapState), new BootstrapState(this) },
                { typeof(MainMenuState), new MainMenuState() },
                { typeof(CoreGameState) , new CoreGameState()}
            };
            
            _setState(typeof(BootstrapState)).Forget();
        }

        public async void SwitchState(Type type)
        {
            if (_currentState != null)
            {
                await _currentState.Exit();
            }

            await _setState(type);
        }

        private async UniTask _setState(Type type)
        {
            _currentState = _states[type];

            if (!_currentState.IsInitialized)
            {
                await _currentState.Initialize();

                _currentState.IsInitialized = true;
            }
            
            await _states[type].Enter();
        }
        
    }
}