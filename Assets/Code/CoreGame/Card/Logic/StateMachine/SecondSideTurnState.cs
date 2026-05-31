using System.Linq;
using CoreGame.Card.Data;
using CoreGame.Card.Logic.AI;
using Cysharp.Threading.Tasks;

namespace CoreGame.Card.Logic.StateMachine
{
    public class SecondSideTurnState : IBattleState, IAcceptPlayerInput
    {
        private readonly BattleStateMachine _machine;
        public EBattlePhase Phase => EBattlePhase.SecondSideTurn;
        public bool IsInitialized { get; set; }

        
        public SecondSideTurnState(BattleStateMachine machine)
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

            if (_machine.Model.SideB.Hero.AI != null)
            {
                _processAI();
            }
            
            return UniTask.CompletedTask;
        }

        public UniTask Exit()
        {
            return UniTask.CompletedTask;
        }

        public bool TryPlayCard(int cardIndex, string targetId)
        {
            BattleUnit actor = _machine.Model.SideB.Hero;

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

            if (unit == null)
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

            unit.Energy -= unit.MoveLineCost;
            unit.Line = unit.Line == EBattleLine.Frontline
                ? EBattleLine.Backline
                : EBattleLine.Frontline;

            return true;
        }

        public void EndTurn()
        {
            _machine.SwitchState(typeof(TurnResolutionState));
        }
        
        private void _processAI()
        {
            BattleUnit ai = _machine.Model.SideB.Hero;

            while (true)
            {
                AIAction action = ai.AI.SelectAction(ai, _machine.Model);
                if (action?.Card == null)
                {
                    break;
                }

                if (ai.Energy < action.Card.GetEnergyCost(ai.Stats))
                {
                    break;
                }

                BattleUnit target = _machine.FindUnit(action.TargetId);
                if (target == null)
                {
                    break;
                }

                _machine.Processor.ApplyCard(ai, action.Card, target, _machine.Model);
            
                _spendCard(ai, action.Card);
            }

            EndTurn();
        }
        
        
        private void _spendCard(BattleUnit actor, CardBattleState card)
        {
            if (card.Config.Charges > 0)
            {
                card.ChargesLeft--;
                
                if (card.ChargesLeft <= 0)
                {
                    actor.Hand.Remove(card);
                    actor.Discard.Remove(card);
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