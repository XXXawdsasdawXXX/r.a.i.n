using Core.StateMachine;

namespace Core.GameLoop
{
    internal sealed class Bootstrap : Essential.Mono
    {
        public void Awake()
        {
            GameStateMachine gameStateMachine = new GameStateMachine();
        }
    }
}