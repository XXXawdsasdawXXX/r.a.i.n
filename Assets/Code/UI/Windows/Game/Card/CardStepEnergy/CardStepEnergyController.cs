using Core.ServiceLocator;
using CoreGame.Card.Data;
using CoreGame.Card.Logic;
using Cysharp.Threading.Tasks;
using UI.Windows.Base;
using UI.Windows.Card.CardDeck.CardStepEnergy;

namespace UI.Windows.Game.Card.CardStepEnergy
{
    public class CardStepEnergyController : UIWindowController<CardStepEnergyView>
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
                _battleService.TurnStarted += _onStartNewTurn;
            }
            else
            {
                _battleService.TurnStarted -= _onStartNewTurn;
            }
        }

        private void _onStartNewTurn(BattleModel model)
        {
            if (model.Phase.Value is EBattlePhase.FirstSideTurn)
            {
                view.Open();
                _updateEnergy(model.SideA.Hero);
                _battleService.CardPlayed += _onCardPlayed;
            }
            else
            {
                _battleService.CardPlayed -= _onCardPlayed;
                view.Close();
            }
        }

        private void _onCardPlayed(BattleModel model)
        {
            _updateEnergy(model.SideA.Hero);    
        }

        private void _updateEnergy(BattleUnit unit)
        {
            view.SetValue(unit.Energy, unit.MaxEnergy);
        }
    }
}