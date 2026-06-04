using Core.Network;
using Core.ServiceLocator;
using CoreGame.Card.Data;
using CoreGame.Card.Logic;
using CoreGame.Entities.Characters.Hero;
using Cysharp.Threading.Tasks;
using Essential;
using UI.Windows.Base;

namespace UI.Windows.Game.Card.CardHandDeck
{
    public class CardHandDeckController : UIWindowController<CardHandDeckView>
    {
        private UserProvider _userProvider;
        private BattleService _battleService;
        private BattleModel _battleModel;


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
                _battleService.TurnStarted += _onBattleUpdated;
                _battleService.CardPlayed += _onBattleUpdated;
            }
            else
            {
                _userProvider.HeroCreated -= _onHeroCreated;
                _battleService.TurnStarted -= _onBattleUpdated;
                _battleService.CardPlayed -= _onBattleUpdated;
            }
        }

        private void _onHeroCreated()
        {
            view.SetHeroStats(_userProvider.GetHeroComponent<Hero>().Model.Stats);   
        }

        private void _onBattleUpdated(BattleModel battleModel)
        {
            _battleModel = battleModel;
            _refreshHand();

            if (_isMyTurn(_battleModel, _userProvider.Id))
            {
                view.Open();
            }
            else
            {
                view.Close();
            }
        }

        private void _refreshHand()
        {
            if (_battleModel == null)
            {
                return;
            }

            string myId = _userProvider.Id;
            BattleSide mySide = _getMySide(_battleModel, myId);
            bool isMyTurn = _isMyTurn(_battleModel, myId);

            view.SetHeroStats(mySide.Hero.Stats);
            view.DisplayHand(mySide.Hero.Hand, _onCardClicked);
            view.SetInteractable(isMyTurn);
        }

        private void _onCardClicked(string cardId)
        {
            Log.Info(this, $"try play card enter {cardId} {_battleModel == null} || {!_isMyTurn(_battleModel, _userProvider.Id)}");
            if (_battleModel == null || !_isMyTurn(_battleModel, _userProvider.Id))
            {
                return;
            }

            // TODO: ICardTargetResolver — self / ally / enemy / multi / AoE после выбора карты
            string targetId = _getDefaultTargetId(_battleModel, _userProvider.Id);
            if (!_battleService.TryPlayCard(cardId, targetId))
            {
                Log.Info(this, $"Card play rejected. cardId={cardId}, target={targetId}");
                return;
            }

            Log.Info(this, $"try play card seccuses {cardId}");
            _refreshHand();
        }

        private static BattleSide _getMySide(BattleModel battle, string playerId)
        {
            return battle.SideA.Hero.UnitId == playerId
                ? battle.SideA
                : battle.SideB;
        }

        /// <summary>Временно: только герой противника. Заменить на <see cref="CoreGame.Card.Logic.Targeting.ICardTargetResolver"/>.</summary>
        private static string _getDefaultTargetId(BattleModel battle, string playerId)
        {
            BattleSide enemySide = battle.SideA.Hero.UnitId == playerId
                ? battle.SideB
                : battle.SideA;

            return enemySide.Hero.UnitId;
        }

        private static bool _isMyTurn(BattleModel battle, string playerId)
        {
            bool isSideA = battle.SideA.Hero.UnitId == playerId;

            return battle.Phase.Value switch
            {
                EBattlePhase.FirstSideTurn => isSideA,
                EBattlePhase.SecondSideTurn => !isSideA,
                _ => false
            };
        }
    }
}
