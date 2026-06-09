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
                { typeof(AllySideTurnState), new AllySideTurnState(this) },
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
            StartBattle(attacker, defender, null, mode, enemyDifficulty, enemyDeckProfile);
        }

        public void StartBattle(
            HeroModel sideAHero,
            HeroModel sideBHero,
            HeroModel allyHero,
            EBattleMode mode = EBattleMode.PvE,
            EEnemyAIDifficulty enemyDifficulty = EEnemyAIDifficulty.Normal,
            EnemyDeckProfile enemyDeckProfile = null)
        {
            DeckDefinition sideADeck = _deckRepository.ResolvePlayerDeck(sideAHero, _cardLibrary);
            DeckDefinition sideBDeck = _deckRepository.ResolveEnemyDeck(sideBHero, enemyDeckProfile, _cardLibrary);
            BattleUnit sideAUnit = _buildUnit(sideAHero, sideADeck);
            BattleUnit sideBUnit = _buildUnit(sideBHero, sideBDeck);

            bool isCoOp = mode == EBattleMode.CoOpPvE && allyHero != null;
            BattleSide allySide = null;

            if (isCoOp)
            {
                DeckDefinition allyDeck = _deckRepository.ResolvePlayerDeck(allyHero, _cardLibrary);
                BattleUnit allyUnit = _buildUnit(allyHero, allyDeck);
                allySide = new BattleSide(allyUnit);
            }

            if (mode is EBattleMode.PvE or EBattleMode.CoOpPvE)
            {
                if (enemyDeckProfile != null)
                {
                    enemyDifficulty = enemyDeckProfile.Difficulty;
                }

                sideBUnit.AI = new PriorityAI(enemyDifficulty);
            }

            Model = new BattleModel
            {
                BattleId = Guid.NewGuid().ToString(),
                Mode = mode,
                Phase = new ReactiveProperty<EBattlePhase>(EBattlePhase.WaitingBattle),
                TurnNumber = 0,
                TurnTimeRemaining = new ReactiveProperty<float>(0),
                SideA = new BattleSide(sideAUnit),
                SideB = new BattleSide(sideBUnit),
                AllySide = allySide,
            };

            SwitchState(typeof(StartBattleState));
        }
        
        public BattleUnit FindUnit(string unitId)
        {
            if (string.IsNullOrEmpty(unitId))
            {
                return null;
            }

            IEnumerable<BattleUnit> units = Model.SideA.GetAllUnits().Concat(Model.SideB.GetAllUnits());
            if (Model.HasAllySide)
            {
                units = units.Concat(Model.AllySide.GetAllUnits());
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

        public void EnsureClientModelShell()
        {
            if (Model?.Phase != null)
            {
                return;
            }

            Model = new BattleModel
            {
                Phase = new ReactiveProperty<EBattlePhase>(EBattlePhase.WaitingBattle),
                TurnTimeRemaining = new ReactiveProperty<float>(0),
            };
        }
        
        private BattleUnit _buildUnit(HeroModel hero, DeckDefinition deck)
        {
            return BattleUnit.FromHero(hero, deck?.Cards, _cardLibrary.AllCards);
        }
    }
}