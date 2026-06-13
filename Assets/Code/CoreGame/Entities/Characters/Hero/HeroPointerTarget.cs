using System;
using Core.GameLoop;
using Core.Input;
using Core.Network;
using Core.ServiceLocator;
using CoreGame.Camera;
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

        [SerializeField] private Hero _hero;
        [SerializeField] private Collider2D _collider;
        [SerializeField] private GameObject _contextMenu;

        public bool IsHovered { get; private set; }
        public Hero Hero => _hero != null ? _hero : NetworkObject.GetComponent<Hero>();
        public Collider2D Collider => _collider != null ? _collider : GetComponent<Collider2D>();
        public GameObject ContextMenu => _contextMenu;

        public string DisplayName =>
            Hero?.Name != null && !string.IsNullOrEmpty(Hero.Name.Name)
                ? Hero.Name.Name
                : "Player";

        public string HeroObjectId => Hero != null ? Hero.ObjectId.ToString() : string.Empty;

        public string RuntimeListenerName => $"HeroPointerTarget:{name}";

        private InputManager _input;
        private UnityEngine.Camera _camera;

        private void Awake()
        {
            if (_collider == null)
            {
                _collider = GetComponent<Collider2D>();
            }

            if (_hero == null)
            {
                _hero = GetComponentInParent<Hero>();
            }
        }

        public void BindInput(InputManager input, UnityEngine.Camera camera)
        {
            _input = input;
            _camera = camera;
        }

        public bool CanOpenContextMenu()
        {
            if (Hero == null || !Hero.IsSpawned)
            {
                return false;
            }

            if (_isLocalHero())
            {
                return false;
            }

            return !_isInBattle();
        }

        public void GameUpdate(float deltaTime)
        {
            if (!IsClientStarted || Collider == null)
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

        private bool _isLocalHero()
        {
            if (Hero.IsOwner)
            {
                return true;
            }

            UserProvider userProvider = Container.Instance.GetService<UserProvider>();
            if (userProvider?.Hero == null)
            {
                return false;
            }

            return userProvider.Hero.ObjectId == Hero.ObjectId;
        }

        private bool _isInBattle()
        {
            return Hero.Model != null && Hero.Model.InBattle;
        }
    }
}
