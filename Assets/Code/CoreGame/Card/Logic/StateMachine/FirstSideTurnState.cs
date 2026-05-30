using CoreGame.Card.Data;
using Cysharp.Threading.Tasks;

namespace CoreGame.Card.Logic.StateMachine
{
    public class FirstSideTurnState : IBattleState
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
            return UniTask.CompletedTask;
        }

        public UniTask Exit()
        {
            return UniTask.CompletedTask;
        }
    }
}