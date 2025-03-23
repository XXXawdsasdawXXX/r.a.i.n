using System;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace Core.Audio
{
    [Serializable]
    public class AudioParameterController
    {
        [SerializeField] private EAudioParamType _paramName;
     
        private PARAMETER_DESCRIPTION _parameterDescription;
        private PARAMETER_ID _parameterID;
        
        public void InitParam()
        {
            RuntimeManager.StudioSystem.getParameterDescriptionByName(_paramName.ToString(), out _parameterDescription);
            _parameterID = _parameterDescription.id;
        }

        public void SetValue(float value)
        {
            RuntimeManager.StudioSystem.setParameterByID(_parameterID, value);
        }

        public float GetValue()
        {
            RuntimeManager.StudioSystem.getParameterByID(_parameterID, out var value);
            return value;
        }
    }
}