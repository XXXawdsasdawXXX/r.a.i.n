using System.Linq;
using CoreGame.Card.Data;
using Cysharp.Threading.Tasks;

namespace CoreGame.Card.Logic.StateMachine
{
    public class FirstSideTurnState : IBattleState, IAcceptPlayerInput
    {
        private readonly BattleStateMachine _machine;
        public EBattlePhase Phase => EBattlePhase.SecondSideTurn;
        public bool IsInitialized { get; set; }

        
        public FirstSideTurnState(BattleStateMachine machine)
        {
            _machine = machine;
        }
        
        public UniTask Initialize()
        {
            return UniTask.CompletedTask;
        }

        public UniTask Enter()
        {            
            _machine.Model.TurnTimeRemaining = BattleModel.MAX_TURN_TIME;

            return UniTask.CompletedTask;
        }

        public UniTask Exit()
        {
            return UniTask.CompletedTask;
        }

        public bool TryPlayCard(int cardIndex, string targetId)
        {
            BattleUnit actor = _machine.Model.SideA.Hero;

            if (cardIndex < 0 || cardIndex >= actor.Hand.Count)
            {
                return false;
            }

            CardBattleState card = actor.Hand[cardIndex];
            int cost = card.GetEnergyCost(actor.Stats);

            if (actor.Energy < cost)
            {
                return false;
            }

            if (actor.Statuses.Any(s => s.Type == EStatusType.Stun))
            {
                return false;
            }

            if (actor.IsInArmorStance && card.Config.Type.HasFlag(ECardType.Attack))
            {
                return false;
            }

            BattleUnit target = _machine.FindUnit(targetId);
            _machine.Processor.ApplyCard(actor, card, target, _machine.Model);
            _spendCard(actor, card);

            return true;
        }

        public bool TryMoveLine(string unitId)
        {
            BattleUnit unit = _machine.FindUnit(unitId);
            if (unit == null) return false;
            if (unit.Energy < unit.MoveLineCost) return false;
            if (unit.Statuses.Any(s => s.Type == EStatusType.Stun)) return false;

            unit.Energy -= unit.MoveLineCost;
            unit.Line = unit.Line == EBattleLine.Frontline
                ? EBattleLine.Backline
                : EBattleLine.Frontline;

            return true;
        }

        public void EndTurn()
        {
            _machine.SwitchState(typeof(SecondSideTurnState));
        }
        
        private void _spendCard(BattleUnit actor, CardBattleState card)
        {
            if (card.Config.Charges > 0)
            {
                card.ChargesLeft--;
                actor.Hand.Remove(card);
                
                if (card.ChargesLeft > 0)
                {
                    actor.Discard.Add(card);
                }
            }
            else
            {
                actor.Hand.Remove(card);
                actor.Discard.Add(card);
            }
        }
    }
}