using System;
using System.Collections.Generic;
using Essential;
using Object = UnityEngine.Object;

namespace Core.ServiceLocator
{
    internal static class ContextBuilder
    {
        internal static ContextEntities BuildContext()
        {
            ContextEntities context = new()
            {
                Services = new Dictionary<Type, IService>(),
                Views = new Dictionary<Type, MonoView>(),
                Mono = new Dictionary<Type, IMono>(),
                Objects = Object.FindObjectsOfType<Essential.Mono>()
            };

            foreach (Essential.Mono mono in context.Objects)
            {
                _registry(mono, context);
            }

            return context;
        }

        internal static ContextEntities BuildContext(Type[] allTypes)
        {
            ContextEntities context = new()
            {
                Services = new Dictionary<Type, IService>(),
                Views = new Dictionary<Type, MonoView>(),
                Mono = new Dictionary<Type, IMono>(),
                Objects = Object.FindObjectsOfType<Essential.Mono>()
            };

            foreach (Essential.Mono mono in context.Objects)
            {
                _registry(mono, context);
            }

            if (allTypes != null)
            {
                foreach (Type type in allTypes)
                {
                    _createAndRegistry(type, context);
                }
            }

            return context;
        }

        private static void _registry(Essential.Mono mono, ContextEntities context)
        {
            Type type = mono.GetType();

            if (mono is IMono monoInterface && !context.Mono.ContainsKey(type))
            {
                context.Mono[type] = monoInterface;
            }

            else if (mono is IService service && !context.Services.ContainsKey(type))
            {
                context.Services[type] = service;
            }

            else if (mono is MonoView view && !context.Views.ContainsKey(type))
            {
                context.Views[type] = view;
            }

            Log.Info($"[ContextBuilder] _registry {type.Name}");
        }

        private static void _createAndRegistry(Type type, ContextEntities context)
        {
            if (typeof(IService).IsAssignableFrom(type) && !typeof(Essential.Mono).IsAssignableFrom(type)
                                                        && !type.IsAbstract)
            {
                IService instance = (IService)Activator.CreateInstance(type);
                context.Services[type] = instance;
            }

            else if (typeof(IMono).IsAssignableFrom(type) && !typeof(Essential.Mono).IsAssignableFrom(type)
                                                          && !type.IsAbstract)
            {
                IMono instance = (IMono)Activator.CreateInstance(type);
                context.Mono[type] = instance;
            }

            Log.Info($"[ContextBuilder] _createAndRegistry {type.Name}");
        }
    }
}