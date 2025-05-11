using Code.CoreGame.Entities.Characters;
using Code.CoreGame.Entities.Characters.Controllers;
using Code.CoreGame.Entities.Characters.Hero;
using Core.GameLoop;
using Core.Network;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using Essential;
using UnityEngine;

namespace Code.CoreGame.Harvest
{
    public class ResourcesManager : Essential.Mono, IService, IInitializeListener, ISubscriber
    {
        public bool IsInitialized { get; set; }

        [SerializeField] private Resource[] _resources;
        
        private UserProvider _userProvider;
        private Hero _hero;
        private Miner _miner;

        public UniTask Initialize()
        {
            _userProvider = Container.Instance.GetService<UserProvider>();
            
            return UniTask.CompletedTask;
        }
        
        public void Subscribe()
        {
            Log.Info(this, $"subscribe");
            _userProvider.HeroSetted += OnHeroSetted;

            foreach (Resource resource in _resources)
            {
                resource.Mained += OnMained;
            }
        }

        private void OnMained(Resource obj)
        {
            if (!_miner.IsMining)
            {
                _miner.StartHarvest(obj);
            }
        }

        public void Unsubscribe()
        {
            _userProvider.HeroSetted -= OnHeroSetted;
            
            foreach (Resource resource in _resources)
            {
                resource.Mained -= OnMained;
            }
        }

        private void OnHeroSetted()
        {
            _hero = _userProvider.GetHeroComponent<Hero>();
            Log.Info(this, $"_hero != null {_hero != null}", Log.Orange);
            _miner = _hero.GetCharacterComponent<Miner>();
            Log.Info(this, $"_miner != null {_miner != null}", Log.Orange);
        }
    }
}