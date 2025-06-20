using System;
using System.Collections.Generic;
using System.Linq;
using Essential;
using Object = UnityEngine.Object;

namespace Core.ServiceLocator
{
    internal static class ContextBuilder
    {
        internal static ContextEntities BuildContext(Type[] allTypes)
        {
            ContextEntities newContext = new()
            {
                Services = new Dictionary<Type, IService>(),
                Views = new Dictionary<Type, MonoView>(),
                Mono = new Dictionary<Type, IMono>(),
                Objects = Object.FindObjectsOfType<Essential.Mono>(true).ToList()
            };
            
            foreach (Essential.Mono mono in newContext.Objects)
            {
                _registryTypes(mono, newContext);
            }

            foreach (Type type in allTypes) 
            {
                _createAndRegistryType(type, newContext);
            }

            return newContext;
        }
        
        internal static void BuildChildContext(this ContextEntities existingContext)
        {
            existingContext.Child = null;
            existingContext.SetChildContext(_buildContext(existingContext));
        }

        internal static void BuildChildContext(this ContextEntities existingContext, Type[] allTypes)
        {
            existingContext.Child = null;
            existingContext.SetChildContext(BuildContext(existingContext, allTypes));
        }

        internal static ContextEntities BuildContext(ContextEntities existingContext, Type[] allTypes)
        {
            if (allTypes == null)
            {
                return _buildContext(existingContext);
            }
            
            ContextEntities newContext = new()
            {
                Services = new Dictionary<Type, IService>(),
                Views = new Dictionary<Type, MonoView>(),
                Mono = new Dictionary<Type, IMono>(),
                Objects = Object.FindObjectsOfType<Essential.Mono>(true).ToList()
            };
            
            ContextEntities currentContext = existingContext;
        
            while (currentContext != null)
            {
                newContext.Objects.RemoveAll(obj => currentContext.ContainsObject(obj));
                currentContext = currentContext.Parent;
            }
            
            foreach (Essential.Mono mono in newContext.Objects)
            {
                _registryTypes(mono, existingContext, newContext);
            }

            foreach (Type type in allTypes) 
            {
                _createAndRegistryType(type, existingContext, newContext);
            }

            return newContext;
        }

        private static ContextEntities _buildContext(ContextEntities existingContext)
        {
            ContextEntities newContext = new()
            {
                Services = new Dictionary<Type, IService>(),
                Views = new Dictionary<Type, MonoView>(),
                Mono = new Dictionary<Type, IMono>(),
                Objects = Object.FindObjectsOfType<Essential.Mono>(true).ToList()
            };
            
            ContextEntities currentContext = existingContext;
        
            while (currentContext != null)
            {
                newContext.Objects.RemoveAll(obj => currentContext.ContainsObject(obj));
                currentContext = currentContext.Parent;
            }
            
            foreach (Essential.Mono mono in newContext.Objects)
            {
                _registryTypes(mono, existingContext, newContext);
            }

            return newContext;
        }

        private static void _registryTypes(Essential.Mono mono, ContextEntities existingContext, ContextEntities newContext)
        {
            Type type = mono.GetType();

            if (mono is IMono monoInterface 
                &&  !existingContext.ContainsMono(type) && !newContext.Mono.ContainsKey(type) )
            {
                newContext.Mono[type] = monoInterface;
            }

            else if (mono is IService service 
                     && !existingContext.ContainsService(type) && !newContext.Services.ContainsKey(type))
            {
                newContext.Services[type] = service;
            }

            else if (mono is MonoView view 
                     && !existingContext.ContainsView(type) && !newContext.Views.ContainsKey(type))
            {
                newContext.Views[type] = view;
            }
        }

        private static void _registryTypes(Essential.Mono mono, ContextEntities context)
        {
            Type type = mono.GetType();

            if (mono is IMono monoInterface  && !context.Mono.ContainsKey(type))
            {
                context.Mono[type] = monoInterface;
            }

            else if (mono is IService service && !context.Services.ContainsKey(type))
            {
                context.Services[type] = service;
            }

            else if (mono is MonoView view  && !context.Views.ContainsKey(type))
            {
                context.Views[type] = view;
            }
        }

        private static void _createAndRegistryType(Type type, ContextEntities existingContext, ContextEntities newContext)
        {
            if (typeof(IService).IsAssignableFrom(type) && !existingContext.ContainsService(type)
                                                        && !typeof(Essential.Mono).IsAssignableFrom(type)
                                                        && !type.IsAbstract)
            {
                IService instance = (IService)Activator.CreateInstance(type);
                newContext.Services[type] = instance;
            }

            else if (typeof(IMono).IsAssignableFrom(type) && !existingContext.ContainsMono(type)
                                                          && !typeof(Essential.Mono).IsAssignableFrom(type)
                                                          && !type.IsAbstract)
            {
                IMono instance = (IMono)Activator.CreateInstance(type);
                newContext.Mono[type] = instance;
            }
        }


        private static void _createAndRegistryType(Type type, ContextEntities newContext)
        {
            if (typeof(IService).IsAssignableFrom(type) && !typeof(Essential.Mono).IsAssignableFrom(type)
                                                        && !type.IsAbstract)
            {
                IService instance = (IService)Activator.CreateInstance(type);
                newContext.Services[type] = instance;
            }

            else if (typeof(IMono).IsAssignableFrom(type) && !typeof(Essential.Mono).IsAssignableFrom(type)
                                                          && !type.IsAbstract)
            {
                IMono instance = (IMono)Activator.CreateInstance(type);
                newContext.Mono[type] = instance;
            }
        }
    }
}