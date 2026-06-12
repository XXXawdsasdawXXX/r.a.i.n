using System;
using System.Collections.Generic;
using Core.GameLoop;
using Core.Input;
using Core.ServiceLocator;
using CoreGame.Interaction;
using Cysharp.Threading.Tasks;
using FishNet;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CoreGame.Entities.Characters.Hero
{
    public sealed class HeroContextMenuService : IService, IInitializeListener, IStartListener, ISubscriber
    {
        public bool IsInitialized { get; set; }
        public bool HasActiveMenu => !string.IsNullOrEmpty(_activeHeroObjectId);

        public event Action<HeroContextMenuRequest> OpenRequested;
        public event Action CloseRequested;

        private InputManager _input;
        private WorldPointerService _pointerService;
        private HeroSpawner _heroSpawner;
        private string _activeHeroObjectId;

        public UniTask Initialize()
        {
            _input = Container.Instance.GetService<InputManager>();
            _pointerService = Container.Instance.GetService<WorldPointerService>();
            return UniTask.CompletedTask;
        }

        public UniTask GameStart()
        {
            return UniTask.CompletedTask;
        }

        public void Subscribe()
        {
            _input.ActionEnded += _onActionEnded;
            _heroSpawner = Container.Instance.GetService<HeroSpawner>();
            _heroSpawner.HeroDespawned += UnregisterHero;
            _pointerService.Clicked += _onPointerClicked;
        }

        public void Unsubscribe()
        {
            _input.ActionEnded -= _onActionEnded;

            if (_heroSpawner != null)
            {
                _heroSpawner.HeroDespawned -= UnregisterHero;
            }

            if (_pointerService != null)
            {
                _pointerService.Clicked -= _onPointerClicked;
            }

            RequestClose();
        }

        public void UnregisterHero(Hero hero)
        {
            if (hero != null && _activeHeroObjectId == hero.ObjectId.ToString())
            {
                RequestClose();
            }
        }

        public void RequestClose()
        {
            if (!HasActiveMenu)
            {
                return;
            }

            _activeHeroObjectId = null;
            CloseRequested?.Invoke();
        }

        public void NotifyMenuClosed()
        {
            _activeHeroObjectId = null;
        }

        private void _onActionEnded(EInputAction action)
        {
            if (!InstanceFinder.IsClientStarted)
            {
                return;
            }

            if (action == EInputAction.Esc)
            {
                RequestClose();
            }
            else if (action == EInputAction.LeftClick)
            {
                _handleLeftClickOutside();
            }
        }

        private void _onPointerClicked(IWorldPointerTarget target, EInputAction action)
        {
            if (action != EInputAction.RightClick || target is not HeroPointerTarget heroTarget)
            {
                return;
            }

            _openContextMenu(heroTarget);
        }

        private void _openContextMenu(HeroPointerTarget target)
        {
            if (target == null || !target.CanOpenContextMenu())
            {
                RequestClose();
                return;
            }

            if (_activeHeroObjectId == target.HeroObjectId)
            {
                RequestClose();
                return;
            }

            RequestClose();
            _activeHeroObjectId = target.HeroObjectId;
            OpenRequested?.Invoke(new HeroContextMenuRequest(target));
        }

        private void _handleLeftClickOutside()
        {
            if (!HasActiveMenu || _isPointerOverScreenSpaceUi())
            {
                return;
            }

            RequestClose();
        }

        private bool _isPointerOverScreenSpaceUi()
        {
            if (EventSystem.current == null)
            {
                return false;
            }

            PointerEventData pointerData = new(EventSystem.current)
            {
                position = _input.MousePosition
            };

            List<RaycastResult> results = new();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (RaycastResult result in results)
            {
                Canvas canvas = result.gameObject.GetComponentInParent<Canvas>();
                if (canvas != null && canvas.renderMode != RenderMode.WorldSpace)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
