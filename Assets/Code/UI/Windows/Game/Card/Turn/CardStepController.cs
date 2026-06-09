using Core.ServiceLocator;
using CoreGame.Card.Data;
using CoreGame.Card.Logic;
using Cysharp.Threading.Tasks;
using UI.Windows.Base;

namespace UI.Windows.Game.Card.Turn
{
    public class CardStepController : UIWindowController<CardStepView>
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
            CommandResult result = _battleService.EndTurnWithResult();
        }

        private void _updateTurn(BattleModel model)
        {
            view.SetStep(model.TurnNumber.ToString());
            bool isMyTurn = model != null && model.Phase != null && model.Phase.Value == EBattlePhase.FirstSideTurn;
            view.SetEndStepVisible(isMyTurn);
        }
    }
}