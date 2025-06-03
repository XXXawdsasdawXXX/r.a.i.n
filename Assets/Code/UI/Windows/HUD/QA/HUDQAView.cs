using UI.Components;
using UI.Windows.Base;
using UnityEngine;

namespace UI.Windows.HUD.QA
{
    public class HUDQAView : UIWindowView
    {
        [field: Header("Base")]
        [field: SerializeField] public UIButton ButtonOpen { get; private set; }
        [field: SerializeField] public UIButton ButtonClose { get; private set; }
        
        [field: Space, Header("HP")]
        [field: SerializeField] public UIButton ButtonAddHP { get; private set; }
        [field: SerializeField] public UIButton ButtonRemoveHP { get; private set; }
        [field: SerializeField] public UIInputField InputFieldHP { get; private set; }
        
        [field: Space, Header("Game resource")]
        [field: SerializeField] public UIButton ButtonAddResource { get; private set; }
        [field: SerializeField] public UIInputField InputFieldResource { get; private set; }
        [field: SerializeField] public UIDropDown DropDownResourceType { get; private set; }
    }
}