using System;
using CoreGame.Card.Data;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
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
            public Sprite Icon;
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
            _image.sprite = model.Icon;
        }

        public void UpdateViewFromConfig(CardConfiguration config)
        {
            if (config == null)
            {
                return;
            }

            SetModel(new Model
            {
                Id = config.Id,
                InstanceId = config.Id,
                Name = config.Name,
                Type = config.Type,
                Description = config.Description,
                EnergyPrice = config.BaseEnergyCost,
                CurrentCharge = config.Charges,
                MaxCharge = config.Charges
            });
        }

        protected override void onClick()
        {
            base.onClick();
            
            CardClicked?.Invoke(CurrentModel.InstanceId);
            
        }

        public override void Disable()
        {
            base.Disable();

            CardClicked = null;
        }

#if UNITY_EDITOR
        [Button("Update View From Preview Config")]
        private void _updateViewFromPreviewConfig(CardConfiguration cardConfiguration)
        {
            UpdateViewFromConfig(cardConfiguration);
        }   
#endif
    }
}