using System;
using UnityEngine;

namespace Core.Libraries.Assets
{
    [Serializable]
    public class PrefabLibrary : Library<string, GameObject>
    {
        protected override bool ThisIs(GameObject value, string key)
        {
            return value.name.Equals(key);
        }
    }
}