using UI.Components;
using UI.Windows.Base;
using UnityEngine;
using UnityEngine.Localization;

namespace UI.Windows.MainMenu.Connection
{
    public class ConnectionWindowView : UIWindowView
    {
        [field: SerializeField] public UIText TextUserIP { get; private set; }
        [field: SerializeField] public UIButton ButtonServer { get; private set; }
        [field: SerializeField] public UIButton ButtonHost { get; private set; }
        [field: SerializeField] public UIButton ButtonClient { get; private set; }
        [field: SerializeField] public UIInputField InputFieldHostIP { get; private set; }

        [SerializeField] private LocalizedString _yourIpMessage = new LocalizedString();

        public LocalizedString YourIpMessage => _yourIpMessage;
    }
}
