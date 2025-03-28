using System.Collections.Generic;
using Core.GameLoop;
using Cysharp.Threading.Tasks;
using Essential;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Object;
using FishNet.Object;
using FishNet.Transporting;
using FishNet.Utility.Performance;
using Unity.Mathematics;
using UnityEngine;

namespace Core.Network
{
    public struct PlayerConnectionData
    {
        public NetworkConnection Connection;
        public NetworkObject HeroInstance;

        public bool IsThisConnectionID(int transportIndex)
        {
            return Connection.ClientId == transportIndex;
        }
    }
    
    public class HeroPool : NetworkBehaviour, IInitializeListener, ISubscriber
    {
        private readonly Dictionary<NetworkConnection, NetworkObject> _heroes = new();
        
        [SerializeField] private NetworkObject _heroPrefab;
        [SerializeField] private PrefabObjects _prefabObjects;

        [SerializeField] private Color _logColor;
        
        public UniTask GameInitialize()
        {
            return UniTask.CompletedTask;
        }

        public UniTask Subscribe()
        {
            NetworkManager.SceneManager.OnClientLoadedStartScenes += SceneManager_OnClientLoadedStartScenes;
            NetworkManager.ClientManager.OnClientConnectionState += ClientManagerOnClientConnectionState;
            NetworkManager.ServerManager.OnRemoteConnectionState += ServerManager_OnRemoveConnection;

            return UniTask.CompletedTask;
        }

        private void ServerManager_OnRemoveConnection(NetworkConnection arg1, RemoteConnectionStateArgs arg2)
        {
                Log.Info($"on remove connection {_heroes?.ContainsKey(arg1)} {arg2.ConnectionState}" +
                         $"\n nc id {arg1.ClientId} rcsa {arg2.ConnectionId}", _logColor, this);
     
        }

        public void Unsubscribe()
        {
            NetworkManager.ClientManager.OnClientConnectionState -= ClientManagerOnClientConnectionState;
            NetworkManager.SceneManager.OnClientLoadedStartScenes -= SceneManager_OnClientLoadedStartScenes;
        }

        private void SceneManager_OnClientLoadedStartScenes(NetworkConnection arg1, bool arg2)
        {
            if (_heroes.ContainsKey(arg1))
            {
                return;
            }

            NetworkObject nob = NetworkManager.GetPooledInstantiated(_heroPrefab, Vector3.zero, quaternion.identity, true);
            NetworkManager.ServerManager.Spawn(nob, arg1);
            nob.gameObject.name += "_0";
          
            Log.Info(
                $"on client start scene -> PrefabId: {_heroPrefab.PrefabId}, CollectionId: {_heroPrefab.SpawnableCollectionId} ",
                _logColor, this);
            
            _heroes.Add(arg1, nob);
        }

        private async void ClientManagerOnClientConnectionState(ClientConnectionStateArgs obj)
        {
            /*if (obj.ConnectionState == LocalConnectionState.Stopped)
            {
      
                Log.Info(
                    $"on client connection state -> PrefabId: {_heroPrefab.PrefabId}, CollectionId: {_heroPrefab.SpawnableCollectionId} ",
                    _logColor, this);
                _prefabObjects.AddObject(_heroPrefab);
                NetworkObject networkObject = RetrieveObject(_heroPrefab.PrefabId, _prefabObjects.CollectionId,
                    ObjectPoolRetrieveOption.MakeActive);

                networkObject.gameObject.name += "_1";

                UniTask.Delay(1000);
                networkObject.gameObject.SetActive(true);
            }*/
        }

        /*
        /// <summary>
        /// Достает объект из пула или создает новый.
        /// </summary>
        public override NetworkObject RetrieveObject(int prefabId, ushort collectionId, 
            ObjectPoolRetrieveOption options, Transform parent = null, 
            Vector3? position = null, Quaternion? rotation = null, 
            Vector3? scale = null, bool asServer = true)
        {
            NetworkObject hero = base.RetrieveObject(prefabId, collectionId, options, parent, position, rotation, scale, asServer);
        
            if (hero == null)
            {
                hero = Instantiate(_heroPrefab, position ?? Vector3.zero, rotation ?? Quaternion.identity);
            }

            hero.transform.SetParent(parent);
            hero.gameObject.SetActive(true);

            return hero;
        }
        */
    }
}