using System.Collections.Generic;
using Core.AssetManagement;
using Core.GameLoop;
using Core.Libraries.Assets;
using Core.Libraries.Installers;
using Core.Scenes;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.StateMachine
{
    public class CoreGameState : IState
    {
        public bool IsInitialized { get;set; }

        private InstallerLibrary _installerLibrary;
        private AssetLibrary _assetLibrary;
        
        private SceneService _sceneService;
        private GameEventDispatcher _gameEventDispatcher;

        private List<GameObject> _coreEntities = new();


        public UniTask Initialize()
        {
            _sceneService = Container.Instance.GetService<SceneService>();
            _gameEventDispatcher = Container.Instance.GetService<GameEventDispatcher>();
            
            _installerLibrary = Container.Instance.GetConfig<InstallerLibrary>();
            _assetLibrary = Container.Instance.GetConfig<AssetLibrary>();
            
            return UniTask.CompletedTask;
        }

        public async UniTask Enter()
        {
            await _sceneService.LoadSceneAsync(EScene.Game_0); //todo use player progress
            
            _coreEntities.Add(AssetProvider.Instantiate(_assetLibrary.Windows.Get(AssetKey.CANVAS_CORE_GAME)));
            _coreEntities.Add(AssetProvider.Instantiate(_assetLibrary.SceneComponents.Get(AssetKey.CAMERA))); 
            _coreEntities.Add(AssetProvider.Instantiate(_assetLibrary.SceneComponents.Get(AssetKey.POOL_HERO)));
            _coreEntities.Add(AssetProvider.Instantiate(_assetLibrary.SceneComponents.Get(AssetKey.POOL_ITEM)));
            
            ContextEntities coreGameContext = ContextBuilder.BuildContext(_installerLibrary.CoreGameInstaller.GetTypes());
            Container.Instance.Context.SetChildContext(coreGameContext);
            _gameEventDispatcher.Register(Container.Instance.GetGameListeners());

            SubscribeToEvents(true);
        }

        public UniTask Exit()
        {
            SubscribeToEvents(false);
            
            _gameEventDispatcher.Dispose();

            foreach (GameObject entity in _coreEntities)
            {
                Object.Destroy(entity);
            }
            _coreEntities.Clear();
            
            return UniTask.CompletedTask;
        }

        private void SubscribeToEvents(bool flag)
        {
            if (flag)
            {
                _sceneService.SceneUnloaded += OnSwitchScene;
                _sceneService.SceneLoaded += OnSwitchScene;
            }
            else
            {
                _sceneService.SceneUnloaded -= OnSwitchScene;
                _sceneService.SceneLoaded -= OnSwitchScene;
            }
        }

        private void OnSwitchScene(EScene obj)
        {
            _gameEventDispatcher.Dispose();
            
            ContextEntities sceneContext = ContextBuilder
                .BuildContext(_installerLibrary.GetSceneInstaller(obj)?.GetTypes()); 
            
            Container.Instance.Context.Child.SetChildContext(sceneContext);
         
            _gameEventDispatcher.Register(Container.Instance.GetGameListeners());
        }
    }
}