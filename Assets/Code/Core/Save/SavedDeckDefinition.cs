using System;
using System.Collections.Generic;

namespace Core.Save
{
    [Serializable]
    public class SavedDeckDefinition
    {
        public string Id;
        public string Name;
        public List<string> Cards = new();
    }
}
