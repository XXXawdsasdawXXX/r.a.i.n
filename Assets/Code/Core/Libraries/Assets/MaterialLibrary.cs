using System;
using UnityEngine;

namespace Core.Libraries.Assets
{

    [Serializable]
    public class MaterialLibrary : Library<string, Material>
    {
        public const string WORLD = "World";

        protected override bool ThisIs(Material value, string key)
        {
            return value.name.Equals(key);
        }
    }
}