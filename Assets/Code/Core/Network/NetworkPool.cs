using System;
using System.Collections.Generic;
using Core.GameLoop;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using Essential;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using UnityEngine;
using UnityEngine.Serialization;

namespace Core.Network
{
    public abstract class NetworkPool : NetworkBehaviour, IService, IInitializeListener
    {
        public event Action<NetworkObject> Spawned;
        public event Action<NetworkObject> Despawned;

        public bool IsInitialized { get; set; }
        protected NetworkManager networkManager { get; private set; }
        
        [SerializeField] protected NetworkObject prefab;
      
        private readonly List<NetworkObject> _instances = new();
        
        private GameEventDispatcher _gameEventDispatcher;
        
        
        public virtual UniTask Initialize()
        {
            _gameEventDispatcher = Container.Instance.GetService<GameEventDispatcher>();

            networkManager = InstanceFinder.NetworkManager;

            return UniTask.CompletedTask;
        }

        [ServerRpc(RequireOwnership = false)]
        public virtual void ServerDespawn(NetworkObject instance, NetworkConnection connection = null)
        {
            if (_instances.Contains(instance))
            {
                _gameEventDispatcher.RemoveListeners(getGameListeners(instance));

                networkManager.ServerManager.Despawn(instance);

                _instances.Remove(instance);

                onDespawned(instance, connection);

                Despawned?.Invoke(instance);
            }
            else
            {
                Log.Error(this, $"Pool has not the object with name'{instance.name}'");
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void ServerSpawn(Vector3 position, NetworkConnection connection = null)
        {
            spawn(position, connection);
        }

        protected async void spawn(Vector3 position, NetworkConnection connection = null)
        {
            NetworkObject instance = networkManager.GetPooledInstantiated(prefab, transform, true);

            instance.transform.position = position;

            networkManager.ServerManager.Spawn(instance, connection);

            await onSpawned(instance, connection);

            _instances.Add(instance);
            
            Spawned?.Invoke(instance);
        }

        protected virtual UniTask onSpawned(NetworkObject instance, NetworkConnection connection)
        {
            return UniTask.CompletedTask;
        }

        protected virtual void onDespawned(in NetworkObject instance, NetworkConnection connection)
        {
        }

        protected virtual IGameListener[] getGameListeners(in NetworkObject networkBehaviour)
        {
            return networkBehaviour.GetComponentsInChildren<IGameListener>(true);
        }

        protected void registerGameListener(IGameListener[] listeners)
        {
            _gameEventDispatcher.InitializeListeners(listeners);
        }
    }
}