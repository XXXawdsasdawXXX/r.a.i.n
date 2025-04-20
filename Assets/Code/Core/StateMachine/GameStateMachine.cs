using System;
using System.Collections.Generic;
using Core.GameLoop;
using Core.ServiceLocator;

namespace Core.StateMachine
{
    public class GameStateMachine : IService, IExitListener
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

                _currentState.IsInitialized = true;
            }
            
            await _states[type].Enter();
        }

        public void GameExit()
        {
            // _currentState.Exit();
        }
    }
}