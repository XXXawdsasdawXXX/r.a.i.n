using System;
using CoreGame.Card.Data;
using UI.Components;
using UnityEngine;

namespace UI.Windows.Game.Card.Map
{
    public class BattleGridCellView : UIButton
    {
        public event Action<BattleGridCellView> Clicked;

        [field: SerializeField] public UIImage Render { get; private set; }
        [field: SerializeField] public EBattleLine Line { get; private set; }
        [field: SerializeField] public int CellIndex { get; private set; }
        [field: SerializeField] public EBattleSideUi Side { get; private set; }

        [SerializeField] private UIHighlightMaterialController.EType _highlightType = UIHighlightMaterialController.EType.Outline;

        public Material HighlightMaterialTemplate => BattleHighlightStyle.ResolveHighlightMaterial(Render?.Image?.material);
        public Material OccupiedHighlightMaterialTemplate => BattleHighlightStyle.ResolveOccupiedHighlightMaterial(Render?.Image?.material);

        public bool HasOccupiedHighlightBinding => OccupiedHighlightMaterialTemplate != null;
        public bool IsOccupied => _occupied;

     
        private UIHighlightMaterialController _uiHighlightController;
        private bool _occupied;

        private void Awake()
        {
            _uiHighlightController = new UIHighlightMaterialController(Render.Image, _highlightType);
        }

        public void SetHighlighted(bool value)
        {
            if (_occupied)
            {
                return;
            }
            
            if (!value)
            {
                _uiHighlightController?.Reset();
                return;
            }

            _uiHighlightController?.Apply(HighlightMaterialTemplate);
        }
        
        public void SetOccupied(bool value)
        {
            _occupied = value;

            if (_occupied)
            {
                if (OccupiedHighlightMaterialTemplate != null)
                {
                    _uiHighlightController?.Apply(OccupiedHighlightMaterialTemplate);
                }

                return;
            }

            _uiHighlightController?.Reset();
        }

        public void SetHighlightColor(Color color)
        {
            _uiHighlightController?.SetColor(color);
        }

        protected override void onClick()
        {
            base.onClick();
            Clicked?.Invoke(this);
        }

        protected override void OnDestroy()
        {
            _uiHighlightController?.Dispose();
            _uiHighlightController = null;
            base.OnDestroy();
        }
    }

    public enum EBattleSideUi
    {
        Left = 0,
        Right = 1
    }
}
