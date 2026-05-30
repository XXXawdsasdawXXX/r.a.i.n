using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Core.StateMachine
{
    public sealed class GameStateMachine : StateMachine<IState>
    {
        private IState _currentState;

        public GameStateMachine()
        {
            states = new Dictionary<Type, IState>()
            {
                { typeof(BootstrapState), new BootstrapState(this) },
                { typeof(MainMenuState), new MainMenuState() },
                { typeof(CoreGameState) , new CoreGameState()}
            };
            
            setState(typeof(BootstrapState)).Forget();
        }
    }
}