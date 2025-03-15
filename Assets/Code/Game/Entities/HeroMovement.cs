using FishNet.Object;
using UnityEngine;

namespace Game.Entities
{
    public class HeroMovement : NetworkBehaviour
    {
        [SerializeField] private Rigidbody2D _rigidbody2D;
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _cameraYOffset = 4;
      
        private Vector2 _inputDirection;
        private Camera _camera;

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

        private void Update()
        {
            if (!IsOwner)
            {
                return;
            }

            _inputDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        }

        private void FixedUpdate()
        {
            if (!IsOwner)
            {
                return;
            }
       
            _rigidbody2D.velocity = _inputDirection.normalized * _moveSpeed * (float)TimeManager.TickDelta;
        }
    }
}