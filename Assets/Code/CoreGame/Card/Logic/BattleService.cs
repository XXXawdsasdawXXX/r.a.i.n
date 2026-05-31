using System;
using Core.GameLoop;
using Core.Save;
using Core.ServiceLocator;
using CoreGame.Card.Data;
using CoreGame.Card.Logic.AI;
using CoreGame.Card.Logic.StateMachine;
using Cysharp.Threading.Tasks;

namespace CoreGame.Card.Logic
{
    public class BattleService : IService
    {
        public event Action<BattleModel> BattleStarted;
        public event Action<BattleModel> TurnStarted;
        public event Action<BattleModel> BattleFinished;

        private readonly BattleStateMachine _machine = new();

        public BattleService()
        {
            /*_machine.BattleStarted  += m => BattleStarted?.Invoke(m);
            _machine.TurnStarted    += m => TurnStarted?.Invoke(m);
            _machine.BattleFinished += m => BattleFinished?.Invoke(m);*/
            
            //todo подписка на Model.Phase
        }

        public void StartBattle(HeroModel attacker, HeroModel defender, EBattleMode mode = EBattleMode.PvE)
        {
            _machine.StartBattle(attacker, defender, mode);
        }

        public bool TryPlayCard(int cardIndex, string targetId)
        {
            return (_machine.CurrentState as IAcceptPlayerInput)?.TryPlayCard(cardIndex, targetId) ?? false;
        }

        public bool TryMoveLine(string unitId)
        {
            return (_machine.CurrentState as IAcceptPlayerInput)?.TryMoveLine(unitId) ?? false;
        }

        public void EndTurn()
        {
            (_machine.CurrentState as IAcceptPlayerInput)?.EndTurn();
        }

        public void OnTimerExpired()
        {
            _machine.OnTimerExpired();
        }

        public BattleUnit FindUnit(string unitId)
        {
            return _machine.FindUnit(unitId);
        }
    }
}