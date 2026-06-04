using System.Collections.Generic;
using CoreGame.Card.Data;
using UI.Windows.Base;
using UnityEngine;

namespace UI.Windows.Game.Card.Map
{
    public class BattleSideController : UIWindowController<BattleSideView>
    {
        public Dictionary<BattleUnit, RectTransform> _unitCells;
    }
}