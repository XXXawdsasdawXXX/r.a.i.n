using Core.Interfaces;
using Cysharp.Threading.Tasks;

namespace Core.StateMachine
{
    public interface IState : IInitializable
    {
        UniTask Enter();

        UniTask Exit();
    }
}