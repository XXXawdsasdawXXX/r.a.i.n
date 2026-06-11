using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Core.Localization.Editor
{
    public sealed class LocalizationEditorPreviewWindow : EditorWindow
    {
        private readonly List<Locale> _locales = new List<Locale>();
        private string[] _localeLabels = System.Array.Empty<string>();
        private int _selectedIndex;

        public static void Open()
        {
            LocalizationEditorPreviewWindow window = GetWindow<LocalizationEditorPreviewWindow>();
            window.titleContent = new GUIContent("Localization Preview");
            window.Show();
        }

        private void OnEnable()
        {
            _reloadLocales();
            _syncSelectedIndex();
            LocalizationSettings.SelectedLocaleChanged += _onSelectedLocaleChanged;
            LocalizationEditorSettings.EditorEvents.LocaleAdded += _onLocalesChanged;
            LocalizationEditorSettings.EditorEvents.LocaleRemoved += _onLocalesChanged;
        }

        private void OnDisable()
        {
            LocalizationSettings.SelectedLocaleChanged -= _onSelectedLocaleChanged;
            LocalizationEditorSettings.EditorEvents.LocaleAdded -= _onLocalesChanged;
            LocalizationEditorSettings.EditorEvents.LocaleRemoved -= _onLocalesChanged;
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Editor Locale Preview", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Switch locale here to preview UI in the Scene/Game view without Play Mode. " +
                "UIText components with a Localized String reference will refresh automatically.",
                MessageType.Info);

            if (!LocalizationSettings.HasSettings)
            {
                EditorGUILayout.HelpBox(
                    "Localization Settings not found. Create Localization Settings via Window > Asset Management > Localization Tables.",
                    MessageType.Warning);
                return;
            }

            _ensureLocalizationReady();

            if (_locales.Count == 0)
            {
                EditorGUILayout.HelpBox("No locales found in the project.", MessageType.Warning);
                if (GUILayout.Button("Refresh Locales"))
                {
                    _reloadLocales();
                }

                return;
            }

            EditorGUI.BeginChangeCheck();
            _selectedIndex = EditorGUILayout.Popup("Preview Locale", _selectedIndex, _localeLabels);
            if (EditorGUI.EndChangeCheck())
            {
                _applySelectedLocale();
            }

            if (GUILayout.Button("Refresh UI"))
            {
                _applySelectedLocale();
                RepaintAllViews();
            }
        }

        private void _onSelectedLocaleChanged(Locale _)
        {
            _syncSelectedIndex();
            Repaint();
        }

        private void _onLocalesChanged(Locale _)
        {
            _reloadLocales();
            _syncSelectedIndex();
            Repaint();
        }

        private void _reloadLocales()
        {
            _locales.Clear();
            _locales.AddRange(LocalizationEditorSettings.GetLocales());

            _localeLabels = new string[_locales.Count];
            for (int i = 0; i < _locales.Count; i++)
            {
                Locale locale = _locales[i];
                _localeLabels[i] = $"{locale.LocaleName} ({locale.Identifier.Code})";
            }
        }

        private void _syncSelectedIndex()
        {
            if (_locales.Count == 0)
            {
                _selectedIndex = 0;
                return;
            }

            Locale selected = LocalizationSettings.SelectedLocale;
            if (selected == null)
            {
                return;
            }

            for (int i = 0; i < _locales.Count; i++)
            {
                if (_locales[i].Equals(selected))
                {
                    _selectedIndex = i;
                    return;
                }
            }
        }

        private void _applySelectedLocale()
        {
            if (_selectedIndex < 0 || _selectedIndex >= _locales.Count)
            {
                return;
            }

            _ensureLocalizationReady();
            LocalizationSettings.SelectedLocale = _locales[_selectedIndex];
            RepaintAllViews();
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

        private static void RepaintAllViews()
        {
            EditorApplication.QueuePlayerLoopUpdate();
            SceneView.RepaintAll();
        }
    }
}
