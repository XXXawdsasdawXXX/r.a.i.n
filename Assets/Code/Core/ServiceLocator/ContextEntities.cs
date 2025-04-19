using System;
using System.Collections.Generic;

namespace Core.ServiceLocator
{
    internal class ContextEntities
    {
        public Essential.Mono[] Objects;
        public Dictionary<Type, MonoView> Views;
        
        public Dictionary<Type, IService> Services;
        public Dictionary<Type, IMono> Mono;

        public ContextEntities Parent;
        public ContextEntities Child;
    }
}