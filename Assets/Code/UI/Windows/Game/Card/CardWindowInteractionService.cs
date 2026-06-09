using System;
using System.Linq;
using Core.Network;
using CoreGame.Card.Data;
using CoreGame.Card.Logic;
using CoreGame.Entities.Characters.Hero;
using UI.Windows.Game.Card.Map;

namespace UI.Windows.Game.Card
{
    public class CardWindowInteractionService
    {
        private readonly CardWindowVisuals _visuals;
        private readonly UserProvider _userProvider;
        private readonly BattleService _battleService;
        private readonly Action<string> _showCommandMessage;
        private readonly CardWindowSelectionState _selectionState = new CardWindowSelectionState();
        private readonly CardWindowTargetingRules _targetingRules;

        private BattleModel _battleModel;

        public CardWindowInteractionService(
            CardWindowVisuals visuals,
            UserProvider userProvider,
            BattleService battleService,
            Action<string> showCommandMessage = null)
        {
            _visuals = visuals;
            _userProvider = userProvider;
            _battleService = battleService;
            _showCommandMessage = showCommandMessage;
            _targetingRules = new CardWindowTargetingRules(
                unit => BattleGridRules.GetOwnerSide(_battleModel, unit),
                _findSideByHeroUnitId);
        }

        public void SetBattleModel(BattleModel battleModel)
        {
            _battleModel = battleModel;
            _visuals?.UpdateUnits(battleModel);
            _visuals?.UpdateAnchors(battleModel);
            _visuals?.RefreshOccupiedCells(battleModel);
        }

        public void ResetSelections()
        {
            _clearSelections();
        }

        public bool TrySelectMoveTarget(string cardId)
        {
            if (_battleModel == null || string.IsNullOrEmpty(cardId))
            {
                return false;
            }

            _clearSelections();

            _selectionState.PendingCardId = cardId;
            _selectionState.PendingMoveUnitId = null;
            _selectionState.IsMoveTargetSelection = true;
            _selectionState.IsMoveCellSelection = false;
            _visuals?.SetHeroesHighlight(false, EBattleHighlightColorType.AllyTarget, false, EBattleHighlightColorType.AllyTarget);

            BattleSide mySide = _getMySide();
            if (ReferenceEquals(mySide, _battleModel.SideA))
            {
                _visuals?.SetHeroesHighlight(true, EBattleHighlightColorType.AllyTarget, false, EBattleHighlightColorType.AllyTarget);
                _visuals?.SetCompanionHighlights(true, false, EBattleHighlightColorType.AllyTarget);
            }
            else
            {
                _visuals?.SetHeroesHighlight(false, EBattleHighlightColorType.AllyTarget, true, EBattleHighlightColorType.AllyTarget);
                _visuals?.SetCompanionHighlights(false, true, EBattleHighlightColorType.AllyTarget);
            }

            _visuals?.SetGridHighlighted(false);
            _visuals?.HighlightMoveTargetSide(_battleModel, mySide);

            return true;
        }

        public bool TrySelectSummonCell(string cardId)
        {
            if (_battleModel == null || string.IsNullOrEmpty(cardId))
            {
                return false;
            }

            _clearSelections();
            _selectionState.PendingSummonCardId = cardId;
            _selectionState.IsSummonCellSelection = true;

            BattleSide mySide = _getMySide();
            _visuals?.HighlightAvailableCellsForSide(_battleModel, mySide);

            return true;
        }

