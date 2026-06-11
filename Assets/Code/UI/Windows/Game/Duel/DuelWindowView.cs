using UI.Components;
using UI.Windows.Base;
using UnityEngine;

namespace UI.Windows.Game.Duel
{
    public class DuelWindowView : UIWindowView
    {
        [field: SerializeField] public UIText TextTitle { get; private set; }
        [field: SerializeField] public UIText TextBody { get; private set; }
        [field: SerializeField] public UIText TextGold { get; private set; }
        [field: SerializeField] public UIInputField InputStake { get; private set; }
        [field: SerializeField] public UIButton ButtonPrimary { get; private set; }
        [field: SerializeField] public UIButton ButtonSecondary { get; private set; }
    }
}
