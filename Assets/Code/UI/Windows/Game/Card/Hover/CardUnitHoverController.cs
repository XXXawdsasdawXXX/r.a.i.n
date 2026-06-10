using System;
using System.Threading;
using CoreGame.Card.Data;
using CoreGame.Card.Logic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace UI.Windows.Game.Card.Hover
{
    public sealed class CardUnitHoverController 
    {
        private const float ShowDelaySeconds = 1.5f;

        private readonly BattleService _battleService;
        private readonly CardUnitHoverView _view;
        
        private CardWindowVisuals _visuals;
        private CancellationTokenSource _showDelayCts;
        private string _pendingUnitId;
        private string _visibleUnitId;
        private RectTransform _visibleUnitRect;
        private bool _visibleIsRightSide;

        
        public CardUnitHoverController(BattleService battleService, CardUnitHoverView view)
        {
            _battleService = battleService;
            _view = view;
        }

        public void Bind(CardWindowVisuals visuals)
        {
            if (ReferenceEquals(_visuals, visuals))
            {
                return;
            }

            Unbind();
            _visuals = visuals;
            if (_visuals == null)
            {
                return;
            }

            _visuals.UnitHoverEntered += _onUnitHoverEntered;
            _visuals.UnitHoverExited += _onUnitHoverExited;
        }

        public void Unbind()
        {
            if (_visuals == null)
            {
                return;
            }

            _visuals.UnitHoverEntered -= _onUnitHoverEntered;
            _visuals.UnitHoverExited -= _onUnitHoverExited;
            _visuals = null;
            _cancelPendingShow();
        }

        public void Hide()
        {
            _cancelPendingShow();
            _visibleUnitId = null;
            _visibleUnitRect = null;
            _view?.Hide();
        }

        public void RefreshVisibleTooltip()
        {
            if (_view == null || string.IsNullOrEmpty(_visibleUnitId) || _visibleUnitRect == null)
            {
                return;
            }

            BattleUnit unit = _battleService.FindUnit(_visibleUnitId);
            if (unit == null)
            {
                Hide();
                return;
            }

            _view.Show(unit, _visibleUnitRect, _visibleIsRightSide);
        }

        private void _onUnitHoverEntered(string unitId, RectTransform unitRect, bool isRightSide)
        {
            if (_view == null || string.IsNullOrEmpty(unitId) || _battleService == null)
            {
                return;
            }

            BattleUnit unit = _battleService.FindUnit(unitId);
            if (unit == null)
            {
                Hide();
                return;
            }

            if (unitId == _visibleUnitId)
            {
                _visibleUnitRect = unitRect;
                _visibleIsRightSide = isRightSide;
                _view.Show(unit, unitRect, isRightSide);
                return;
            }

            _cancelPendingShow();
            _pendingUnitId = unitId;
            _showDelayCts = new CancellationTokenSource();
            _scheduleShow(unitRect, isRightSide, unitId, _showDelayCts.Token).Forget();
        }

        private void _onUnitHoverExited()
        {
            Hide();
        }

        private async UniTaskVoid _scheduleShow(
            RectTransform unitRect,
            bool isRightSide,
            string unitId,
            CancellationToken token)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(ShowDelaySeconds), cancellationToken: token);
                if (_pendingUnitId != unitId)
                {
                    return;
                }

                BattleUnit unit = _battleService.FindUnit(unitId);
                if (unit == null)
                {
                    return;
                }

                _visibleUnitId = unitId;
                _visibleUnitRect = unitRect;
                _visibleIsRightSide = isRightSide;
                _view.Show(unit, unitRect, isRightSide);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void _cancelPendingShow()
        {
            _showDelayCts?.Cancel();
            _showDelayCts?.Dispose();
            _showDelayCts = null;
            _pendingUnitId = null;
        }
    }
}
