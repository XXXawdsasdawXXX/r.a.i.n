using System;
using System.Collections.Generic;
using Core.ServiceLocator;

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

            SwitchState(typeof(BootstrapState));
        }

        public async void SwitchState(Type type)
        {
            if (_currentState != null)
            {
                await _currentState.Exit();
            }
            
            _currentState = _states[type];

            if (!_currentState.IsInitialized)
            {
                await _currentState.Initialize();
            }
            
            _states[type].Enter();
        }
    }
}