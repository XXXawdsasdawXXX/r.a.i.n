using System;
using System.Collections.Generic;
using Core.ServiceLocator;
using FishNet.Connection;
using FishNet.Object;

namespace Core.Network
{
    public class UserProvider : IService
    {
        public event Action HeroCreated; 
        public NetworkConnection Connection { get; private set; }
        public NetworkObject Hero { get; private set; }
        public string Id { get; private set; }

        private readonly Dictionary<Type, object> _heroComponents = new();

        
        public void SetConnection(NetworkConnection connection)
        {
            Connection = connection;
            Id = Connection.ClientId.ToString();
        }

        public void SetHero(NetworkObject hero)
        {
            Hero = hero;
            
            _heroComponents.Clear();
            
            HeroCreated?.Invoke();
        }

        public T GetHeroComponent<T>() where T: class
        {
            Type type = typeof(T);
            
            if (_heroComponents.ContainsKey(type))
            {
                return _heroComponents[type] as T;
            }

            T component = Hero.GetComponentInChildren<T>(true);
            
            _heroComponents.Add(type, component);
            
            return component;
        }
    }
}