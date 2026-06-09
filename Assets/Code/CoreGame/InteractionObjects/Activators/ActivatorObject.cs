using Core.GameLoop;
using FishNet;
using FishNet.Connection;
using UnityEngine;
using Channel = FishNet.Transporting.Channel;

namespace CoreGame.InteractionObjects.Activators
{
    public class ActivatorObject : InteractionObject, ISubscriber
    {
        [SerializeField] private GameObject _view;
        [SerializeField] private string _activatorId;

        private bool _isActive;

        protected string ActivatorId => _activatorId;

        public override void StartInteraction()
        {
            if (_view != null)
            {
                _view.SetActive(!_isActive);
            }

            base.StartInteraction();
        }

        private void Awake()
        {
            _ensureActivatorId();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _ensureActivatorId();
        }
#endif

        public void Subscribe()
        {
            Trigger.InteractionPerformed += _onInteractionPerformed;
            InstanceFinder.ClientManager.RegisterBroadcast<ActivatorBroadcast>(_onServerSendChanged);
            InstanceFinder.ServerManager.RegisterBroadcast<ActivatorBroadcast>(_onClientRequestChanged);
        }

        public void Unsubscribe()
        {
            Trigger.InteractionPerformed -= _onInteractionPerformed;
            InstanceFinder.ClientManager?.UnregisterBroadcast<ActivatorBroadcast>(_onServerSendChanged);
            InstanceFinder.ServerManager?.UnregisterBroadcast<ActivatorBroadcast>(_onClientRequestChanged);
        }

        private void _onInteractionPerformed()
        {
            ActivatorBroadcast broadcast = _getActivatorBroadcast();

            if (InstanceFinder.IsClientStarted)
            {
                InstanceFinder.ClientManager.Broadcast(broadcast);
            }
            else if (InstanceFinder.IsServerStarted)
            {
                InstanceFinder.ServerManager.Broadcast(broadcast);
            }
        }

        private void _onClientRequestChanged(NetworkConnection network, ActivatorBroadcast broadcast, Channel channel)
        {
            if (string.IsNullOrEmpty(broadcast.ObjectID))
            {
                return;
            }

            InstanceFinder.ServerManager.Broadcast(broadcast);
        }

        private void _onServerSendChanged(ActivatorBroadcast broadcast, Channel channel)
        {
            if (string.IsNullOrEmpty(broadcast.ObjectID) || broadcast.ObjectID != _activatorId)
            {
                return;
            }

            _isActive = broadcast.IsActive;
            StartInteraction();
        }

        private ActivatorBroadcast _getActivatorBroadcast()
        {
            return new ActivatorBroadcast
            {
                ObjectID = _activatorId,
                IsActive = !_isActive
            };
        }

        private void _ensureActivatorId()
        {
            if (!string.IsNullOrEmpty(_activatorId))
            {
                return;
            }

            _activatorId = _buildHierarchyPath();
        }

        private string _buildHierarchyPath()
        {
            string path = name;
            Transform parent = transform.parent;

            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return $"{gameObject.scene.name}/{path}";
        }
    }
}