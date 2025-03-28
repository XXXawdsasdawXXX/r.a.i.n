using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Transporting;
using UnityEngine;
using Essential;
using Core.GameLoop;
using Core.ServiceLocator;

namespace Core.Network
{
    public class HeroPool : NetworkBehaviour, IInitializeListener, ISubscriber
    {
        private readonly Dictionary<NetworkConnection, NetworkObject> _heroes = new();
        private readonly Color _logColor = new(0.3f, 0.8f, 0.2f);

        [SerializeField] private NetworkObject _heroPrefab;
        
        private NetworkManager _networkManager;
        
        public UniTask GameInitialize()
        {
            _networkManager = Container.Instance.Network;
            
            return UniTask.CompletedTask;
        }

        public UniTask Subscribe()
        {
            _networkManager.SceneManager.OnClientLoadedStartScenes += _sceneManagerOnClientLoadedStartScenes;
            _networkManager.ServerManager.OnRemoteConnectionState += _serverManagerOnRemoveConnection;

            return UniTask.CompletedTask;
        }

        public void Unsubscribe()
        {
            _networkManager.SceneManager.OnClientLoadedStartScenes -= _sceneManagerOnClientLoadedStartScenes;
        }

        private void _sceneManagerOnClientLoadedStartScenes(NetworkConnection connection, bool isServer)
        {
            if (_heroes.ContainsKey(connection))
            {
                return;
            }

            NetworkObject pooledInstantiated = _networkManager.GetPooledInstantiated(
                _heroPrefab, 
                Vector3.zero, 
                Quaternion.identity, 
                true);
            
            _networkManager.ServerManager.Spawn(pooledInstantiated, connection);

            Log.Info($"on client start scene -> " +
                     $"PrefabId: {_heroPrefab.PrefabId}, " +
                     $"CollectionId: {_heroPrefab.SpawnableCollectionId}", _logColor, this);
            
            _heroes.Add(connection, pooledInstantiated);
        }

        private void _serverManagerOnRemoveConnection(NetworkConnection connection, RemoteConnectionStateArgs state)
        {
            Log.Info($"on remove connection {_heroes?.ContainsKey(connection)} {state.ConnectionState}" +
                     $"\n nc id {connection.ClientId} state {state.ConnectionId}", _logColor, this);
        }
    }
}