using System;
using System.Collections.Generic;
using System.Linq;
using Core.GameLoop;
using Core.Save;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using FishNet;
using FishNet.Broadcast;
using Channel = FishNet.Transporting.Channel;

namespace CoreGame.Harvest
{
    public class ResourceStorage : IService, ILoadListener, ISubscriber
    {
        private GameModel _model;

        public struct ResourceBroadcast : IBroadcast
        {
            public Dictionary<EResource, int> Collection;
        
            public ResourceBroadcast(Dictionary<EResource, int> collection)
            {
                Collection = collection;
            }
        }

        public event Action<KeyValuePair<EResource, int>> ValueChanged;
        
        public event Action<Dictionary<EResource, int>> CollectionChanged;
        
        public Dictionary<EResource, int> Collection { get; private set; }

        
        public UniTask GameLoad(GameModel model)
        {
            _model = model;
            
            Dictionary<int, int> savedCollection = model.World.ResourcesStorage;

            Collection = new Dictionary<EResource, int>();

            EResource[] resourceTypes = Enum.GetValues(typeof(EResource)).Cast<EResource>().ToArray();
            
            foreach ((int resourceType, int amount) in savedCollection)
            {
                Collection.Add((EResource)resourceType, amount);
            }

            if (savedCollection.Count() < resourceTypes.Length - 1)
            {
                for (int i = 1; i < resourceTypes.Length; i++)
                {
                    if(!Collection.ContainsKey(resourceTypes[i]))
                    {
                        Collection.Add(resourceTypes[i], 0);
                    }
                }
            }
    
            CollectionChanged?.Invoke(Collection);
    
            return UniTask.CompletedTask;
        }

        public void Subscribe()
        {
            InstanceFinder.ClientManager.RegisterBroadcast<ResourceBroadcast>(_onServerSendChanged);
        }

        public void Unsubscribe()
        {
            InstanceFinder.ClientManager.UnregisterBroadcast<ResourceBroadcast>(_onServerSendChanged);
        }
        
        public void Add(EResource resource, int value)
        {
            if (!Collection.ContainsKey(resource))
            {
                Collection.Add(resource, 0);
            }
            
            Collection[resource] += value;

            _model.World.ResourcesStorage[(int)resource] = Collection[resource];   
            
            InstanceFinder.ServerManager.Broadcast(new ResourceBroadcast(Collection));
            
            ValueChanged?.Invoke(new KeyValuePair<EResource, int>(resource, Collection[resource]));
        }

        public void Spend(EResource resource, int value)
        {
            if (!Collection.ContainsKey(resource))
            {
                Collection.Add(resource, 0);
            }
            
            Collection[resource] -= value;
            
            _model.World.ResourcesStorage[(int)resource] = Collection[resource];
            
            InstanceFinder.ServerManager.Broadcast(new ResourceBroadcast(Collection));
            
            ValueChanged?.Invoke(new KeyValuePair<EResource, int>(resource, Collection[resource]));
        }
        
        private void _onServerSendChanged(ResourceBroadcast arg1, Channel arg2)
        {
            Collection = arg1.Collection;
        }
    }
}