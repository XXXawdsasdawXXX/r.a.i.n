using Core.Network;
using Core.ServiceLocator;
using CoreGame.Card.Data;
using CoreGame.Card.Logic;
using CoreGame.Entities.Characters.Hero;
using Cysharp.Threading.Tasks;
using Essential;
using System.Collections.Generic;
using System.Linq;
using Core.GameLoop;
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
        
        private UserProvider _userProvider;
        private BattleService _battleService;
        private GameEventDispatcher _gameEventDispatcher;
        
        private BattleModel _battleModel;
        private CardBattleState _pendingTargetCard;
        private BattleSide _pendingMoveSide;
        private string _pendingCardId;
        private string _pendingMoveUnitId;
        private string _pendingSummonCardId;
        private string _pendingTargetCardActorId;
        private string _pendingTargetCardActorSideHeroId;
        private readonly Dictionary<BattleUnitView, string> _viewToUnitId = new Dictionary<BattleUnitView, string>();
        private readonly List<BattleUnitView> _leftCompanionViews = new List<BattleUnitView>();
        private readonly List<BattleUnitView> _rightCompanionViews = new List<BattleUnitView>();
        private bool _isMoveTargetSelection;
        private bool _isMoveCellSelection;
        private bool _isSummonCellSelection;
        private bool _isUnitTargetSelection;

        
        public override UniTask InitializeWindow(UIWindowManager manager)
        {
            _battleService = Container.Instance.GetService<BattleService>();
            _userProvider = Container.Instance.GetService<UserProvider>();
            _gameEventDispatcher = Container.Instance.GetService<GameEventDispatcher>();
            _validateInspectorBindings();
     
            _leftSideView.SetCellsHighlighted(false);
            _rightSideView.SetCellsHighlighted(false);
            
            return base.InitializeWindow(manager);
        }

        public override void SubscribeToEvents(bool flag)
        {
            base.SubscribeToEvents(flag);

            Log.Info(this, "subscribe");
            
            if (flag)
            {
                _battleService.BattleStarted += _openView;
                _battleService.BattleFinished += _closeView;
                _battleService.TurnStarted += _updateUnitViews;
                _battleService.CardPlayed += _updateUnitViews;
                _leftHeroView.Clicked += _onLeftHeroClicked;
                _rightHeroView.Clicked += _onRightHeroClicked;
                _bindCells(_leftSideView, true);
                _bindCells(_rightSideView, true);
            }
            else
            {
                _battleService.BattleStarted -= _openView;
                _battleService.BattleFinished -= _closeView;
                _battleService.TurnStarted -= _updateUnitViews;
                _battleService.CardPlayed -= _updateUnitViews;
                _leftHeroView.Clicked -= _onLeftHeroClicked;
                _rightHeroView.Clicked -= _onRightHeroClicked;
                _bindCells(_leftSideView, false);
                _bindCells(_rightSideView, false);
            }
        }

        private void _closeView(BattleModel _)
        {
            view.Close();
            _clearSelections();
            Log.Info(this, "Close view");
        }

        private void _openView(BattleModel model)
        {
            view.Open();
            _updateUnitViews(model);
            Log.Info(this, "Open view");
        }

        private void _updateUnitViews(BattleModel battleModel)
        {
            _battleModel = battleModel;
            _leftHeroView.Set(battleModel?.SideA?.Hero);
            _rightHeroView.Set(battleModel?.SideB?.Hero);
            _updateCompanionViews();
            _updateHeroAnchors();
            _updateCompanionAnchors();
            _refreshOccupiedCells();
        }

        public bool TrySelectMoveTarget(string cardId)
        {
            if (_battleModel == null || string.IsNullOrEmpty(cardId))
            {
                return false;
            }
            
            _clearSelections();

            _pendingCardId = cardId;
            _pendingMoveUnitId = null;
            _isMoveTargetSelection = true;
            _isMoveCellSelection = false;
            _leftHeroView.SetHighlighted(false);
            _rightHeroView.SetHighlighted(false);
            
            BattleSide mySide = _getMySide();
            if (ReferenceEquals(mySide, _battleModel.SideA))
            {
                _leftHeroView.SetHighlighted(true);
                _setCompanionHighlights(_leftCompanionViews, true);
            }
            else
            {
                _rightHeroView.SetHighlighted(true);
                _setCompanionHighlights(_rightCompanionViews, true);
            }

            _leftSideView.SetCellsHighlighted(false);
            _rightSideView.SetCellsHighlighted(false);
            _setCompanionsHighlightByMoveSide(mySide);

            Log.Info(this, $"[MoveUI] select unit for move card={cardId}");
            return true;
        }
        
        public bool TrySelectSummonCell(string cardId)
        {
            if (_battleModel == null || string.IsNullOrEmpty(cardId))
            {
                return false;
            }

            _clearSelections();
            _pendingSummonCardId = cardId;
            _isSummonCellSelection = true;
            
            BattleSide mySide = _getMySide();
            _highlightAvailableCellsForSide(mySide);

            Log.Info(this, $"[SummonUI] select cell for summon card={cardId}");
            return true;
        }
        
        public bool TrySelectCardTarget(CardBattleState card, BattleSide mySide)
        {
            if (_battleModel == null || card?.Config == null || mySide?.Hero == null)
            {
                return false;
            }

            if (!_requiresManualTargetSelection(card))
            {
                return false;
            }

            _clearSelections();
            _pendingTargetCard = card;
            _pendingTargetCardActorId = mySide.Hero.UnitId;
            _pendingTargetCardActorSideHeroId = mySide.Hero.UnitId;
            _isUnitTargetSelection = true;

            bool leftValid = _isValidTargetForPendingCard(_battleModel?.SideA?.Hero);
            bool rightValid = _isValidTargetForPendingCard(_battleModel?.SideB?.Hero);
            _leftHeroView.SetHighlighted(leftValid);
            _rightHeroView.SetHighlighted(rightValid);
            _setCompanionsHighlightByTargetRules();
            
            _leftSideView.SetCellsHighlighted(false);
            _rightSideView.SetCellsHighlighted(false);

            Log.Info(this, $"[TargetUI] select unit target for card={card.InstanceId}");
            return true;
        }

        private void _onLeftHeroClicked()
        {
            _onUnitClicked(_battleModel?.SideA?.Hero?.UnitId);
        }

        private void _onRightHeroClicked()
        {
            _onUnitClicked(_battleModel?.SideB?.Hero?.UnitId);
        }

        private void _onUnitClicked(string targetUnitId)
        {
            if (string.IsNullOrEmpty(targetUnitId))
            {
                return;
            }
            
            if (_isUnitTargetSelection)
            {
                _playCardOnSelectedTarget(targetUnitId);
                return;
            }
            
            if (!_isMoveTargetSelection)
            {
                return;
            }

            BattleUnit unit = _battleService.FindUnit(targetUnitId);
            if (unit == null)
            {
                Log.Info(this, $"[MoveUI] unit not found {targetUnitId}");
                return;
            }

            BattleSide unitSide = BattleGridRules.GetOwnerSide(_battleModel, unit);
            BattleSide mySide = _getMySide();
            if (!ReferenceEquals(unitSide, mySide))
            {
                Log.Info(this, $"[MoveUI] reject unit from other side. unit={targetUnitId}");
                return;
            }

            _pendingMoveUnitId = targetUnitId;
            _pendingMoveSide = unitSide;
            _isMoveTargetSelection = false;
            _isMoveCellSelection = true;
            _leftHeroView.SetHighlighted(false);
            _rightHeroView.SetHighlighted(false);
            _highlightAvailableCellsForSide(unitSide);
            Log.Info(this, $"[MoveUI] unit selected unit={targetUnitId}. Now click free highlighted grid cell.");
        }

        private void _clearMoveSelection()
        {
            _pendingCardId = null;
            _pendingMoveUnitId = null;
            _pendingMoveSide = null;
            _isMoveTargetSelection = false;
            _isMoveCellSelection = false;
            _pendingSummonCardId = null;
            _isSummonCellSelection = false;
            _leftHeroView.SetHighlighted(false);
            _rightHeroView.SetHighlighted(false);
            _leftSideView.SetCellsHighlighted(false);
            _rightSideView.SetCellsHighlighted(false);
            _setCompanionHighlights(_leftCompanionViews, false);
            _setCompanionHighlights(_rightCompanionViews, false);
        }
        
        private void _clearTargetSelection()
        {
            _pendingTargetCard = null;
            _pendingTargetCardActorId = null;
            _pendingTargetCardActorSideHeroId = null;
            _isUnitTargetSelection = false;
            _leftHeroView.SetHighlighted(false);
            _rightHeroView.SetHighlighted(false);
            _setCompanionHighlights(_leftCompanionViews, false);
            _setCompanionHighlights(_rightCompanionViews, false);
        }

        private void _clearSelections()
        {
            _clearMoveSelection();
            _clearTargetSelection();
        }
        
        private void _tryMoveToCell(EBattleLine line, int cellIndex)
        {
            if (!_isMoveCellSelection || string.IsNullOrEmpty(_pendingMoveUnitId))
            {
                Log.Info(this, "[MoveUI] skip move: no selected unit/card");
                return;
            }

            BattleUnit unit = _battleService.FindUnit(_pendingMoveUnitId);
            if (unit == null)
            {
                Log.Info(this, $"[MoveUI] skip move: unit not found {_pendingMoveUnitId}");
                _clearMoveSelection();
                return;
            }

            CommandResult moveResult = _battleService.TryPlayMoveCardToCellWithResult(_pendingCardId, _pendingMoveUnitId, line, cellIndex);
            bool moved = moveResult == CommandResult.Success;
            Log.Info(this, $"[MoveUI] move result card={_pendingCardId} unit={_pendingMoveUnitId} to={line}/{cellIndex} reason={CommandResultText.ToDebugText(moveResult)}");

            if (!moved)
            {
                Log.Info(this, $"[MoveUI] move rejected. Card is not spent. {CommandResultText.ToDebugText(moveResult)}");
                return;
            }

            _clearMoveSelection();
        }

        private void _onCellClicked(BattleGridCellView cell)
        {
            if (cell == null || (!_isMoveCellSelection && !_isSummonCellSelection))
            {
                return;
            }
            
            if (_isSummonCellSelection)
            {
                _trySummonToCell(cell);
                return;
            }

            bool isLeftMoveSide = ReferenceEquals(_pendingMoveSide, _battleModel?.SideA);
            bool isOwnCell = isLeftMoveSide
                ? cell.Side == EBattleSideUi.Left
                : cell.Side == EBattleSideUi.Right;

            if (!isOwnCell)
            {
                Log.Info(this, $"[MoveUI] reject enemy cell side={cell.Side} for unit={_pendingMoveUnitId}");
                return;
            }

            Log.Info(this, $"[MoveUI] cell clicked line={cell.Line} cell={cell.CellIndex}");
            _tryMoveToCell(cell.Line, cell.CellIndex);
        }

        private void _onCompanionClicked(BattleUnitView view)
        {
            if (view == null || !_viewToUnitId.TryGetValue(view, out string unitId))
            {
                return;
            }

            _onUnitClicked(unitId);
        }
        
        private void _playCardOnSelectedTarget(string targetUnitId)
        {
            if (!_isUnitTargetSelection || _pendingTargetCard == null)
            {
                return;
            }

            BattleUnit target = _battleService.FindUnit(targetUnitId);
            if (!_isValidTargetForPendingCard(target))
            {
                Log.Info(this, $"[TargetUI] invalid selected target. unit={targetUnitId}");
                return;
            }

            CommandResult playResult = _battleService.TryPlayCardWithResult(_pendingTargetCard.InstanceId, targetUnitId);
            if (playResult != CommandResult.Success)
            {
                Log.Info(this, $"[TargetUI] card play rejected. reason={CommandResultText.ToDebugText(playResult)}");
                return;
            }

            Log.Info(this, $"[TargetUI] card played. card={_pendingTargetCard.InstanceId}, target={targetUnitId}");
            _clearSelections();
        }

        private bool _isValidTargetForPendingCard(BattleUnit target)
        {
            if (_pendingTargetCard?.Config?.Effects == null || target == null || _battleModel == null)
            {
                return false;
            }

            BattleSide actorSide = _findSideByHeroUnitId(_pendingTargetCardActorSideHeroId);
            BattleSide targetSide = BattleGridRules.GetOwnerSide(_battleModel, target);
            if (actorSide == null || targetSide == null)
            {
                return false;
            }

            bool isEnemy = !ReferenceEquals(actorSide, targetSide);
            bool isSelf = target.UnitId == _pendingTargetCardActorId;
            bool isAlly = ReferenceEquals(actorSide, targetSide);
            bool isCompanion = target.IsCompanion;
            
            bool hasManualTargetEffect = false;
            bool hasEnemyManualTarget = false;
            foreach (CardEffectConfiguration effect in _pendingTargetCard.Config.Effects)
            {
                if (!_requiresManualTarget(effect))
                {
                    continue;
                }

                hasManualTargetEffect = true;
                if (effect.Target == EEffectTarget.SelectedEnemy || effect.Target == EEffectTarget.EnemyCompanions)
                {
                    hasEnemyManualTarget = true;
                }
                
                bool valid = effect.Target switch
                {
                    EEffectTarget.Self => isSelf,
                    EEffectTarget.SelectedAlly => isAlly,
                    EEffectTarget.SelectedAnyAllyUnit => isAlly,
                    EEffectTarget.SelectedEnemy => isEnemy,
                    EEffectTarget.EnemyCompanions => isEnemy && isCompanion,
                    _ => false
                };

                if (valid)
                {
                    return true;
                }
            }

            if (hasEnemyManualTarget)
            {
                return isEnemy;
            }

            return !hasManualTargetEffect;
        }

        private static bool _requiresManualTargetSelection(CardBattleState card)
        {
            return card.Config.Effects.Any(_requiresManualTarget);
        }

        private static bool _requiresManualTarget(CardEffectConfiguration effect)
        {
            if (effect == null)
            {
                return false;
            }

            // Summon/move не должны открывать ручной target-mode.
            if (effect.Type == EEffectType.SummonCompanion || effect.Type == EEffectType.MoveLine)
            {
                return false;
            }

            EEffectTarget target = effect.Target;
            return target == EEffectTarget.SelectedEnemy
                   || target == EEffectTarget.SelectedAlly
                   || target == EEffectTarget.SelectedAnyAllyUnit
                   || target == EEffectTarget.Self
                   || target == EEffectTarget.EnemyCompanions;
        }

        private BattleSide _findSideByHeroUnitId(string heroUnitId)
        {
            if (_battleModel == null || string.IsNullOrEmpty(heroUnitId))
            {
                return null;
            }

            if (_battleModel.SideA?.Hero?.UnitId == heroUnitId)
            {
                return _battleModel.SideA;
            }

            if (_battleModel.SideB?.Hero?.UnitId == heroUnitId)
            {
                return _battleModel.SideB;
            }

            return null;
        }

        private void _updateCompanionViews()
        {
            _setCompanionViews(_leftCompanionViews, _battleModel?.SideA);
            _setCompanionViews(_rightCompanionViews, _battleModel?.SideB);
        }

        private void _setCompanionViews(List<BattleUnitView> views, BattleSide side)
        {
            if (views == null)
            {
                return;
            }

            if (side == null)
            {
                foreach (BattleUnitView view in views)
                {
                    if (view == null)
                    {
                        continue;
                    }

                    view.Set(null);
                    _viewToUnitId.Remove(view);
                }

                return;
            }
            
            _ensureCompanionPoolSize(views, side.Companions.Count, ReferenceEquals(side, _battleModel.SideA) ? _leftCompanionRoot : _rightCompanionRoot);

            for (int i = 0; i < views.Count; i++)
            {
                BattleUnitView view = views[i];
                if (view == null)
                {
                    continue;
                }

                BattleUnit unit = (side != null && i < side.Companions.Count) ? side.Companions[i] : null;
                view.Set(unit);

                if (unit == null)
                {
                    _viewToUnitId.Remove(view);
                }
                else
                {
                    _viewToUnitId[view] = unit.UnitId;
                }
            }
        }

        private void _updateCompanionAnchors()
        {
            if (_battleModel == null)
            {
                return;
            }

            _anchorCompanions(_leftCompanionViews, _leftSideView, _battleModel.SideA);
            _anchorCompanions(_rightCompanionViews, _rightSideView, _battleModel.SideB);
        }

        private void _anchorCompanions(List<BattleUnitView> views, BattleSideView sideView, BattleSide side)
        {
            if (views == null || sideView == null || side == null)
            {
                return;
            }

            int max = Mathf.Min(views.Count, side.Companions.Count);
            for (int i = 0; i < max; i++)
            {
                _anchorHero(views[i], sideView, side.Companions[i]);
            }
        }

        private void _setCompanionsHighlightByTargetRules()
        {
            bool hasEnemyManualTarget = _pendingTargetCard?.Config?.Effects != null
                                        && _pendingTargetCard.Config.Effects.Any(effect =>
                                            effect != null
                                            && (effect.Target == EEffectTarget.SelectedEnemy
                                                || effect.Target == EEffectTarget.EnemyCompanions));
            if (hasEnemyManualTarget)
            {
                _leftHeroView.SetHighlighted(_isEnemyHero(_battleModel?.SideA?.Hero));
                _rightHeroView.SetHighlighted(_isEnemyHero(_battleModel?.SideB?.Hero));
            }

            _setCompanionTargetHighlight(_leftCompanionViews);
            _setCompanionTargetHighlight(_rightCompanionViews);
        }

        private void _setCompanionsHighlightByMoveSide(BattleSide moveSide)
        {
            bool leftSide = ReferenceEquals(moveSide, _battleModel?.SideA);
            _setCompanionHighlights(_leftCompanionViews, leftSide);
            _setCompanionHighlights(_rightCompanionViews, !leftSide);
        }

        private void _setCompanionTargetHighlight(List<BattleUnitView> views)
        {
            if (views == null)
            {
                return;
            }

            foreach (BattleUnitView view in views)
            {
                if (view == null)
                {
                    continue;
                }

                if (!_viewToUnitId.TryGetValue(view, out string unitId))
                {
                    view.SetHighlighted(false);
                    continue;
                }

                BattleUnit unit = _battleService.FindUnit(unitId);
                view.SetHighlighted(_isValidTargetForPendingCard(unit));
            }
        }

        private static void _setCompanionHighlights(List<BattleUnitView> views, bool value)
        {
            if (views == null)
            {
                return;
            }

            foreach (BattleUnitView view in views)
            {
                if (view != null)
                {
                    view.SetHighlighted(value);
                }
            }
        }

        private void _refreshOccupiedCells()
        {
            if (_battleModel == null)
            {
                return;
            }

            _leftSideView.ClearOccupiedHighlights();
            _rightSideView.ClearOccupiedHighlights();
            _markSideOccupiedCells(_battleModel.SideA, _leftSideView);
            _markSideOccupiedCells(_battleModel.SideB, _rightSideView);
        }
        
        private void _highlightAvailableCellsForSide(BattleSide side)
        {
            _leftSideView.SetCellsHighlighted(false);
            _rightSideView.SetCellsHighlighted(false);

            if (side == null)
            {
                return;
            }
            
            BattleSideView sideView = ReferenceEquals(side, _battleModel.SideA) ? _leftSideView : _rightSideView;
            if (sideView == null)
            {
                return;
            }

            for (int cell = 0; cell < BattleGridRules.CELLS_PER_LINE; cell++)
            {
                _setCellHighlightIfFree(side, sideView, EBattleLine.Frontline, cell);
                _setCellHighlightIfFree(side, sideView, EBattleLine.Backline, cell);
            }
        }

        private static void _setCellHighlightIfFree(BattleSide side, BattleSideView sideView, EBattleLine line, int cellIndex)
        {
            bool occupied = side.GetAllUnits()
                .Where(u => u != null && u.HP > 0)
                .Any(u => u.Line == line && u.LineCellIndex == cellIndex);

            sideView.SetCellHighlighted(line, cellIndex, !occupied);
        }

        private void _trySummonToCell(BattleGridCellView cell)
        {
            if (!_isSummonCellSelection || string.IsNullOrEmpty(_pendingSummonCardId) || _battleModel == null)
            {
                return;
            }

            BattleSide mySide = _getMySide();
            bool isLeft = ReferenceEquals(mySide, _battleModel.SideA);
            bool isOwnCell = isLeft ? cell.Side == EBattleSideUi.Left : cell.Side == EBattleSideUi.Right;
            if (!isOwnCell)
            {
                Log.Info(this, "[SummonUI] reject enemy side cell");
                return;
            }

            if (mySide?.Hero == null)
            {
                return;
            }
            
            CommandResult playResult = _battleService.TryPlaySummonCardToCellWithResult(_pendingSummonCardId, cell.Line, cell.CellIndex);
            if (playResult != CommandResult.Success)
            {
                Log.Info(this, $"[SummonUI] summon rejected. reason={CommandResultText.ToDebugText(playResult)}");
                return;
            }
            
            _updateUnitViews(_battleModel);
            Log.Info(this, $"[SummonUI] companion placed to={cell.Line}/{cell.CellIndex}");
            
            _clearSelections();
        }

        private static void _markSideOccupiedCells(BattleSide side, BattleSideView sideView)
        {
            if (side == null || sideView == null)
            {
                return;
            }

            foreach (BattleUnit unit in side.GetAllUnits())
            {
                if (unit == null || unit.HP <= 0)
                {
                    continue;
                }

                sideView.SetCellOccupied(unit.Line, unit.LineCellIndex, true);
            }
        }
        
        private void _ensureCompanionPoolSize(List<BattleUnitView> pool, int count, Transform root)
        {
            if (pool == null || root == null || _companionViewPrefab == null)
            {
                return;
            }

            while (pool.Count < count)
            {
                BattleUnitView view = Instantiate(_companionViewPrefab, root);
                view.Clicked += () => _onCompanionClicked(view);
                IGameListener[] listeners = view.GetComponentsInChildren<IGameListener>(true);
                if (listeners.Length > 0)
                {
                    _gameEventDispatcher.InitializeListeners(listeners);
                }
                pool.Add(view);
            }
        }

        private void _bindCells(BattleSideView sideView, bool bind)
        {
            if (sideView == null)
            {
                return;
            }

            for (int i = 0; i < BattleGridRules.CELLS_PER_LINE; i++)
            {
                BattleGridCellView front = sideView.GetCell(EBattleLine.Frontline, i);
                BattleGridCellView back = sideView.GetCell(EBattleLine.Backline, i);

                _bindCell(front, bind);
                _bindCell(back, bind);
            }
        }

        private void _bindCell(BattleGridCellView cell, bool bind)
        {
            if (cell == null)
            {
                return;
            }

            if (bind)
            {
                cell.Clicked += _onCellClicked;
            }
            else
            {
                cell.Clicked -= _onCellClicked;
            }
        }

        private void _validateInspectorBindings()
        {
            if (_leftCompanionRoot == null || _rightCompanionRoot == null || _companionViewPrefab == null)
            {
                Log.Info(this, "[CompanionUI] Missing inspector binding. Check LeftCompanionRoot/RightCompanionRoot/CompanionViewPrefab.");
            }

            _validateOccupiedHighlight(_leftSideView, EBattleSideUi.Left);
            _validateOccupiedHighlight(_rightSideView, EBattleSideUi.Right);
        }

        private void _validateOccupiedHighlight(BattleSideView sideView, EBattleSideUi side)
        {
            if (sideView == null)
            {
                return;
            }

            for (int i = 0; i < BattleGridRules.CELLS_PER_LINE; i++)
            {
                _logIfCellHasNoOccupiedHighlight(sideView.GetCell(EBattleLine.Frontline, i), side, EBattleLine.Frontline, i);
                _logIfCellHasNoOccupiedHighlight(sideView.GetCell(EBattleLine.Backline, i), side, EBattleLine.Backline, i);
            }
        }

        private void _logIfCellHasNoOccupiedHighlight(BattleGridCellView cell, EBattleSideUi side, EBattleLine line, int cellIndex)
        {
            if (cell == null || cell.HasOccupiedHighlightBinding)
            {
                return;
            }

            Log.Info(this, $"[CompanionUI] OccupiedHighlight is not assigned. side={side}, line={line}, cell={cellIndex}");
        }

        private void _updateHeroAnchors()
        {
            if (_battleModel == null)
            {
                return;
            }

            _anchorHero(_leftHeroView, _leftSideView, _battleModel.SideA.Hero);
            _anchorHero(_rightHeroView, _rightSideView, _battleModel.SideB.Hero);
        }

        private void _anchorHero(BattleUnitView heroView, BattleSideView sideView, BattleUnit hero)
        {
            if (heroView == null || sideView == null || hero == null)
            {
                return;
            }

            BattleGridCellView targetCell = sideView.GetCell(hero.Line, hero.LineCellIndex);
            if (targetCell == null)
            {
                return;
            }

            Transform heroViewTransform = heroView.transform;
            heroViewTransform.SetParent(targetCell.transform, false);
            heroViewTransform.localPosition = Vector3.zero;
            heroViewTransform.localScale = Vector3.one;

            Log.Info(this, $"[GridUI] anchor hero unit={hero.UnitId} line={hero.Line} cell={hero.LineCellIndex}");
        }

        private BattleSide _getMySide()
        {
            if (_battleModel == null)
            {
                return null;
            }

            Hero hero = _userProvider.GetHeroComponent<Hero>();
            string heroId = hero?.Model?.HeroId;

            if (!string.IsNullOrEmpty(heroId))
            {
                if (_battleModel.SideA.Hero.UnitId == heroId)
                {
                    return _battleModel.SideA;
                }

                if (_battleModel.SideB.Hero.UnitId == heroId)
                {
                    return _battleModel.SideB;
                }
            }

            return _battleModel.SideA;
        }
        
        private bool _isEnemyHero(BattleUnit unit)
        {
            if (unit == null || _battleModel == null)
            {
                return false;
            }

            BattleSide mySide = _findSideByHeroUnitId(_pendingTargetCardActorSideHeroId);
            BattleSide targetSide = BattleGridRules.GetOwnerSide(_battleModel, unit);
            return mySide != null && targetSide != null && !ReferenceEquals(mySide, targetSide);
        }
    }
}