using Core.Network;
using Core.ServiceLocator;
using CoreGame.Card.Data;
using CoreGame.Card.Logic;
using Cysharp.Threading.Tasks;
using Essential;
using UI.Windows.Base;
using UI.Windows.Card.CardDeck.CardStep;

namespace UI.Windows.Game.Card.CardStep
{
    public class CardStepController : UIWindowController<CardStepView>
    {
        private BattleService _battleService;
        private UserProvider _userProvider;

        
        public override UniTask InitializeWindow(UIWindowManager manager)
        {
            _battleService = Container.Instance.GetService<BattleService>();
            _userProvider = Container.Instance.GetService<UserProvider>();
            
            return base.InitializeWindow(manager);
        }

        public override void SubscribeToEvents(bool flag)
        {
            base.SubscribeToEvents(flag);

            if (flag)
            {
                _battleService.BattleStarted += _subscribeToTimer;
                _battleService.BattleFinished += _dispose;
                _battleService.TurnStarted += _updateTurn;
                view.ButtonEndStep.Clicked += _endStep;
            }
            else
            {
                _battleService.BattleStarted -= _subscribeToTimer;
                _battleService.BattleFinished -= _dispose;
                _battleService.TurnStarted -= _updateTurn;
                view.ButtonEndStep.Clicked -= _endStep;
            }
        }

        private void _subscribeToTimer(BattleModel model)
        {
            model.TurnTimeRemaining.SubscribeProperty(view.SetTime);
        }

        private void _dispose(BattleModel model)
        {
            model.TurnTimeRemaining.UnsubscribeProperty(view.SetTime);
        }

        private void _endStep()
        {
            Log.Info(this, "click");
            _battleService.EndTurn();
        }

        private void _updateTurn(BattleModel model)
        {
            view.SetStep(model.TurnNumber.ToString());
        }
    }
}