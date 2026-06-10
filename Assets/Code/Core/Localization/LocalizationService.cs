using System;
using System.Collections.Generic;
using System.Text;
using Core.GameLoop;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
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

        private void _onSelectedLocaleChanged(UnityEngine.Localization.Locale _)
        {
            LocaleChanged?.Invoke();
        }

        public string Get(string table, string key, string fallback = null)
        {
            if (!IsInitialized)
            {
                return fallback ?? key;
            }

            string localized = LocalizationSettings.StringDatabase.GetLocalizedString(table, key);
            return string.IsNullOrEmpty(localized) ? fallback ?? key : localized;
        }

        public string Format(string table, string key, params object[] args)
        {
            if (!IsInitialized)
            {
                return fallbackFormat(key, args);
            }

            return LocalizationSettings.StringDatabase.GetLocalizedString(table, key, args);
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

        public string GetCardName(string cardId)
        {
            return Get(LocalizationTables.Cards, LocalizationKeys.Cards.CardName(cardId), cardId);
        }

        public string GetCardDescription(string cardId)
        {
            return Get(LocalizationTables.Cards, LocalizationKeys.Cards.CardDescription(cardId), string.Empty);
        }

        public string GetCompanionName(string companionAssetName)
        {
            if (string.IsNullOrEmpty(companionAssetName))
            {
                return string.Empty;
            }

            string companionId = _toCompanionLocalizationId(companionAssetName);
            return Get(
                LocalizationTables.Cards,
                LocalizationKeys.Cards.CompanionName(companionId),
                companionId);
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
                        LocalizationKeys.Cards.CardType(part),
                        part));
                }

                return string.Join(", ", localizedParts);
            }

            return Get(
                LocalizationTables.Cards,
                LocalizationKeys.Cards.CardType(enumName),
                enumName);
        }

        public string GetStatusName(string statusKey, string fallback)
        {
            return Get(LocalizationTables.Cards, LocalizationKeys.Cards.Status(statusKey), fallback);
        }

        public string GetCommandResultText(string key, string fallback, params object[] args)
        {
            if (!IsInitialized)
            {
                return fallbackFormat(fallback, args);
            }

            return LocalizationSettings.StringDatabase.GetLocalizedString(
                LocalizationTables.Cards,
                key,
                args);
        }

        public string BuildCompanionInfo(bool isTemporary, int turnsLeft, int cardsPerTurn)
        {
            string lifeText = isTemporary
                ? Format(LocalizationTables.CoreGame, LocalizationKeys.CoreGame.CompanionTemporary, turnsLeft)
                : Get(LocalizationTables.CoreGame, LocalizationKeys.CoreGame.CompanionLifetime);

            string cardsPerTurnText = Format(
                LocalizationTables.CoreGame,
                LocalizationKeys.CoreGame.CompanionCardsPerTurn,
                cardsPerTurn);

            return $"{lifeText} | {cardsPerTurnText}";
        }

        public string BuildStatusLine(string statusKey, string fallbackName, float value, int duration)
        {
            StringBuilder line = new StringBuilder();
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
                line.Append(Get(LocalizationTables.Cards, LocalizationKeys.Cards.HoverTurnShort, " turn."));
            }

            line.Append(')');
            return line.ToString();
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
                "[LocalizationService] No locales were loaded from Addressables. " +
                "Registering fallback locales (en, ru). " +
                "Run 'R.A.I.N/Localization/Setup Project Localization' in the Editor for full setup.");

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

        private string _getLocaleDisplayName(Locale locale)
        {
            if (locale == null)
            {
                return string.Empty;
            }

            if (!IsInitialized)
            {
                return locale.Identifier.Code switch
                {
                    "en" => "English",
                    "ru" => "Русский",
                    _ => string.IsNullOrEmpty(locale.LocaleName) ? locale.Identifier.Code : locale.LocaleName
                };
            }

            return locale.Identifier.Code switch
            {
                "en" => Get(LocalizationTables.MainMenu, LocalizationKeys.Common.LocaleEnglish, "English"),
                "ru" => Get(LocalizationTables.MainMenu, LocalizationKeys.Common.LocaleRussian, "Русский"),
                _ => string.IsNullOrEmpty(locale.LocaleName) ? locale.Identifier.Code : locale.LocaleName
            };
        }

        private static string fallbackFormat(string template, object[] args)
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
