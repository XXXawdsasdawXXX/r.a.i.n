using System;
using Core.GameLoop;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using FishNet;
using FishNet.Broadcast;
using UnityEngine.Scripting;
using Channel = FishNet.Transporting.Channel;

namespace Game.Time
{
    [Preserve]
    public class GameTime : IService, ISubscriber, IInitializeListener, IUpdateListener
    {
        public struct GameTimeBroadcast : IBroadcast
        {
            public double TotalSeconds;
            public GameTimeBroadcast(double totalSeconds)
            {
                TotalSeconds = totalSeconds;
            }
        }

        public string RuntimeListenerName => "GameTime";
        public TimeSpan Current { get; private set; }

        private float _timeScale;
        private double _lastUpdateTime;


        public UniTask GameInitialize()
        {
            _timeScale = Container.Instance.GetConfig<GameTimeSettings>().TimeScale;
            
            return UniTask.CompletedTask;
        }

        public UniTask Subscribe()
        {
            InstanceFinder.ClientManager.RegisterBroadcast<GameTimeBroadcast>(_onServerSendChanged);
            
            return UniTask.CompletedTask;
        }

        public void Unsubscribe()
        {
            InstanceFinder.ClientManager.UnregisterBroadcast<GameTimeBroadcast>(_onServerSendChanged);
        }

        public void GameUpdate(float deltaTime)
        {
            if (InstanceFinder.IsServerStarted)
            {
                _updateServerTime(deltaTime);
            }
            else if (InstanceFinder.IsClientStarted)
            {
                _updateClientTime(deltaTime);
            }
        }

        private void _updateServerTime(float deltaTime)
        {
            Current += TimeSpan.FromSeconds(deltaTime * _timeScale);

            if (UnityEngine.Time.time - _lastUpdateTime >= 1.0f)
            {
                _lastUpdateTime = UnityEngine.Time.time;

                double totalSeconds = Current.TotalSeconds;
                        
                InstanceFinder.ServerManager.Broadcast(new GameTimeBroadcast(totalSeconds));
            }
        }

        private void _updateClientTime(float deltaTime)
        {
            Current += TimeSpan.FromSeconds(deltaTime * _timeScale);
        }

        private void _onServerSendChanged(GameTimeBroadcast broadcast, Channel _)
        {
            TimeSpan serverTime = TimeSpan.FromSeconds(broadcast.TotalSeconds);
           
            Current = serverTime;
        }
    }
}
