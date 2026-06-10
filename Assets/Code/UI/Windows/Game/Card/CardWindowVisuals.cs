using System;
using System.Collections.Generic;
using Core.GameLoop;
using CoreGame.Card.Data;
using CoreGame.Card.Logic;
using UI.Windows.Game.Card.Map;
using UI.Windows.Game.Card.Unit;
using UnityEngine;

namespace UI.Windows.Game.Card
{
    public class CardWindowVisuals
    {
        public event Action<string> CompanionClicked;
        public event Action<string, RectTransform, bool> UnitHoverEntered;
        public event Action UnitHoverExited;

        private readonly BattleUnitView _leftHeroView;
        private readonly BattleUnitView _rightHeroView;
        private readonly Transform _leftCompanionRoot;
        private readonly Transform _rightCompanionRoot;
        private readonly BattleUnitView _companionViewPrefab;
        private readonly BattleSideView _leftSideView;
        private readonly BattleSideView _rightSideView;
        private readonly GameEventDispatcher _gameEventDispatcher;

        private readonly Dictionary<BattleUnitView, string> _viewToUnitId = new Dictionary<BattleUnitView, string>();
        private readonly List<BattleUnitView> _leftCompanionViews = new List<BattleUnitView>();
        private readonly List<BattleUnitView> _rightCompanionViews = new List<BattleUnitView>();
        private BattleUnitView _hoveredUnitView;

        public CardWindowVisuals(
            BattleUnitView leftHeroView,
            BattleUnitView rightHeroView,
            Transform leftCompanionRoot,
            Transform rightCompanionRoot,
            BattleUnitView companionViewPrefab,
            BattleSideView leftSideView,
            BattleSideView rightSideView,
            GameEventDispatcher gameEventDispatcher)
        {
            _leftHeroView = leftHeroView;
            _rightHeroView = rightHeroView;
            _leftCompanionRoot = leftCompanionRoot;
            _rightCompanionRoot = rightCompanionRoot;
            _companionViewPrefab = companionViewPrefab;
            _leftSideView = leftSideView;
            _rightSideView = rightSideView;
            _gameEventDispatcher = gameEventDispatcher;
        }

        public void SetGridHighlighted(bool highlighted)
        {
            _leftSideView?.SetCellsHighlighted(highlighted);
            _rightSideView?.SetCellsHighlighted(highlighted);
        }

        public void ResetSelectionHighlights()
        {
            SetHeroesHighlight(false, EBattleHighlightColorType.AllyTarget, false, EBattleHighlightColorType.AllyTarget);
            SetGridHighlighted(false);
            SetCompanionHighlights(false, false, EBattleHighlightColorType.AllyTarget);
        }

        public void UpdateUnits(BattleModel battleModel)
        {
            _leftHeroView?.SetSide(false);
            _rightHeroView?.SetSide(true);

            if (battleModel?.IsCoOp == true)
            {
                _leftHeroView?.Set(battleModel.SideA?.Hero);
                _rightHeroView?.Set(battleModel.EnemySide?.Hero);
            }
            else
            {
                _leftHeroView?.Set(battleModel?.SideA?.Hero);
                _rightHeroView?.Set(battleModel?.SideB?.Hero);
            }

            _bindHeroUnitIds(battleModel);
            _updateCompanionViews(battleModel);
            _tryRefreshHoveredUnitTooltip();
        }

        public void PlayCardEffect(string unitId, ECardType cardType)
        {
            if (string.IsNullOrEmpty(unitId))
            {
                return;
            }

            if (_tryPlayHeroEffect(_leftHeroView, unitId, cardType))
            {
                return;
            }

            if (_tryPlayHeroEffect(_rightHeroView, unitId, cardType))
            {
                return;
            }

            _tryPlayCompanionEffect(_leftCompanionViews, unitId, cardType);
            _tryPlayCompanionEffect(_rightCompanionViews, unitId, cardType);
        }

        public void PlayReactionEffect(string unitId, EEffectType effectType)
        {
            if (string.IsNullOrEmpty(unitId))
            {
                return;
            }

            if (_tryPlayHeroReaction(_leftHeroView, unitId, effectType))
            {
                return;
            }

            if (_tryPlayHeroReaction(_rightHeroView, unitId, effectType))
            {
                return;
            }

            _tryPlayCompanionReaction(_leftCompanionViews, unitId, effectType);
            _tryPlayCompanionReaction(_rightCompanionViews, unitId, effectType);
        }

