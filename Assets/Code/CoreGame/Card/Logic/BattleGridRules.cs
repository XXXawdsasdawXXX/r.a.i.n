using System.Linq;
using CoreGame.Card.Data;

namespace CoreGame.Card.Logic
{
    public static class BattleGridRules
    {
        public const int CELLS_PER_LINE = 3;

        public static bool TryMoveUnitToCell(BattleModel battle, BattleUnit unit, EBattleLine line, int cellIndex)
        {
            if (battle == null || unit == null)
            {
                return false;
            }

            if (cellIndex < 0 || cellIndex >= CELLS_PER_LINE)
            {
                return false;
            }

            BattleSide side = GetOwnerSide(battle, unit);
            if (side == null)
            {
                return false;
            }

            if (IsCellOccupied(side, line, cellIndex, unit.UnitId))
            {
                return false;
            }

            unit.Line = line;
            unit.LineCellIndex = cellIndex;
            return true;
        }

        public static BattleSide GetOwnerSide(BattleModel battle, BattleUnit unit)
        {
            if (battle == null || unit == null)
            {
                return null;
            }

            if (battle.SideA != null && battle.SideA.GetAllUnits().Contains(unit))
            {
                return battle.SideA;
            }

            if (battle.HasAllySide && battle.AllySide != null && battle.AllySide.GetAllUnits().Contains(unit))
            {
                return battle.AllySide;
            }

            if (battle.SideB != null && battle.SideB.GetAllUnits().Contains(unit))
            {
                return battle.SideB;
            }

            return null;
        }

        private static bool IsCellOccupied(BattleSide side, EBattleLine line, int cellIndex, string exceptUnitId)
        {
            return side.GetAllUnits()
                .Where(u => u != null && u.HP > 0)
                .Where(u => u.UnitId != exceptUnitId)
                .Any(u => u.Line == line && u.LineCellIndex == cellIndex);
        }
    }
}
