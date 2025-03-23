using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Libraries
{
    [Serializable]
    public abstract class Library<TKey, TType> 
    {
        [SerializeField] private TType[] _assets;

        private Dictionary<TKey, TType> _hash = new();
        
        public TType Get(TKey key)
        {
            if (_hash.ContainsKey(key))
            {
                return _hash[key];
            }
            
            foreach (TType value in _assets)        
            {
                if (ThisIs(value, key))
                {
                    _hash.Add(key, value);
                    return value;
                }
            }

            throw new Exception($"Material library has not material with name {key}");
        }

        protected abstract bool ThisIs(TType value, TKey key);
    }
}