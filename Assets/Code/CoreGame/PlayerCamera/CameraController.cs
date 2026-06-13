using Core.GameLoop;
using Core.Network;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CoreGame.PlayerCamera
{
    public class CameraController : IMono, IInitializeListener, IStartListener, IUpdateListener
    {
        public bool IsInitialized { get; set; }
        public string RuntimeListenerName => "CameraController";
        
        private CameraView _cameraView;
        private UserProvider _userProvider;
        
        public UniTask Initialize()
        {
            _userProvider = Container.Instance.GetService<UserProvider>();
            
            return UniTask.CompletedTask;
        }

        public UniTask GameStart()
        {
            _cameraView = Container.Instance.GetView<CameraView>();
         
            return UniTask.CompletedTask;
        }

        public void GameUpdate(float deltaTime)
        {
            if (_cameraView != null && _userProvider.Hero != null)
            {
                Vector3 position = _userProvider.Hero.transform.position;

                position.z = -10;
                
                _cameraView.transform.position = position;
            }
            else
            {
            }
        }
    }
}