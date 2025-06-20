using System;
using System.Collections.Generic;

namespace Core.Save
{
    [Serializable]
    public class WorldModel
    {
        public string Name;
        public DateTime CreateTime;
        public DateTime ExitTime;
        public TimeSpan GameTime;
        public Dictionary<int, int> ResourcesStorage = new();
        public Dictionary<string, int> SceneResources = new();

        /*public WorldModel()
        { 
            ResourcesStorage = new Dictionary<int, int>();
            SceneResources = new Dictionary<string, int>();
        }*/
    }
}