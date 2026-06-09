using CoreGame.Card.Data;

namespace CoreGame.Card.Logic
{
    public static class BattleParticipantResolver
    {
        public static BattleSide GetSideForPlayer(BattleModel battle, string playerId)
        {
            if (battle == null || string.IsNullOrEmpty(playerId))
            {
                return null;
            }

            if (battle.SideA?.Hero?.UnitId == playerId)
            {
                return battle.SideA;
            }

            if (battle.HasAllySide && battle.AllySide?.Hero?.UnitId == playerId)
            {
                return battle.AllySide;
            }

            if (battle.SideB?.Hero?.UnitId == playerId)
            {
                return battle.SideB;
            }

            return null;
        }

        public static bool IsPlayerSideA(BattleModel battle, string playerId)
        {
            return battle?.SideA?.Hero?.UnitId == playerId;
        }

        public static bool IsPlayerAlly(BattleModel battle, string playerId)
        {
            return battle.HasAllySide && battle.AllySide?.Hero?.UnitId == playerId;
        }

        public static bool IsPlayerSideB(BattleModel battle, string playerId)
        {
            return battle?.SideB?.Hero?.UnitId == playerId;
        }

        public static bool IsMyTurn(BattleModel battle, string playerId)
        {
            if (battle?.Phase == null || string.IsNullOrEmpty(playerId))
            {
                return false;
            }

            return battle.Phase.Value switch
            {
                EBattlePhase.FirstSideTurn => IsPlayerSideA(battle, playerId),
                EBattlePhase.AllySideTurn => IsPlayerAlly(battle, playerId),
                EBattlePhase.SecondSideTurn => IsPlayerSideB(battle, playerId) && battle.SideB?.Hero?.AI == null,
                _ => false
            };
        }

        public static BattleSide GetActiveSide(BattleModel battle)
        {
            if (battle?.Phase == null)
            {
                return null;
            }

            return battle.Phase.Value switch
            {
                EBattlePhase.FirstSideTurn => battle.SideA,
                EBattlePhase.AllySideTurn => battle.AllySide,
                EBattlePhase.SecondSideTurn => battle.SideB,
                _ => null
            };
        }

        public static BattleSide GetEnemySideFor(BattleSide side, BattleModel battle)
        {
            if (battle == null || side == null)
            {
                return null;
            }

            if (battle.Mode == EBattleMode.CoOpPvE)
            {
                if (ReferenceEquals(side, battle.SideB))
                {
                    return battle.SideA;
                }

                return battle.SideB;
            }

            return ReferenceEquals(side, battle.SideA) ? battle.SideB : battle.SideA;
        }

        public static bool IsHumanControlledSide(BattleSide side, BattleModel battle)
        {
            if (battle == null || side?.Hero == null)
            {
                return false;
            }

            return side.Hero.AI == null;
        }
    }
}
