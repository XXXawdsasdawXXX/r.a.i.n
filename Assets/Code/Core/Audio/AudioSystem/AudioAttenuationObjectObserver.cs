using Core.Data;
using Core.GameLoop;
using Core.Network;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using Essential;
using FishNet.Object;
using Plugins.FishNet.Runtime.Managing.Object;
using UnityEngine;
using UnityEngine.Scripting;

namespace Core.Audio
{
    [Preserve]
    public class AudioAttenuationObjectObserver : IMono, IInitializeListener, ISubscriber
    {
        private FMODUnity.StudioListener _listener;
        private PlayerSpawner _playerSpawner;

        public UniTask GameInitialize()
        {
            _playerSpawner = Object.FindObjectOfType<PlayerSpawner>();

            if (Camera.main != null)
            {
                _listener = Camera.main.GetComponent<FMODUnity.StudioListener>();
            }
            else
            {
                Log.Error("No find camera.main", this);
            }

            return UniTask.CompletedTask;
        }

        public UniTask Subscribe()
        {
         //   _playerSpawner.OnSpawned += PlayerSpawnerOnOnSpawned;
            return UniTask.CompletedTask;
        }

        public void Unsubscribe()
        {
           // _playerSpawner.OnSpawned += PlayerSpawnerOnOnSpawned;
        }

        private void PlayerSpawnerOnOnSpawned(NetworkObject obj)
        {
            _listener.AttenuationObject = obj.gameObject;

            Log.Info($"{obj.name}", this);
        }
    }
}