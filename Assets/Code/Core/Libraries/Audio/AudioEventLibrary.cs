using System;
using FMODUnity;

namespace Core.Libraries
{
    [Serializable]
    public class AudioEventLibrary : Library<string, EventReference>
    {
        public const string BUTTON_DOWN = "ButtonDown";
        public const string BUTTON_UP = "ButtonUp";
        
        protected override bool ThisIs(EventReference value, string key)
        {
            string[] path = value.Path.Split('/');
            
            return path[^1].Equals(key);
        }
    }
}