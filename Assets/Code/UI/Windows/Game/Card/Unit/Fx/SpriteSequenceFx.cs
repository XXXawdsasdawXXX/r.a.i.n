using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UI.Windows.Game.Card.Unit.Impacts;
using UnityEngine.UI;

namespace UI.Windows.Game.Card.Unit.Fx
{
    public sealed class SpriteSequenceFx : ICardImpact, IUnitImpact
    {
        private readonly BattleUnitView _view;

        public SpriteSequenceFx(BattleUnitView view)
        {
            _view = view;
        }

        public async UniTask Play(UnitFxSettings settings, CancellationToken cancellationToken)
        {
            if (_view == null || settings == null || !_view.TryGetImpactTargets(out RectTransform _, out Image overlayImage))
            {
                return;
            }

            // Базовый frame-by-frame стиль без внешних библиотек.
            float duration = Mathf.Max(0.05f, settings.Duration);
            int steps = Mathf.Max(2, settings.Steps);
            float stepDuration = duration / steps;
            float maxScale = Mathf.Max(1f, settings.Scale);

            overlayImage.gameObject.SetActive(true);

            for (int i = 0; i < steps; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                float t = (float)i / Mathf.Max(1, steps - 1);
                float alpha = Mathf.Sin(t * Mathf.PI) * settings.Color.a;
                float scale = Mathf.Lerp(1f, maxScale, Mathf.Sin(t * Mathf.PI));

                Color c = settings.Color;
                c.a = alpha;
                overlayImage.color = c;
                _view.SetImpactScale(scale);

                await UniTask.Delay(TimeSpan.FromSeconds(stepDuration), cancellationToken: cancellationToken);
            }
        }
    }
}
