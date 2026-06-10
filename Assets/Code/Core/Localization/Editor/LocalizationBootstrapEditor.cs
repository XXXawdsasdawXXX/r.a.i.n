using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Localization;
using UnityEditor.Localization.Plugins.CSV;
using UnityEditor.Localization.Plugins.CSV.Columns;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace Core.Localization.Editor
{
    public static class LocalizationBootstrapEditor
    {
        private const string LocalizationRoot = "Assets/Localization";
        private const string SettingsPath = LocalizationRoot + "/Localization Settings.asset";
        private const string LocalesPath = LocalizationRoot + "/Locales";
        private const string TablesPath = LocalizationRoot + "/StringTables";

        [MenuItem("R.A.I.N/Localization/Setup Project Localization")]
        public static void SetupProjectLocalization()
        {
            AddressableAssetSettingsDefaultObject.GetSettings(true);

            LocalizationSettings settings = AssetDatabase.LoadAssetAtPath<LocalizationSettings>(SettingsPath);
            if (settings == null)
            {
                _ensureDirectory(LocalizationRoot);
                settings = ScriptableObject.CreateInstance<LocalizationSettings>();
                AssetDatabase.CreateAsset(settings, SettingsPath);
            }

            LocalizationEditorSettings.ActiveLocalizationSettings = settings;

            _ensureLocale("en", "English", 0);
            _ensureLocale("ru", "Russian", 1);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            _importAllTables();

            EditorUtility.DisplayDialog(
                "Localization",
                "Localization setup complete.\n\n" +
                "- Locales: English (en), Russian (ru)\n" +
                "- Tables: MainMenu, CoreGame, Cards\n\n" +
                "Rebuild Addressables if you use remote content.",
                "OK");
        }

        [MenuItem("R.A.I.N/Localization/Reimport CSV Tables")]
        public static void ReimportCsvTables()
        {
            _importAllTables();
            EditorUtility.DisplayDialog("Localization", "CSV tables reimported.", "OK");
        }

        [MenuItem("R.A.I.N/Localization/Editor Preview")]
        public static void OpenEditorPreview()
        {
            LocalizationEditorPreviewWindow.Open();
        }

        private static void _importAllTables()
        {
            _importTable(LocalizationTables.MainMenu, "Localization/main_menu.csv");
            _importTable(LocalizationTables.CoreGame, "Localization/core_game.csv");
            _importTable(LocalizationTables.Cards, "Localization/cards.csv");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void _ensureDirectory(string assetsPath)
        {
            string fullPath = Path.Combine(Application.dataPath, assetsPath.Substring("Assets/".Length));
            Directory.CreateDirectory(fullPath);
        }

        private static void _ensureLocale(string code, string displayName, ushort sortOrder)
        {
            _ensureDirectory(LocalesPath);

            string path = $"{LocalesPath}/{displayName} ({code}).asset";
            Locale locale = AssetDatabase.LoadAssetAtPath<Locale>(path);
            if (locale == null)
            {
                locale = Locale.CreateLocale(new LocaleIdentifier(code));
                locale.LocaleName = displayName;
                locale.SortOrder = sortOrder;
                AssetDatabase.CreateAsset(locale, path);
            }

            if (LocalizationEditorSettings.GetLocale(code) == null)
            {
                LocalizationEditorSettings.AddLocale(locale);
            }
        }

        private static void _importTable(string tableName, string csvRelativePath)
        {
            string csvPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", csvRelativePath));
            if (!File.Exists(csvPath))
            {
                Debug.LogWarning($"[LocalizationBootstrap] CSV not found: {csvPath}");
                return;
            }

            StringTableCollection collection = LocalizationEditorSettings.GetStringTableCollection(tableName);
            if (collection == null)
            {
                collection = LocalizationEditorSettings.CreateStringTableCollection(
                    tableName,
                    $"{TablesPath}/{tableName}");
            }

            List<CsvColumns> columnMappings = _createProjectColumnMappings();
            _ensureCsvExtension(collection, csvRelativePath, columnMappings);

            using (StreamReader reader = new StreamReader(csvPath, Encoding.UTF8))
            {
                Csv.ImportInto(reader, collection, columnMappings, createUndo: false);
            }

            foreach (StringTable table in collection.StringTables)
            {
                EditorUtility.SetDirty(table);
            }

            EditorUtility.SetDirty(collection.SharedData);
            EditorUtility.SetDirty(collection);

            Debug.Log($"[LocalizationBootstrap] Imported '{tableName}' from {csvRelativePath}");
        }

        private static List<CsvColumns> _createProjectColumnMappings()
        {
            List<CsvColumns> columns = new List<CsvColumns>
            {
                new KeyIdColumns
                {
                    IncludeId = false,
                    IncludeSharedComments = false
                }
            };

            foreach (Locale locale in LocalizationEditorSettings.GetLocales())
            {
                columns.Add(new LocaleColumns
                {
                    LocaleIdentifier = locale.Identifier,
                    FieldName = locale.Identifier.Code,
                    IncludeComments = false
                });
            }

            return columns;
        }

        private static void _ensureCsvExtension(
            StringTableCollection collection,
            string csvRelativePath,
            List<CsvColumns> columnMappings)
        {
            CsvExtension extension = null;
            foreach (CollectionExtension existing in collection.Extensions)
            {
                if (existing is CsvExtension csvExtension)
                {
                    extension = csvExtension;
                    break;
                }
            }

            if (extension == null)
            {
                extension = new CsvExtension();
                collection.AddExtension(extension);
            }

            extension.File = csvRelativePath;
            extension.Columns.Clear();
            extension.Columns.AddRange(columnMappings);
            EditorUtility.SetDirty(collection);
        }
    }
}
