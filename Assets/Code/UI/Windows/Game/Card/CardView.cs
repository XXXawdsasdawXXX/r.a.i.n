using System;
using CoreGame.Card.Data;
using Essential;
using UI.Components;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Windows.Game.Card
{
    public class CardView : UIButton
    {
        [Serializable]
        public struct Model
        {
            /// <summary><see cref="CardConfiguration.Id"/> — тип карты из библиотеки.</summary>
            public string Id;
            /// <summary>Уникальный экземпляр в бою; для <see cref="CardPlayRules.FindCardInHand"/> при дубликатах Id.</summary>
            public string InstanceId;
            public string Name;
            public ECardType Type;
            public string Description;
            public int EnergyPrice;
            public int CurrentCharge;
            public int MaxCharge;
        }
        
        [field: SerializeField] public Model CurrentModel { get; private set; }

        /// <summary><see cref="Model.Id"/> (<see cref="CardConfiguration.Id"/>).</summary>
        public event Action<string> CardClicked;

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
            
            CardClicked?.Invoke(CurrentModel.InstanceId);
            
            Log.Info($"click to card with id {CurrentModel.Id}");
        }

        public override void Disable()
        {
            base.Disable();

            CardClicked = null;
        }
    }
}