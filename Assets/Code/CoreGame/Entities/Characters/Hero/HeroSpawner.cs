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
        private readonly Dictionary<NetworkConnection, NetworkObject> _heroes = new();
        private UserProvider _userProvider;

        
        public override UniTask Initialize()
        {
            _userProvider = Container.Instance.GetService<UserProvider>();

            return base.Initialize();
        }

        public void Subscribe()
        {
            networkManager.SceneManager.OnClientLoadedStartScenes += _sceneManagerOnClientLoadedStartScenes;
            networkManager.ServerManager.OnRemoteConnectionState += _serverManagerOnRemoveConnection;
        }

        public void Unsubscribe()
        {
            networkManager.SceneManager.OnClientLoadedStartScenes -= _sceneManagerOnClientLoadedStartScenes;
            networkManager.ServerManager.OnRemoteConnectionState -= _serverManagerOnRemoveConnection;
        }
        
        protected override UniTask onSpawned(NetworkObject instance, NetworkConnection connection)
        {
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
        
        private void _sceneManagerOnClientLoadedStartScenes(NetworkConnection connection, bool isServer)
        {
            if (_heroes.ContainsKey(connection))
            {
                return;
            }

            spawn(Vector3.zero, connection);
        }

        [ObserversRpc]
        private void _initializeHeroComponents(NetworkObject instance)
        {
            Hero hero = instance.GetComponent<Hero>();
         
            hero.InitializeComponents();
            
            registerGameListener(getGameListeners(instance));
        }
        
        
        [TargetRpc]
        private void _setUserHero(NetworkConnection connection, NetworkObject instance)
        {
            UserProvider userProvider = Container.Instance.GetService<UserProvider>();
            userProvider.SetConnection(connection);
            userProvider.SetHero(instance);
        }

        private void _serverManagerOnRemoveConnection(NetworkConnection connection, RemoteConnectionStateArgs state)
        {
            _heroes.Remove(connection);
        }

        public bool TryGetHero(NetworkConnection connection, out Hero hero)
        {
            hero = null;

            if (connection == null || !_heroes.TryGetValue(connection, out NetworkObject heroObject))
            {
                return false;
            }

            hero = heroObject.GetComponent<Hero>();
            return hero != null;
        }

        public IEnumerable<(NetworkConnection connection, Hero hero)> GetAllHeroes()
        {
            foreach (KeyValuePair<NetworkConnection, NetworkObject> pair in _heroes)
            {
                Hero hero = pair.Value?.GetComponent<Hero>();
                if (hero != null)
                {
                    yield return (pair.Key, hero);
                }
            }
        }
    }
}