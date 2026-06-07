using Core.Network;
using Core.ServiceLocator;
using CoreGame.Card.Data;
using CoreGame.Card.Logic;
using Cysharp.Threading.Tasks;
using UI.Windows.Game.Card.Map;
using UI.Windows.Game.Card.Unit;
using UI.Windows.Base;
using UnityEngine;

namespace UI.Windows.Game.Card
{
    public class CardWindowController : UIWindowController<CardWindowView>
    {
        [SerializeField] private BattleUnitView _leftHeroView;
        [SerializeField] private BattleUnitView _rightHeroView;
        [SerializeField] private Transform _leftCompanionRoot;
        [SerializeField] private Transform _rightCompanionRoot;
        [SerializeField] private BattleUnitView _companionViewPrefab;
        [SerializeField] private BattleSideView _leftSideView;
        [SerializeField] private BattleSideView _rightSideView;

        private BattleService _battleService;
        private CardWindowVisuals _visuals;
        private CardWindowInteractionService _interactionService;

        
        public override UniTask InitializeWindow(UIWindowManager manager)
        {
            _battleService = Container.Instance.GetService<BattleService>();

            _visuals = new CardWindowVisuals(
                _leftHeroView,
                _rightHeroView,
                _leftCompanionRoot,
                _rightCompanionRoot,
                _companionViewPrefab,
                _leftSideView,
                _rightSideView,
                Container.Instance.GetService<Core.GameLoop.GameEventDispatcher>());

            _interactionService = new CardWindowInteractionService(
                _visuals,
                Container.Instance.GetService<Core.Network.UserProvider>(),
                _battleService,
                ShowCommandMessage);

            _visuals.ValidateInspectorBindings(this);
            _visuals.SetGridHighlighted(false);

            return base.InitializeWindow(manager);
        }

        public override void SubscribeToEvents(bool flag)
        {
            base.SubscribeToEvents(flag);

            if (flag)
            {
                _battleService.BattleStarted += _onBattleStarted;
                _battleService.BattleFinished += _onBattleFinished;
                _battleService.TurnStarted += _onBattleUpdated;
                _battleService.CardPlayed += _onBattleUpdated;
                _battleService.CardPlayedDetailed += _onCardPlayedDetailed;
                _battleService.TurnStarted += _onTurnStarted;
                _leftHeroView.Clicked += _onLeftHeroClicked;
                _rightHeroView.Clicked += _onRightHeroClicked;
                _visuals?.BindCells(_onCellClicked, true);
                if (_visuals != null)
                {
                    _visuals.CompanionClicked += _onCompanionClicked;
                }
            }
            else
            {
                _battleService.BattleStarted -= _onBattleStarted;
                _battleService.BattleFinished -= _onBattleFinished;
                _battleService.TurnStarted -= _onBattleUpdated;
                _battleService.CardPlayed -= _onBattleUpdated;
                _battleService.CardPlayedDetailed -= _onCardPlayedDetailed;
                _battleService.TurnStarted -= _onTurnStarted;
                _leftHeroView.Clicked -= _onLeftHeroClicked;
                _rightHeroView.Clicked -= _onRightHeroClicked;
                _visuals?.BindCells(_onCellClicked, false);
                if (_visuals != null)
                {
                    _visuals.CompanionClicked -= _onCompanionClicked;
                }
            }
        }

        public bool TrySelectMoveTarget(string cardId)
        {
            return _interactionService != null && _interactionService.TrySelectMoveTarget(cardId);
        }

        public bool TrySelectSummonCell(string cardId)
        {
            return _interactionService != null && _interactionService.TrySelectSummonCell(cardId);
        }

        public bool TrySelectCardTarget(CardBattleState card, BattleSide mySide)
        {
            return _interactionService != null && _interactionService.TrySelectCardTarget(card, mySide);
        }

        public void ShowCommandMessage(string message)
        {
            view?.ShowCommandMessage(message);
        }

        public void ClearCommandMessage()
        {
            view?.ClearCommandMessage();
        }

        private void _onBattleStarted(BattleModel model)
        {
            view.Open();
            view.ClearCommandMessage();
            _onBattleUpdated(model);
        }

        private void _onBattleFinished(BattleModel _)
        {
            view.Close();
            view.ClearCommandMessage();
            _interactionService?.ResetSelections();
        }

        private void _onBattleUpdated(BattleModel battleModel)
        {
            _interactionService?.SetBattleModel(battleModel);
        }

        private void _onCardPlayedDetailed(BattleCardPlayedEvent battleEvent)
        {
            if (battleEvent?.Card?.Config == null)
            {
                return;
            }

            _visuals?.PlayCardEffect(battleEvent.ActorUnitId, battleEvent.Card.Config.Type);
        }

        private void _onTurnStarted(BattleModel _)
        {
            view.ClearCommandMessage();
            _interactionService?.ResetSelections();
        }

        private void _onLeftHeroClicked()
        {
            _interactionService?.OnHeroClicked(isLeftHero: true);
        }

        private void _onRightHeroClicked()
        {
            _interactionService?.OnHeroClicked(isLeftHero: false);
        }

        private void _onCompanionClicked(string unitId)
        {
            _interactionService?.OnUnitClicked(unitId);
        }

        private void _onCellClicked(BattleGridCellView cell)
        {
            _interactionService?.OnCellClicked(cell);
        }
    }
}
