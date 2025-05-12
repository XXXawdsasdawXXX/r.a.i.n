using Core.AssetManagement;
using Core.GameLoop;
using Core.Libraries.Assets;
using Core.Libraries.Configs;
using Core.Libraries.Installers;
using Core.Save;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using Essential;
using UnityEngine;

namespace Core.StateMachine
{
    public class BootstrapState : IState
    {
        public bool IsInitialized { get; set; }

        private readonly GameStateMachine _gameStateMachine;

        public BootstrapState(GameStateMachine gameStateMachine)
        {
            _gameStateMachine = gameStateMachine;
        }

        public async UniTask Initialize()
        {
            InstallerLibrary installerLibrary = await AssetProvider
                .LoadScriptableObject<InstallerLibrary>(AssetKey.INSTALLER_LIBRARY_PATH);
            AssetLibrary assetLibrary = await AssetProvider
                .LoadScriptableObject<AssetLibrary>(AssetKey.ASSET_LIBRARY_PATH);
            ScriptableObject audioEventLibrary = await AssetProvider
                .LoadScriptableObject(AssetKey.AUDIO_LIBRARY_PATH);
            ConfigLibrary configLibrary = await AssetProvider
                .LoadScriptableObject<ConfigLibrary>(AssetKey.CONFIG_LIBRARY_PATH);

            if (Log.PROFILER_IS_ACTIVE)
            {
                AssetProvider.Instantiate(assetLibrary.Windows.Get(AssetKey.CANVAS_PROFILER));
            }

            ContextEntities projectContext = ContextBuilder.BuildContext(installerLibrary.ProjectsInstaller.GetTypes());
            projectContext.Services.Add(typeof(GameStateMachine), _gameStateMachine);
            Container container = new(projectContext);

            container.AddConfig(installerLibrary);
            container.AddConfig(assetLibrary);
            container.AddConfig(audioEventLibrary);
            foreach (ScriptableObject config in configLibrary.Configs)
            {
                container.AddConfig(config);
            }

            var saveService = container.GetService<SaveService>();
            var model = container.GetService<GameModel>();
            var loadedModel = saveService.LoadLast<GameModel>() ?? new GameModel(); 
            
            model.test = loadedModel.test;

            container.GetService<GameEventDispatcher>().Initialize();

            /*GameEventDispatcher gameEventDispatcher = container.GetService<GameEventDispatcher>();
           
            await gameEventDispatcher.Register(container.GetGameListeners());*/
        }

        public UniTask Enter()
        {
            _gameStateMachine.SwitchState(typeof(MainMenuState));

            return UniTask.CompletedTask;
        }

        public UniTask Exit()
        {
            return UniTask.CompletedTask;
        }
    }
}