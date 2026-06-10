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

            if (battle.SideA?.GetAllUnits().Contains(unit) == true)
            {
                return battle.SideA;
            }

            if (battle.SideB?.GetAllUnits().Contains(unit) == true)
            {
                return battle.SideB;
            }

            if (battle.EnemySide?.GetAllUnits().Contains(unit) == true)
            {
                return battle.EnemySide;
            }

            return null;
        }

        public static BattleSide GetOpponentSide(BattleModel battle, BattleSide side)
        {
            if (battle == null || side == null)
            {
                return null;
            }

            if (battle.IsCoOp)
            {
                if (ReferenceEquals(side, battle.SideA) || ReferenceEquals(side, battle.SideB))
                {
                    return battle.EnemySide;
                }

                if (ReferenceEquals(side, battle.EnemySide))
                {
                    return battle.SideA;
                }
            }

            if (ReferenceEquals(side, battle.SideA))
            {
                return battle.SideB;
            }

            if (ReferenceEquals(side, battle.SideB))
            {
                return battle.SideA;
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
