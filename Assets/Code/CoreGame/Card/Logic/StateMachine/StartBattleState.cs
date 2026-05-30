using CoreGame.Card.Data;
using Cysharp.Threading.Tasks;

namespace CoreGame.Card.Logic.StateMachine
{
    public class StartBattleState : IBattleState
    {
        public EBattlePhase Phase => EBattlePhase.StartBattle;
        public bool IsInitialized { get; set; }

        private readonly BattleStateMachine _machine;


        public StartBattleState(BattleStateMachine machine)
        {
            _machine = machine;
        }

        public UniTask Initialize()
        {
            return UniTask.CompletedTask;
        }

        public UniTask Enter()
        {
            //todo load battle view

            return UniTask.CompletedTask;
        }

        public UniTask Exit()
        {
            return UniTask.CompletedTask;
        }
    }
}