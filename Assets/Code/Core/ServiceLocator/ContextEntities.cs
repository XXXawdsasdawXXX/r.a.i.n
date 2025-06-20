using System;
using System.Collections.Generic;

namespace Core.ServiceLocator
{
    internal class ContextEntities
    {
        public List<Essential.Mono> Objects;
        public Dictionary<Type, MonoView> Views;
        
        public Dictionary<Type, IService> Services;
        public Dictionary<Type, IMono> Mono;

        public ContextEntities Parent;
        public ContextEntities Child;

        public void SetChildContext(ContextEntities context)
        {
            Child = context;
            Child.Parent = this;
        }

        public bool ContainsMono(Type type)
        {
            if (Mono.ContainsKey(type))
            {
                return true;
            }
            
            return Parent != null && Parent.ContainsMono(type);
        }
        
        public bool ContainsService(Type type)
        {
            if (Services.ContainsKey(type))
            {
                return true;
            }
            
            return Parent != null && Parent.ContainsService(type);
        }
        
        public bool ContainsView(Type type)
        {
            if (Views.ContainsKey(type))
            {
                return true;
            }
            
            return Parent != null && Parent.ContainsView(type);
        }
        
        public bool ContainsObject(Essential.Mono mono)
        {
            Objects.RemoveAll(o => o == null || !o.gameObject);
            
            if (Objects.Contains(mono))
            {
                return true;
            }
            
            return Parent != null && Parent.Objects.Contains(mono);
        }
    }
}