        public void UpdateAnchors(BattleModel battleModel)
        {
            if (battleModel == null)
            {
                return;
            }

            _anchorHero(_leftHeroView, _leftSideView, battleModel.SideA?.Hero);

            if (battleModel.IsCoOp)
            {
                _anchorHero(_rightHeroView, _rightSideView, battleModel.EnemySide?.Hero);
            }
            else
            {
                _anchorHero(_rightHeroView, _rightSideView, battleModel.SideB?.Hero);
            }

            _anchorCompanions(_leftCompanionViews, _leftSideView, battleModel.SideA);
            if (battleModel.IsCoOp)
            {
                _anchorCoOpAllyHero(battleModel);
                _anchorCompanions(_rightCompanionViews, _rightSideView, battleModel.EnemySide);
            }
            else
            {
                _anchorCompanions(_rightCompanionViews, _rightSideView, battleModel.SideB);
            }
        }

        private void _anchorCoOpAllyHero(BattleModel battleModel)
        {
            if (battleModel?.SideB?.Hero == null)
            {
                return;
            }

            int allyViewIndex = battleModel.SideA?.Companions.Count ?? 0;
            if (allyViewIndex >= _leftCompanionViews.Count)
            {
                return;
            }

            _anchorHero(_leftCompanionViews[allyViewIndex], _leftSideView, battleModel.SideB.Hero);
        }

        public void RefreshOccupiedCells(BattleModel battleModel)
        {
            if (battleModel == null)
            {
                return;
            }

            _leftSideView?.ClearOccupiedHighlights();
            _rightSideView?.ClearOccupiedHighlights();
            _markSideOccupiedCells(battleModel.SideA, _leftSideView);
            if (battleModel.IsCoOp)
            {
                _markSideOccupiedCells(battleModel.SideB, _leftSideView);
                _markSideOccupiedCells(battleModel.EnemySide, _rightSideView);
            }
            else
            {
                _markSideOccupiedCells(battleModel.SideB, _rightSideView);
            }
        }

        public void HighlightAvailableCellsForSide(BattleModel battleModel, BattleSide side)
        {
            SetGridHighlighted(false);
            if (battleModel == null || side == null)
            {
                return;
            }

            BattleSideView sideView = _resolveSideView(battleModel, side);
            if (sideView == null)
            {
                return;
            }

            EBattleHighlightColorType colorType = ReferenceEquals(side, battleModel.SideA)
                                                  || (battleModel.IsCoOp && ReferenceEquals(side, battleModel.SideB))
                ? EBattleHighlightColorType.AllyCell
                : EBattleHighlightColorType.EnemyCell;

            for (int cell = 0; cell < BattleGridRules.CELLS_PER_LINE; cell++)
            {
                _setCellHighlightIfFree(sideView, EBattleLine.Frontline, cell, colorType);
                _setCellHighlightIfFree(sideView, EBattleLine.Backline, cell, colorType);
            }
        }

        public void HighlightMoveTargetSide(BattleModel battleModel, BattleSide moveSide)
        {
            bool leftSide = ReferenceEquals(moveSide, battleModel?.SideA);
            SetCompanionHighlights(leftSide, !leftSide, EBattleHighlightColorType.AllyTarget);
        }

        public void SetHeroesHighlight(
            bool leftEnabled,
            EBattleHighlightColorType leftColor,
            bool rightEnabled,
            EBattleHighlightColorType rightColor)
        {
            _setUnitHighlight(_leftHeroView, leftEnabled, leftColor);
            _setUnitHighlight(_rightHeroView, rightEnabled, rightColor);
        }

        public void SetCompanionHighlights(bool leftEnabled, bool rightEnabled, EBattleHighlightColorType colorType)
        {
            _setCompanionHighlights(_leftCompanionViews, leftEnabled, colorType);
            _setCompanionHighlights(_rightCompanionViews, rightEnabled, colorType);
        }

        public void SetCompanionTargetHighlights(Func<string, bool> isValidTarget, Func<string, bool> isEnemyTarget)
        {
            _setCompanionTargetHighlight(_leftCompanionViews, isValidTarget, isEnemyTarget);
            _setCompanionTargetHighlight(_rightCompanionViews, isValidTarget, isEnemyTarget);
        }

