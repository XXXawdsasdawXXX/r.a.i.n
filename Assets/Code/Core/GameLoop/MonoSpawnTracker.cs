using System.Collections.Generic;
using Core.Scenes;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using Essential;
using FishNet;
using Plugins.FishNet.Runtime.Managing.Object;
using UnityEngine;
using UnityEngine.Scripting;

namespace Core.GameLoop
{
    [Preserve]
    internal sealed class MonoSpawnTracker : IService, IInitializeListener, ISubscriber
    {
        public bool IsInitialized { get; set; }

        private GameEventDispatcher _gameEventDispatcher;
        private PlayerSpawner _playerSpawner;

        private readonly HashSet<Essential.Mono> _observeMono = new();

        public UniTask Initialize()
        {
            _gameEventDispatcher = Container.Instance.GetService<GameEventDispatcher>();

            return UniTask.CompletedTask;
        }
        
        public void Subscribe()
        {
            /*Essential.Mono.Started += _onMonoStarted;
            Essential.Mono.Destroyed += _onMonoDestroyed;

            InstanceFinder.ServerManager.OnSpawn += _onMonoStarted;
            InstanceFinder.ServerManager.OnDespawn += _onMonoDestroyed;*/
        }
        
        public void Unsubscribe()
        {
            /*Essential.Mono.Started -= _onMonoStarted;
            Essential.Mono.Destroyed -= _onMonoDestroyed;

            InstanceFinder.ServerManager.OnSpawn -= _onMonoStarted;
            InstanceFinder.ServerManager.OnDespawn -= _onMonoDestroyed;*/
        }
        
        /*private void _onMonoStarted(Essential.Mono obj)
        {
            if (obj is IGameListener gameListener && _observeMono.Add(obj))
            {
                Log.Info(this, $"Mono started {obj.GetType().Name} add to collection");
                _gameEventDispatcher.InitializeListener(gameListener);
            }
        }

        private void _onMonoDestroyed(Essential.Mono obj)
        {
            if (obj is IGameListener gameListener && _observeMono.Remove(obj))
            {
                _gameEventDispatcher.RemoveListener(gameListener);
            }
        }*/
    }
}