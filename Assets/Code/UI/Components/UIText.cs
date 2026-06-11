using DG.Tweening;
using Plugins.Demigiant.DOTween.Modules;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace UI.Components
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class UIText : UISelectable
    {
        [SerializeField] private TextMeshProUGUI _textMeshPro;
        [SerializeField] private LocalizedString _localizedString = new LocalizedString();

        [SerializeField] private bool _dynamicOverride;
        private Tween _tween;
        private LocalizedString.ChangeHandler _changeHandler;

        public LocalizedString LocalizedString
        {
            get => _localizedString;
            set
            {
                _clearHandler();
                _localizedString = value;
                _dynamicOverride = false;

                if (isActiveAndEnabled)
                {
                    _registerHandler();
                }
            }
        }

        private void OnEnable()
        {
            _registerHandler();
            LocalizationSettings.SelectedLocaleChanged += _onLocaleChanged;
        }

        private void OnDisable()
        {
            _clearHandler();
            LocalizationSettings.SelectedLocaleChanged -= _onLocaleChanged;
        }

        private void OnDestroy()
        {
            _clearHandler();
        }

        private void OnValidate()
        {
            if (_dynamicOverride)
            {
                return;
            }

            _localizedString?.RefreshString();
        }

        public void SetText(string text)
        {
            _dynamicOverride = true;

            if (_textMeshPro != null)
            {
                _textMeshPro.SetText(text);
            }
        }

        public override void SetInteractable(bool isInteractable)
        {
            _textMeshPro.raycastTarget = isInteractable;
        }

        public void Colorize(ColorTweenData tweenData)
        {
            _tween?.Kill();

            _tween = _textMeshPro.DOColor(tweenData.Color, tweenData.Duration)
                .SetEase(tweenData.Ease)
                .SetLink(gameObject, LinkBehaviour.KillOnDisable);
        }

        internal void EditorRefreshLocalization()
        {
            _dynamicOverride = false;
            _registerHandler();
            _localizedString?.RefreshString();
        }

        private void _registerHandler()
        {
            if (_dynamicOverride || _localizedString == null || _localizedString.IsEmpty)
            {
                return;
            }

            _ensureLocalizationReady();

            if (_changeHandler == null)
            {
                _changeHandler = _onLocalizedStringChanged;
            }

            _localizedString.StringChanged -= _changeHandler;
            _localizedString.StringChanged += _changeHandler;
        }

        private void _clearHandler()
        {
            if (_localizedString != null && _changeHandler != null)
            {
                _localizedString.StringChanged -= _changeHandler;
            }
        }

        private void _onLocalizedStringChanged(string value)
        {
            if (_dynamicOverride || _textMeshPro == null || string.IsNullOrEmpty(value))
            {
                return;
            }

            _textMeshPro.SetText(value);
            _markEditorDirty();
        }

        private void _onLocaleChanged(UnityEngine.Localization.Locale _)
        {
            if (_dynamicOverride)
            {
                return;
            }

            _localizedString?.RefreshString();
        }

        private static void _ensureLocalizationReady()
        {
            if (!LocalizationSettings.HasSettings)
            {
                return;
            }

            if (!LocalizationSettings.InitializationOperation.IsDone)
            {
                LocalizationSettings.InitializationOperation.WaitForCompletion();
            }
        }

        private void _markEditorDirty()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                return;
            }

            _textMeshPro.SetAllDirty();
            UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
            UnityEditor.SceneView.RepaintAll();
#endif
        }
    }
}

#if UNITY_EDITOR
namespace UI.Components
{
    [UnityEditor.InitializeOnLoad]
    internal static class UITextEditorHook
    {
        static UITextEditorHook()
        {
            UnityEditor.EditorApplication.projectChanged += _refreshAll;
            LocalizationSettings.SelectedLocaleChanged += _onSelectedLocaleChanged;
        }

        private static void _onSelectedLocaleChanged(UnityEngine.Localization.Locale _)
        {
            _refreshAll();
        }

        private static void _refreshAll()
        {
            if (Application.isPlaying)
            {
                return;
            }

            UIText[] texts = Object.FindObjectsOfType<UIText>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                texts[i].EditorRefreshLocalization();
            }
        }
    }
}
#endif