        public bool TrySelectCardTarget(CardBattleState card, BattleSide mySide)
        {
            if (_battleModel == null || card?.Config == null || mySide?.Hero == null)
            {
                return false;
            }

            if (!_targetingRules.RequiresManualTargetSelection(card))
            {
                return false;
            }

            _clearSelections();
            _selectionState.PendingTargetCard = card;
            _selectionState.PendingTargetCardActorId = mySide.Hero.UnitId;
            _selectionState.PendingTargetCardActorSideHeroId = mySide.Hero.UnitId;
            _selectionState.IsUnitTargetSelection = true;

            bool leftValid = _isValidTargetForPendingCard(_battleModel?.SideA?.Hero);
            bool rightValid = _isValidTargetForPendingCard(_battleModel?.SideB?.Hero);
            _visuals?.SetHeroesHighlight(
                leftValid,
                _isEnemyHero(_battleModel?.SideA?.Hero) ? EBattleHighlightColorType.EnemyTarget : EBattleHighlightColorType.AllyTarget,
                rightValid,
                _isEnemyHero(_battleModel?.SideB?.Hero) ? EBattleHighlightColorType.EnemyTarget : EBattleHighlightColorType.AllyTarget);
            _setCompanionsHighlightByTargetRules();
            _visuals?.SetGridHighlighted(false);

            return true;
        }

        public void OnHeroClicked(bool isLeftHero)
        {
            string unitId = isLeftHero
                ? _battleModel?.SideA?.Hero?.UnitId
                : _battleModel?.SideB?.Hero?.UnitId;
            OnUnitClicked(unitId);
        }

        public void OnUnitClicked(string targetUnitId)
        {
            if (string.IsNullOrEmpty(targetUnitId))
            {
                return;
            }

            if (_selectionState.IsUnitTargetSelection)
            {
                _playCardOnSelectedTarget(targetUnitId);
                return;
            }

            if (!_selectionState.IsMoveTargetSelection)
            {
                return;
            }

            BattleUnit unit = _battleService.FindUnit(targetUnitId);
            if (unit == null)
            {
                return;
            }

            BattleSide unitSide = BattleGridRules.GetOwnerSide(_battleModel, unit);
            BattleSide mySide = _getMySide();
            if (!ReferenceEquals(unitSide, mySide))
            {
                return;
            }

            _selectionState.PendingMoveUnitId = targetUnitId;
            _selectionState.PendingMoveSide = unitSide;
            _selectionState.IsMoveTargetSelection = false;
            _selectionState.IsMoveCellSelection = true;
            _visuals?.SetHeroesHighlight(false, EBattleHighlightColorType.AllyTarget, false, EBattleHighlightColorType.AllyTarget);
            _visuals?.HighlightAvailableCellsForSide(_battleModel, unitSide);
        }

        public void OnCellClicked(BattleGridCellView cell)
        {
            if (cell == null || (!_selectionState.IsMoveCellSelection && !_selectionState.IsSummonCellSelection))
            {
                return;
            }

            if (_selectionState.IsSummonCellSelection)
            {
                _trySummonToCell(cell);
                return;
            }

            bool isLeftMoveSide = ReferenceEquals(_selectionState.PendingMoveSide, _battleModel?.SideA);
            bool isOwnCell = isLeftMoveSide
                ? cell.Side == EBattleSideUi.Left
                : cell.Side == EBattleSideUi.Right;

            if (!isOwnCell)
            {
                return;
            }

            _tryMoveToCell(cell.Line, cell.CellIndex);
        }

        private void _clearMoveSelection()
        {
            _selectionState.ClearMoveSelection();
            _visuals?.ResetSelectionHighlights();
        }

        private void _clearTargetSelection()
        {
            _selectionState.ClearTargetSelection();
            _visuals?.ResetSelectionHighlights();
        }

        private void _clearSelections()
        {
            _clearMoveSelection();
            _clearTargetSelection();
        }

        private void _tryMoveToCell(EBattleLine line, int cellIndex)
        {
            if (!_selectionState.IsMoveCellSelection || string.IsNullOrEmpty(_selectionState.PendingMoveUnitId))
            {
                return;
            }

            BattleUnit unit = _battleService.FindUnit(_selectionState.PendingMoveUnitId);
            if (unit == null)
            {
                _clearMoveSelection();
                return;
            }

            CommandResult moveResult = _battleService.TryPlayMoveCardToCellWithResult(_selectionState.PendingCardId, _selectionState.PendingMoveUnitId, line, cellIndex);
            bool moved = moveResult == CommandResult.Success;
            if (!moved)
            {
                _showCommandError(moveResult);
                return;
            }

            _clearMoveSelection();
        }

