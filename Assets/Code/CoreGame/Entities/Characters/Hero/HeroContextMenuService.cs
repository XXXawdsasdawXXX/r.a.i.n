using System;
using System.Collections.Generic;
using Core.GameLoop;
using Core.Input;
using Core.ServiceLocator;
using CoreGame.Camera;
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
        private UnityEngine.Camera _camera;
        private HeroSpawner _heroSpawner;
        private string _activeHeroObjectId;
        private readonly List<HeroPointerTarget> _pointerTargets = new();

        public UniTask Initialize()
        {
            _input = Container.Instance.GetService<InputManager>();
            return UniTask.CompletedTask;
        }

        public UniTask GameStart()
        {
            _camera = null;
            _resolveCamera();
            _registerExistingHeroes();
            return UniTask.CompletedTask;
        }

        public void Subscribe()
        {
            _input.ActionEnded += _onActionEnded;
            _heroSpawner = Container.Instance.GetService<HeroSpawner>();
            _heroSpawner.HeroSpawned += RegisterHero;
            _heroSpawner.HeroDespawned += UnregisterHero;
            _registerExistingHeroes();
        }

        public void Unsubscribe()
        {
            _input.ActionEnded -= _onActionEnded;

            if (_heroSpawner != null)
            {
                _heroSpawner.HeroSpawned -= RegisterHero;
                _heroSpawner.HeroDespawned -= UnregisterHero;
            }

            foreach (HeroPointerTarget pointerTarget in _pointerTargets)
            {
                if (pointerTarget != null)
                {
                    pointerTarget.RightClicked -= _onPointerRightClicked;
                }
            }

            _pointerTargets.Clear();
            RequestClose();
        }

        public void RegisterHero(Hero hero)
        {
            if (hero?.PointerTarget == null)
            {
                return;
            }

            RegisterPointerTarget(hero.PointerTarget);
        }

        public void RegisterPointerTarget(HeroPointerTarget pointerTarget)
        {
            if (pointerTarget == null || _pointerTargets.Contains(pointerTarget))
            {
                return;
            }

            _resolveCamera();
            pointerTarget.BindInput(_input, _camera);
            pointerTarget.RightClicked += _onPointerRightClicked;
            _pointerTargets.Add(pointerTarget);
        }

        public void UnregisterHero(Hero hero)
        {
            if (hero?.PointerTarget == null)
            {
                return;
            }

            HeroPointerTarget pointerTarget = hero.PointerTarget;
            pointerTarget.RightClicked -= _onPointerRightClicked;
            _pointerTargets.Remove(pointerTarget);

            if (_activeHeroObjectId == hero.ObjectId.ToString())
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

            switch (action)
            {
                case EInputAction.RightClick:
                    _handleRightClick();
                    break;
                case EInputAction.LeftClick:
                    _handleLeftClickOutside();
                    break;
                case EInputAction.Esc:
                    RequestClose();
                    break;
            }
        }

        private void _onPointerRightClicked(HeroPointerTarget pointerTarget)
        {
            _openContextMenu(pointerTarget);
        }

        private void _handleRightClick()
        {
            if (_isPointerOverScreenSpaceUi())
            {
                return;
            }

            _openContextMenu(_findPointerTargetUnderCursor());
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

        private void _registerExistingHeroes()
        {
            Hero[] heroes = UnityEngine.Object.FindObjectsOfType<Hero>(true);
            foreach (Hero hero in heroes)
            {
                RegisterHero(hero);
            }
        }

        private HeroPointerTarget _findPointerTargetUnderCursor()
        {
            UnityEngine.Camera camera = _resolveCamera();
            if (camera == null)
            {
                return null;
            }

            Ray ray = camera.ScreenPointToRay(_input.MousePosition);
            RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(ray, Mathf.Infinity);

            HeroPointerTarget bestTarget = null;
            float bestDistance = float.PositiveInfinity;

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider == null)
                {
                    continue;
                }

                HeroPointerTarget pointerTarget = hit.collider.GetComponent<HeroPointerTarget>()
                    ?? hit.collider.GetComponentInParent<HeroPointerTarget>();
                if (pointerTarget == null)
                {
                    continue;
                }

                if (hit.distance < bestDistance)
                {
                    bestDistance = hit.distance;
                    bestTarget = pointerTarget;
                }
            }

            if (bestTarget != null)
            {
                return bestTarget;
            }

            foreach (HeroPointerTarget pointerTarget in _pointerTargets)
            {
                if (pointerTarget != null && pointerTarget.IsHovered)
                {
                    return pointerTarget;
                }
            }

            return null;
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

            if (_camera == null)
            {
                _camera = UnityEngine.Camera.main;
            }

            foreach (HeroPointerTarget pointerTarget in _pointerTargets)
            {
                pointerTarget?.BindInput(_input, _camera);
            }

            return _camera;
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
