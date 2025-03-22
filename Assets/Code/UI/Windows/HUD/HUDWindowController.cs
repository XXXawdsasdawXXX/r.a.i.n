using Core.Data;
using Core.GameLoop;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using Essential;
using Game.World;
using UI.Windows.Base;

namespace UI.Windows.HUD
{
    public class HUDWindowController : UIWindowController<HUDWindowView>,
        IInitializeListener,
        IStartListener,
        IUpdateListener
    {
        private GameTime _gameTime;

        private Cache<int> _lastUpdateMinute;
        private float _currentValue;

        public UniTask GameInitialize()
        {
            _gameTime = Container.Instance.GetService<GameTime>();

            _lastUpdateMinute = new Cache<int>();

            return UniTask.CompletedTask;
        }

        public UniTask GameStart()
        {
            view.Open();

            return UniTask.CompletedTask;
        }

        public void GameUpdate(float deltaTime)
        {
            if (_lastUpdateMinute.Update(_gameTime.Current.Hours * 60 + _gameTime.Current.Minutes))
            {
                float gameTimeNormalize = _lastUpdateMinute.Value / 1440f;
             
                view.ImageGameTime.SetFillAmount(gameTimeNormalize);
            }
        }
    }
}