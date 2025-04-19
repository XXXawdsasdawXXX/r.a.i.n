using System;
using System.Collections.Generic;
using System.Linq;
using Core.GameLoop;
using UnityEngine;

namespace Core.ServiceLocator
{
    public sealed class Container 
    {
        public static Container Instance { get; private set; }
        internal ContextEntities Context { get; }
        
        private readonly List<ScriptableObject> _configs = new();
        
        internal Container(ContextEntities projectContext)
        {
            Context = projectContext;
            
            Instance = this;
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

        public T GetService<T>() where T : class, IService
        {
            Type type = typeof(T);
            
            ContextEntities lowContext = Context;
           
            while (lowContext.Child != null)
            {
                lowContext = lowContext.Child;
            }

            while (lowContext != null)
            {
                if (lowContext.Services.TryGetValue(type, out IService sceneService))
                {
                    return sceneService as T;
                }

                lowContext = lowContext.Parent;
            }
            
            return default;
        }

        public T GetView<T>() where T : MonoView
        {
            Type type = typeof(T);
      
            ContextEntities lowContext = Context.Child;
           
            while (lowContext.Child != null)
            {
                lowContext = lowContext.Child;
            }

            while (lowContext != null)
            {
                if (lowContext.Views.TryGetValue(type, out MonoView monoView))
                {
                    return monoView as T;
                }

                lowContext = lowContext.Parent;
            }

            return default;
        }

        public void AddConfig(ScriptableObject config)
        {
            _configs.Add(config);
        }
        
        private List<T> _getContainerComponents<T>()
        {
            List<T> list = new();

            ContextEntities lowContext = Context;
           
            while (lowContext.Child != null)
            {
                lowContext = lowContext.Child;
            }

            while (lowContext != null)
            {
                list.AddRange(lowContext.Services.OfType<T>());
                list.AddRange(lowContext.Mono.OfType<T>());
                list.AddRange(lowContext.Views.OfType<T>());
                list.AddRange(lowContext.Objects.OfType<T>());
                
                lowContext = lowContext.Parent;
            }

            return list;
        }
    }
}