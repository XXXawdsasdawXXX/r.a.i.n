using System;
using System.Collections.Generic;
using Core.GameLoop;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.Scripting;

namespace Core.Localization
{
    [Preserve]
    public sealed class LocalizationService : IService, IInitializeListener, IExitListener
    {
        public bool IsInitialized { get; set; }

        public const string LocalePrefsKey = "selected_locale_code";

        private static readonly string[] SupportedLocaleCodes = { "en", "ru" };

        public event Action LocaleChanged;

        public async UniTask Initialize()
        {
            await LocalizationSettings.InitializationOperation;
            _ensureRuntimeLocales();
            _applySavedLocale();
            _ensureDefaultLocale();
            LocalizationSettings.SelectedLocaleChanged -= _onSelectedLocaleChanged;
            LocalizationSettings.SelectedLocaleChanged += _onSelectedLocaleChanged;
            IsInitialized = true;
        }

        public void GameExit()
        {
            LocalizationSettings.SelectedLocaleChanged -= _onSelectedLocaleChanged;
        }

        public string Get(string table, string key, string fallback = null)
        {
            if (string.IsNullOrEmpty(table) || string.IsNullOrEmpty(key))
            {
                return fallback ?? string.Empty;
            }

            if (!IsInitialized)
            {
                return fallback ?? key;
            }

            string localized = LocalizationSettings.StringDatabase.GetLocalizedString(table, key);
            return string.IsNullOrEmpty(localized) ? fallback ?? key : localized;
        }

        public string Get(LocalizedString reference, string fallback = null)
        {
            if (reference == null || reference.IsEmpty)
            {
                return fallback ?? string.Empty;
            }

            string referenceFallback = fallback ?? _resolveReferenceKey(reference);

            if (!IsInitialized)
            {
                return referenceFallback;
            }

            string localized = reference.GetLocalizedString();
            return string.IsNullOrEmpty(localized) ? referenceFallback : localized;
        }

        public string Format(string table, string key, params object[] args)
        {
            if (!IsInitialized)
            {
                return _fallbackFormat(key, args);
            }

            return LocalizationSettings.StringDatabase.GetLocalizedString(table, key, args);
        }

        public string Format(LocalizedString reference, params object[] args)
        {
            if (reference == null || reference.IsEmpty)
            {
                return _fallbackFormat(string.Empty, args);
            }

            if (!IsInitialized)
            {
                return _fallbackFormat(_resolveReferenceKey(reference), args);
            }

            return reference.GetLocalizedString(args);
        }

        public IReadOnlyList<LocaleOption> GetLocaleOptions()
        {
            IList<Locale> locales = LocalizationSettings.AvailableLocales?.Locales;
            if (locales == null || locales.Count == 0)
            {
                return Array.Empty<LocaleOption>();
            }

            List<LocaleOption> options = new List<LocaleOption>(locales.Count);
            for (int i = 0; i < locales.Count; i++)
            {
                Locale locale = locales[i];
                options.Add(new LocaleOption(
                    i,
                    locale.Identifier.Code,
                    _getLocaleDisplayName(locale)));
            }

            return options;
        }

        public int GetSelectedLocaleIndex()
        {
            IList<Locale> locales = LocalizationSettings.AvailableLocales?.Locales;
            Locale selected = LocalizationSettings.SelectedLocale;
            if (locales == null || selected == null)
            {
                return 0;
            }

            for (int i = 0; i < locales.Count; i++)
            {
                if (locales[i].Equals(selected))
                {
                    return i;
                }
            }

            return 0;
        }

        public void SetLocaleByIndex(int index)
        {
            IList<Locale> locales = LocalizationSettings.AvailableLocales?.Locales;
            if (locales == null || index < 0 || index >= locales.Count)
            {
                return;
            }

            Locale locale = locales[index];
            if (LocalizationSettings.SelectedLocale != null
                && LocalizationSettings.SelectedLocale.Equals(locale))
            {
                return;
            }

            LocalizationSettings.SelectedLocale = locale;
            PlayerPrefs.SetString(LocalePrefsKey, locale.Identifier.Code);
            PlayerPrefs.Save();
        }

