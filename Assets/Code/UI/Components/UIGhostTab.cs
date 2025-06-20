using Core.GameLoop;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UI.Data;
using UnityEngine;

namespace UI.Components
{
    public class UIGhostTab : Essential.Mono /*IPoolableUIElement,*/
    {
        public bool IsInitialized { get; set; }

        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private GameObject _root;
        [SerializeField] private RectTransform _body;
        [SerializeField] private UISettings _uiSettings;
        
        private float _originY;
        private Sequence _sequence;
        

        /*public UniTask GameStart()
        {
            _originY = _body.localPosition.y;
            return UniTask.CompletedTask;
        }*/

        [Button]
        public void Enable()
        {
            _resetView();

            _root.SetActive(true);

            _sequence?.Kill();

            _sequence = DOTween.Sequence();

            _sequence.Append(_canvasGroup
                .DOFade(_uiSettings.GhostTabAlpaTween.Value, _uiSettings.GhostTabAlpaTween.Duration)
                .SetEase(_uiSettings.GhostTabAlpaTween.Ease));

            _sequence.Append(_body
                .DOLocalMoveY(_uiSettings.GhostTabMoveYTween.Value, _uiSettings.GhostTabMoveYTween.Duration)
                .SetEase(_uiSettings.GhostTabMoveYTween.Ease));
        }

        [Button]
        public void Disable()
        {
            _sequence?.Kill();

            _root.SetActive(false);
        }

        private void _resetView()
        {
            _canvasGroup.alpha = 0;

            Vector3 localPosition = _body.localPosition;
            localPosition.y = _originY;
            _body.localPosition = localPosition;
        }
    }
}