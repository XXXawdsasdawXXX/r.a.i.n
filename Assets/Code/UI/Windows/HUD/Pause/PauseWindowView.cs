using UI.Components;
using UI.Windows.Base;
using UnityEngine;

namespace UI.Windows.HUD.Pause
{
    public class PauseWindowView : UIWindowView
    {
        [field: SerializeField] public UISlider SliderMusic { get; private set; }
        [field: SerializeField] public UISlider SliderSFX { get; private set; }
    }
}