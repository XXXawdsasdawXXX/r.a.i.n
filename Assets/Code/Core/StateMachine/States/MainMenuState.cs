using Core.GameLoop;
using Core.Scenes;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;

namespace Core.StateMachine
{
    public class MainMenuState : IState
    {
        public bool IsInitialized { get; private set; }

        private GameEventDispatcher _gameEventDispatcher;
        private SceneService _sceneService;

        public UniTask Initialize()
        {
            _gameEventDispatcher = Container.Instance.GetService<GameEventDispatcher>();
            _sceneService = Container.Instance.GetService<SceneService>();

            IsInitialized = true;
            
            return UniTask.CompletedTask;
        }

        public async UniTask Enter()
        {
            _gameEventDispatcher.Dispose();
            
            await _sceneService.LoadSceneAsync(EScene.Menu);
            
            Container.Instance.Context.Child = ContextBuilder.BuildContext();
            
            _gameEventDispatcher.Register(Container.Instance.GetGameListeners());
        }

        public UniTask Exit()
        {
            return UniTask.CompletedTask;
        }
    }
}