using System;
using Core.GameLoop;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using Essential;
using FishNet.Object;
using FishNet.Object.Synchronizing;

namespace Game.World
{
    public class GameTime : NetworkBehaviour, IService, ISubscriber, IUpdateListener
    {
        public TimeSpan Current => _time.Value;
        
        private readonly SyncVar<TimeSpan> _time = new();

        public override void OnStartClient()
        {
            enabled = IsOwner;
        }
        
        public void GameUpdate(float deltaTime)
        {
            _updateTime(deltaTime);
        }

        public UniTask Subscribe()
        {
            _time.OnChange += _onTimeChange;
            
            return UniTask.CompletedTask;
        }

        public void Unsubscribe()
        {
            _time.OnChange -= _onTimeChange;
        }

        [ServerRpc(RequireOwnership = false)]
        private void _updateTime(float second)
        {
            _time.Value += TimeSpan.FromSeconds(second * 1000);
        }

        private void _onTimeChange(TimeSpan prev, TimeSpan next, bool asserver)
        {
            //Log.Info($"{Current.}");
        }
    }
}