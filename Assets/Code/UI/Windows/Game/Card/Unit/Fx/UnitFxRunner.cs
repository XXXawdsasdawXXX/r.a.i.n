using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UI.Windows.Game.Card.Unit.Impacts;

namespace UI.Windows.Game.Card.Unit.Fx
{
    public sealed class UnitFxRunner
    {
        private CancellationTokenSource _activeCts;
        private readonly BattleUnitView _view;

        public UnitFxRunner(BattleUnitView view)
        {
            _view = view;
        }

        public void Stop()
        {
            _activeCts?.Cancel();
            _activeCts?.Dispose();
            _activeCts = null;
            _view?.ResetImpactVisualState();
        }

        public void Play(IUnitImpact impact, UnitFxSettings settings)
        {
            if (impact == null || _view == null || settings == null)
            {
                return;
            }

            Stop();
            _activeCts = new CancellationTokenSource();
            _runUnitImpactAsync(impact, settings, _activeCts.Token).Forget();
        }

        public void Play(ICardImpact impact, UnitFxSettings settings)
        {
            if (impact == null || _view == null || settings == null)
            {
                return;
            }

            Stop();
            _activeCts = new CancellationTokenSource();
            _runCardImpactAsync(impact, settings, _activeCts.Token).Forget();
        }

        private async UniTaskVoid _runUnitImpactAsync(
            IUnitImpact impact,
            UnitFxSettings settings,
            CancellationToken token)
        {
            try
            {
                await impact.Play(settings, token);
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _view?.ResetImpactVisualState();
            }
        }

        private async UniTaskVoid _runCardImpactAsync(
            ICardImpact impact,
            UnitFxSettings settings,
            CancellationToken token)
        {
            try
            {
                await impact.Play(settings, token);
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _view?.ResetImpactVisualState();
            }
        }
    }
}
