using Core.ServiceLocator;
using CoreGame.Card.Data;
using CoreGame.Card.Logic;
using Cysharp.Threading.Tasks;
using UI.Windows.Base;

namespace UI.Windows.Card.CardDeck
{
    public class CardWindowController : UIWindowController<CardWindowView>
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
                _battleService.BattleStarted += _openView;
                _battleService.BattleFinished += _closeView;
            }
            else
            {
                _battleService.BattleStarted -= _openView;
                _battleService.BattleFinished -= _closeView;
            }
        }

        private void _closeView(BattleModel _)
        {
            view.Close();
        }

        private void _openView(BattleModel _)
        {
            view.Open();
        }
    }
}