using Core.AssetManagement;
using Core.GameLoop;
using Core.Libraries.Installers;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.StateMachine
{
    public class BootstrapState : IState
    {
        public bool IsInitialized { get; private set; }
        
        private readonly GameStateMachine _gameStateMachine;
        
        public BootstrapState(GameStateMachine gameStateMachine)
        {
            _gameStateMachine = gameStateMachine;
        }
        
        public async UniTask Initialize()
        {
            InstallerLibrary installerLibrary = await AssetProvider
                .LoadScriptableObject<InstallerLibrary>(AssetPath.INSTALLER_LIBRARY_PATH);

            ContextEntities projectContext = ContextBuilder.BuildContext(installerLibrary.ProjectsInstaller.GetTypes());
            
            Container container = new(projectContext);
            
            container.AddConfig(installerLibrary);
            ScriptableObject audioEventLibrary = await AssetProvider.LoadScriptableObject(AssetPath.AUDIO_LIBRARY_PATH); 
        
            container.AddConfig(audioEventLibrary);
            
            ScriptableObject assetLibrary = await AssetProvider.LoadScriptableObject(AssetPath.ASSET_LIBRARY_PATH); 
            container.AddConfig(assetLibrary);
            
            GameEventDispatcher gameEventDispatcher = container.GetService<GameEventDispatcher>();
            gameEventDispatcher.Register(container.GetGameListeners());
            
            container.Context.Services.Add(typeof(GameStateMachine), _gameStateMachine);
            
            IsInitialized = true;
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