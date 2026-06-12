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
    public sealed class HeroContextMenuService : IService, IInitializeListener, ISubscriber
    {
        public bool IsInitialized { get; set; }
        public bool HasActiveMenu => !string.IsNullOrEmpty(_activeHeroObjectId);

        public event Action<HeroContextMenuRequest> OpenRequested;
        public event Action CloseRequested;

        private InputManager _input;
        private CameraView _cameraView;
        private HeroSpawner _heroSpawner;
        private string _activeHeroObjectId;
        private readonly List<HeroContextTarget> _targets = new();
        private readonly List<Collider2D> _physicsHits = new();
        private ContactFilter2D _hitFilter;

        public UniTask Initialize()
        {
            _input = Container.Instance.GetService<InputManager>();
            _cameraView = Container.Instance.GetView<CameraView>();
            _hitFilter = new ContactFilter2D();
            _hitFilter.NoFilter();
            return UniTask.CompletedTask;
        }

        public void Subscribe()
        {
            _input.ActionEnded += _onActionEnded;
            _heroSpawner = Container.Instance.GetService<HeroSpawner>();
            _heroSpawner.HeroSpawned += RegisterTarget;
            _heroSpawner.HeroDespawned += UnregisterTarget;
        }

        public void Unsubscribe()
        {
            _input.ActionEnded -= _onActionEnded;

            if (_heroSpawner != null)
            {
                _heroSpawner.HeroSpawned -= RegisterTarget;
                _heroSpawner.HeroDespawned -= UnregisterTarget;
            }

            _targets.Clear();
            RequestClose();
        }

        public void RegisterTarget(Hero target)
        {
            if (target?.ContextTarget == null || _targets.Contains(target.ContextTarget))
            {
                return;
            }

            _targets.Add(target.ContextTarget);
        }

        public void UnregisterTarget(Hero target)
        {
            if (target?.ContextTarget == null)
            {
                return;
            }

            _targets.Remove(target.ContextTarget);

            if (_activeHeroObjectId == target.ObjectId.ToString())
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

        private void _handleRightClick()
        {
            if (_isPointerOverScreenSpaceUi())
            {
                return;
            }

            HeroContextTarget target = _findTargetUnderCursor();
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

        private HeroContextTarget _findTargetUnderCursor()
        {
            if (_cameraView?.Camera == null)
            {
                return null;
            }

            Vector2 worldPoint = _getMouseWorldPoint();

            HeroContextTarget physicsHit = _findTargetViaPhysics(worldPoint);
            if (physicsHit != null)
            {
                return physicsHit;
            }

            return _findTargetViaRegisteredTargets(worldPoint);
        }

        private Vector2 _getMouseWorldPoint()
        {
            Vector3 screenPoint = _input.MousePosition;
            screenPoint.z = -_cameraView.Camera.transform.position.z;
            Vector3 worldPoint = _cameraView.ScreenToWorldPoint(screenPoint);
            worldPoint.z = 0f;
            return worldPoint;
        }

        private HeroContextTarget _findTargetViaPhysics(Vector2 worldPoint)
        {
            Physics2D.SyncTransforms();

            _physicsHits.Clear();
            Physics2D.OverlapPoint(worldPoint, _hitFilter, _physicsHits);

            HeroContextTarget bestTarget = null;
            float bestZ = float.NegativeInfinity;

            foreach (Collider2D hit in _physicsHits)
            {
                if (!_tryResolveContextTarget(hit, out HeroContextTarget target))
                {
                    continue;
                }

                if (!target.CanOpenContextMenu())
                {
                    continue;
                }

                float z = target.transform.position.z;
                if (z >= bestZ)
                {
                    bestZ = z;
                    bestTarget = target;
                }
            }

            return bestTarget;
        }

        private HeroContextTarget _findTargetViaRegisteredTargets(Vector2 worldPoint)
        {
            HeroContextTarget bestTarget = null;
            float bestZ = float.NegativeInfinity;

            for (int index = _targets.Count - 1; index >= 0; index--)
            {
                HeroContextTarget target = _targets[index];
                if (target == null)
                {
                    _targets.RemoveAt(index);
                    continue;
                }

                if (!target.CanOpenContextMenu() || !target.OverlapsPointer(worldPoint))
                {
                    continue;
                }

                float z = target.transform.position.z;
                if (z >= bestZ)
                {
                    bestZ = z;
                    bestTarget = target;
                }
            }

            return bestTarget;
        }

        private static bool _tryResolveContextTarget(Collider2D hit, out HeroContextTarget target)
        {
            target = hit.GetComponent<HeroContextTarget>()
                     ?? hit.GetComponentInParent<HeroContextTarget>();
            if (target != null)
            {
                return true;
            }

            Hero hero = hit.GetComponentInParent<Hero>();
            target = hero?.ContextTarget;
            return target != null;
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
