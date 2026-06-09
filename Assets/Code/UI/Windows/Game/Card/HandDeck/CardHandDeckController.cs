using System.Linq;
using Core.Network;
using Core.ServiceLocator;
using CoreGame.Card.Data;
using CoreGame.Card.Logic;
using CoreGame.Entities.Characters.Hero;
using Cysharp.Threading.Tasks;
using UI.Windows.Game.Card;
using UI.Windows.Base;
using UnityEngine;

namespace UI.Windows.Game.Card.HandDeck
{
    public class CardHandDeckController : UIWindowController<CardHandDeckView>
    {
        private UserProvider _userProvider;
        private BattleService _battleService;
        private BattleModel _battleModel;
        
        [SerializeField] private CardWindowController _cardWindowController;


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
                _battleService.BattleStarted += _onBattleUpdated;
                _battleService.TurnStarted += _onBattleUpdated;
                _battleService.CardPlayed += _onBattleUpdated;
            }
            else
            {
                _battleService.BattleStarted -= _onBattleUpdated;
                _battleService.TurnStarted -= _onBattleUpdated;
                _battleService.CardPlayed -= _onBattleUpdated;
            }
        }

        private void _onBattleUpdated(BattleModel battleModel)
        {
            _battleModel = battleModel;
            _refreshHand();

            if (_isMyTurn(_battleModel, _getLocalHeroId()))
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

            string myId = _getLocalHeroId();
            BattleSide mySide = _getMySide(_battleModel, myId);
            bool isMyTurn = _isMyTurn(_battleModel, myId);

            view.SetHeroStats(mySide.Hero.Stats);
            view.DisplayHand(mySide.GetHandForOwner(myId), _onCardClicked);
            view.SetInteractable(isMyTurn);
        }

        private void _onCardClicked(string cardId)
        {
            string myId = _getLocalHeroId();

            if (_battleModel == null || !_isMyTurn(_battleModel, myId))
            {
                return;
            }

            BattleSide mySide = _getMySide(_battleModel, myId);
            if (mySide?.Hero == null)
            {
                return;
            }

            CardBattleState card = _findCard(mySide, cardId);
            if (card == null)
            {
                _showCommandError(CommandResult.CardNotFound);
                return;
            }

            if (!CardPlayRules.CanPlayCard(mySide.Hero, card))
            {
                _showCommandError(CardPlayRules.GetPlayRejectionReason(mySide.Hero, card));
                return;
            }

            _cardWindowController?.ClearCommandMessage();
            if (_isMoveCard(card) && _cardWindowController != null)
            {
                if (_cardWindowController.TrySelectMoveTarget(cardId))
                {
                    return;
                }
            }
            
            if (_isSummonCard(card) && _cardWindowController != null)
            {
                if (_cardWindowController.TrySelectSummonCell(cardId))
                {
                    return;
                }
            }
            
            if (_cardWindowController != null && _cardWindowController.TrySelectCardTarget(card, mySide))
            {
                return;
            }

            string targetId = _resolveTargetId(_battleModel, myId, cardId);
            CommandResult playResult = _battleService.TryPlayCardWithResult(cardId, targetId);
            if (playResult != CommandResult.Success)
            {
                _showCommandError(playResult);
                return;
            }

            _refreshHand();
        }

        private void _showCommandError(CommandResult result)
        {
            _cardWindowController?.ShowCommandMessage(CommandResultText.ToDebugText(result));
        }

        private static BattleSide _getMySide(BattleModel battle, string playerId)
        {
            BattleSide side = BattleParticipantResolver.GetSideForPlayer(battle, playerId);
            return side ?? battle.SideA;
        }

        private static bool _isMyTurn(BattleModel battle, string playerId)
        {
            return BattleParticipantResolver.IsMyTurn(battle, playerId);
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

        /// <summary>
        /// UI-MVP резолвер цели: до реализации выбора юнита/клетки в поле.
        /// Позже заменить на отдельный ICardTargetResolver + интерактивный выбор цели.
        /// </summary>
        private static string _resolveTargetId(BattleModel battle, string playerId, string cardId)
        {
            BattleSide mySide = _getMySide(battle, playerId);
            BattleSide enemySide = _getEnemySide(battle, mySide);

            CardBattleState selectedCard = _findCard(mySide, cardId);
            if (selectedCard?.Config?.Effects == null || selectedCard.Config.Effects.Count == 0)
            {
                return enemySide.Hero.UnitId;
            }

            // Если у карты есть любой enemy-target эффект, целимся во вражеского героя.
            if (selectedCard.Config.Effects.Any(effect => _isEnemyTarget(effect.Target)))
            {
                return enemySide.Hero.UnitId;
            }

            // Иначе self/ally карта — кидаем на своего героя (удобно для теста бафов/хила).
            return mySide.Hero.UnitId;
        }

        private static CardBattleState _findCard(BattleSide mySide, string cardId)
        {
            string ownerId = mySide?.Hero?.UnitId;
            return CardPlayRules.FindCardInHand(mySide?.GetHandForOwner(ownerId), cardId);
        }

        private static bool _isEnemyTarget(EEffectTarget target)
        {
            return target switch
            {
                EEffectTarget.SelectedEnemy => true,
                EEffectTarget.AllEnemies => true,
                EEffectTarget.EnemyFrontline => true,
                EEffectTarget.EnemyBackline => true,
                EEffectTarget.EnemyCompanions => true,
                _ => false
            };
        }

        private static bool _isMoveCard(CardBattleState card)
        {
            return card?.Config?.Effects != null
                   && card.Config.Effects.Any(effect => effect.Type == EEffectType.MoveLine);
        }
        
        private static bool _isSummonCard(CardBattleState card)
        {
            return card?.Config?.Effects != null
                   && card.Config.Effects.Any(effect => effect.Type == EEffectType.SummonCompanion);
        }

        private static BattleSide _getEnemySide(BattleModel battle, BattleSide mySide)
        {
            if (battle.Mode == EBattleMode.CoOpPvE
                && (ReferenceEquals(mySide, battle.SideA) || ReferenceEquals(mySide, battle.AllySide)))
            {
                return battle.SideB;
            }

            return ReferenceEquals(mySide, battle.SideA) ? battle.SideB : battle.SideA;
        }
    }
}