        public void ValidateInspectorBindings(UnityEngine.Object owner)
        {
            if (_leftCompanionRoot == null || _rightCompanionRoot == null || _companionViewPrefab == null)
            {
            }

            if (BattleHighlightStyle.HighlightMaterial == null)
            {
            }

            _validateOccupiedHighlight(owner, _leftSideView, EBattleSideUi.Left);
            _validateOccupiedHighlight(owner, _rightSideView, EBattleSideUi.Right);
        }

        public void BindCells(Action<BattleGridCellView> onCellClicked, bool bind)
        {
            _bindCells(_leftSideView, onCellClicked, bind);
            _bindCells(_rightSideView, onCellClicked, bind);
        }

        public void BindHoverEvents(bool bind)
        {
            _bindHover(_leftHeroView, bind);
            _bindHover(_rightHeroView, bind);
            _bindHover(_leftCompanionViews, bind);
            _bindHover(_rightCompanionViews, bind);

            if (!bind)
            {
                _hoveredUnitView = null;
                UnitHoverExited?.Invoke();
            }
        }

        private void _updateCompanionViews(BattleModel battleModel)
        {
            if (battleModel?.IsCoOp == true)
            {
                _setCoOpAllyViews(battleModel);
                _setCompanionViews(_rightCompanionViews, battleModel.EnemySide, _rightCompanionRoot);
                return;
            }

            _setCompanionViews(_leftCompanionViews, battleModel?.SideA, _leftCompanionRoot);
            _setCompanionViews(_rightCompanionViews, battleModel?.SideB, _rightCompanionRoot);
        }

        private void _setCoOpAllyViews(BattleModel battleModel)
        {
            int allyUnitCount = (battleModel.SideA?.Companions.Count ?? 0)
                                + 1
                                + (battleModel.SideB?.Companions.Count ?? 0);
            _ensureCompanionPoolSize(_leftCompanionViews, allyUnitCount, _leftCompanionRoot);

            int index = 0;
            index = _fillCompanionViews(_leftCompanionViews, battleModel.SideA, index, false);
            index = _fillHeroAsAllyView(_leftCompanionViews, battleModel.SideB?.Hero, index, false);
            index = _fillCompanionViews(_leftCompanionViews, battleModel.SideB, index, false);

            for (int i = index; i < _leftCompanionViews.Count; i++)
            {
                BattleUnitView view = _leftCompanionViews[i];
                if (view == null)
                {
                    continue;
                }

                view.SetSide(false);
                view.Set(null);
                _viewToUnitId.Remove(view);
            }
        }

        private int _fillCompanionViews(List<BattleUnitView> views, BattleSide side, int startIndex, bool isRightSide)
        {
            if (side?.Companions == null)
            {
                return startIndex;
            }

            for (int i = 0; i < side.Companions.Count; i++)
            {
                if (startIndex >= views.Count)
                {
                    break;
                }

                BattleUnitView view = views[startIndex++];
                view.SetSide(isRightSide);
                view.Set(side.Companions[i]);
                _viewToUnitId[view] = side.Companions[i].UnitId;
            }

            return startIndex;
        }

        private int _fillHeroAsAllyView(List<BattleUnitView> views, BattleUnit hero, int startIndex, bool isRightSide)
        {
            if (hero == null || startIndex >= views.Count)
            {
                return startIndex;
            }

            BattleUnitView view = views[startIndex++];
            view.SetSide(isRightSide);
            view.Set(hero);
            _viewToUnitId[view] = hero.UnitId;
            return startIndex;
        }

