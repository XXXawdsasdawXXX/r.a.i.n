using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace Core.Save
{
    [Serializable]
    public class WorldModel
    {
        public Dictionary<int, int> ResourcesStorage;
        public Dictionary<float2, int> SceneResources;

        public WorldModel()
        { 
            ResourcesStorage = new Dictionary<int, int>();
            SceneResources = new Dictionary<float2, int>();
        }
    }
}