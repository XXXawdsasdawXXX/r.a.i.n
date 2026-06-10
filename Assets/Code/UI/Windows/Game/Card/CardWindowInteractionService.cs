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
                heroUnitId => BattleParticipantHelper.FindSideByHeroUnitId(_battleModel, heroUnitId));
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
            bool isLeftSide = _isLeftSide(mySide);
            if (isLeftSide)
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

            BattleSide leftHeroSide = BattleParticipantHelper.GetUiLeftHeroSide(_battleModel);
            BattleSide rightHeroSide = BattleParticipantHelper.GetUiRightHeroSide(_battleModel);
            bool leftValid = _isValidTargetForPendingCard(leftHeroSide?.Hero);
            bool rightValid = _isValidTargetForPendingCard(rightHeroSide?.Hero);
            _visuals?.SetHeroesHighlight(
                leftValid,
                _isEnemyHero(leftHeroSide?.Hero) ? EBattleHighlightColorType.EnemyTarget : EBattleHighlightColorType.AllyTarget,
                rightValid,
                _isEnemyHero(rightHeroSide?.Hero) ? EBattleHighlightColorType.EnemyTarget : EBattleHighlightColorType.AllyTarget);
            _setCompanionsHighlightByTargetRules();
            _visuals?.SetGridHighlighted(false);

            return true;
        }

        public void OnHeroClicked(bool isLeftHero)
        {
            BattleSide heroSide = isLeftHero
                ? BattleParticipantHelper.GetUiLeftHeroSide(_battleModel)
                : BattleParticipantHelper.GetUiRightHeroSide(_battleModel);
            OnUnitClicked(heroSide?.Hero?.UnitId);
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

            bool isLeftMoveSide = _isLeftSide(_selectionState.PendingMoveSide);
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

            CommandResult moveResult = _battleService.TryPlayMoveCardToCellWithResult(
                _selectionState.PendingCardId,
                _selectionState.PendingMoveUnitId,
                line,
                cellIndex,
                _getLocalHeroId());
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
            bool isLeft = _isLeftSide(mySide);
            bool isOwnCell = isLeft ? cell.Side == EBattleSideUi.Left : cell.Side == EBattleSideUi.Right;
            if (!isOwnCell)
            {
                return;
            }

            if (mySide?.Hero == null)
            {
                return;
            }

            CommandResult playResult = _battleService.TryPlaySummonCardToCellWithResult(
                _selectionState.PendingSummonCardId,
                cell.Line,
                cell.CellIndex,
                _getLocalHeroId());
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

            CommandResult playResult = _battleService.TryPlayCardWithResult(
                _selectionState.PendingTargetCard.InstanceId,
                targetUnitId,
                _getLocalHeroId());
            if (playResult != CommandResult.Success)
            {
                _showCommandError(playResult);
                return;
            }

            _clearSelections();
        }

        private void _setCompanionsHighlightByTargetRules()
        {
            BattleSide leftHeroSide = BattleParticipantHelper.GetUiLeftHeroSide(_battleModel);
            BattleSide rightHeroSide = BattleParticipantHelper.GetUiRightHeroSide(_battleModel);

            bool hasEnemyManualTarget = _selectionState.PendingTargetCard?.Config?.Effects != null
                                        && _selectionState.PendingTargetCard.Config.Effects.Any(effect =>
                                            effect != null
                                            && (effect.Target == EEffectTarget.SelectedEnemy
                                                || effect.Target == EEffectTarget.EnemyCompanions));
            if (hasEnemyManualTarget)
            {
                _visuals?.SetHeroesHighlight(
                    _isEnemyHero(leftHeroSide?.Hero),
                    EBattleHighlightColorType.EnemyTarget,
                    _isEnemyHero(rightHeroSide?.Hero),
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
            return BattleParticipantHelper.FindSideByHeroUnitId(_battleModel, heroUnitId);
        }

        private bool _isLeftSide(BattleSide side)
        {
            if (_battleModel == null || side == null)
            {
                return false;
            }

            if (_battleModel.IsCoOp)
            {
                return ReferenceEquals(side, _battleModel.SideA) || ReferenceEquals(side, _battleModel.SideB);
            }

            return ReferenceEquals(side, _battleModel.SideA);
        }

        private BattleSide _getMySide()
        {
            if (_battleModel == null)
            {
                return null;
            }

            string heroId = _userProvider.Id;
            if (string.IsNullOrEmpty(heroId))
            {
                Hero hero = _userProvider.GetHeroComponent<Hero>();
                heroId = hero?.Model?.HeroId;
            }

            return BattleParticipantHelper.GetMySide(_battleModel, heroId);
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

        private bool _isEnemyHero(BattleUnit unit)
        {
            if (unit == null || _battleModel == null)
            {
                return false;
            }

            BattleSide mySide = _findSideByHeroUnitId(_selectionState.PendingTargetCardActorSideHeroId);
            BattleSide targetSide = BattleGridRules.GetOwnerSide(_battleModel, unit);
            return mySide != null && targetSide != null && !BattleParticipantHelper.IsAllySide(_battleModel, mySide, targetSide);
        }

        private bool _isAllyUnit(BattleUnit unit)
        {
            if (unit == null || _battleModel == null)
            {
                return false;
            }

            BattleSide mySide = _getMySide();
            BattleSide targetSide = BattleGridRules.GetOwnerSide(_battleModel, unit);
            return BattleParticipantHelper.IsAllySide(_battleModel, mySide, targetSide);
        }

        private void _showCommandError(CommandResult result)
        {
            _showCommandMessage?.Invoke(CommandResultText.ToDebugText(result));
        }
    }
}
