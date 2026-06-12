using System;
using System.Collections.Generic;
using Core.GameLoop;
using Core.Network;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Transporting;
using UnityEngine;

namespace CoreGame.Entities.Characters.Hero
{
    public class HeroSpawner : NetworkPool, ISubscriber
    {
        public event Action<Hero> HeroSpawned;
        public event Action<Hero> HeroDespawned;

        private readonly Dictionary<NetworkConnection, NetworkObject> _heroes = new();
        
        private UserProvider _userProvider;

        
        public override UniTask Initialize()
        {
            _userProvider = Container.Instance.GetService<UserProvider>();

            return base.Initialize();
        }

        public void Subscribe()
        {
        }

        public void Unsubscribe()
        {
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            networkManager.SceneManager.OnClientLoadedStartScenes += _sceneManagerOnClientLoadedStartScenes;
            networkManager.ServerManager.OnRemoteConnectionState += _serverManagerOnRemoveConnection;

            _spawnMissingHeroes();
        }

        public override void OnStopServer()
        {
            networkManager.SceneManager.OnClientLoadedStartScenes -= _sceneManagerOnClientLoadedStartScenes;
            networkManager.ServerManager.OnRemoteConnectionState -= _serverManagerOnRemoveConnection;

            base.OnStopServer();
        }
        
        public bool TryGetConnectionForHeroObjectId(int heroObjectId, out NetworkConnection connection)
        {
            foreach (KeyValuePair<NetworkConnection, NetworkObject> entry in _heroes)
            {
                if (entry.Value != null && entry.Value.ObjectId == heroObjectId)
                {
                    connection = entry.Key;
                    return true;
                }
            }

            connection = null;
            return false;
        }

        public bool TryGetHeroObjectId(NetworkConnection connection, out int heroObjectId)
        {
            if (_heroes.TryGetValue(connection, out NetworkObject heroObject) && heroObject != null)
            {
                heroObjectId = heroObject.ObjectId;
                return true;
            }

            heroObjectId = 0;
            return false;
        }

        public bool TryGetHero(NetworkConnection connection, out Hero hero)
        {
            hero = null;
            if (!_heroes.TryGetValue(connection, out NetworkObject heroObject) || heroObject == null)
            {
                return false;
            }

            hero = heroObject.GetComponent<Hero>();
            return hero != null;
        }

        protected override UniTask onSpawned(NetworkObject instance, NetworkConnection connection)
        {
            if (!IsServerInitialized)
            {
                return UniTask.CompletedTask;
            }

            networkManager.SceneManager.AddOwnerToDefaultScene(instance);

            _heroes[connection] = instance;

            _initializeHeroComponents(instance);
            _setUserHero(connection, instance);

            return UniTask.CompletedTask;
        }

        protected override IGameListener[] getGameListeners(in NetworkObject networkBehaviour)
        {
            var hero = networkBehaviour.GetComponent<Hero>();
            List<IGameListener> listeners = new();

            foreach ((Type _, ICharacterComponent component) in hero.Components)
            {
                if (component is IGameListener listener)
                {
                    listeners.Add(listener);
                }
            }

            listeners.AddRange(hero.GetComponentsInChildren<IGameListener>(true));

            return listeners.ToArray();
        }

        [ObserversRpc]
        private void _initializeHeroComponents(NetworkObject instance)
        {
            Hero hero = instance.GetComponent<Hero>();

            hero.InitializeComponents();

            registerGameListener(getGameListeners(instance));
            
            HeroSpawned?.Invoke(hero);
        }

        [TargetRpc]
        private void _setUserHero(NetworkConnection connection, NetworkObject instance)
        {
            if (!connection.IsLocalClient)
            {
                return;
            }

            _userProvider.SetConnection(connection);
            _userProvider.SetHero(instance);
        }

        private void _sceneManagerOnClientLoadedStartScenes(NetworkConnection connection, bool isServer)
        {
            if (!isServer)
            {
                return;
            }

            _trySpawnHero(connection);
        }

        private void _spawnMissingHeroes()
        {
            if (!networkManager.IsServerStarted || !IsServerInitialized)
            {
                return;
            }

            foreach (NetworkConnection connection in networkManager.ServerManager.Clients.Values)
            {
                _trySpawnHero(connection);
            }

            NetworkConnection localConnection = networkManager.ClientManager.Connection;
            if (localConnection.IsValid)
            {
                _trySpawnHero(localConnection);
            }
        }

        private void _trySpawnHero(NetworkConnection connection)
        {
            if (!IsServerInitialized)
            {
                return;
            }

            if (!connection.IsValid || !connection.IsAuthenticated)
            {
                return;
            }

            if (!connection.LoadedStartScenes(true))
            {
                return;
            }

            if (_heroes.ContainsKey(connection))
            {
                return;
            }

            spawn(Vector3.zero, connection);
        }

        protected override void onDespawned(in NetworkObject instance, NetworkConnection connection)
        {
            if (instance.TryGetComponent(out Hero hero))
            {
                HeroDespawned?.Invoke(hero);
            }
            
            base.onDespawned(instance, connection);
        }

        private void _serverManagerOnRemoveConnection(NetworkConnection connection, RemoteConnectionStateArgs state)
        {
            if (state.ConnectionState != RemoteConnectionState.Stopped)
            {
                return;
            }

            _heroes.Remove(connection);
        }
    }
}