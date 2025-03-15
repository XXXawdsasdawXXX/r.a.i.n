using Core.Network;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using Essential;
using FishNet;
using FishNet.Connection;
using FishNet.Managing.Server;
using FishNet.Object;
using FishNet.Transporting;
using UnityEngine;

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
            InstanceFinder.ServerManager.OnRemoteConnectionState += ServerManagerOnOnRemoteConnectionState;

            return UniTask.CompletedTask;
        }

        private void ServerManagerOnOnRemoteConnectionState(NetworkConnection arg1, RemoteConnectionStateArgs arg2)
        {
            arg1.OnObjectRemoved += OnMonoDestroyed;
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
                
                Log.Info("destroyed", Color.cyan, obj);
                
                _gameEventDispatcher.RemoveSpawnableListener(gameListener);
            }
        }
    }
}