using System;
using Core.GameLoop;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using Essential;
using FishNet;
using FishNet.Broadcast;
using FishNet.Connection;
using UnityEngine;
using UnityEngine.Scripting;
using Channel = FishNet.Transporting.Channel;

namespace Game.World
{
    [Preserve]
    public class GameTime : IService, ISubscriber, IUpdateListener
    {
        public struct GameTimeBroadcast : IBroadcast
        {
            public double TotalSeconds;
            public GameTimeBroadcast(double totalSeconds)
            {
                TotalSeconds = totalSeconds;
            }
        }

        public TimeSpan Current { get; private set; }
        
        private const float TIME_SCALE = 6000f;
        
        private double _lastUpdateTime;


        public UniTask Subscribe()
        {
            InstanceFinder.ServerManager.RegisterBroadcast<GameTimeBroadcast>(_onClientRequestChanged);
            InstanceFinder.ClientManager.RegisterBroadcast<GameTimeBroadcast>(_onServerSendChanged);
            
            return UniTask.CompletedTask;
        }

        public void Unsubscribe()
        {
            InstanceFinder.ServerManager.UnregisterBroadcast<GameTimeBroadcast>(_onClientRequestChanged);
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
            // Учитываем множитель времени
            Current += TimeSpan.FromSeconds(deltaTime * TIME_SCALE);

            if (Time.time - _lastUpdateTime >= 1.0f)
            {
                _lastUpdateTime = Time.time;

                double totalSeconds = Current.TotalSeconds;
                        
                InstanceFinder.ServerManager.Broadcast(new GameTimeBroadcast(totalSeconds));
            }
        }

        private void _updateClientTime(float deltaTime)
        {
            // Клиент обновляет время локально, пока не получит синхронизацию с сервера
            Current += TimeSpan.FromSeconds(deltaTime * TIME_SCALE);
        }

        private void _onClientRequestChanged(NetworkConnection _, GameTimeBroadcast broadcast, Channel _2)
        {
            // Сервер получил от клиента (можно игнорировать, так как сервер сам управляет временем)
        }

        private void _onServerSendChanged(GameTimeBroadcast broadcast, Channel _)
        {
            // Клиент получил обновление от сервера и синхронизировал время
            TimeSpan serverTime = TimeSpan.FromSeconds(broadcast.TotalSeconds);
           
            Log.Info($"_onServerSendChanged {serverTime}", this);
            
            /*if (Current > serverTime)
            {
                return;
            }*/
            
            Current = serverTime;
        }
    }
}
