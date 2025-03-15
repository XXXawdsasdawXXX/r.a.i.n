using System.Collections.Generic;
using Core.Network;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using FishNet;
using FishNet.Connection;
using FishNet.Transporting;
using UnityEngine.Scripting;

namespace Core.GameLoop
{
    [Preserve]
    public class MonoSpawnTracker : IService, IInitializeListener, ISubscriber
    {
        private GameEventDispatcher _gameEventDispatcher;
        private PlayerSpawner _playerSpawner;

        private readonly HashSet<Essential.Mono> _observeMono  = new();

        public UniTask GameInitialize()
        {
            _gameEventDispatcher = Container.Instance.GetService<GameEventDispatcher>();
            
            return UniTask.CompletedTask;
        }

        public UniTask Subscribe()
        {
            Essential.Mono.Started += OnMonoStarted;
            Essential.Mono.Destroyed -= OnMonoDestroyed;
            InstanceFinder.ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;

            return UniTask.CompletedTask;
        }

        public void Unsubscribe()
        {
            Essential.Mono.Started -= OnMonoStarted;
            Essential.Mono.Destroyed -= OnMonoDestroyed;
            InstanceFinder.ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
        }

        private void OnRemoteConnectionState(NetworkConnection connection, RemoteConnectionStateArgs args)
        {
            connection.OnObjectRemoved += OnMonoDestroyed;
        }

        private void OnMonoStarted(Essential.Mono obj)
        {
            if (obj.TryGetComponent(out IGameListener gameListener) && !_observeMono.Contains(obj))
            {
                _observeMono.Add(obj);
                _gameEventDispatcher.AddSpawnableListener(gameListener);
            }
        }

        private void OnMonoDestroyed(Essential.Mono obj)
        {
            if (obj.TryGetComponent(out IGameListener gameListener) && _observeMono.Contains(obj))
            {
                _observeMono.Remove(obj);
                _gameEventDispatcher.RemoveSpawnableListener(gameListener);
            }
        }
    }
}