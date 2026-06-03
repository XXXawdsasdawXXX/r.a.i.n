using System.Linq;
using Core.StateMachine;
using CoreGame.Card.Data;
using CoreGame.Card.Logic.AI;
using Cysharp.Threading.Tasks;

namespace CoreGame.Card.Logic.StateMachine
{
    public class TurnResolutionState : IBattleState
    {
        public EBattlePhase Phase => EBattlePhase.Resolution;
        public bool IsInitialized { get; set; }

        private readonly BattleStateMachine _machine;

        
        public TurnResolutionState(BattleStateMachine machine)
        {
            _machine = machine;
        }
        
        public UniTask Initialize()
        {
            return  UniTask.CompletedTask;
        }

        public UniTask Enter()
        {
            _processCompanions(_machine.Model.SideA, _machine.Model.SideB);
            _processCompanions(_machine.Model.SideB, _machine.Model.SideA);
            
            _processStatuses(_machine.Model.SideA);
            _processStatuses(_machine.Model.SideB);

            if (_checkBattleEnd())
            {
                _machine.SwitchState(typeof(EndBattleState));
                
                return UniTask.CompletedTask;
            }
            
            _machine.SwitchState(typeof(StartTurnState));
            
            return UniTask.CompletedTask;
        }

        public UniTask Exit()
        {
            return UniTask.CompletedTask;
        }

        private void _processCompanions(BattleSide side, BattleSide targetSide)
        {
            foreach (BattleUnit companion in side.Companions)
            {
                if (companion.HP <= 0 || companion.AI == null)
                {
                    continue;
                }

                AIAction action = companion.AI.SelectAction(companion, _machine.Model);

                if (action == null)
                {
                    continue;
                }

                BattleUnit target = targetSide.GetUnit(action.TargetId);

                if (target != null)
                {
                    _machine.Processor.ApplyCard(companion, action.Card, target, _machine.Model);
                }
            }
        }
        
        private void _processStatuses(BattleSide side)
        {
            foreach (BattleUnit unit in side.GetAllUnits())
            {
                _machine.Processor.TickStatuses(unit, _machine.Model);
            }

            //todo тоже вот нужно какой то фитбек во вьюху дать
            side.Companions.RemoveAll(c => c.HP <= 0);
        }
        
        private bool _checkBattleEnd()
        {
            bool activeDead  = _machine.Model.SideA.Hero.HP <= 0;
            bool waitingDead = _machine.Model.SideB.Hero.HP <= 0;

            if (!activeDead && !waitingDead)
            {
                return false;
            }

            _machine.SwitchState(typeof(EndBattleState));// todo убираем в место где вызываем метод
            
            return true;
        }
    }
}