using System.Collections.Generic;
using System.Linq;
using CoreGame.Card.Data;

namespace CoreGame.Card.Logic
{
    public static class BattleGridRules
    {
        public const int CELLS_PER_LINE = 3;

        public static void AssignCoOpStartPositions(BattleModel battle)
        {
            if (battle?.SideA?.Hero == null || battle.SideB?.Hero == null)
            {
                return;
            }

            battle.SideA.Hero.Line = EBattleLine.Frontline;
            battle.SideA.Hero.LineCellIndex = 0;

            battle.SideB.Hero.Line = EBattleLine.Frontline;
            battle.SideB.Hero.LineCellIndex = 2;

            if (battle.EnemySide?.Hero != null)
            {
                battle.EnemySide.Hero.Line = EBattleLine.Frontline;
                battle.EnemySide.Hero.LineCellIndex = 1;
            }
        }

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

            if (IsCellOccupied(battle, side, line, cellIndex, unit.UnitId))
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

        public static IEnumerable<BattleUnit> GetEnemyUnits(BattleModel battle, BattleSide actorSide)
        {
            if (battle == null || actorSide == null)
            {
                yield break;
            }

            if (battle.IsCoOp && ReferenceEquals(actorSide, battle.EnemySide))
            {
                foreach (BattleUnit unit in _getCoOpTeamUnits(battle))
                {
                    if (unit != null && unit.HP > 0)
                    {
                        yield return unit;
                    }
                }

                yield break;
            }

            BattleSide enemySide = GetOpponentSide(battle, actorSide);
            if (enemySide == null)
            {
                yield break;
            }

            foreach (BattleUnit unit in enemySide.GetAllUnits())
            {
                if (unit != null && unit.HP > 0)
                {
                    yield return unit;
                }
            }
        }

        public static bool IsCellOccupied(
            BattleModel battle,
            BattleSide side,
            EBattleLine line,
            int cellIndex,
            string exceptUnitId = null)
        {
            if (battle?.IsCoOp == true && _isHumanSide(battle, side))
            {
                return _isCoOpTeamCellOccupied(battle, line, cellIndex, exceptUnitId);
            }

            return _isSideCellOccupied(side, line, cellIndex, exceptUnitId);
        }

        private static bool _isHumanSide(BattleModel battle, BattleSide side)
        {
            return ReferenceEquals(side, battle.SideA) || ReferenceEquals(side, battle.SideB);
        }

        private static bool _isCoOpTeamCellOccupied(BattleModel battle, EBattleLine line, int cellIndex, string exceptUnitId)
        {
            foreach (BattleUnit unit in _getCoOpTeamUnits(battle))
            {
                if (unit == null || unit.HP <= 0 || unit.UnitId == exceptUnitId)
                {
                    continue;
                }

                if (unit.Line == line && unit.LineCellIndex == cellIndex)
                {
                    return true;
                }
            }

            return false;
        }

        private static IEnumerable<BattleUnit> _getCoOpTeamUnits(BattleModel battle)
        {
            if (battle?.SideA != null)
            {
                foreach (BattleUnit unit in battle.SideA.GetAllUnits())
                {
                    yield return unit;
                }
            }

            if (battle?.SideB != null)
            {
                foreach (BattleUnit unit in battle.SideB.GetAllUnits())
                {
                    yield return unit;
                }
            }
        }

        private static bool _isSideCellOccupied(BattleSide side, EBattleLine line, int cellIndex, string exceptUnitId)
        {
            return side.GetAllUnits()
                .Where(u => u != null && u.HP > 0)
                .Where(u => u.UnitId != exceptUnitId)
                .Any(u => u.Line == line && u.LineCellIndex == cellIndex);
        }
    }
}
