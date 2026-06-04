using System;
using UI.Components;
using UnityEngine;

namespace UI.Windows.Game.Card.Unit
{
    [Serializable]
    public class UIBattleStateIcon
    {
        [field: SerializeField] public UIImage Icon { get; private set; }
        [field: SerializeField] public UIText Value { get; private set; }
    }
}