using UI.Components;
using UI.Windows.Base;
using UnityEngine;

namespace UI.Windows.Connection
{
    public class ConnectionWindowView : UIWindowView
    {
        [field: SerializeField] public UIText TextUserIP { get; private set; }
        [field: SerializeField] public UIButton ButtonServer { get; private set; }
        [field: SerializeField] public UIButton ButtonHost { get; private set; }
        [field: SerializeField] public UIButton ButtonClient { get; private set; }
        [field: SerializeField] public UIInputField InputFieldHostIP { get; private set; }
    }
}