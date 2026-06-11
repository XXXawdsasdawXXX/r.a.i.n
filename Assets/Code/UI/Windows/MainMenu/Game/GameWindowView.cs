using UI.Components;
using UI.Windows.Base;
using UnityEngine;
using UnityEngine.Localization;

namespace UI.Windows.MainMenu.Game
{
    public class GameWindowView : UIWindowView
    {
        [field: SerializeField] public GameObject ObjectLocker { get; private set; }
        [field: SerializeField] public UIText TextUserIP { get; private set; }
        [field: SerializeField] public UIButton ButtonContinue { get; private set; }
        [field: SerializeField] public UIButton ButtonJoin { get; private set; }
        [field: SerializeField] public UIButton ButtonDelete { get; private set; }
        [field: SerializeField] public UIRadioGroup<UIText> WorldsRadioGroup { get; private set; }
        [field: SerializeField] public UIScroll Scroll { get; private set; }

        [SerializeField] private LocalizedString _ipMessage = new LocalizedString();

        public LocalizedString IpMessage => _ipMessage;
    }
}
