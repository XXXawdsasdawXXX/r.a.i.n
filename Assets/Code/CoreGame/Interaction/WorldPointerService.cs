using System;
using System.Collections.Generic;
using System.Linq;
using Core.GameLoop;
using Core.Input;
using Core.ServiceLocator;
using CoreGame.Camera;
using Cysharp.Threading.Tasks;
using FishNet;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CoreGame.Interaction
{
    public sealed class WorldPointerService : IService, IInitializeListener, IStartListener, ISubscriber, IUpdateListener
    {
        private static WorldPointerService _instance;

        public bool IsInitialized { get; set; }
        public string RuntimeListenerName => nameof(WorldPointerService);

        public IWorldPointerTarget HoveredTarget { get; private set; }

        public event Action<IWorldPointerTarget> HoverEntered;
        public event Action<IWorldPointerTarget> HoverExited;
        public event Action<IWorldPointerTarget, EInputAction> Clicked;

        private readonly List<IWorldPointerTarget> _targets = new();
        private InputManager _input;
        private UnityEngine.Camera _camera;

        public static void Register(IWorldPointerTarget target)
        {
            _instance?._register(target);
        }

        public static void Unregister(IWorldPointerTarget target)
        {
            _instance?._unregister(target);
        }

        public UniTask Initialize()
        {
            _instance = this;
            _input = Container.Instance.GetService<InputManager>();
            return UniTask.CompletedTask;
        }

        public UniTask GameStart()
        {
            _camera = null;
            _resolveCamera();
            _registerExistingTargets();
            return UniTask.CompletedTask;
        }

        public void Subscribe()
        {
            _input.ActionEnded += _onActionEnded;
            _registerExistingTargets();
        }

        public void Unsubscribe()
        {
            _input.ActionEnded -= _onActionEnded;
            _setHovered(null);
            _targets.Clear();
        }

        public void GameUpdate(float deltaTime)
        {
            if (!InstanceFinder.IsClientStarted)
            {
                _setHovered(null);
                return;
            }

            if (_isPointerOverScreenSpaceUi())
            {
                _setHovered(null);
                return;
            }

            UnityEngine.Camera camera = _resolveCamera();
            if (camera == null)
            {
                _setHovered(null);
                return;
            }

            _setHovered(_findBestTarget(camera));
        }

        private void _register(IWorldPointerTarget target)
        {
            if (target == null || _targets.Contains(target))
            {
                return;
            }

            _targets.Add(target);
        }

        private void _unregister(IWorldPointerTarget target)
        {
            if (target == null)
            {
                return;
            }

            _targets.Remove(target);

            if (HoveredTarget == target)
            {
                _setHovered(null);
            }
        }

        private void _registerExistingTargets()
        {
            IWorldPointerTarget[] targets = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>(true)
                .OfType<IWorldPointerTarget>()
                .ToArray();
            foreach (IWorldPointerTarget target in targets)
            {
                _register(target);
            }
        }

        private IWorldPointerTarget _findBestTarget(UnityEngine.Camera camera)
        {
            Ray ray = camera.ScreenPointToRay(_input.MousePosition);
            RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(ray, Mathf.Infinity);

            IWorldPointerTarget bestTarget = null;
            float bestDistance = float.PositiveInfinity;
            int bestPriority = int.MinValue;

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider == null)
                {
                    continue;
                }

                IWorldPointerTarget pointerTarget = _resolvePointerTarget(hit.collider);
                if (pointerTarget == null || !pointerTarget.IsPointerEnabled)
                {
                    continue;
                }

                if (hit.distance > bestDistance)
                {
                    continue;
                }

                if (Mathf.Approximately(hit.distance, bestDistance) && pointerTarget.SortingPriority < bestPriority)
                {
                    continue;
                }

                bestDistance = hit.distance;
                bestPriority = pointerTarget.SortingPriority;
                bestTarget = pointerTarget;
            }

            return bestTarget;
        }

        private static IWorldPointerTarget _resolvePointerTarget(Collider2D collider)
        {
            if (collider.TryGetComponent(out IWorldPointerTarget target))
            {
                return target;
            }

            return collider.GetComponentInParent<IWorldPointerTarget>();
        }

        private void _setHovered(IWorldPointerTarget target)
        {
            if (HoveredTarget == target)
            {
                return;
            }

            if (HoveredTarget != null)
            {
                HoveredTarget.NotifyHoverChanged(false);
                HoverExited?.Invoke(HoveredTarget);
            }

            HoveredTarget = target;

            if (HoveredTarget != null)
            {
                HoveredTarget.NotifyHoverChanged(true);
                HoverEntered?.Invoke(HoveredTarget);
            }
        }

        private void _onActionEnded(EInputAction action)
        {
            if (!InstanceFinder.IsClientStarted || HoveredTarget == null || _isPointerOverScreenSpaceUi())
            {
                return;
            }

            if (action is not (EInputAction.LeftClick or EInputAction.RightClick))
            {
                return;
            }

            HoveredTarget.NotifyClicked(action);
            Clicked?.Invoke(HoveredTarget, action);
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
