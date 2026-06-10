using System;
using CoreGame.Card.Data;

namespace CoreGame.Card.Logic
{
    public static class BattleParticipantHelper
    {
        public static BattleSide GetMySide(BattleModel battle, string playerUnitId)
        {
            if (battle == null)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(playerUnitId))
            {
                if (battle.SideA?.Hero?.UnitId == playerUnitId)
                {
                    return battle.SideA;
                }

                if (battle.SideB?.Hero?.UnitId == playerUnitId)
                {
                    return battle.SideB;
                }
            }

            return battle.SideA;
        }

        public static BattleSide GetOpponentSide(BattleModel battle, BattleSide mySide)
        {
            if (battle == null || mySide == null)
            {
                return null;
            }

            if (battle.IsCoOp)
            {
                return battle.EnemySide;
            }

            return ReferenceEquals(mySide, battle.SideA) ? battle.SideB : battle.SideA;
        }

        public static bool IsMyTurn(BattleModel battle, string playerUnitId)
        {
            if (battle?.Phase == null || string.IsNullOrEmpty(playerUnitId))
            {
                return false;
            }

            bool isSideA = battle.SideA?.Hero?.UnitId == playerUnitId;
            bool isSideB = battle.SideB?.Hero?.UnitId == playerUnitId;

            return battle.Phase.Value switch
            {
                EBattlePhase.FirstSideTurn => isSideA,
                EBattlePhase.SecondSideTurn => isSideB,
                _ => false
            };
        }

        public static bool IsAllySide(BattleModel battle, BattleSide actorSide, BattleSide targetSide)
        {
            if (battle == null || actorSide == null || targetSide == null)
            {
                return false;
            }

            if (ReferenceEquals(actorSide, targetSide))
            {
                return true;
            }

            if (!battle.IsCoOp)
            {
                return false;
            }

            bool actorIsHuman = ReferenceEquals(actorSide, battle.SideA) || ReferenceEquals(actorSide, battle.SideB);
            bool targetIsHuman = ReferenceEquals(targetSide, battle.SideA) || ReferenceEquals(targetSide, battle.SideB);
            return actorIsHuman && targetIsHuman;
        }

        public static BattleSide GetUiRightSide(BattleModel battle)
        {
            if (battle == null)
            {
                return null;
            }

            return battle.IsCoOp ? battle.EnemySide : battle.SideB;
        }

        public static BattleSide GetUiLeftSide(BattleModel battle)
        {
            return battle?.SideA;
        }
    }
}
