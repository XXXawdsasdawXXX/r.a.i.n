using Core.GameLoop;
using Core.Network;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using Essential;
using UnityEngine;
using UnityEngine.Scripting;

namespace Core.Audio
{
    [Preserve]
    public class AudioAttenuationController : IMono, IInitializeListener, ISubscriber
    {
        private FMODUnity.StudioListener _listener;
        private HeroPool _heroPool;

        public UniTask GameInitialize()
        {
            if (Camera.main != null)
            {
                _listener = Camera.main.GetComponent<FMODUnity.StudioListener>();
            }
            else
            {
                Log.Error("No find camera.main", this);
            }

            _heroPool = Container.Instance.GetService<HeroPool>();

            return UniTask.CompletedTask;
        }


        public UniTask Subscribe()
        {
            _heroPool.HeroSpawned += _setObject;
            
            return UniTask.CompletedTask;
        }

        public void Unsubscribe()
        {
            _heroPool.HeroSpawned -= _setObject;
        }

        private void _setObject(GameObject gameObject)
        {
            _listener.AttenuationObject = gameObject;
        }
    }
}