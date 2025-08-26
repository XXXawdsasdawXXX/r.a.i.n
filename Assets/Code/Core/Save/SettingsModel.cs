using System;
using System.Collections.Generic;

namespace Core.Save
{
    [Serializable]
    public class SettingsModel
    {
        public float SFXVolume;
        public float MusicVolume;
       
        public List<string> PreviousConnectedIPs;
        public int LastConnectedIPIndex;

        public SettingsModel()
        {
            SFXVolume = 0.7f;
            MusicVolume = 0.7f;
            PreviousConnectedIPs = new List<string>();
            LastConnectedIPIndex = 0;
        }
    }
}