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
                _battleService.CardPlayed += _updateEnergy;
            }
            else
            {
                _battleService.CardPlayed -= _updateEnergy;
            }
        }

        private void _updateEnergy(BattleUnit unit, CardBattleState cardBattleState)
        {
            view.SetValue(unit.Energy, unit.MaxEnergy);
        }
    }
}