using Sirenix.OdinInspector;
using UI.Components;
using UI.Windows.Base;
using UnityEngine;

namespace UI.Windows.Game.Card.Unit
{
    public class BattleUnitView : UIWindowView
    {
        [field: SerializeField] public UIImage Render { get; private set; }
        [field: Title("Params")]
        [field: SerializeField] public UIImage HealthFill { get; private set; }
        [field: SerializeField] public UIText HealthText { get; private set; }
        [field: SerializeField] public UIBattleStateIcon Armor { get; private set; }
        [field: SerializeField] public UIBattleStateIcon Attack { get; private set; }
    }
}