using System;
using Code.CoreGame.Entities.Characters.Interfaces;
using Code.CoreGame.Entities.Params;
using Code.CoreGame.Harvest;
using Core.Data;
using Core.GameLoop;
using Core.Network;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;

namespace Code.CoreGame.Entities.Characters.Controllers
{
    public class Miner : IUpdateListener, ICharacterComponent
    {
        public event Action Started;
        public event Action Ended;
        public string RuntimeListenerName => "Mainer";

        public Condition Condition { get; } = new ();
        public MinerProcess Process { get; private set; }
        public bool IsMining { get; private set; }

        private readonly IHarvestAnimator _animator;
        private readonly Health _health;

        public Miner(IHarvestAnimator animator, Health health)
        {
            _animator = animator;
            _health = health;
        }

        public void GameUpdate(float deltaTime)
        {
            if (!IsMining || !Condition.AreMet())
            {
                if (IsMining)
                {
                    StopHarvest();
                }
                
                return;
            }

            Process.CurrentTime += deltaTime;

            if (Process.CurrentTime >= Process.MaxTime)
            {
               _health.UpdateHealth(Process.Health);
                
                Process.CurrentTime = 0;
            }
        }

        public void StartHarvest(Resource resource)
        {
            Process = new MinerProcess
            {
                MaxTime = 5,
                CurrentTime = 0,
                Health = -3
            };
            
            _animator.StartHarvest();

            IsMining = true;
            
            Started?.Invoke();
        }

        public void StopHarvest()
        {
            Process = null;
            
            _animator.StopHarvest();

            IsMining = false;
            
            Ended?.Invoke();
        }
    }

    public class MinerProcess
    {
        public float MaxTime;
        public float CurrentTime;
        public int Health;
    }
}