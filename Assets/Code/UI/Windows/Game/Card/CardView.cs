using System;
using CoreGame.Card;
using CoreGame.Card.Data;
using UI.Components;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Windows.Card.CardDeck
{
    public class CardView : UIButton
    {
        [Serializable]
        public struct Model
        {
            public string Id;
            public string Name;
            public ECardType Type;
            public string Description;
            public int EnergyPrice;
            public int CurrentCharge;
            public int MaxCharge;
        }
        
        [field: SerializeField] public Model CurrentModel { get; private set; }

        public event Action<string> UsedCard; 

        [SerializeField] private UIText _textName;
        [SerializeField] private UIText _textType;
        [SerializeField] private UIText _textEnergyPrice;
        [SerializeField] private UIText _textDescription;
        [SerializeField] private UIText _textCharge;
        [SerializeField] private Image _image;

        
        public override void SetInteractable(bool isInteractable)
        {
            _image.raycastTarget = isInteractable;
        }

        public void SetModel(Model model)
        {
            CurrentModel = model;
            
            _textName.SetText(model.Name);
            _textType.SetText(model.Type.ToString());
            _textEnergyPrice.SetText(model.EnergyPrice.ToString());
            _textDescription.SetText(model.Description);
            _textCharge.SetText($"{model.CurrentCharge}/{model.MaxCharge}");
        }

        protected override void onClick()
        {
            base.onClick();
            
            UsedCard?.Invoke(CurrentModel.Id);
        }

        public override void Disable()
        {
            base.Disable();

            UsedCard = null;
        }
    }
}