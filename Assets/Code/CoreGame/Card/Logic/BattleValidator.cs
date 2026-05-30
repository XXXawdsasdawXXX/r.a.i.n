using System.Linq;
using CoreGame.Card.Data;

namespace CoreGame.Card.Logic
{
    public class BattleValidator
    {
        public bool CanPlayCard(BattleModel battle, string unitId, int cardIndex, string targetId)
        {
            if (battle.Phase != EBattlePhase.SecondSideTurn)
            {
                return false;
            }

            BattleUnit actor = _findUnit(unitId, battle);
        
            if (actor == null)
            {
                return false;
            }

            // стан - нельзя играть карты
            if (actor.Statuses.Any(s => s.Type == EStatusType.Stun))
            {
                return false;
            }

            // оборонительная стойка - нельзя атаковать
            if (actor.IsInArmorStance && cardIndex >= 0 && cardIndex < actor.Hand.Count)
            {
                CardBattleState card = actor.Hand[cardIndex];
                if (card.Config.Type.HasFlag(ECardType.Attack))
                {
                    return false;
                }
            }

            if (cardIndex < 0 || cardIndex >= actor.Hand.Count)
            {
                return false;
            }

            CardBattleState selectedCard = actor.Hand[cardIndex];
            int cost = selectedCard.GetEnergyCost(actor.Stats);

            if (actor.Energy < cost)
            {
                return false;
            }

            return true;
        }

        public bool CanMoveLine(BattleModel battle, BattleUnit unit)
        {
            if (battle.Phase != EBattlePhase.SecondSideTurn)
            {
                return false;
            }

            if (unit.Energy < unit.MoveLineCost)
            {
                return false;
            }

            if (unit.Statuses.Any(s => s.Type == EStatusType.Stun))
            {
                return false;
            }

            return true;
        }

        private BattleUnit _findUnit(string unitId, BattleModel battle)
        {
            return battle.SideA.GetAllUnits()
                .Concat(battle.SideB.GetAllUnits())
                .FirstOrDefault(u => u.UnitId == unitId);
        }
    }
}