        private void _setCompanionViews(List<BattleUnitView> views, BattleSide side, Transform root)
        {
            if (views == null)
            {
                return;
            }

            bool isRightSide = ReferenceEquals(root, _rightCompanionRoot);

            if (side == null)
            {
                foreach (BattleUnitView view in views)
                {
                    if (view == null)
                    {
                        continue;
                    }

                    view.SetSide(isRightSide);
                    view.Set(null);
                    _viewToUnitId.Remove(view);
                }

                return;
            }

            _ensureCompanionPoolSize(views, side.Companions.Count, root);

            for (int i = 0; i < views.Count; i++)
            {
                BattleUnitView view = views[i];
                if (view == null)
                {
                    continue;
                }

                view.SetSide(isRightSide);
                BattleUnit unit = i < side.Companions.Count ? side.Companions[i] : null;
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

        private void _ensureCompanionPoolSize(List<BattleUnitView> pool, int count, Transform root)
        {
            if (pool == null || root == null || _companionViewPrefab == null)
            {
                return;
            }

            while (pool.Count < count)
            {
                BattleUnitView view = UnityEngine.Object.Instantiate(_companionViewPrefab, root);
                view.SetSide(ReferenceEquals(root, _rightCompanionRoot));
                view.Clicked += () => _onCompanionClicked(view);
                _bindHover(view, true);
                IGameListener[] listeners = view.GetComponentsInChildren<IGameListener>(true);
                if (listeners.Length > 0)
                {
                    _gameEventDispatcher.InitializeListeners(listeners);
                }

                pool.Add(view);
            }
        }

        private void _onCompanionClicked(BattleUnitView view)
        {
            if (view != null && _viewToUnitId.TryGetValue(view, out string unitId))
            {
                CompanionClicked?.Invoke(unitId);
            }
        }

        private void _bindHeroUnitIds(BattleModel battleModel)
        {
            _bindHeroUnitId(_leftHeroView, battleModel?.SideA?.Hero);

            if (battleModel?.IsCoOp == true)
            {
                _bindHeroUnitId(_rightHeroView, battleModel.EnemySide?.Hero);
            }
            else
            {
                _bindHeroUnitId(_rightHeroView, battleModel?.SideB?.Hero);
            }
        }

        private void _bindHeroUnitId(BattleUnitView view, BattleUnit unit)
        {
            if (view == null)
            {
                return;
            }

            if (unit == null)
            {
                _viewToUnitId.Remove(view);
            }
            else
            {
                _viewToUnitId[view] = unit.UnitId;
            }
        }

        private bool _tryPlayHeroEffect(BattleUnitView heroView, string unitId, ECardType cardType)
        {
            if (heroView == null || !_viewToUnitId.TryGetValue(heroView, out string heroUnitId))
            {
                return false;
            }

            if (heroUnitId != unitId)
            {
                return false;
            }

            heroView.PlayCardFx(cardType);
            return true;
        }

        private void _tryPlayCompanionEffect(List<BattleUnitView> views, string unitId, ECardType cardType)
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

                if (_viewToUnitId.TryGetValue(view, out string mappedUnitId) && mappedUnitId == unitId)
                {
                    view.PlayCardFx(cardType);
                    return;
                }
            }
        }

        private bool _tryPlayHeroReaction(BattleUnitView heroView, string unitId, EEffectType effectType)
        {
            if (heroView == null || !_viewToUnitId.TryGetValue(heroView, out string heroUnitId))
            {
                return false;
            }

            if (heroUnitId != unitId)
            {
                return false;
            }

            heroView.PlayReactionFx(effectType);
            return true;
        }

        private void _tryPlayCompanionReaction(List<BattleUnitView> views, string unitId, EEffectType effectType)
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

