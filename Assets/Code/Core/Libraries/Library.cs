using System;
using UnityEngine;

namespace Core.Libraries
{
    [Serializable]
    public abstract class Library<TType, TKey> where TType : class
    {
        [SerializeField] private TType[] _assets;

        public TType Get(TKey key)
        {
            foreach (TType value in _assets)        
            {
                if (ThisIs(value, key))
                {
                    return value;
                }
            }

            throw new Exception($"Material library has not material with name {key}");
        }

        protected abstract bool ThisIs(TType value, TKey key);
    }
}