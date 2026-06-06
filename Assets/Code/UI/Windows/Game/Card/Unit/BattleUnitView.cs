using Sirenix.OdinInspector;
using CoreGame.Card.Data;
using UI.Components;
using UI.Windows.Base;
using UnityEngine;
using System.Linq;

namespace UI.Windows.Game.Card.Unit
{
    public class BattleUnitView : UIWindowView
    {
        public event System.Action Clicked;

        [field: SerializeField] public UIImage Render { get; private set; }
        [field: Title("Params")]
        [field: SerializeField] public UIImage HealthFill { get; private set; }
        [field: SerializeField] public UIText HealthText { get; private set; }
        [field: SerializeField] public UIBattleStateIcon Armor { get; private set; }
        [field: SerializeField] public UIBattleStateIcon Attack { get; private set; }
        [SerializeField] private UIText _companionInfo;

        [SerializeField] private UIButton _clickArea;
        [SerializeField] private GameObject _highlight;

        public void Set(BattleUnit unit)
        {
            if (unit == null)
            {
                Close();
                return;
            }

            Open();

            float maxHp = Mathf.Max(1f, unit.MaxHP);
            float hp = Mathf.Max(0f, unit.HP);

            HealthFill.SetFillAmount(hp / maxHp);
            HealthText.SetText($"{Mathf.CeilToInt(hp)}/{Mathf.CeilToInt(maxHp)}");

            _setStateIcon(Armor, unit.Armor);
            _setStateIcon(Attack, unit.AutoActionType == EAutoActionType.AttackEnemyHero ? unit.AutoActionValue : 0f);
            _setCompanionInfo(unit);
        }

        private void OnEnable()
        {
            if (_clickArea != null)
            {
                _clickArea.Clicked += _onClicked;
            }
        }

        private void OnDisable()
        {
            if (_clickArea != null)
            {
                _clickArea.Clicked -= _onClicked;
            }
        }

        public void SetHighlighted(bool value)
        {
            if (_highlight != null)
            {
                _highlight.SetActive(value);
            }
        }

        private void _onClicked()
        {
            Clicked?.Invoke();
        }

        private static void _setStateIcon(UIBattleStateIcon stateIcon, float value)
        {
            if (stateIcon?.Icon == null || stateIcon.Value == null)
            {
                return;
            }

            bool show = value > 0f;
            stateIcon.Icon.gameObject.SetActive(show);
            stateIcon.Value.gameObject.SetActive(show);

            if (show)
            {
                stateIcon.Value.SetText(Mathf.CeilToInt(value).ToString());
            }
        }

        private void _setCompanionInfo(BattleUnit unit)
        {
            if (_companionInfo == null)
            {
                return;
            }

            if (unit == null || !unit.IsCompanion)
            {
                _companionInfo.gameObject.SetActive(false);
                return;
            }

            int turnsLeft = unit.Statuses?
                .FirstOrDefault(status => status.Type == EStatusType.SummonDuration)?.Duration ?? 0;
            bool isTemporary = turnsLeft > 0;

            string lifeText = isTemporary ? $"Temporary: {turnsLeft} turn(s)" : "Lifetime: until death";
            _companionInfo.SetText($"{lifeText} | Cards/turn: {Mathf.Max(0, unit.CompanionCardsPerTurn)}");
            _companionInfo.gameObject.SetActive(true);
        }
    }
}