                if (_viewToUnitId.TryGetValue(view, out string mappedUnitId) && mappedUnitId == unitId)
                {
                    view.PlayReactionFx(effectType);
                    return;
                }
            }
        }

        private BattleSideView _resolveSideView(BattleModel battleModel, BattleSide side)
        {
            if (battleModel == null || side == null)
            {
                return null;
            }

            if (ReferenceEquals(side, battleModel.SideA) || (battleModel.IsCoOp && ReferenceEquals(side, battleModel.SideB)))
            {
                return _leftSideView;
            }

            return _rightSideView;
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

        private static void _anchorHero(BattleUnitView heroView, BattleSideView sideView, BattleUnit hero)
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

        private static void _setCellHighlightIfFree(BattleSideView sideView, EBattleLine line, int cellIndex, EBattleHighlightColorType colorType)
        {
            BattleGridCellView cellView = sideView.GetCell(line, cellIndex);
            if (cellView == null)
            {
                return;
            }

            if (cellView.IsOccupied)
            {
                sideView.SetCellHighlighted(line, cellIndex, false, colorType);
                return;
            }

            sideView.SetCellHighlighted(line, cellIndex, true, colorType);
        }

        private static void _setCompanionHighlights(List<BattleUnitView> views, bool value, EBattleHighlightColorType colorType)
        {
            if (views == null)
            {
                return;
            }

            foreach (BattleUnitView view in views)
            {
                if (view != null)
                {
                    _setUnitHighlight(view, value, colorType);
                }
            }
        }

        private static void _setUnitHighlight(BattleUnitView view, bool enabled, EBattleHighlightColorType colorType)
        {
            if (view == null)
            {
                return;
            }

            UIHighlightMaterialController highlightController = view.HighlightController;
            if (highlightController == null)
            {
                return;
            }

            if (!enabled)
            {
                highlightController.Reset();
                return;
            }

            Color color = BattleHighlightStyle.GetColor(colorType);
            Material template = view.HighlightMaterialTemplate;
            highlightController.SetColor(color);
            highlightController.Apply(template);
        }

        private void _setCompanionTargetHighlight(
            List<BattleUnitView> views,
            Func<string, bool> isValidTarget,
            Func<string, bool> isEnemyTarget)
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
                    _setUnitHighlight(view, false, EBattleHighlightColorType.AllyTarget);
                    continue;
                }

                bool isValid = isValidTarget != null && isValidTarget(unitId);
                bool isEnemy = isEnemyTarget != null && isEnemyTarget(unitId);
                _setUnitHighlight(view, isValid, isEnemy ? EBattleHighlightColorType.EnemyTarget : EBattleHighlightColorType.AllyTarget);
            }
        }

        private static void _validateOccupiedHighlight(UnityEngine.Object owner, BattleSideView sideView, EBattleSideUi side)
        {
            if (sideView == null)
            {
                return;
            }

            for (int i = 0; i < BattleGridRules.CELLS_PER_LINE; i++)
            {
                _logIfCellHasNoOccupiedHighlight(owner, sideView.GetCell(EBattleLine.Frontline, i), side, EBattleLine.Frontline, i);
                _logIfCellHasNoOccupiedHighlight(owner, sideView.GetCell(EBattleLine.Backline, i), side, EBattleLine.Backline, i);
            }
        }

        private static void _logIfCellHasNoOccupiedHighlight(UnityEngine.Object owner, BattleGridCellView cell, EBattleSideUi side, EBattleLine line, int cellIndex)
        {
            if (cell == null || cell.HasOccupiedHighlightBinding)
            {
                return;
            }

        }

        private static void _bindCells(BattleSideView sideView, Action<BattleGridCellView> onCellClicked, bool bind)
        {
            if (sideView == null || onCellClicked == null)
            {
                return;
            }

            for (int i = 0; i < BattleGridRules.CELLS_PER_LINE; i++)
            {
                _bindCell(sideView.GetCell(EBattleLine.Frontline, i), onCellClicked, bind);
                _bindCell(sideView.GetCell(EBattleLine.Backline, i), onCellClicked, bind);
            }
        }

        private static void _bindCell(BattleGridCellView cell, Action<BattleGridCellView> onCellClicked, bool bind)
        {
            if (cell == null)
            {
                return;
            }

            if (bind)
            {
                cell.Clicked += onCellClicked;
            }
            else
            {
                cell.Clicked -= onCellClicked;
            }
        }

        private void _bindHover(BattleUnitView view, bool bind)
        {
            if (view == null)
            {
                return;
            }

            view.HoverEntered -= _onUnitHoverEntered;
            view.HoverExited -= _onUnitHoverExited;

            if (bind)
            {
                view.HoverEntered += _onUnitHoverEntered;
                view.HoverExited += _onUnitHoverExited;
            }
        }

        private void _bindHover(List<BattleUnitView> views, bool bind)
        {
            if (views == null)
            {
                return;
            }

            foreach (BattleUnitView view in views)
            {
                _bindHover(view, bind);
            }
        }

        private void _onUnitHoverEntered(BattleUnitView view)
        {
            _hoveredUnitView = view;
            if (view == null)
            {
                return;
            }

            if (_viewToUnitId.TryGetValue(view, out string unitId))
            {
                UnitHoverEntered?.Invoke(unitId, view.transform as RectTransform, view.IsRightSide);
            }
        }

        private void _onUnitHoverExited(BattleUnitView view)
        {
            if (view != null && _hoveredUnitView != null && !ReferenceEquals(view, _hoveredUnitView))
            {
                return;
            }

            _hoveredUnitView = null;
            UnitHoverExited?.Invoke();
        }

        private void _tryRefreshHoveredUnitTooltip()
        {
            if (_hoveredUnitView == null)
            {
                return;
            }

            if (!_viewToUnitId.TryGetValue(_hoveredUnitView, out string unitId))
            {
                UnitHoverExited?.Invoke();
                return;
            }

            UnitHoverEntered?.Invoke(unitId, _hoveredUnitView.transform as RectTransform, _hoveredUnitView.IsRightSide);
        }
    }
}
