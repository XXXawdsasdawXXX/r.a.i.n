using System;
using Core.Input;
using Core.Network;
using Core.ServiceLocator;
using CoreGame.Interaction;
using FishNet.Object;
using UnityEngine;

namespace CoreGame.Entities.Characters.Hero
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class HeroPointerTarget : NetworkBehaviour, IWorldPointerTarget
    {
        public event Action<IWorldPointerTarget> HoverEntered;
        public event Action<IWorldPointerTarget> HoverExited;
        public event Action<IWorldPointerTarget> LeftClicked;
        public event Action<IWorldPointerTarget> RightClicked;

        [SerializeField] private Hero _hero;
        [SerializeField] private Collider2D _collider;
        [SerializeField] private GameObject _contextMenu;
        [SerializeField] private int _sortingPriority = 10;

        public bool IsHovered { get; private set; }
        public bool IsPointerEnabled => isActiveAndEnabled && IsClientStarted && Collider != null && Collider.enabled;
        public Hero Hero => _hero != null ? _hero : NetworkObject.GetComponent<Hero>();
        public Collider2D Collider => _collider != null ? _collider : GetComponent<Collider2D>();
        public GameObject ContextMenu => _contextMenu;
        public int SortingPriority => _sortingPriority;

        public string DisplayName =>
            Hero?.Name != null && !string.IsNullOrEmpty(Hero.Name.Name)
                ? Hero.Name.Name
                : "Player";

        public string HeroObjectId => Hero != null ? Hero.ObjectId.ToString() : string.Empty;

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

        private void OnEnable()
        {
            WorldPointerService.Register(this);
        }

        private void OnDisable()
        {
            WorldPointerService.Unregister(this);
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
