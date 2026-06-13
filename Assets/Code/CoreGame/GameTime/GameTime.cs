using System;
using Core.GameLoop;
using Core.Save;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using FishNet;
using FishNet.Broadcast;
using UnityEngine.Scripting;
using Channel = FishNet.Transporting.Channel;

namespace CoreGame.GameTime
{
    [Preserve]
    public class GameTime : IService, ISubscriber, IInitializeListener, IUpdateListener, ILoadListener
    {
        public struct GameTimeBroadcast : IBroadcast
        {
            public double TotalSeconds;
            public GameTimeBroadcast(double totalSeconds)
            {
                TotalSeconds = totalSeconds;
            }
        }

        public bool IsInitialized { get; set; }
        public string RuntimeListenerName => "GameTime";
        public TimeSpan Current => _gameModel.World.GameTime;

        private GameModel _gameModel;
        private float _timeScale;
        private double _lastUpdateTime;

        public UniTask Initialize()
        {
            _timeScale = Container.Instance.GetSO<GameTimeSettings>().TimeScale;

            _gameModel = Container.Instance.GetService<GameModel>();
            
            return UniTask.CompletedTask;;
        }

        public void Subscribe()
        {
            InstanceFinder.ClientManager.RegisterBroadcast<GameTimeBroadcast>(_onServerSendChanged);
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
            _gameModel.World.GameTime += TimeSpan.FromSeconds(deltaTime * _timeScale);

            if (UnityEngine.Time.time - _lastUpdateTime >= 1.0f)
            {
                _lastUpdateTime = UnityEngine.Time.time;

                double totalSeconds = Current.TotalSeconds;
                        
                InstanceFinder.ServerManager.Broadcast(new GameTimeBroadcast(totalSeconds));
            }
        }

        private void _updateClientTime(float deltaTime)
        {
            _gameModel.World.GameTime += TimeSpan.FromSeconds(deltaTime * _timeScale);
        }

        private void _onServerSendChanged(GameTimeBroadcast broadcast, Channel _)
        {
            TimeSpan serverTime = TimeSpan.FromSeconds(broadcast.TotalSeconds);
           
            _gameModel.World.GameTime = serverTime;
        }

        public UniTask GameLoad(GameModel model)
        {
            return UniTask.CompletedTask;
        }
    }
}