        private void _trySummonToCell(BattleGridCellView cell)
        {
            if (!_selectionState.IsSummonCellSelection || string.IsNullOrEmpty(_selectionState.PendingSummonCardId) || _battleModel == null)
            {
                return;
            }

            BattleSide mySide = _getMySide();
            bool isLeft = ReferenceEquals(mySide, _battleModel.SideA);
            bool isOwnCell = isLeft ? cell.Side == EBattleSideUi.Left : cell.Side == EBattleSideUi.Right;
            if (!isOwnCell)
            {
                return;
            }

            if (mySide?.Hero == null)
            {
                return;
            }

            CommandResult playResult = _battleService.TryPlaySummonCardToCellWithResult(_selectionState.PendingSummonCardId, cell.Line, cell.CellIndex);
            if (playResult != CommandResult.Success)
            {
                _showCommandError(playResult);
                return;
            }

            SetBattleModel(_battleModel);
            _clearSelections();
        }

        private void _playCardOnSelectedTarget(string targetUnitId)
        {
            if (!_selectionState.IsUnitTargetSelection || _selectionState.PendingTargetCard == null)
            {
                return;
            }

            BattleUnit target = _battleService.FindUnit(targetUnitId);
            if (!_isValidTargetForPendingCard(target))
            {
                return;
            }

            CommandResult playResult = _battleService.TryPlayCardWithResult(_selectionState.PendingTargetCard.InstanceId, targetUnitId);
            if (playResult != CommandResult.Success)
            {
                _showCommandError(playResult);
                return;
            }

            _clearSelections();
        }

        private void _setCompanionsHighlightByTargetRules()
        {
            bool hasEnemyManualTarget = _selectionState.PendingTargetCard?.Config?.Effects != null
                                        && _selectionState.PendingTargetCard.Config.Effects.Any(effect =>
                                            effect != null
                                            && (effect.Target == EEffectTarget.SelectedEnemy
                                                || effect.Target == EEffectTarget.EnemyCompanions));
            if (hasEnemyManualTarget)
            {
                _visuals?.SetHeroesHighlight(
                    _isEnemyHero(_battleModel?.SideA?.Hero),
                    EBattleHighlightColorType.EnemyTarget,
                    _isEnemyHero(_battleModel?.SideB?.Hero),
                    EBattleHighlightColorType.EnemyTarget);
            }

            _visuals?.SetCompanionTargetHighlights(
                unitId =>
                {
                    BattleUnit unit = _battleService.FindUnit(unitId);
                    return _isValidTargetForPendingCard(unit);
                },
                unitId =>
                {
                    BattleUnit unit = _battleService.FindUnit(unitId);
                    return unit != null && !_isAllyUnit(unit);
                });
        }

        private bool _isValidTargetForPendingCard(BattleUnit target)
        {
            return _targetingRules.IsValidTargetForPendingCard(
                _battleModel,
                _selectionState.PendingTargetCard,
                _selectionState.PendingTargetCardActorId,
                _selectionState.PendingTargetCardActorSideHeroId,
                target);
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

            BattleSide mySide = _findSideByHeroUnitId(_selectionState.PendingTargetCardActorSideHeroId);
            BattleSide targetSide = BattleGridRules.GetOwnerSide(_battleModel, unit);
            return mySide != null && targetSide != null && !ReferenceEquals(mySide, targetSide);
        }

        private bool _isAllyUnit(BattleUnit unit)
        {
            if (unit == null || _battleModel == null)
            {
                return false;
            }

            BattleSide mySide = _getMySide();
            BattleSide targetSide = BattleGridRules.GetOwnerSide(_battleModel, unit);
            return mySide != null && targetSide != null && ReferenceEquals(mySide, targetSide);
        }

        private void _showCommandError(CommandResult result)
        {
            _showCommandMessage?.Invoke(CommandResultText.ToDebugText(result));
        }
    }
}
