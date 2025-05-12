using Core.Interfaces;
using Core.Save;
using Cysharp.Threading.Tasks;

namespace Core.GameLoop
{
    public interface IGameListener
    {
    }

    public interface ISubscriber : IGameListener
    {
        void Subscribe();
        void Unsubscribe();
    }
    
    public  interface IInitializeListener : IGameListener, IInitializable
    {
    }

    public interface ILoadListener : IGameListener
    {
        UniTask GameLoad(GameModel model);
    }

    public interface IStartListener : IGameListener
    {
        UniTask GameStart();
    }

    public interface IRuntimeListener : IGameListener
    {
        string RuntimeListenerName { get; }
    }
    public interface IUpdateListener : IRuntimeListener
    {
        void GameUpdate(float deltaTime);
    }
    
    public interface IFixedUpdateListener : IRuntimeListener
    {
        void GameFixedUpdate(float fixedDeltaTime);
    }
    
    public interface IExitListener : IGameListener
    {
        void GameExit();
    }
}