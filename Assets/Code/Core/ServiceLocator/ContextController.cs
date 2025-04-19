using Core.GameLoop;
using Core.Scenes;
using Cysharp.Threading.Tasks;

namespace Core.ServiceLocator
{
    public class ContextController : IMono, IInitializeListener, ISubscriber
    {
        private SceneService _sceneService;

        public UniTask GameInitialize()
        {
            _sceneService = Container.Instance.GetService<SceneService>();
            
            return UniTask.CompletedTask;
        }
        
        public UniTask Subscribe()
        {
            _sceneService.SceneUnloaded += OnSceneUnloaded;
            _sceneService.SceneLoaded += OnSceneLoaded;

            return UniTask.CompletedTask;
        }

        public void Unsubscribe()
        {
            _sceneService.SceneUnloaded -= OnSceneUnloaded;
            _sceneService.SceneLoaded -= OnSceneLoaded;
        }
        
        private void OnSceneLoaded(EScene obj)
        {

        }

        private void OnSceneUnloaded(EScene obj)
        {
            
        }
    }
}