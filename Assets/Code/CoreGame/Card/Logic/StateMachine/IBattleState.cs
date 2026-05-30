using Core.StateMachine;
using CoreGame.Card.Data;

namespace CoreGame.Card.Logic.StateMachine
{
    public interface IBattleState : IState
    {
        EBattlePhase Phase { get; }
    }
}