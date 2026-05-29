using Core.Data;
using Core.GameLoop;
using Core.ServiceLocator;
using CoreGame.Time;
using Cysharp.Threading.Tasks;
using UI.Windows.Base;

namespace UI.Windows.HUD
{
    public class HUDGameTimeController : UIWindowController<HUDGameTimeView>, IUpdateListener
    {
        public bool IsInitialized { get; set; }
        public string RuntimeListenerName => "HUDWindowController";
        
        private GameTime _gameTime;
        private Cache<int> _lastUpdateMinute;
        private float _currentValue;

        public override UniTask InitializeWindow(UIWindowManager manager)
        {
            _gameTime = Container.Instance.GetService<GameTime>();

            _lastUpdateMinute = new Cache<int>();

            return base.InitializeWindow(manager);
        }

        public override void StartWindow()
        {
            Open();
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