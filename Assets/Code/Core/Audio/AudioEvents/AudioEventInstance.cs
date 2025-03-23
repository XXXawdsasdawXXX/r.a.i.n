using Essential;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

namespace Core.Audio
{
    public class AudioEventInstance : MonoBehaviour
    {
        [SerializeField] private EventReference _event;
        
        private EventInstance _instance;
        
        private void OnEnable()
        {
            PlayEvent();
        }

        private void OnDisable()
        {
            StopEvent();
        }
        
        [ContextMenu("Play")]
        private void PlayEvent()
        {
            if (_event.IsNull)
            {
                Log.Error($"{gameObject.name} event reference is null", this);
                return;
            }
            
            Log.Info($"Play {_event.Path}", this);
            _instance = RuntimeManager.CreateInstance(_event);
            _instance.set3DAttributes(transform.position.To3DAttributes());
            _instance.start();
            _instance.release();
        }

        private void StopEvent()
        {
            _instance.stop(STOP_MODE.ALLOWFADEOUT);
        }
    }
}