using Core.GameLoop;
using Cysharp.Threading.Tasks;
using Essential;
using FishNet;
using FishNet.Connection;
using UnityEngine;
using Channel = FishNet.Transporting.Channel;

namespace Game.InteractionObjects.Activators
{
    public class Laptop : InteractionObject, ISubscriber
    {
        [SerializeField] private GameObject _viewDisplay;

        private ActivatorBroadcast _activatorBroadcast;

        public override void StartInteraction()
        {
            _viewDisplay.SetActive(!_activatorBroadcast.IsActive);

            base.StartInteraction();
        }

        public UniTask Subscribe()
        {
            Trigger.InteractionPerformed += _onInteractionPerformed;
            InstanceFinder.ClientManager.RegisterBroadcast<ActivatorBroadcast>(_onServerSendChanged);
            InstanceFinder.ServerManager.RegisterBroadcast<ActivatorBroadcast>(_onClientRequestChanged);

            return UniTask.CompletedTask;
        }

        public void Unsubscribe()
        {
            Trigger.InteractionPerformed -= _onInteractionPerformed;
            InstanceFinder.ClientManager.UnregisterBroadcast<ActivatorBroadcast>(_onServerSendChanged);
            InstanceFinder.ServerManager.UnregisterBroadcast<ActivatorBroadcast>(_onClientRequestChanged);
        }

        private void _onInteractionPerformed()
        {
            if (InstanceFinder.IsClientStarted)
            {
                InstanceFinder.ClientManager.Broadcast(_getActivatorBroadcast());
            }
            else if (InstanceFinder.IsServerStarted)
            {
                InstanceFinder.ServerManager.Broadcast(_getActivatorBroadcast());
            }
        }

        private void _onClientRequestChanged(NetworkConnection network, ActivatorBroadcast broadcast, Channel channel)
        {
            InstanceFinder.ServerManager.Broadcast(broadcast);
        }

        private void _onServerSendChanged(ActivatorBroadcast broadcast, Channel channel)
        {
            _activatorBroadcast = broadcast;

            StartInteraction();
        }

        private ActivatorBroadcast _getActivatorBroadcast()
        {
            return string.IsNullOrEmpty(_activatorBroadcast.ObjectID)
                ? new ActivatorBroadcast()
                {
                    ObjectID = gameObject.GetInstanceID().ToString(),
                    IsActive = true
                }
                : new ActivatorBroadcast()
                {
                    ObjectID = _activatorBroadcast.ObjectID,
                    IsActive = !_activatorBroadcast.IsActive
                };
        }
    }
}