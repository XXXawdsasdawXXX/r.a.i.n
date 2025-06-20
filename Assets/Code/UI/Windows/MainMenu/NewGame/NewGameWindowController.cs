using Core.GameLoop;
using Core.Save;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using UI.Windows.Base;

namespace UI.Windows.MainMenu.NewGame
{
    public class NewGameWindowController : UIWindowController<NewGameWindowView>
    {
        public bool IsInitialized { get; set; }
        
        private GameModel _gameModel;
        
        public override UniTask InitializeWindow()
        {
            _gameModel = Container.Instance.GetService<GameModel>();
        
            return UniTask.CompletedTask;
        }
        
        public override void SubscribeToEvents(bool flag)
        {
            if (flag)
            {
                view.ButtonCreate.Clicked += _addWorld;
            }
            else
            {
                
            }
        }

        private void _addWorld()
        {
            _gameModel.Worlds.Add(new WorldModel
            {
                CreateTime = default,
                GameTime = default,
            });
        }
    }
}