using CoreGame.Card.Data;
using Cysharp.Threading.Tasks;

namespace CoreGame.Card.Logic.StateMachine
{
    public class SecondSideTurnState : IBattleState
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
            throw new System.NotImplementedException();
        }

        public UniTask Enter()
        {
            throw new System.NotImplementedException();
        }

        public UniTask Exit()
        {
            throw new System.NotImplementedException();
        }

    }
}