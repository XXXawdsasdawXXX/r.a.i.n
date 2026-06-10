using Core.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;

namespace UI.Components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UIText))]
    [ExecuteAlways]
    public sealed class UILabelLocalizer : MonoBehaviour
    {
        [SerializeField] private UIText _uiText;
        [SerializeField] private string _table;
        [SerializeField] private string _key;
        [SerializeField] private bool _useAutoResolve = true;

        private LocalizationService _localization;
        private LocalizeStringEvent _localizeStringEvent;

        private void Awake()
        {
            _bindComponents();
        }

        private void OnEnable()
        {
            _bindComponents();
            _resolveBinding();

            if (UILabelLocalizationMap.ShouldSkip(gameObject))
            {
                return;
            }

            if (_hasExplicitLocalizationComponent())
            {
                return;
            }

            _localization = LocalizationService.TryGet();
            _ensureLocalizationReady();
            _refresh();

            LocalizationSettings.SelectedLocaleChanged -= _onSelectedLocaleChanged;
            LocalizationSettings.SelectedLocaleChanged += _onSelectedLocaleChanged;

            if (_localization != null)
            {
                _localization.LocaleChanged -= _refresh;
                _localization.LocaleChanged += _refresh;
            }
        }

        private void OnDisable()
        {
            LocalizationSettings.SelectedLocaleChanged -= _onSelectedLocaleChanged;

            if (_localization != null)
            {
                _localization.LocaleChanged -= _refresh;
            }
        }

        internal void EditorRefresh()
        {
            _bindComponents();
            _resolveBinding();
            _ensureLocalizationReady();
            _refresh();
        }

        private void _bindComponents()
        {
            if (_uiText == null)
            {
                _uiText = GetComponent<UIText>();
            }

            if (_localizeStringEvent == null)
            {
                _localizeStringEvent = GetComponent<LocalizeStringEvent>();
            }
        }

        private void _onSelectedLocaleChanged(Locale _)
        {
            _refresh();
        }

        private void _resolveBinding()
        {
            if (!string.IsNullOrEmpty(_table) && !string.IsNullOrEmpty(_key))
            {
                return;
            }

            if (!_useAutoResolve)
            {
                return;
            }

            TextMeshProUGUI textMesh = _uiText != null
                ? _uiText.GetComponentInChildren<TextMeshProUGUI>(true)
                : GetComponentInChildren<TextMeshProUGUI>(true);

            if (textMesh == null)
            {
                return;
            }

            if (!UILabelLocalizationMap.TryResolve(textMesh.text, out string table, out string key))
            {
                return;
            }

            _table = table;
            _key = key;
        }

        private bool _hasExplicitLocalizationComponent()
        {
            if (_localizeStringEvent == null)
            {
                return false;
            }

            return _localizeStringEvent.StringReference != null
                && !_localizeStringEvent.StringReference.IsEmpty;
        }

        private void _ensureLocalizationReady()
        {
            if (!LocalizationSettings.HasSettings)
            {
                return;
            }

            if (LocalizationSettings.InitializationOperation.IsDone)
            {
                return;
            }

            LocalizationSettings.InitializationOperation.WaitForCompletion();
        }

        private string _getLocalizedText()
        {
            if (string.IsNullOrEmpty(_table) || string.IsNullOrEmpty(_key))
            {
                return null;
            }

            if (Application.isPlaying && _localization != null && _localization.IsInitialized)
            {
                return _localization.Get(_table, _key);
            }

            if (!LocalizationSettings.HasSettings)
            {
                return null;
            }

            _ensureLocalizationReady();
            return LocalizationSettings.StringDatabase.GetLocalizedString(_table, _key);
        }

        private void _refresh()
        {
            if (UILabelLocalizationMap.ShouldSkip(gameObject))
            {
                return;
            }

            if (_hasExplicitLocalizationComponent())
            {
                return;
            }

            if (_uiText == null)
            {
                return;
            }

            string localized = _getLocalizedText();
            if (string.IsNullOrEmpty(localized))
            {
                return;
            }

            _uiText.SetText(localized);
            _markEditorDirty();
        }

        private void _markEditorDirty()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                return;
            }

            TextMeshProUGUI textMesh = _uiText.GetComponentInChildren<TextMeshProUGUI>(true);
            if (textMesh != null)
            {
                textMesh.SetAllDirty();
            }

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
    internal static class UILabelLocalizerEditorHook
    {
        static UILabelLocalizerEditorHook()
        {
            UnityEditor.EditorApplication.projectChanged += _refreshAll;
            LocalizationSettings.SelectedLocaleChanged += _onSelectedLocaleChanged;
        }

        private static void _onSelectedLocaleChanged(Locale _)
        {
            _refreshAll();
        }

        private static void _refreshAll()
        {
            if (Application.isPlaying)
            {
                return;
            }

            UILabelLocalizer[] localizers = Object.FindObjectsOfType<UILabelLocalizer>(true);
            for (int i = 0; i < localizers.Length; i++)
            {
                localizers[i].EditorRefresh();
            }
        }
    }
}
#endif
