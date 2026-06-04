using Cysharp.Threading.Tasks;
using UI.Windows.Base;
using UI.Windows.Game.Card.Map;

namespace UI.Windows.Game.Card.Unit
{
    public class BattleUnitController : UIWindowController<BattleSideView>
    {
        public override UniTask InitializeWindow(UIWindowManager manager)
        {
            return base.InitializeWindow(manager);
        }
    }
}