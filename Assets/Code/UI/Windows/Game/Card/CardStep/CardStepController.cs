using Core.Network;
using Core.ServiceLocator;
using CoreGame.Card.Data;
using CoreGame.Card.Logic;
using Cysharp.Threading.Tasks;
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
                _battleService.TurnStarted += _updateTurn;
                view.ButtonEndStep.Clicked += _endStep;
            }
            else
            {
                _battleService.TurnStarted -= _updateTurn;
                view.ButtonEndStep.Clicked -= _endStep;
            }
        }

        private void _endStep()
        {
            _battleService.EndTurn(_userProvider.Id);
        }

        private void _updateTurn(BattleModel obj)
        {
            view.SetStep(obj.TurnNumber.ToString());
        }
    }
}