using System;
using System.Collections.Generic;
using System.Linq;
using Core.Data;
using Core.GameLoop;
using Core.Save;
using Core.ServiceLocator;
using Core.StateMachine;
using CoreGame.Card.Data;
using CoreGame.Card.Logic;
using CoreGame.Card.Logic.AI;
using Cysharp.Threading.Tasks;
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
        public event Action<BattleCardPlayedEvent> CardPlayedFromStateMachine;

        private CardLibrary _cardLibrary;
        private IDeckRepository _deckRepository;
        
        
        public BattleStateMachine()
        {
        }

        public UniTask Initialize()
        {
            _cardLibrary = Container.Instance.GetSO<CardLibrary>();
            _deckRepository = new DeckRepository();
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
            
            return base.setState(type);
        }

        public void StartBattle(
            HeroModel attacker,
            HeroModel defender,
            EBattleMode mode = EBattleMode.PvE,
            EEnemyAIDifficulty enemyDifficulty = EEnemyAIDifficulty.Normal,
            EnemyDeckProfile enemyDeckProfile = null)
        {
            DeckDefinition attackerDeck = _deckRepository.ResolvePlayerDeck(attacker, _cardLibrary);
            DeckDefinition defenderDeck = _deckRepository.ResolveEnemyDeck(defender, enemyDeckProfile, _cardLibrary);
            BattleUnit attackerUnit = _buildUnit(attacker, attackerDeck);
            BattleUnit defenderUnit = _buildUnit(defender, defenderDeck);

            if (mode == EBattleMode.PvE)
            {
                if (enemyDeckProfile != null)
                {
                    enemyDifficulty = enemyDeckProfile.Difficulty;
                }

                defenderUnit.AI = new PriorityAI(enemyDifficulty);
            }

            Model = new BattleModel
            {
                BattleId = Guid.NewGuid().ToString(),
                Mode = mode,
                Phase = new ReactiveProperty<EBattlePhase>(EBattlePhase.WaitingBattle),
                TurnNumber = 0,
                TurnTimeRemaining = new ReactiveProperty<float>(0),
                SideA = new BattleSide(attackerUnit),
                SideB = new BattleSide(defenderUnit),
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

        public void NotifyCardPlayed(BattleCardPlayedEvent battleEvent)
        {
            if (battleEvent == null)
            {
                return;
            }

            CardPlayedFromStateMachine?.Invoke(battleEvent);
        }
        
        private BattleUnit _buildUnit(HeroModel hero, DeckDefinition deck)
        {
            return BattleUnit.FromHero(hero, deck?.Cards, _cardLibrary.AllCards);
        }
    }
}