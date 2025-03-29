using Core.Audio;
using Core.GameLoop;
using Core.Libraries;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;

namespace Game.Entities.Hero
{
    public class HeroAnimationEvent : Essential.Mono, IInitializeListener
    {
        private Audio _audio;

        public UniTask GameInitialize()
        {
            _audio = Container.Instance.GetService<Audio>();
            
            return UniTask.CompletedTask;
        }

        private void PlayStep()
        {
            _audio.OneShot(AudioEventLibrary.STEP, transform.position);
        }
    }
}