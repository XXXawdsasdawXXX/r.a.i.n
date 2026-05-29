using Sirenix.OdinInspector;
using UI.Components;
using UI.Windows.Base;
using UnityEngine;

namespace UI.Windows.HUD.GameResources
{
    public class HudResourcesView : UIWindowView
    {
        [field: SerializeField] public RectTransform RootResources { get; private set; }
        
        [field: SerializeField] public UIResourceBoxView[] ResourceBoxViews { get; private set; }

#if UNITY_EDITOR
        [Button]
        private void _findAllResourceBoxes()
        {
            ResourceBoxViews = GetComponentsInChildren<UIResourceBoxView>(true);
        }
#endif
    }
}