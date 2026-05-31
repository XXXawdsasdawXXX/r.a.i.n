using Core.Network;
using Core.ServiceLocator;
using CoreGame.Card.Data;
using CoreGame.Card.Logic;
using CoreGame.Entities.Characters.Hero;
using Cysharp.Threading.Tasks;
using UI.Windows.Base;

namespace UI.Windows.Card.CardDeck
{
    public class CardHandDeckController : UIWindowController<CardHandDeckView>
    {
        public bool IsInitialized { get; set; }
        
        private UserProvider _userProvider;
        private BattleService _battleService;


        public override UniTask InitializeWindow(UIWindowManager manager)
        {
            _userProvider = Container.Instance.GetService<UserProvider>();
            _battleService = Container.Instance.GetService<BattleService>();
            
            view.InitializePool();
            
            return base.InitializeWindow(manager);
        }

        public override void SubscribeToEvents(bool flag)
        {
            base.SubscribeToEvents(flag);

            if (flag)
            {
                _userProvider.HeroCreated += _onHeroCreated;
                _battleService.TurnStarted += _updateHand;
            }
            else
            {
                _userProvider.HeroCreated -= _onHeroCreated;
                _battleService.TurnStarted -= _updateHand;
            }
        }

        private void _onHeroCreated()
        {
            view.SetHeroStats(_userProvider.GetHeroComponent<Hero>().Model.Stats);   
        }

        private void _updateHand(BattleModel battleModel)
        {
            string myId = _userProvider.Id;
    
            BattleSide mySide = battleModel.SideA.Hero.OwnerId == myId
                ? battleModel.SideA
                : battleModel.SideB;

            bool isMyTurn = battleModel.SideA.Hero.OwnerId == myId;

            view.SetHeroStats(mySide.Hero.Stats);
            view.SetCards(mySide.Hand);
            view.SetInteractable(isMyTurn);
        }
    }
}