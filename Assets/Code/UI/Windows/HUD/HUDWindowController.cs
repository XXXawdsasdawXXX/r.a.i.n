using System;
using Core.GameLoop;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
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

        public UniTask GameInitialize()
        {
            _gameTime = Container.Instance.GetService<GameTime>();
            
            return UniTask.CompletedTask;
        }

        public UniTask GameStart()
        {
            view.Open();
            
            return UniTask.CompletedTask;
        }

        public void GameUpdate(float deltaTime)
        {
            float gameTimeNormalize = _gameTime.Current.Hours / 24f;
            
            view.ImageGameTime.SetFillAmount(gameTimeNormalize);
        }
    }
}