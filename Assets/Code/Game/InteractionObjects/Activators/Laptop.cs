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

            Log.Info($"change display state {!_activatorBroadcast.IsActive}", this);

            base.StartInteraction();
        }

        public UniTask Subscribe()
        {
            Trigger.InteractionPerformed += OnInteractionPerformed;
            InstanceFinder.ClientManager.RegisterBroadcast<ActivatorBroadcast>(OnServerSendChanged);
            InstanceFinder.ServerManager.RegisterBroadcast<ActivatorBroadcast>(OnClientRequestChanged);

            Log.Info("subscribe", this);
            return UniTask.CompletedTask;
        }

        public void Unsubscribe()
        {
            Trigger.InteractionPerformed -= OnInteractionPerformed;
            InstanceFinder.ClientManager.UnregisterBroadcast<ActivatorBroadcast>(OnServerSendChanged);
            InstanceFinder.ServerManager.UnregisterBroadcast<ActivatorBroadcast>(OnClientRequestChanged);
            Log.Info("unsubscribe", this);
        }

        private void OnInteractionPerformed()
        {
            Log.Info($"OnInteractionPerformed; " +
                     $"client: {InstanceFinder.IsClientStarted}; " +
                     $"server: {InstanceFinder.IsServerStarted}", this);

            if (InstanceFinder.IsClientStarted)
            {
                InstanceFinder.ClientManager.Broadcast(GetActivatorBroadcast());
            }
            else if (InstanceFinder.IsServerStarted)
            {
                InstanceFinder.ServerManager.Broadcast(GetActivatorBroadcast());
            }
        }

        private void OnClientRequestChanged(NetworkConnection network, ActivatorBroadcast broadcast, Channel channel)
        {
            Log.Info("On Client Request Changed", this);

            InstanceFinder.ServerManager.Broadcast(broadcast);
        }

        private void OnServerSendChanged(ActivatorBroadcast broadcast, Channel channel)
        {
            Log.Info("On Server Send Changed", this);

            _activatorBroadcast = broadcast;

            StartInteraction();
        }

        private ActivatorBroadcast GetActivatorBroadcast()
        {
            return string.IsNullOrEmpty(_activatorBroadcast.ObjectID)
                ? new ActivatorBroadcast()
                {
                    ObjectID = ID.Value,
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