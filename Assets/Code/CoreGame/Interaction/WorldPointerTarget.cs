using System;
using Core.Input;
using UnityEngine;

namespace CoreGame.Interaction
{
    [RequireComponent(typeof(Collider2D))]
    public class WorldPointerTarget : MonoBehaviour, IWorldPointerTarget
    {
        public event Action<IWorldPointerTarget> HoverEntered;
        public event Action<IWorldPointerTarget> HoverExited;
        public event Action<IWorldPointerTarget> LeftClicked;
        public event Action<IWorldPointerTarget> RightClicked;

        [SerializeField] private Collider2D _collider;
        [SerializeField] private int _sortingPriority;
        [SerializeField] private bool _isEnabled = true;

        public bool IsHovered { get; private set; }
        public bool IsPointerEnabled => _isEnabled && isActiveAndEnabled && Collider != null && Collider.enabled;
        public Collider2D Collider => _collider != null ? _collider : GetComponent<Collider2D>();
        public int SortingPriority => _sortingPriority;

        private void Awake()
        {
            if (_collider == null)
            {
                _collider = GetComponent<Collider2D>();
            }
        }

        protected virtual void OnEnable()
        {
            WorldPointerService.Register(this);
        }

        protected virtual void OnDisable()
        {
            WorldPointerService.Unregister(this);
        }

        public void NotifyHoverChanged(bool isHovered)
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

        public void NotifyClicked(EInputAction action)
        {
            switch (action)
            {
                case EInputAction.LeftClick:
                    LeftClicked?.Invoke(this);
                    break;
                case EInputAction.RightClick:
                    RightClicked?.Invoke(this);
                    break;
            }
        }
    }
}
