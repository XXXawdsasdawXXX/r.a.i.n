using Sirenix.OdinInspector;
using Core.Localization;
using CoreGame.Card.Data;
using UI.Components;
using UI.Windows.Base;
using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;
using UI.Windows.Game.Card.Unit.Fx;
using UI.Windows.Game.Card.Unit.Impacts;
using UnityEngine.UI;

namespace UI.Windows.Game.Card.Unit
{
    public class BattleUnitView : UIWindowView
    {
        public event Action Clicked;
        public event Action<BattleUnitView> HoverEntered;
        public event Action<BattleUnitView> HoverExited;

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
        [SerializeField] private ECardImpactType _defaultCardImpactType = ECardImpactType.ShaderPulse;
        [SerializeField] private UnitFxSettings _defaultCardFxSettings = new UnitFxSettings();
        [SerializeField] private List<CardFxBinding> _cardFxBindings = new List<CardFxBinding>();

        [Title("Reaction FX")]
        [SerializeField] private EUnitImpactType _defaultUnitImpactType = EUnitImpactType.SpriteSequence;
        [SerializeField] private UnitFxSettings _defaultReactionFxSettings = new UnitFxSettings();
        [SerializeField] private List<EffectReactionBinding> _effectReactionBindings = new List<EffectReactionBinding>();

        private readonly Dictionary<ECardImpactType, ICardImpact> _cardImpacts = new Dictionary<ECardImpactType, ICardImpact>();
        private readonly Dictionary<EUnitImpactType, IUnitImpact> _unitImpacts = new Dictionary<EUnitImpactType, IUnitImpact>();
        private UnitFxRunner _fxRunner;
        private Color _defaultRenderColor = Color.white;
        private bool _isRightSide;
        private BattleUnit _currentUnit;

        public bool IsRightSide => _isRightSide;


        
        public void Set(BattleUnit unit)
        {
            HighlightController?.Reset();
            _currentUnit = unit;
            
            if (unit == null)
            {
                Close();
                return;
            }

            Open();
            _applyRenderMirror();

            float maxHp = Mathf.Max(1f, unit.MaxHP);
            float hp = Mathf.Max(0f, unit.HP);

            HealthFill.SetFillAmount(hp / maxHp);
            HealthText.SetText($"{Mathf.CeilToInt(hp)}/{Mathf.CeilToInt(maxHp)}");

            _setStateIcon(Armor, unit.Armor);
            _setStateIcon(Attack, unit.AutoActionType == EAutoActionType.AttackEnemyHero ? unit.AutoActionValue : 0f);
            _setCompanionInfo(unit);
        }

        public void SetSide(bool isRightSide)
        {
            _isRightSide = isRightSide;
            _applyRenderMirror();
        }

        private void OnEnable()
        {
            HighlightController = new UIHighlightMaterialController(Render.Image, _highlightType);
            _cacheRenderDefaults();
            _applyRenderMirror();
            _fxRunner = new UnitFxRunner(this);
            _initializeImpactDictionaries();

            if (_clickArea != null)
            {
                _clickArea.Clicked += _onClicked;
                _clickArea.Selected += _onHoverEntered;
                _clickArea.Deselected += _onHoverExited;
            }
        }

        private void OnDisable()
        {
            HighlightController?.Reset();
            _fxRunner?.Stop();

            if (_clickArea != null)
            {
                _clickArea.Clicked -= _onClicked;
                _clickArea.Selected -= _onHoverEntered;
                _clickArea.Deselected -= _onHoverExited;
            }
        }

        private void _onClicked()
        {
            Clicked?.Invoke();
        }

        private void _onHoverEntered()
        {
            if (_currentUnit == null)
            {
                return;
            }

            HoverEntered?.Invoke(this);
        }

        private void _onHoverExited()
        {
            HoverExited?.Invoke(this);
        }

        public void PlayCardFx(ECardType cardType)
        {
            CardFxBinding binding = _resolveCardBinding(cardType);
            ECardImpactType impactType = binding != null ? binding.ImpactType : _defaultCardImpactType;
            if (_cardImpacts.TryGetValue(impactType, out ICardImpact impact))
            {
                _fxRunner?.Play(impact, binding?.Settings ?? _defaultCardFxSettings);
            }
        }

        public void PlayReactionFx(EEffectType effectType)
        {
            EffectReactionBinding binding = _resolveReactionBinding(effectType);
            EUnitImpactType impactType = binding != null ? binding.ImpactType : _defaultUnitImpactType;
            if (_unitImpacts.TryGetValue(impactType, out IUnitImpact impact))
            {
                _fxRunner?.Play(impact, binding?.Settings ?? _defaultReactionFxSettings);
            }
        }

        private CardFxBinding _resolveCardBinding(ECardType cardType)
        {
            foreach (CardFxBinding binding in _cardFxBindings)
            {
                if (binding == null)
                {
                    continue;
                }

                if (cardType.HasFlag(binding.Matches))
                {
                    return binding;
                }
            }

            return null;
        }

