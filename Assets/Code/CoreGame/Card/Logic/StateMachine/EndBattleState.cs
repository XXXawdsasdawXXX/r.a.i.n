using Core.StateMachine;
using CoreGame.Card.Data;
using Cysharp.Threading.Tasks;

namespace CoreGame.Card.Logic.StateMachine
{
    public class EndBattleState : IBattleState
    {
        public EBattlePhase Phase => EBattlePhase.Finished;

        private readonly BattleStateMachine _machine;
        public bool IsInitialized { get; set; }

        public EndBattleState(BattleStateMachine machine)
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