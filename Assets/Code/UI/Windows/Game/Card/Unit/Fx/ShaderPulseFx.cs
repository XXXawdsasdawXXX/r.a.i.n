using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UI.Windows.Game.Card.Unit.Impacts;
using UnityEngine.UI;
using DG.Tweening;

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
            if (_view == null || settings == null || !_view.TryGetImpactTargets(out RectTransform _, out Image overlayImage))
            {
                return;
            }

            float duration = Mathf.Max(0.05f, settings.Duration);
            float targetScale = Mathf.Max(1f, settings.Scale);
            Color baseColor = _view.GetDefaultRenderColor();
            Color toColor = settings.Color;
            toColor.a = baseColor.a;

            overlayImage.color = baseColor;
            _view.SetImpactScale(1f);

            Sequence sequence = DOTween.Sequence()
                .SetUpdate(true)
                .Append(DOVirtual.Float(0f, 1f, duration * 0.4f, t =>
                {
                    overlayImage.color = Color.Lerp(baseColor, toColor, t);
                    _view.SetImpactScale(Mathf.Lerp(1f, targetScale, t));
                }))
                .Append(DOVirtual.Float(0f, 1f, duration * 0.6f, t =>
                {
                    overlayImage.color = Color.Lerp(toColor, baseColor, t);
                    _view.SetImpactScale(Mathf.Lerp(targetScale, 1f, t));
                }))
                .SetLink(_view.gameObject, LinkBehaviour.KillOnDisable);

            using (cancellationToken.Register(() => sequence.Kill()))
            {
                await sequence.AsyncWaitForCompletion().AsUniTask();
            }
        }
    }
}
