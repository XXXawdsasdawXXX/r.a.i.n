using Sirenix.OdinInspector;
using CoreGame.Card.Data;
using UI.Components;
using UI.Windows.Base;
using UnityEngine;
using System.Linq;
using Essential;
using DG.Tweening;
using System;
using System.Collections.Generic;

namespace UI.Windows.Game.Card.Unit
{
    public class BattleUnitView : UIWindowView
    {
        public event System.Action Clicked;

        public UIHighlightMaterialController HighlightController { get; private set; }
        public Material HighlightMaterialTemplate => BattleHighlightStyle.ResolveHighlightMaterial(Render?.Image?.material);
        [field: SerializeField] public UIImage Render { get; private set; }
        [SerializeField] private UIHighlightMaterialController.EType _highlightType = UIHighlightMaterialController.EType.Outline;
        
        [field: Title("Params")]
        [field: SerializeField] public UIImage HealthFill { get; private set; }
        [field: SerializeField] public UIText HealthText { get; private set; }
        [field: SerializeField] public UIBattleStateIcon Armor { get; private set; }
        [field: SerializeField] public UIBattleStateIcon Attack { get; private set; }
        
        [SerializeField] private UIText _companionInfo;

        [SerializeField] private UIButton _clickArea;

        [Title("Play Card FX")]
        [SerializeField] private RectTransform _fxRoot;
        [SerializeField] private UIImage _fxOverlay;
        [SerializeField] private CardFxPreset _defaultFxPreset = new CardFxPreset(new Color(0.35f, 0.65f, 1f, 0.6f), 1.08f, 0.45f);
        [SerializeField] private List<CardFxBinding> _cardFxBindings = new List<CardFxBinding>
        {
            new CardFxBinding(ECardType.Attack, new CardFxPreset(new Color(1f, 0.35f, 0.35f, 0.6f), 1.1f, 0.4f)),
            new CardFxBinding(ECardType.Summon, new CardFxPreset(new Color(0.55f, 1f, 0.45f, 0.6f), 1.12f, 0.5f)),
            new CardFxBinding(ECardType.Spell, new CardFxPreset(new Color(0.35f, 0.65f, 1f, 0.6f), 1.08f, 0.45f)),
        };

        private Sequence _cardFxSequence;


        
        public void Set(BattleUnit unit)
        {
            HighlightController?.Reset();
            
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
            HighlightController = new UIHighlightMaterialController(Render.Image, _highlightType);
            Log.Info(this, $"[HighlightUnit] enable renderImage={Render?.Image != null} template={HighlightMaterialTemplate?.name ?? "null"}");

            if (_clickArea != null)
            {
                _clickArea.Clicked += _onClicked;
            }
        }

        private void OnDisable()
        {
            Log.Info(this, "[HighlightUnit] disable reset");
            HighlightController?.Reset();
            _cardFxSequence?.Kill();
            _cardFxSequence = null;
            if (_fxOverlay != null)
            {
                _fxOverlay.gameObject.SetActive(false);
            }

            if (_clickArea != null)
            {
                _clickArea.Clicked -= _onClicked;
            }
        }

        private void _onClicked()
        {
            Clicked?.Invoke();
        }

        public void PlayCardFx(ECardType cardType)
        {
            if (_fxRoot == null || _fxOverlay == null || _fxOverlay.Image == null)
            {
                return;
            }

            CardFxPreset preset = _resolvePreset(cardType);
            _cardFxSequence = _playFxSequence(preset);
        }

        private CardFxPreset _resolvePreset(ECardType cardType)
        {
            foreach (CardFxBinding binding in _cardFxBindings)
            {
                if (binding == null)
                {
                    continue;
                }

                if (cardType.HasFlag(binding.Matches))
                {
                    return binding.Preset ?? _defaultFxPreset;
                }
            }

            return _defaultFxPreset;
        }

        private Sequence _playFxSequence(CardFxPreset preset)
        {
            _cardFxSequence?.Kill();

            Color fxColor = preset != null ? preset.Color : _defaultFxPreset.Color;
            float duration = Mathf.Max(0.05f, preset != null ? preset.Duration : _defaultFxPreset.Duration);
            float punch = Mathf.Max(1f, preset != null ? preset.PunchScale : _defaultFxPreset.PunchScale);

            _fxOverlay.Image.color = new Color(fxColor.r, fxColor.g, fxColor.b, 0f);
            _fxOverlay.gameObject.SetActive(true);
            _fxRoot.localScale = Vector3.one;

            return DOTween.Sequence()
                .SetLink(gameObject, LinkBehaviour.KillOnDisable)
                .Append(_fxOverlay.Image.DOFade(fxColor.a, duration * 0.4f))
                .Join(_fxRoot.DOScale(punch, duration * 0.4f))
                .Append(_fxOverlay.Image.DOFade(0f, duration * 0.6f))
                .Join(_fxRoot.DOScale(1f, duration * 0.6f))
                .OnComplete(() =>
                {
                    _fxOverlay.gameObject.SetActive(false);
                    _cardFxSequence = null;
                });
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
        
        protected override void OnDestroy()
        {
            Log.Info(this, "[HighlightUnit] destroy dispose");
            HighlightController?.Dispose();
            HighlightController = null;
            
            base.OnDestroy();
        }

        [Serializable]
        private sealed class CardFxBinding
        {
            [SerializeField] private ECardType _matches;
            [SerializeField] private CardFxPreset _preset = new CardFxPreset(new Color(0.35f, 0.65f, 1f, 0.6f), 1.08f, 0.45f);

            public ECardType Matches => _matches;
            public CardFxPreset Preset => _preset;

            public CardFxBinding(ECardType matches, CardFxPreset preset)
            {
                _matches = matches;
                _preset = preset;
            }
        }

        [Serializable]
        private sealed class CardFxPreset
        {
            [SerializeField] private Color _color = new Color(0.35f, 0.65f, 1f, 0.6f);
            [SerializeField] private float _punchScale = 1.08f;
            [SerializeField] private float _duration = 0.45f;

            public Color Color => _color;
            public float PunchScale => _punchScale;
            public float Duration => _duration;

            public CardFxPreset(Color color, float punchScale, float duration)
            {
                _color = color;
                _punchScale = punchScale;
                _duration = duration;
            }
        }
    }
}