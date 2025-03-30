using Core.GameLoop;
using Core.Input;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using Essential;
using FishNet.Object;
using Game.Camera;
using UnityEngine;

namespace Game.Entities.Hero
{
    public class HeroMovement : NetworkBehaviour, IInitializeListener, IFixedUpdateListener
    {
        public string RuntimeListenerName => "HeroMovement";
        
        [SerializeField] private Rigidbody2D _rigidbody2D;
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _cameraYOffset = 4;

        private CameraView _camera;
        private InputManager _inputManager;
        
        public override void OnStartClient()
        {
            base.OnStartClient();
           
            if (!IsOwner)
            {
                return;
            }
            
            _camera = Container.Instance.GetView<CameraView>();

            if (_camera != null)
            {
                Transform cameraTransform = _camera.transform;
                Transform playerTransform = transform;

                Vector3 cameraPosition = playerTransform.position + new Vector3(0, _cameraYOffset, -10);

                cameraTransform.SetParent(playerTransform);
                cameraTransform.position = cameraPosition;
                
                Log.Info("spawn",Color.magenta, this);
            }
        }

        public UniTask GameInitialize()
        {
            Log.Info("initialize",Color.magenta, this);
            _inputManager = Container.Instance.GetService<InputManager>();
            
            return UniTask.CompletedTask;
        }

        public void GameFixedUpdate(float fixedDeltaTime)
        {
            _rigidbody2D.velocity = _inputManager.Direction.normalized * _moveSpeed * fixedDeltaTime;
        }
    }
}