        private EffectReactionBinding _resolveReactionBinding(EEffectType effectType)
        {
            foreach (EffectReactionBinding binding in _effectReactionBindings)
            {
                if (binding == null)
                {
                    continue;
                }

                if (binding.Matches == effectType)
                {
                    return binding;
                }
            }

            return null;
        }

        private void _initializeImpactDictionaries()
        {
            _cardImpacts.Clear();
            _unitImpacts.Clear();

            _cardImpacts[ECardImpactType.ShaderPulse] = new ShaderPulseFx(this);
            _cardImpacts[ECardImpactType.SpriteSequence] = new SpriteSequenceFx(this);

            _unitImpacts[EUnitImpactType.ShaderPulse] = new ShaderPulseFx(this);
            _unitImpacts[EUnitImpactType.SpriteSequence] = new SpriteSequenceFx(this);
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

            LocalizationService localization = LocalizationService.TryGet();
            string companionInfo = localization != null
                ? localization.BuildCompanionInfo(isTemporary, turnsLeft, Mathf.Max(0, unit.CompanionCardsPerTurn))
                : $"{(isTemporary ? $"Temporary: {turnsLeft} turn(s)" : "Lifetime: until death")} | Cards/turn: {Mathf.Max(0, unit.CompanionCardsPerTurn)}";
            _companionInfo.SetText(companionInfo);
            _companionInfo.gameObject.SetActive(true);
        }
        
        protected override void OnDestroy()
        {
            HighlightController?.Dispose();
            HighlightController = null;
            _fxRunner?.Stop();
            _fxRunner = null;
            _cardImpacts.Clear();
            _unitImpacts.Clear();
            
            base.OnDestroy();
        }

        public bool TryGetImpactTargets(out RectTransform fxRoot, out UnityEngine.UI.Image overlayImage)
        {
            fxRoot = _fxRoot != null ? _fxRoot : transform as RectTransform;
            overlayImage = Render != null ? Render.Image : null;
            return fxRoot != null && overlayImage != null;
        }

        public void SetImpactScale(float scale)
        {
            RectTransform target = _fxRoot != null ? _fxRoot : transform as RectTransform;
            if (target == null)
            {
                return;
            }

            target.localScale = Vector3.one * Mathf.Max(0.01f, scale);
        }

        public void ResetImpactVisualState()
        {
            if (_fxRoot != null || transform is RectTransform)
            {
                RectTransform target = _fxRoot != null ? _fxRoot : transform as RectTransform;
                target.localScale = Vector3.one;
            }

            if (Render?.Image == null)
            {
                return;
            }

            Render.Image.color = _defaultRenderColor;
            _applyRenderMirror();
        }

        public Color GetDefaultRenderColor()
        {
            return _defaultRenderColor;
        }

        private void _cacheRenderDefaults()
        {
            if (Render?.Image == null)
            {
                return;
            }

            _defaultRenderColor = Render.Image.color;
        }

        private void _applyRenderMirror()
        {
            if (Render?.Image == null)
            {
                return;
            }

            RectTransform rect = Render.Image.rectTransform;
            Vector3 scale = rect.localScale;
            float x = Mathf.Abs(scale.x) > 0.001f ? Mathf.Abs(scale.x) : 1f;
            scale.x = _isRightSide ? -x : x;
            rect.localScale = scale;
        }

        [Serializable]
        private sealed class CardFxBinding
        {
            [SerializeField] private ECardType _matches;
            [SerializeField] private ECardImpactType _impactType = ECardImpactType.ShaderPulse;
            [SerializeField] private UnitFxSettings _settings = new UnitFxSettings();

            public ECardType Matches => _matches;
            public ECardImpactType ImpactType => _impactType;
            public UnitFxSettings Settings => _settings;

            public CardFxBinding(ECardType matches, ECardImpactType impactType, UnitFxSettings settings)
            {
                _matches = matches;
                _impactType = impactType;
                _settings = settings;
            }
        }

        [Serializable]
        private sealed class EffectReactionBinding
        {
            [SerializeField] private EEffectType _matches;
            [SerializeField] private EUnitImpactType _impactType = EUnitImpactType.SpriteSequence;
            [SerializeField] private UnitFxSettings _settings = new UnitFxSettings();

            public EEffectType Matches => _matches;
            public EUnitImpactType ImpactType => _impactType;
            public UnitFxSettings Settings => _settings;

            public EffectReactionBinding(EEffectType matches, EUnitImpactType impactType, UnitFxSettings settings)
            {
                _matches = matches;
                _impactType = impactType;
                _settings = settings;
            }
        }

        private enum ECardImpactType
        {
            ShaderPulse = 0,
            SpriteSequence = 1
        }

        private enum EUnitImpactType
        {
            ShaderPulse = 0,
            SpriteSequence = 1
        }
    }
}