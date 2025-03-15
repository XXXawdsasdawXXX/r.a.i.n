using Core.Network;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using Essential;
using FishNet.Object;

namespace Core.GameLoop
{
    public class MonoSpawnTracker : IService, IInitializeListener, ISubscriber
    {
        private GameEventDispatcher _gameEventDispatcher;
        private PlayerSpawner _playerSpawner;

        public UniTask GameInitialize()
        {
            _gameEventDispatcher = Container.Instance.GetService<GameEventDispatcher>();
            
            return UniTask.CompletedTask;
        }

        public UniTask Subscribe()
        {
            Essential.Mono.Started += OnMonoStarted;
            Essential.Mono.Destroyed -= OnMonoDestroyed;

            return UniTask.CompletedTask;
        }

        public void Unsubscribe()
        {
            Essential.Mono.Started -= OnMonoStarted;
            Essential.Mono.Destroyed -= OnMonoDestroyed;
        }

        private void OnMonoStarted(Essential.Mono obj)
        {
            if (obj.TryGetComponent(out IGameListener gameListener))
            {
                _gameEventDispatcher.AddSpawnableListener(gameListener);
            }
        }

        private void OnMonoDestroyed(Essential.Mono obj)
        {
            if (obj.TryGetComponent(out IGameListener gameListener))
            {
                /*if (obj is NetworkBehaviour networkBehaviour && !networkBehaviour.IsOwner)
                {
                    Log.Info("is not owner", obj);
                    return;
                }*/
                
                _gameEventDispatcher.RemoveSpawnableListener(gameListener);
            }
        }
    }
}