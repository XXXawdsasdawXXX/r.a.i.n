using UI.Components;
using UI.Windows.Base;
using UnityEngine;

namespace UI.Windows.HUD
{
    public class HUDWindowView : UIWindowView
    {
        [field: SerializeField] public UIImage ImageGameTime { get; private set; }
    }
}