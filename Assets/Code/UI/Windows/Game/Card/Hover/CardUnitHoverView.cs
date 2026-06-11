using System.Collections.Generic;
using System.Text;
using Core.Localization;
using CoreGame.Card.Data;
using DG.Tweening;
using UI.Components;
using UI.Windows.Base;
using UnityEngine;

namespace UI.Windows.Game.Card.Hover
{
    public class CardUnitHoverView : UIWindowView
    {
        [SerializeField] private UIText _unitHoverInfoText;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Vector2 _offset = new Vector2(160f, 0f);
        [SerializeField] private float _fadeDuration = 0.2f;

        private Tween _fadeTween;

        private void Awake()
        {
            _canvasGroup.alpha = 0f;
            base.Close();
        }

        protected override void OnDestroy()
        {
            _killFadeTween();
            base.OnDestroy();
        }

        public void Show(BattleUnit unit, RectTransform unitRect, bool isRightSide)
        {
            _setPositionNearUnit(unitRect, isRightSide);
            
            _unitHoverInfoText.SetText(_buildUnitHoverInfo(unit));

            _canvasGroup.alpha = 0;
            
            base.Open();
            
            _killFadeTween();
        
            _canvasGroup.alpha = Mathf.Clamp01(_canvasGroup.alpha);
            _fadeTween = _canvasGroup.DOFade(1f, _fadeDuration)
                .SetEase(Ease.OutQuad)
                .SetLink(body.gameObject, LinkBehaviour.KillOnDisable)
                .OnComplete(() => _fadeTween = null);
        }

        public void Hide()
        {
            _killFadeTween();
            _fadeTween = _canvasGroup.DOFade(0f, _fadeDuration)
                .SetEase(Ease.OutQuad)
                .SetLink(body.gameObject, LinkBehaviour.KillOnDisable)
                .OnComplete(() =>
                {
                    _fadeTween = null;
                    base.Close();
                });
        }
        
        private void _setPositionNearUnit(RectTransform unitRect, bool isRightSide)
        {
            Vector3 worldPos = unitRect.position;
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, worldPos);
            float offsetX = isRightSide ? -_offset.x : _offset.x;
            body.position = screenPoint + new Vector2(offsetX, _offset.y);
        }

        private static string _buildUnitHoverInfo(BattleUnit unit)
        {
            LocalizationService localization = LocalizationService.TryGet();
            StringBuilder builder = new StringBuilder(128);
            int sectionsCount = 0;

            int summonTurnsLeft = _getSummonTurnsLeft(unit);
            bool isTemporary = summonTurnsLeft > 0;
            if (isTemporary)
            {
                builder.AppendLine(_getText(localization, LocalizationTables.Cards, "ui.cards.hover.temporary_unit", "Temporary unit"));
                sectionsCount++;
            }

            if (unit.AutoActionType != EAutoActionType.None)
            {
                if (sectionsCount > 0)
                {
                    builder.AppendLine();
                }

                builder.Append(_getText(localization, LocalizationTables.Cards, "ui.cards.hover.auto_action", "Auto-action: "));
                builder.Append(_getAutoActionDescription(unit.AutoActionType, localization));
                if (unit.AutoActionValue > 0f)
                {
                    builder.Append(" (");
                    builder.Append(Mathf.CeilToInt(unit.AutoActionValue));
                    builder.Append(')');
                }

                sectionsCount++;
            }

            List<string> statusLines = _collectStatusLines(unit.Statuses, localization);
            if (statusLines.Count > 0)
            {
                if (sectionsCount > 0)
                {
                    builder.AppendLine();
                    builder.AppendLine();
                }

                builder.AppendLine(_getText(localization, LocalizationTables.Cards, "ui.cards.hover.effects", "Effects:"));
                foreach (string statusLine in statusLines)
                {
                    builder.Append("- ");
                    builder.AppendLine(statusLine);
                }

                sectionsCount++;
            }

            if (isTemporary)
            {
                if (sectionsCount > 0)
                {
                    builder.AppendLine();
                }

                builder.Append(_getText(localization, LocalizationTables.Cards, "ui.cards.hover.turns_left", "Turns left: "));
                builder.Append(summonTurnsLeft);
                sectionsCount++;
            }

            if (sectionsCount == 0)
            {
                return _getText(localization, LocalizationTables.Cards, "ui.cards.hover.no_effects", "No active effects");
            }

            return builder.ToString();
        }

