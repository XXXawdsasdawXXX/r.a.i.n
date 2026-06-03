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

        
        public void SetTime(float time)
        {
            //todo optimaze allocation 
            _textTime.SetText(time.ToString(@"mm\:ss"));
        }

        public void SetStep(string step)
        {
            _textStep.SetText(step);
        }
    }
}