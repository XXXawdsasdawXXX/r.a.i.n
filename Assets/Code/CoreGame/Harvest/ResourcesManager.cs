using Core.GameLoop;
using Core.Network;
using Core.Save;
using Core.ServiceLocator;
using CoreGame.Entities.Characters.Controllers;
using CoreGame.Entities.Characters.Hero;
using Cysharp.Threading.Tasks;
using Essential;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace CoreGame.Harvest
{
    public class ResourcesManager : Essential.Mono, IService, IInitializeListener,ILoadListener ,ISubscriber
    {
        public bool IsInitialized { get; set; }

        [SerializeField] private ResourceCollection _resourceCollection;
        [SerializeField] private ResourceSource[] _resourcesSources;
        
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
            _userProvider.HeroCreated += _onHeroCreated;

            foreach (ResourceSource resource in _resourcesSources)
            {
                resource.HarvestStarted += OnHarvestStarted;
            }
        }

        public UniTask GameLoad(GameModel model)
        {
            foreach (ResourceSource resource in _resourcesSources)
            {
                Log.Info(this, $"load {resource.gameObject.name}");
                resource.SetValue(resource.Config.MaxValue);
            }
            
            return UniTask.CompletedTask;
        }

        public void Unsubscribe()
        {
            _userProvider.HeroCreated -= _onHeroCreated;
            
            foreach (ResourceSource resource in _resourcesSources)
            {
                resource.HarvestStarted -= OnHarvestStarted;
            }
        }

        private void _onHeroCreated()
        {
            _hero = _userProvider.GetHeroComponent<Hero>();
            Log.Info(this, $"_hero != null {_hero != null}", Log.Orange);
            _miner = _hero.GetCharacterComponent<Miner>();
            Log.Info(this, $"_miner != null {_miner != null}", Log.Orange);
        }

        private void OnHarvestStarted(ResourceSource resourceSource)
        {
            if (!_miner.IsMining)
            {
                _miner.StartHarvest(resourceSource);
            }
        }

#if UNITY_EDITOR

        [Button]
        private void _validateResources()
        {
            foreach (ResourceSource resource in _resourcesSources)
            {
                resource.Validate(_resourceCollection.Library.Get(resource.Type));
                EditorUtility.SetDirty(resource);
            }
            AssetDatabase.SaveAssets();
        }

        [Button]
        private void _findResources()
        {
            _resourcesSources = GetComponentsInChildren<ResourceSource>(true);
        }
#endif
    }
}