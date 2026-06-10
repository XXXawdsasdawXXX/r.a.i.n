using System;
using System.Collections.Generic;
using System.Linq;
using Core.Data;
using Core.GameLoop;
using Core.Save;
using Core.ServiceLocator;
using Core.StateMachine;
using CoreGame.Card.Data;
using CoreGame.Card.Logic.AI;
using CoreGame.Card.Logic.Network;
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
        public bool HasActiveBattle => !string.IsNullOrEmpty(Model?.BattleId);
        public BattleProcessor Processor { get; private set; }
        public event Action<BattleCardPlayedEvent> CardPlayedFromStateMachine;

        private CardLibrary _cardLibrary;
        private IDeckRepository _deckRepository;
        
        
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
                { typeof(EnemyTurnState), new EnemyTurnState(this) },
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
            EnemyDeckProfile enemyDeckProfile = null,
            string attackerUnitId = null,
            string defenderUnitId = null)
        {
            DeckDefinition attackerDeck = _deckRepository.ResolvePlayerDeck(attacker, _cardLibrary);
            DeckDefinition defenderDeck = _deckRepository.ResolveEnemyDeck(defender, enemyDeckProfile, _cardLibrary);
            BattleUnit attackerUnit = _buildUnit(attacker, attackerDeck);
            BattleUnit defenderUnit = _buildUnit(defender, defenderDeck);

            _applyUnitId(attackerUnit, attackerUnitId ?? attacker.HeroId);
            _applyUnitId(defenderUnit, defenderUnitId ?? defender.HeroId);

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

        public void StartCoOpBattle(
            HeroModel playerOne,
            HeroModel playerTwo,
            HeroModel aiEnemy,
            EEnemyAIDifficulty enemyDifficulty = EEnemyAIDifficulty.Normal,
            EnemyDeckProfile enemyDeckProfile = null,
            string playerOneUnitId = null,
            string playerTwoUnitId = null)
        {
            DeckDefinition playerOneDeck = _deckRepository.ResolvePlayerDeck(playerOne, _cardLibrary);
            DeckDefinition playerTwoDeck = _deckRepository.ResolvePlayerDeck(playerTwo, _cardLibrary);
            DeckDefinition enemyDeck = _deckRepository.ResolveEnemyDeck(aiEnemy, enemyDeckProfile, _cardLibrary);

            BattleUnit playerOneUnit = _buildUnit(playerOne, playerOneDeck);
            BattleUnit playerTwoUnit = _buildUnit(playerTwo, playerTwoDeck);
            BattleUnit enemyUnit = _buildUnit(aiEnemy, enemyDeck);

            _applyUnitId(playerOneUnit, playerOneUnitId ?? playerOne.HeroId);
            _applyUnitId(playerTwoUnit, playerTwoUnitId ?? playerTwo.HeroId);
            _applyUnitId(enemyUnit, Guid.NewGuid().ToString());

            if (enemyDeckProfile != null)
            {
                enemyDifficulty = enemyDeckProfile.Difficulty;
            }

            enemyUnit.AI = new PriorityAI(enemyDifficulty);

            Model = new BattleModel
            {
                BattleId = Guid.NewGuid().ToString(),
                Mode = EBattleMode.CoOpPvE,
                Phase = new ReactiveProperty<EBattlePhase>(EBattlePhase.WaitingBattle),
                TurnNumber = 0,
                TurnTimeRemaining = new ReactiveProperty<float>(0),
                SideA = new BattleSide(playerOneUnit),
                SideB = new BattleSide(playerTwoUnit),
                EnemySide = new BattleSide(enemyUnit),
            };

            BattleGridRules.AssignCoOpStartPositions(Model);

            SwitchState(typeof(StartBattleState));
        }
        
        public BattleUnit FindUnit(string unitId)
        {
            if (string.IsNullOrEmpty(unitId))
            {
                return null;
            }

            IEnumerable<BattleUnit> units = Model.SideA.GetAllUnits()
                .Concat(Model.SideB.GetAllUnits());

            if (Model.EnemySide != null)
            {
                units = units.Concat(Model.EnemySide.GetAllUnits());
            }

            return units.FirstOrDefault(u => u.UnitId == unitId);
        }

        public void NotifyCardPlayed(BattleCardPlayedEvent battleEvent)
        {
            if (battleEvent == null)
            {
                return;
            }

            CardPlayedFromStateMachine?.Invoke(battleEvent);
        }

        /// <summary>
        /// Клиентское зеркало состояния с сервера. Не запускает переходы FSM — только обновляет Model для UI.
        /// На сервере/хосте не вызывать: там Model меняется только через StartBattle и состояния.
        /// </summary>
        public void ApplyRemoteSnapshot(BattleModel incoming)
        {
            if (incoming == null)
            {
                return;
            }

            if (!HasActiveBattle)
            {
                Model = incoming;
                return;
            }

            BattleSnapshotSerializer.ApplyPublicSnapshot(Model, incoming);
        }

        public void ClearRemoteBattle()
        {
            Model = _createEmptyModel();
        }

        private static BattleModel _createEmptyModel()
        {
            return new BattleModel
            {
                Phase = new ReactiveProperty<EBattlePhase>(EBattlePhase.WaitingBattle),
                TurnTimeRemaining = new ReactiveProperty<float>(0),
            };
        }
        
        private BattleUnit _buildUnit(HeroModel hero, DeckDefinition deck)
        {
            return BattleUnit.FromHero(hero, deck?.Cards, _cardLibrary.AllCards);
        }

        private static void _applyUnitId(BattleUnit unit, string unitId)
        {
            if (unit == null || string.IsNullOrEmpty(unitId))
            {
                return;
            }

            unit.UnitId = unitId;
            foreach (CardBattleState card in unit.Deck)
            {
                card.OwnerId = unitId;
            }
        }
    }
}