        public static LocalizationService TryGet()
        {
            if (Container.Instance == null)
            {
                return null;
            }

            try
            {
                return Container.Instance.GetService<LocalizationService>();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public string GetCardName(string cardId) =>
            Get(LocalizationTables.Cards, $"card.{cardId}.name", cardId);

        public string GetCardDescription(string cardId) =>
            Get(LocalizationTables.Cards, $"card.{cardId}.description", string.Empty);

        public string GetCompanionName(string companionAssetName)
        {
            if (string.IsNullOrEmpty(companionAssetName))
            {
                return string.Empty;
            }

            string companionId = _toCompanionLocalizationId(companionAssetName);
            return Get(LocalizationTables.Cards, $"companion.{companionId}.name", companionId);
        }

        public string GetCardTypeDisplayName(Enum cardType)
        {
            if (cardType == null)
            {
                return string.Empty;
            }

            string enumName = cardType.ToString();
            if (enumName.IndexOf(',') >= 0)
            {
                string[] parts = enumName.Split(',');
                List<string> localizedParts = new List<string>(parts.Length);

                for (int i = 0; i < parts.Length; i++)
                {
                    string part = parts[i].Trim();
                    localizedParts.Add(Get(
                        LocalizationTables.Cards,
                        $"ui.cards.type.{part.ToLowerInvariant()}",
                        part));
                }

                return string.Join(", ", localizedParts);
            }

            return Get(
                LocalizationTables.Cards,
                $"ui.cards.type.{enumName.ToLowerInvariant()}",
                enumName);
        }

        public string GetStatusName(string statusKey, string fallback) =>
            Get(LocalizationTables.Cards, $"ui.cards.status.{statusKey}", fallback);

        public string BuildCompanionInfo(bool isTemporary, int turnsLeft, int cardsPerTurn)
        {
            string lifeText = isTemporary
                ? Format(LocalizationTables.CoreGame, "ui.core_game.companion.temporary", turnsLeft)
                : Get(LocalizationTables.CoreGame, "ui.core_game.companion.lifetime", "Permanent");

            string cardsPerTurnText = Format(
                LocalizationTables.CoreGame,
                "ui.core_game.companion.cards_per_turn",
                cardsPerTurn);

            return $"{lifeText} | {cardsPerTurnText}";
        }

        public string BuildStatusLine(string statusKey, string fallbackName, float value, int duration)
        {
            System.Text.StringBuilder line = new System.Text.StringBuilder();
            line.Append(GetStatusName(statusKey, fallbackName));

            bool hasValue = value > 0f;
            bool hasDuration = duration > 0;
            if (!hasValue && !hasDuration)
            {
                return line.ToString();
            }

            line.Append(" (");
            if (hasValue)
            {
                line.Append(Mathf.CeilToInt(value));
            }

            if (hasDuration)
            {
                if (hasValue)
                {
                    line.Append(", ");
                }

                line.Append(duration);
                line.Append(Get(LocalizationTables.Cards, "ui.cards.hover.turn_short", " turn."));
            }

            line.Append(')');
            return line.ToString();
        }

        private void _onSelectedLocaleChanged(Locale _)
        {
            LocaleChanged?.Invoke();
        }

        private static string _toCompanionLocalizationId(string assetName)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                return string.Empty;
            }

            if (assetName.StartsWith("Companion_", StringComparison.Ordinal))
            {
                return "companion_" + assetName.Substring("Companion_".Length);
            }

            return assetName.ToLowerInvariant();
        }

        private void _applySavedLocale()
        {
            string savedCode = PlayerPrefs.GetString(LocalePrefsKey, string.Empty);
            if (string.IsNullOrEmpty(savedCode))
            {
                return;
            }

            Locale locale = LocalizationSettings.AvailableLocales?.GetLocale(savedCode);
            if (locale == null)
            {
                return;
            }

            LocalizationSettings.SelectedLocale = locale;
        }

        private static void _ensureRuntimeLocales()
        {
            ILocalesProvider provider = LocalizationSettings.AvailableLocales;
            if (provider == null)
            {
                return;
            }

            IList<Locale> locales = provider.Locales;
            if (locales != null && locales.Count > 0)
            {
                return;
            }

            Debug.LogWarning(
                "[LocalizationService] No locales loaded. Registering fallback en/ru.");

            for (int i = 0; i < SupportedLocaleCodes.Length; i++)
            {
                string code = SupportedLocaleCodes[i];
                if (provider.GetLocale(code) != null)
                {
                    continue;
                }

                Locale locale = Locale.CreateLocale(new LocaleIdentifier(code));
                locale.name = code;
                provider.AddLocale(locale);
            }
        }

        private static void _ensureDefaultLocale()
        {
            if (LocalizationSettings.SelectedLocale != null)
            {
                return;
            }

            IList<Locale> locales = LocalizationSettings.AvailableLocales?.Locales;
            if (locales == null || locales.Count == 0)
            {
                return;
            }

            LocalizationSettings.SelectedLocale = locales[0];
        }

        private static string _getLocaleDisplayName(Locale locale)
        {
            if (locale == null)
            {
                return string.Empty;
            }

            return locale.Identifier.Code switch
            {
                "en" => "English",
                "ru" => "Русский",
                _ => string.IsNullOrEmpty(locale.LocaleName) ? locale.Identifier.Code : locale.LocaleName
            };
        }

        private static string _resolveReferenceKey(LocalizedString reference)
        {
            if (reference == null || reference.IsEmpty)
            {
                return string.Empty;
            }

            TableEntryReference entryReference = reference.TableEntryReference;
            if (entryReference.ReferenceType == TableEntryReference.Type.Name)
            {
                return entryReference.Key;
            }

            return entryReference.KeyId.ToString();
        }

        private static string _fallbackFormat(string template, object[] args)
        {
            if (args == null || args.Length == 0)
            {
                return template;
            }

            try
            {
                return string.Format(template, args);
            }
            catch (FormatException)
            {
                return template;
            }
        }
    }
}
