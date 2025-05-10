using System.Collections.Generic;
using Core.AssetManagement;
using Core.GameLoop;
using Core.Libraries.Assets;
using Core.Libraries.Installers;
using Core.Scenes;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using Essential;
using UnityEngine;
using UnityEngine.SceneManagement;

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

            Container.Instance.Context.BuildChildContext(_installerLibrary.CoreGameInstaller.GetTypes());
            
            Log.Info(this, "build child context");
            Container.Instance.Context.Child.BuildChildContext();
           
            await _gameEventDispatcher.Register(Container.Instance.GetGameListeners());
            
            SubscribeToEvents(true);
        }

        public  UniTask Exit()
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

        private async void OnSwitchScene(EScene obj)
        {
            Log.Info(this, $"On switch scene {obj}", Color.magenta);
            
             _gameEventDispatcher.Dispose();

          
             
            Container.Instance.Context.Child.BuildChildContext(_installerLibrary.GetSceneInstaller(obj)?.GetTypes());
         
            await _gameEventDispatcher.Register(Container.Instance.GetGameListeners());
        }
    }
}