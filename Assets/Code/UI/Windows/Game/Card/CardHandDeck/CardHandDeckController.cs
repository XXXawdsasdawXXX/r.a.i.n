using Core.Network;
using Core.ServiceLocator;
using CoreGame.Entities.Characters.Hero;
using Cysharp.Threading.Tasks;
using UI.Windows.Base;

namespace UI.Windows.Card.CardDeck
{
    public class CardHandDeckController : UIWindowController<CardHandDeckView>
    {
        public bool IsInitialized { get; set; }
        
        private UserProvider _userProvider;

        
        public override UniTask InitializeWindow(UIWindowManager manager)
        {
            _userProvider = Container.Instance.GetService<UserProvider>();
            
            return base.InitializeWindow(manager);
        }

        public override void SubscribeToEvents(bool flag)
        {
            base.SubscribeToEvents(flag);

            if (flag)
            {
                _userProvider.HeroCreated += _onHeroCreated;
            }
            else
            {
                _userProvider.HeroCreated -= _onHeroCreated;
            }
        }

        private void _onHeroCreated()
        {
            view.SetHeroStats(_userProvider.GetHeroComponent<Hero>().Model.Stats);   
        }

        private void SetHandCards()
        {
            //view.SetCards(); //todo set dech
        }
    }
}