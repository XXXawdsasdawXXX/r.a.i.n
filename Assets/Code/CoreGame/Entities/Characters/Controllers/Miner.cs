using System;
using Core.Data;
using Core.GameLoop;
using Core.ServiceLocator;
using CoreGame.Entities.Characters.Interfaces;
using CoreGame.Entities.Params;
using CoreGame.Harvest;
using Essential;

namespace CoreGame.Entities.Characters.Controllers
{
    public class Miner : IUpdateListener, ICharacterComponent
    {
        public event Action Started;
        public event Action Ended;
        public string RuntimeListenerName => "Mainer";

        public Condition Condition { get; } = new();
        public MinerProcess Process { get; private set; }
        public bool IsMining { get; private set; }

        private readonly ResourceStorage _resourceStorage;
        
        private readonly IHarvestAnimator _animator;
        private readonly Health _health;

        public Miner(IHarvestAnimator animator, Health health)
        {
            _resourceStorage = Container.Instance.GetService<ResourceStorage>();
            
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

            if (Process.CurrentTime >= Process.Resource.Config.HarvestTime)
            {
                _health.UpdateHealth(-Process.Resource.Config.HealthPrice);

                Process.Resource.UpdateValue(-Process.Resource.Config.ResourcePerTick);

                _resourceStorage.Add(Process.Resource.Type, Process.Resource.Config.ResourcePerTick);
                
                if (Process.Resource.IsEnded)
                {
                    StopHarvest();
                }
                else
                {
                    Process.CurrentTime = 0;
                }
            }
        }

        public void StartHarvest(Resource resource)
        {
            Process = new MinerProcess
            {
                Resource = resource,
                CurrentTime = 0,
            };

            _animator.StartMine(resource.Config.HarvestType);
            
            Log.Info(this, $"{resource.Config.HarvestType}");

            IsMining = true;

            Started?.Invoke();
        }

        public void StopHarvest()
        {
            Process = null;

            _animator.StopMine();

            IsMining = false;

            Ended?.Invoke();
        }
    }

    public class MinerProcess
    {
        public Resource Resource;
        public float CurrentTime;
    }
}