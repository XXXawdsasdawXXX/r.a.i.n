using Core.AssetManagement;
using Core.GameLoop;
using Core.Localization;
using Core.Libraries.Assets;
using Core.Libraries.Configs;
using Core.Libraries.Installers;
using Core.Save;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;

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
            Container container = await InitializeProjectContext();

            await container.GetService<LocalizationService>().Initialize();
            container.GetService<SaveService>().Initialize().Forget();
            container.GetService<GameEventDispatcher>().Initialize();
            
            LoadGame(container);
        }

        private async UniTask<Container> InitializeProjectContext()
        {
            InstallerStorage installerStorage = await AssetProvider
                .LoadScriptableObject<InstallerStorage>(AssetKey.INSTALLER_STORAGE_PATH);
            
            ConfigStorage configStorage = await AssetProvider
                .LoadScriptableObject<ConfigStorage>(AssetKey.CONFIG_STORAGE_PATH);

            AssetLibrary assetLibrary = configStorage.Get<AssetLibrary>();

            AssetProvider.Instantiate(assetLibrary.SceneComponents.Get(AssetKey.CAMERA));

            if (Profiler.enabled)
            {
                AssetProvider.Instantiate(assetLibrary.UICanvases.Get(AssetKey.CANVAS_PROFILER));
            }

            ContextEntities projectContext = ContextBuilder.BuildContext(installerStorage.ProjectsInstaller.GetTypes());
            projectContext.Services.Add(typeof(GameStateMachine), _gameStateMachine);
            
            Container container = new(projectContext);
            container.AddConfig(installerStorage);
            
            foreach (ScriptableObject config in configStorage.Configs)
            {
                container.AddConfig(config);
            }

            return container;
        }

        private static void LoadGame(Container container)
        {
            SaveService saveService = container.GetService<SaveService>();
            GameModel model = container.GetService<GameModel>();

            GameModel loadedModel = saveService.LoadLastGameModel();

            model.CopyFrom(loadedModel);
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