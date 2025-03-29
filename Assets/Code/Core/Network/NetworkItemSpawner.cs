using System.Collections.Generic;
using Core.ServiceLocator;
using FishNet.Managing;
using FishNet.Object;
using UnityEngine;

namespace Core.Network
{
    public sealed class NetworkItemSpawner : NetworkBehaviour, IService
    {
        [SerializeField] private NetworkManager _networkManager;
        [SerializeField] private NetworkObject _itemPrefab;

        private readonly List<NetworkObject> _itemInstances = new();

        [ServerRpc(RequireOwnership = false)]
        public void SpawnItem(Vector3 position)
        {
            if (_networkManager == null || !_networkManager.IsServerStarted)
            {
                Debug.LogWarning("NetworkManager is not active or server is not started.");
                return;
            }

            NetworkObject drop = _networkManager.GetPooledInstantiated(_itemPrefab, transform, true);

            if (drop == null)
            {
                Debug.LogWarning("Failed to get pooled instance of item.");
                return;
            }

            drop.transform.position = position;
            _networkManager.ServerManager.Spawn(drop);
            _itemInstances.Add(drop);
        }

        [ServerRpc(RequireOwnership = false)]
        public void DespawnItem()
        {
            if (_itemInstances.Count > 0)
            {
                _networkManager.ServerManager.Despawn(_itemInstances[^1]);

                _itemInstances.Remove(_itemInstances[^1]);
            }
        }
    }
}