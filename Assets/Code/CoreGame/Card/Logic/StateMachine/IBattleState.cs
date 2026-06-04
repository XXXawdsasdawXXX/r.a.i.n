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
        /// <param name="cardId"><see cref="CardConfiguration.Id"/> или <see cref="CardBattleState.InstanceId"/>.</param>
        /// <param name="targetId"><see cref="BattleUnit.UnitId"/> цели (позже — выбор из нескольких / AoE).</param>
        bool TryPlayCard(string cardId, string targetId);
        bool TryMoveLine(string unitId);
        void EndTurn();
    }
}