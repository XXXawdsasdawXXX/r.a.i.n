using UI.Components;
using UI.Windows.Base;
using UnityEngine;

namespace UI.Windows.Card.CardDeck.CardStep
{
    public class CardStepView : UIWindowView
    {
        [field: SerializeField] public UIButton ButtonEndStep;
       
        [SerializeField] private UIText _textTime;
        [SerializeField] private UIText _textStep;

        public void SetTime(string time)
        {
            _textTime.SetText(time);
        }

        public void SetStep(string step)
        {
            _textStep.SetText(step);
        }
    }
}