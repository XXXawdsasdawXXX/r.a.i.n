using System;
using Core.GameLoop;
using Core.Input;
using Core.ServiceLocator;
using CoreGame.Camera;
using FishNet;
using FishNet.Object;
using UnityEngine;

namespace CoreGame.Entities.Characters.Hero
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class HeroPointerTarget : NetworkBehaviour, IUpdateListener
    {
        public event Action<HeroPointerTarget> HoverEntered;
        public event Action<HeroPointerTarget> HoverExited;
        public event Action<HeroPointerTarget> LeftClicked;
        public event Action<HeroPointerTarget> RightClicked;

        [SerializeField] private HeroContextTarget _contextTarget;
        [SerializeField] private Collider2D _collider;

        public bool IsHovered { get; private set; }
        public HeroContextTarget ContextTarget => _contextTarget != null ? _contextTarget : GetComponent<HeroContextTarget>();
        public Collider2D Collider => _collider != null ? _collider : GetComponent<Collider2D>();

        public string RuntimeListenerName => $"HeroPointerTarget:{name}";

        private InputManager _input;
        private UnityEngine.Camera _camera;

        private void Awake()
        {
            if (_collider == null)
            {
                _collider = GetComponent<Collider2D>();
            }

            if (_contextTarget == null)
            {
                _contextTarget = GetComponent<HeroContextTarget>();
            }
        }

        public void BindInput(InputManager input, UnityEngine.Camera camera)
        {
            _input = input;
            _camera = camera;
        }

        public void GameUpdate(float deltaTime)
        {
            if (!InstanceFinder.IsClientStarted || Collider == null)
            {
                _setHovered(false);
                return;
            }

            UnityEngine.Camera camera = _resolveCamera();
            if (camera == null)
            {
                _setHovered(false);
                return;
            }

            _setHovered(_isPointerOver(camera));
        }

        public bool TryHandleClick(EInputAction action)
        {
            if (!IsHovered)
            {
                return false;
            }

            switch (action)
            {
                case EInputAction.LeftClick:
                    LeftClicked?.Invoke(this);
                    return true;
                case EInputAction.RightClick:
                    RightClicked?.Invoke(this);
                    return true;
                default:
                    return false;
            }
        }

        public bool IsPointerOver(UnityEngine.Camera camera)
        {
            if (Collider == null || camera == null)
            {
                return false;
            }

            return _isPointerOver(camera);
        }

        private bool _isPointerOver(UnityEngine.Camera camera)
        {
            Vector3 mousePosition = _input != null ? _input.MousePosition : UnityEngine.Input.mousePosition;
            Ray ray = camera.ScreenPointToRay(mousePosition);
            RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(ray, Mathf.Infinity);

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider == Collider)
                {
                    return true;
                }
            }

            return false;
        }

        private UnityEngine.Camera _resolveCamera()
        {
            if (_camera != null)
            {
                return _camera;
            }

            try
            {
                CameraView cameraView = Container.Instance.GetView<CameraView>();
                _camera = cameraView?.Camera;
            }
            catch (Exception)
            {
                _camera = null;
            }

            return _camera != null ? _camera : UnityEngine.Camera.main;
        }

        private void _setHovered(bool isHovered)
        {
            if (IsHovered == isHovered)
            {
                return;
            }

            IsHovered = isHovered;

            if (isHovered)
            {
                HoverEntered?.Invoke(this);
            }
            else
            {
                HoverExited?.Invoke(this);
            }
        }
    }
}
