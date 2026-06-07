using System;
using UI.Components;
using UI.Windows.Base;
using UnityEngine;

namespace UI.Windows.Game.Card.Turn
{
    public class CardStepView : UIWindowView
    {
        [field: SerializeField] public UIButton ButtonEndStep;
       
        [SerializeField] private UIText _textTime;
        [SerializeField] private UIText _textStep;

        
        public void SetTime(float time)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(Mathf.Max(0f, time));
            _textTime.SetText($"{timeSpan.Minutes:00}:{timeSpan.Seconds:00}");
        }

        public void SetStep(string step)
        {
            _textStep.SetText(step);
        }

        public void SetEndStepVisible(bool visible)
        {
            if (ButtonEndStep == null)
            {
                return;
            }

            ButtonEndStep.gameObject.SetActive(visible);
        }
    }
}