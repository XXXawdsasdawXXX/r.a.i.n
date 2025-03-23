using System;
using FMODUnity;
using UnityEngine;

namespace Core.Audio
{
    [Serializable]
    public class AudioEvent
    {
        [SerializeField] private EventReference _eventReference;
        
        public void SetEventReference(EventReference eventReference)
        {
            _eventReference = eventReference;
        }
        
        public void PlayAudioEvent()
        {
            if (_eventReference.IsNull)
            {
                return;
            }

            RuntimeManager.PlayOneShot(_eventReference);
        }

        public void PlayAudioEvent(EventReference eventReference)
        {
            if (eventReference.IsNull)
            {
                return;
            }

            RuntimeManager.PlayOneShot(eventReference);
        }
    }
}