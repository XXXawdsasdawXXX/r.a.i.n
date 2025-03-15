using Core.GameLoop;
using Core.Input;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using Essential;
using FishNet.Object;
using UnityEngine;

namespace Game.Entities
{
    public class HeroMovement : NetworkBehaviour, IInitializeListener
    {
        [SerializeField] private Rigidbody2D _rigidbody2D;
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _cameraYOffset = 4;
      
        private Vector2 _inputDirection;
        private Camera _camera;
        private InputManager _inputManager;

        public UniTask GameInitialize()
        {
            _inputManager = Container.Instance.GetService<InputManager>();
            
            Log.Info($"{ID.Value} {_inputManager != null}", this);
            
            return UniTask.CompletedTask;
        }
        
        public override void OnStartClient()
        {
            base.OnStartClient();

            if (IsOwner)
            {
                _camera = Camera.main;

                if (_camera != null)
                {
                    Transform cameraTransform = _camera.transform;
                    Transform playerTransform = transform;

                    Vector3 cameraPosition = playerTransform.position + new Vector3(0, _cameraYOffset, -10);

                    cameraTransform.SetParent(playerTransform);
                    cameraTransform.position = cameraPosition;
                }
            }
        }
        
        private void FixedUpdate()
        {
            if (!IsOwner)
            {
                return;
            }
       
            _rigidbody2D.velocity = _inputManager.Direction * _moveSpeed * (float)TimeManager.TickDelta;
        }
    }
}