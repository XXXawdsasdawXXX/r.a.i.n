using System;
using System.Collections.Generic;
using Essential;
using UnityEngine;

namespace Core.Data
{
    [Serializable]
    public sealed class UnitySerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, 
        ISerializationCallbackReceiver
    {
        [SerializeField] private List<TKey> _keys = new();
        [SerializeField] private List<TValue> _values = new();

        public void OnBeforeSerialize()
        {
            _keys.Clear();
            _values.Clear();

            foreach ((TKey key, TValue value) in this)
            {
                _keys.Add(key);
                _values.Add(value);
            }
        }

        public void OnAfterDeserialize()
        {
            Clear();

            if (_keys.Count != _values.Count)
            {
                string message = $"Key count ({_keys.Count}) does not match value count ({_values.Count}) " +
                                 "Make sure that both key and value types are serializable.";

                throw new Exception(message);
            }

            for (int i = 0; i < _keys.Count; i++)
            {
                Add(_keys[i], _values[i]);
            }
        }
    }
}