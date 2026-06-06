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
using Essential;
using UnityEngine;

namespace CoreGame.Card.Logic.StateMachine
{
    [Serializable]
    public sealed class BattleStateMachine : StateMachine<IBattleState>, IInitializeListener, ISubscriber
    {
        public bool IsInitialized { get; set; }
        public IBattleState CurrentState => currentState;
        [field: SerializeField] public BattleModel Model { get; private set; } = new();
        public BattleProcessor Processor { get; private set; }

        private CardLibrary _cardLibrary;
        
        
        public BattleStateMachine()
        {
        }

        public UniTask Initialize()
        {
            _cardLibrary = Container.Instance.GetSO<CardLibrary>();
            Processor = new BattleProcessor(_cardLibrary.AllCards);
            states = new Dictionary<Type, IBattleState>()
            {
                { typeof(StartBattleState), new StartBattleState(this) },
                { typeof(StartTurnState), new StartTurnState(this, _cardLibrary) },
                { typeof(FirstSideTurnState), new FirstSideTurnState(this) },
                { typeof(SecondSideTurnState), new SecondSideTurnState(this) },
                { typeof(TurnResolutionState), new TurnResolutionState(this) },
                { typeof(EndBattleState), new EndBattleState(this) },
            };
            
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
            
            Log.Info(this, $"Set state {Model.Phase.Value}", Color.cyan);
            
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
                TurnTimeRemaining = new ReactiveProperty<float>(0),
                SideA = _buildSide(attacker),
                SideB = _buildSide(defender),
            };

            SwitchState(typeof(StartBattleState));
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