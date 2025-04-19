using Cysharp.Threading.Tasks;

namespace Core.StateMachine
{
    public class CoreGameState : IState
    {
        public bool IsInitialized { get; private set; }

        public UniTask Initialize()
        {
            IsInitialized = true;
            
            
            return UniTask.CompletedTask;
        }

        public UniTask Enter()
        {
            return UniTask.CompletedTask;
        }

        public UniTask Exit()
        {
            return UniTask.CompletedTask;
        }
    }
}