using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Core.GameLoop;
using Essential;
using FishNet.Managing;
using UnityEngine;

namespace Core.ServiceLocator
{
    public sealed class Container : MonoBehaviour
    {
        public static Container Instance { get; private set; }

        [field: SerializeField] public NetworkManager Network { get; private set; }
        
        [SerializeField] private List<ScriptableObject> _configs;

        private Essential.Mono[] _allObjects;
        private List<IService> _services = new();
        private List<IMono> _mono = new();

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
            }
            
            DontDestroyOnLoad(gameObject);
         
            Instance = this;
            
            _allObjects = FindObjectsOfType<Essential.Mono>(true);
            
            _initializeTypedList(ref _services);
            _initializeTypedList(ref _mono);
        }

        public List<IGameListener> GetGameListeners()
        {
            return _getContainerComponents<IGameListener>();
        }

        public T GetConfig<T>() where T : ScriptableObject
        {
            foreach (ScriptableObject scriptableObject in _configs)
            {
                if (scriptableObject is T findConfig)
                {
                    return findConfig;
                }
            }

            return null;
        }

        public T GetService<T>() where T : IService
        {
            foreach (IService service in _services)
            {
                if (service is T findService)
                {
                    return findService;
                }
            }

            return default;
        }

        private void _initializeTypedList<T>(ref List<T> list)
        {
            string[] targetAssemblies = { "Core", "Game", "UI" };

            List<Type> serviceTypes = new List<Type>();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (targetAssemblies.Contains(assembly.GetName().Name))
                {
                    serviceTypes.AddRange(assembly.GetTypes().Where(t =>
                        typeof(T).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract &&
                        !typeof(MonoBehaviour).IsAssignableFrom(t)));
                }
            }
            
            foreach (Type serviceType in serviceTypes)
            {
                if (Activator.CreateInstance(serviceType) is T service)
                {
                    list.Add(service);
                }
            }

            T[] typedMono = _allObjects.OfType<T>().ToArray();
            
            if (typedMono.Any())
            {
                list.AddRange(typedMono);
            }
            
            Log.Info($"Initialize instances. Type of {typeof(T).Name}. Count {list.Count} ", this);
        }

        private List<T> _getContainerComponents<T>()
        {
            List<T> list = new();

            list.AddRange(_services.OfType<T>().ToList());
            list.AddRange(_mono.OfType<T>().ToList());

            IEnumerable<T> mbListeners = _allObjects.OfType<T>();
            foreach (T mbListener in mbListeners)
            {
                if (!list.Contains(mbListener))
                {
                    list.Add(mbListener);
                }
            }

            return list;
        }
    }
}