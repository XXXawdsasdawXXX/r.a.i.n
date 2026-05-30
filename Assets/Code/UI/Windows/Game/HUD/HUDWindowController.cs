using Core.ServiceLocator;
using CoreGame.Card.Data;
using CoreGame.Card.Logic;
using Cysharp.Threading.Tasks;
using UI.Windows.Base;

namespace UI.Windows.Game.HUD
{
    public class HUDWindowController : UIWindowController<HUDWindowView>
    {
        private BattleService _battleService;

        
        public override UniTask InitializeWindow(UIWindowManager manager)
        {
            _battleService = Container.Instance.GetService<BattleService>();
            
            return base.InitializeWindow(manager);
        }

        public override void SubscribeToEvents(bool flag)
        {
            base.SubscribeToEvents(flag);

            if (flag)
            {
                _battleService.BattleStarted += _closeHud;
                _battleService.BattleFinished += _openHud;
            }
            else
            {
                _battleService.BattleStarted -= _closeHud;
                _battleService.BattleFinished -= _openHud;
            }
        }

        private void _openHud(BattleModel _)
        {
            view.Open();
        }

        private void _closeHud(BattleModel _)
        {
            view.Close();
        }
    }
}