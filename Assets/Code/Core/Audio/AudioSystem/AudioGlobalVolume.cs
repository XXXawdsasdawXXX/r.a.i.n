using Core.GameLoop;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using FMOD.Studio;
using FMODUnity;
using UnityEngine.Scripting;

namespace Core.Audio
{
    [Preserve]
    public class AudioGlobalVolume : IService, IInitializeListener
    {
        private Bus _musicBus;
        private Bus _effectBus;

        public UniTask GameInitialize()
        {
            return UniTask.CompletedTask;
                        
            /*_musicBus = RuntimeManager.GetBus("bus:/Music");
            _musicBus.setVolume(0);
            _effectBus = RuntimeManager.GetBus("bus:/Effect");
            _effectBus.setVolume(0);
            
            return UniTask.CompletedTask;*/
        }

        public void ChangeEffectVolume(float volume)
        {
            _effectBus.setVolume(volume);
        }

        public  void ChangeMusicVolume(float volume)
        {
            _musicBus.setVolume(volume);
        }
    }
}