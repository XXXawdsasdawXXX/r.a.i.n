using System;
using UnityEngine;

namespace Core.Libraries
{

    [Serializable]
    public class MaterialLibrary : Library<Material, string>
    {
        public const string WORLD = "World";
        
        [SerializeField] private Material[] _materials;

        public Material Get(string key)
        {
            foreach (Material material in _materials)        
            {
                if (material.name.Equals(key))
                {
                    return material;
                }
            }

            throw new Exception($"Material library has not material with name {key}");
        }

        protected override bool ThisIs(Material value, string key)
        {
            return value.name.Equals(key);
        }
    }
}