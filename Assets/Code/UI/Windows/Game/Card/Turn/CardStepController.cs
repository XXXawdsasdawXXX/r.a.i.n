using Core.Network;
using Core.ServiceLocator;
using CoreGame.Card.Data;
using CoreGame.Card.Logic;
using CoreGame.Entities.Characters.Hero;
using Cysharp.Threading.Tasks;
using UI.Windows.Base;

namespace UI.Windows.Game.Card.Turn
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
            _battleService.EndTurnWithResult(_getLocalHeroId());
        }

        private void _updateTurn(BattleModel model)
        {
            view.SetStep(model.TurnNumber.ToString());
            bool isMyTurn = BattleParticipantHelper.IsMyTurn(model, _getLocalHeroId());
            view.SetEndStepVisible(isMyTurn);
        }

        private string _getLocalHeroId()
        {
            if (!string.IsNullOrEmpty(_userProvider.Id))
            {
                return _userProvider.Id;
            }

            Hero hero = _userProvider.GetHeroComponent<Hero>();
            return hero?.Model?.HeroId;
        }
    }
}