        private static string _getText(
            LocalizationService localization,
            string table,
            string key,
            string fallback)
        {
            return localization != null
                ? localization.Get(table, key, fallback)
                : fallback;
        }

        private static int _getSummonTurnsLeft(BattleUnit unit)
        {
            if (unit?.Statuses == null)
            {
                return 0;
            }

            for (int i = 0; i < unit.Statuses.Count; i++)
            {
                StatusEffect status = unit.Statuses[i];
                if (status != null && status.Type == EStatusType.SummonDuration)
                {
                    return Mathf.Max(0, status.Duration);
                }
            }

            return 0;
        }

        private static string _getAutoActionDescription(
            EAutoActionType autoActionType,
            LocalizationService localization)
        {
            switch (autoActionType)
            {
                case EAutoActionType.AttackEnemyHero:
                    return _getText(
                        localization,
                        LocalizationTables.Cards,
                        "ui.cards.hover.attack_enemy_hero",
                        "Attack enemy hero");
                case EAutoActionType.GiveShieldToOwnerHero:
                    return _getText(
                        localization,
                        LocalizationTables.Cards,
                        "ui.cards.hover.shield_owner",
                        "Shield to allied hero");
                default:
                    return _getText(
                        localization,
                        LocalizationTables.Cards,
                        "ui.cards.hover.none",
                        "None");
            }
        }

        private static List<string> _collectStatusLines(
            List<StatusEffect> statuses,
            LocalizationService localization)
        {
            List<string> lines = new List<string>();
            if (statuses == null || statuses.Count == 0)
            {
                return lines;
            }

            foreach (StatusEffect status in statuses)
            {
                if (status == null || status.Type == EStatusType.SummonDuration)
                {
                    continue;
                }

                string statusKey = _getStatusKey(status.Type);
                string fallbackName = status.Type.ToString();
                string line = localization != null
                    ? localization.BuildStatusLine(
                        statusKey,
                        fallbackName,
                        status.Value,
                        status.Duration)
                    : _buildStatusLineFallback(status, fallbackName);

                lines.Add(line);
            }

            return lines;
        }

        private static string _buildStatusLineFallback(StatusEffect status, string fallbackName)
        {
            StringBuilder line = new StringBuilder();
            line.Append(fallbackName);

            bool hasValue = status.Value > 0f;
            bool hasDuration = status.Duration > 0;
            if (!hasValue && !hasDuration)
            {
                return line.ToString();
            }

            line.Append(" (");
            if (hasValue)
            {
                line.Append(Mathf.CeilToInt(status.Value));
            }

            if (hasDuration)
            {
                if (hasValue)
                {
                    line.Append(", ");
                }

                line.Append(status.Duration);
                line.Append(" turn.");
            }

            line.Append(')');
            return line.ToString();
        }

        private static string _getStatusKey(EStatusType statusType)
        {
            switch (statusType)
            {
                case EStatusType.Bleed:
                    return "bleed";
                case EStatusType.Poison:
                    return "poison";
                case EStatusType.Burn:
                    return "burn";
                case EStatusType.Electro:
                    return "electro";
                case EStatusType.Stun:
                    return "stun";
                case EStatusType.Weak:
                    return "weak";
                case EStatusType.Regeneration:
                    return "regen";
                case EStatusType.EnergyCostReduction:
                    return "cost_reduction";
                case EStatusType.CritBoost:
                    return "crit_boost";
                case EStatusType.ArmorStance:
                    return "armor_stance";
                default:
                    return statusType.ToString().ToLowerInvariant();
            }
        }

        private void _killFadeTween()
        {
            _fadeTween?.Kill();
            _fadeTween = null;
        }
    }
}
