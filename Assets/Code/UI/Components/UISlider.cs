using System;
using Core.GameLoop;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Components
{
    public class UISlider : UISelectable, ISubscriber
    {
        public event Action<float> Changed;

        public float Value => _slider == null ? 0 : _slider.value;
        
        [SerializeField] private Slider _slider;

        
        public void Subscribe()
        {
            _slider.onValueChanged.AddListener(value => Changed?.Invoke(value));
        }
        
        public void Unsubscribe()
        {
            _slider.onValueChanged.RemoveAllListeners();
        }

        public override void SetInteractable(bool isInteractable)
        {
            _slider.enabled = isInteractable;
        }

        public void SetValueWithoutNotify(float value)
        {
            _slider.SetValueWithoutNotify(value);
        }
        
        public void SetValue(float value)
        {
            _slider.value = value;
        }
    }
}