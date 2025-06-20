using Core.AssetManagement;
using Core.GameLoop;
using Core.Libraries.Assets;
using Core.Scenes;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using Essential;

namespace Core.StateMachine
{
    public class MainMenuState : IState
    {
        public bool IsInitialized { get; set; }

        private GameEventDispatcher _gameEventDispatcher;
        private SceneService _sceneService;

        private AssetLibrary _assetLibrary;

        public UniTask Initialize()
        {
            _gameEventDispatcher = Container.Instance.GetService<GameEventDispatcher>();

            _sceneService = Container.Instance.GetService<SceneService>();

            _assetLibrary = Container.Instance.GetConfig<AssetLibrary>();

            return UniTask.CompletedTask;
        }

        public async UniTask Enter()
        {
            await _sceneService.LoadSceneAsync(EScene.Menu);

            AssetProvider.Instantiate(_assetLibrary.UICanvases.Get(AssetKey.CANVAS_MAIN_MENU));
            
            Container.Instance.Context.BuildChildContext();

            await _gameEventDispatcher.Register(Container.Instance.GetGameListeners());
        }

        public UniTask Exit()
        {
            _gameEventDispatcher.Dispose();

            Container.Instance.Context.Child = null;

            return UniTask.CompletedTask;
        }
    }
}