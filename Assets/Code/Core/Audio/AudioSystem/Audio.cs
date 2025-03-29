using Core.GameLoop;
using Core.Libraries;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using FMODUnity;
using UnityEngine;
using UnityEngine.Scripting;

namespace Core.Audio
{
    [Preserve]
    public class Audio : IService, IInitializeListener
    {
        private AudioLibrary _audioLibrary;

        public UniTask GameInitialize()
        {
            _audioLibrary = Container.Instance.GetConfig<AudioLibrary>();
            
            return UniTask.CompletedTask;
        }

        /*
        public void OneShot(EventReference eventReference)
        {
            RuntimeManager.PlayOneShot(eventReference);
        }
        */

        public void OneShot(string eventKey)
        {
            EventReference eventReference = _audioLibrary.Events.Get(eventKey);
            
            RuntimeManager.PlayOneShot(eventReference);
        }
        
        public void OneShot(string eventKey, Vector3 position)
        {
            EventReference eventReference = _audioLibrary.Events.Get(eventKey);
            
            RuntimeManager.PlayOneShot(eventReference, position);
        }
    }
}