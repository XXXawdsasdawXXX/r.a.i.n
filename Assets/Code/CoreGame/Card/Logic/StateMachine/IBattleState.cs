using Core.StateMachine;
using CoreGame.Card.Data;

namespace CoreGame.Card.Logic.StateMachine
{
    public interface IBattleState : IState
    {
        EBattlePhase Phase { get; }
    }
    
    public interface IAcceptPlayerInput
    {
        bool TryPlayCard(int cardIndex, string targetId);
        bool TryMoveLine(string unitId);
        void EndTurn();
    }
}