using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UI.Windows.Game.Card.Unit.Impacts;
using UnityEngine.UI;

namespace UI.Windows.Game.Card.Unit.Fx
{
    public sealed class ShaderPulseFx : ICardImpact, IUnitImpact
    {
        private readonly BattleUnitView _view;

        public ShaderPulseFx(BattleUnitView view)
        {
            _view = view;
        }

        public async UniTask Play(UnitFxSettings settings, CancellationToken cancellationToken)
        {
            if (_view == null || settings == null || !_view.TryGetImpactTargets(out RectTransform fxRoot, out Image overlayImage))
            {
                return;
            }

            float duration = Mathf.Max(0.05f, settings.Duration);
            float upPart = duration * 0.4f;
            float downPart = duration * 0.6f;
            Color targetColor = settings.Color;
            float targetScale = Mathf.Max(1f, settings.Scale);

            overlayImage.gameObject.SetActive(true);
            overlayImage.color = new Color(targetColor.r, targetColor.g, targetColor.b, 0f);
            _view.SetImpactScale(1f);

            await _lerpAlphaAndScale(
                overlayImage,
                _view,
                0f,
                targetColor.a,
                1f,
                targetScale,
                upPart,
                cancellationToken);

            await _lerpAlphaAndScale(
                overlayImage,
                _view,
                targetColor.a,
                0f,
                targetScale,
                1f,
                downPart,
                cancellationToken);
        }

        private static async UniTask _lerpAlphaAndScale(
            Image overlayImage,
            BattleUnitView view,
            float fromAlpha,
            float toAlpha,
            float fromScale,
            float toScale,
            float duration,
            CancellationToken cancellationToken)
        {
            if (duration <= 0f)
            {
                return;
            }

            float endTime = Time.unscaledTime + duration;
            while (Time.unscaledTime < endTime)
            {
                cancellationToken.ThrowIfCancellationRequested();
                float t = 1f - Mathf.Clamp01((endTime - Time.unscaledTime) / duration);

                Color color = overlayImage.color;
                color.a = Mathf.Lerp(fromAlpha, toAlpha, t);
                overlayImage.color = color;
                view.SetImpactScale(Mathf.Lerp(fromScale, toScale, t));

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            Color final = overlayImage.color;
            final.a = toAlpha;
            overlayImage.color = final;
            view.SetImpactScale(toScale);
        }
    }
}
