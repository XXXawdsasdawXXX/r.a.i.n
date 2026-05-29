using UI.Components;
using UI.Windows.Base;
using UnityEngine;

namespace UI.Windows.Card.CardDeck.CardStepEnergy
{
    public class CardStepEnergyView : UIWindowView
    {
        [SerializeField] private UIImage _imageFill;
        [SerializeField] private UIText _textValue;

        
        public void SetValue(int current, int max)
        {
            _imageFill.SetFillAmount(current / (float)max);
            _textValue.SetText($"{current}/{max}");
        }
    }
}