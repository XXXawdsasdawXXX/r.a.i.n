using System;
using System.Collections.Generic;
using System.Linq;
using Core.Data;
using Core.GameLoop;
using Core.Save;
using Core.ServiceLocator;
using Core.StateMachine;
using CoreGame.Card.Data;
using Cysharp.Threading.Tasks;

namespace CoreGame.Card.Logic.StateMachine
{
    public sealed class BattleStateMachine : StateMachine<IBattleState>, IInitializeListener, ISubscriber
    {
        public bool IsInitialized { get; set; }
        
        public IBattleState CurrentState => currentState;
        public BattleModel Model { get; private set; }
        
        public BattleProcessor Processor { get; } = new();

        private CardLibrary _cardLibrary;
        
        
        public BattleStateMachine()
        {
            states = new Dictionary<Type, IBattleState>()
            {
                { typeof(StartBattleState), new StartBattleState(this) },
                { typeof(StartTurnState), new StartTurnState(this) },
                { typeof(StartTurnState), new StartTurnState(this) },
                { typeof(FirstSideTurnState), new FirstSideTurnState(this) },
                { typeof(SecondSideTurnState), new SecondSideTurnState(this) },
                { typeof(TurnResolutionState), new TurnResolutionState(this) },
                { typeof(EndBattleState), new EndBattleState(this) },
            };
        }

        public UniTask Initialize()
        {
            _cardLibrary = Container.Instance.GetConfig<CardLibrary>();
            
            return UniTask.CompletedTask;
        }

        public void Subscribe()
        {
        }

        public void Unsubscribe()
        {
        }

        protected override UniTask setState(Type type)
        {
            Model.Phase.Value = states[type].Phase;
            
            return base.setState(type);
        }

        public void StartBattle(HeroModel attacker, HeroModel defender, EBattleMode mode = EBattleMode.PvE)
        {
            Model = new BattleModel
            {
                BattleId = Guid.NewGuid().ToString(),
                Mode = mode,
                Phase = new ReactiveProperty<EBattlePhase>(EBattlePhase.WaitingBattle),
                TurnNumber = 0,
                TurnTimeRemaining = 0,
                SideA = _buildSide(attacker),
                SideB = _buildSide(defender),
            };

            SwitchState(typeof(StartBattleState));
        }
        
        public void OnTimerExpired()
        {
            //todo как будто это должно быть внутри стейта
            (currentState as IAcceptPlayerInput)?.EndTurn();
        }

        public BattleUnit FindUnit(string unitId)
        {
            if (string.IsNullOrEmpty(unitId))
            {
                return null;
            }

            return Model.SideA.GetAllUnits()
                .Concat(Model.SideB.GetAllUnits())
                .FirstOrDefault(u => u.UnitId == unitId);
        }
        
        private BattleSide _buildSide(HeroModel hero)
        {
            return new BattleSide(BattleUnit.FromHero(hero, _cardLibrary.AllCards));
        }
    }
}