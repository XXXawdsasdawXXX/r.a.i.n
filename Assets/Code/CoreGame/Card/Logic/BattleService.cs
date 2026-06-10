using System;
using System.Collections.Generic;
using System.Linq;
using Core.GameLoop;
using Core.Save;
using Core.ServiceLocator;
using CoreGame.Card.Data;
using CoreGame.Card.Logic.Network;
using CoreGame.Card.Logic.StateMachine;
using Cysharp.Threading.Tasks;

namespace CoreGame.Card.Logic
{
    public class BattleService : IService, IInitializeListener, IExitListener
    {
        public bool IsInitialized { get; set; }
        public event Action<BattleModel> BattleStarted;
        public event Action<BattleModel> TurnStarted;
        public event Action<BattleModel> BattleFinished;
        public event Action<BattleModel> CardPlayed;
        public event Action<BattleCardPlayedEvent> CardPlayedDetailed;
        
        private BattleStateMachine _machine;
        private NetworkBattleService _networkBattle;

        private readonly List<HeroModel> _battleHeroes = new List<HeroModel>();

        
        public UniTask Initialize()
        {
            _machine = Container.Instance.GetService<BattleStateMachine>();
            _networkBattle = Container.Instance.GetService<NetworkBattleService>();
            _machine.CardPlayedFromStateMachine += _onCardPlayedFromStateMachine;
            
            return UniTask.CompletedTask;
        }

        public void StartBattle(
            HeroModel attacker,
            HeroModel defender,
            EBattleMode mode = EBattleMode.PvE,
            EEnemyAIDifficulty enemyDifficulty = EEnemyAIDifficulty.Normal,
            EnemyDeckProfile enemyDeckProfile = null)
        {
            StartBattleInternal(attacker, defender, mode, enemyDifficulty, enemyDeckProfile);
        }

        public void StartCoOpBattle(
            HeroModel playerOne,
            HeroModel playerTwo,
            HeroModel aiEnemy,
            EEnemyAIDifficulty enemyDifficulty = EEnemyAIDifficulty.Normal,
            EnemyDeckProfile enemyDeckProfile = null)
        {
            _battleHeroes.Clear();
            _battleHeroes.Add(playerOne);
            _battleHeroes.Add(playerTwo);
            playerOne.InBattle = true;
            playerTwo.InBattle = true;

            _machine.StartCoOpBattle(playerOne, playerTwo, aiEnemy, enemyDifficulty, enemyDeckProfile);
            _subscribeBattle();
        }

        internal void StartBattleInternal(
            HeroModel attacker,
            HeroModel defender,
            EBattleMode mode,
            EEnemyAIDifficulty enemyDifficulty,
            EnemyDeckProfile enemyDeckProfile,
            string attackerUnitId = null,
            string defenderUnitId = null)
        {
            _battleHeroes.Clear();
            _battleHeroes.Add(attacker); 
            _battleHeroes.Add(defender); 
            attacker.InBattle = true;
            defender.InBattle = true;
            
            _machine.StartBattle(attacker, defender, mode, enemyDifficulty, enemyDeckProfile, attackerUnitId, defenderUnitId);
            _subscribeBattle();
        }

        internal void StartCoOpBattleInternal(
            HeroModel playerOne,
            HeroModel playerTwo,
            HeroModel aiEnemy,
            EEnemyAIDifficulty enemyDifficulty,
            EnemyDeckProfile enemyDeckProfile,
            string playerOneUnitId,
            string playerTwoUnitId)
        {
            _battleHeroes.Clear();
            _battleHeroes.Add(playerOne);
            _battleHeroes.Add(playerTwo);
            playerOne.InBattle = true;
            playerTwo.InBattle = true;

            _machine.StartCoOpBattle(
                playerOne,
                playerTwo,
                aiEnemy,
                enemyDifficulty,
                enemyDeckProfile,
                playerOneUnitId,
                playerTwoUnitId);
            _subscribeBattle();
        }

        private void _subscribeBattle()
        {
            _machine.Model.Phase.SubscribeProperty(_onPhaseChanged);
            BattleStarted?.Invoke(_machine.Model);
            _networkBattle?.SyncFromServer(isBattleStarted: true);
        }

        public void ApplyNetworkSnapshot(BattleModel model, BattleStateSyncBroadcast flags)
        {
            if (model == null)
            {
                return;
            }

            bool isInitial = !_machine.HasActiveBattle;
            _machine.ApplyRemoteSnapshot(model);

            if (flags.IsBattleStarted && isInitial)
            {
                BattleStarted?.Invoke(_machine.Model);
            }

            if (flags.IsTurnStarted)
            {
                TurnStarted?.Invoke(_machine.Model);
            }

            if (flags.IsCardPlayed)
            {
                CardPlayed?.Invoke(_machine.Model);
            }

            if (flags.IsBattleFinished)
            {
                BattleFinished?.Invoke(_machine.Model);
                _machine.ClearRemoteBattle();
            }
        }

        public void NotifyHandUpdated()
        {
            CardPlayed?.Invoke(_machine.Model);
        }

        public CommandResult TryPlayCardWithResult(string cardId, string targetId, string requesterUnitId = null)
        {
            if (_networkBattle != null && _networkBattle.IsRemoteClient)
            {
                return _networkBattle.SendPlayCard(cardId, targetId, requesterUnitId);
            }

            return _tryPlayCardLocal(cardId, targetId);
        }

        private CommandResult _tryPlayCardLocal(string cardId, string targetId)
        {
            if (!(_machine.CurrentState is IAcceptPlayerInput acceptPlayerInput))
            {
                return _resultFromState();
            }

            if (!_tryGetActiveSide(out BattleSide activeSide))
            {
                return CommandResult.InvalidPhase;
            }

            CardBattleState card = CardPlayRules.FindCardInHand(activeSide.GetHand(), cardId);
            if (card == null)
            {
                return CommandResult.CardNotFound;
            }

            if (!CardPlayRules.CanPlayCard(activeSide.Hero, card))
            {
                return CardPlayRules.GetPlayRejectionReason(activeSide.Hero, card);
            }
            
            BattleUnit target = _machine.FindUnit(targetId);
            if (!_isValidTargetForCard(_machine.Model, activeSide, card, target))
            {
                return CommandResult.TargetInvalid;
            }

            if (!acceptPlayerInput.TryPlayCard(cardId, targetId))
            {
                return CommandResult.CardApplyRejected;
            }

            CardPlayedDetailed?.Invoke(new BattleCardPlayedEvent
            {
                ActorUnitId = activeSide.Hero?.UnitId,
                TargetUnitId = target?.UnitId,
                Card = card,
                EffectTypes = _collectEffectTypes(card)
            });
            CardPlayed?.Invoke(_machine.Model);
            _tryFinishBattleAfterAction();
            _networkBattle?.SyncFromServer(isCardPlayed: true);
            return CommandResult.Success;
        }
        
        public CommandResult TryPlayMoveCardToCellWithResult(string cardId, string unitId, EBattleLine line, int cellIndex, string requesterUnitId = null)
        {
            if (_networkBattle != null && _networkBattle.IsRemoteClient)
            {
                return _networkBattle.SendMoveToCell(cardId, unitId, line, cellIndex, requesterUnitId);
            }

            return _tryPlayMoveCardLocal(cardId, unitId, line, cellIndex);
        }

        private CommandResult _tryPlayMoveCardLocal(string cardId, string unitId, EBattleLine line, int cellIndex)
        {
            if (!(_machine.CurrentState is IAcceptPlayerInput acceptPlayerInput))
            {
                return CommandResult.InvalidState;
            }

            if (!_tryGetActiveSide(out BattleSide activeSide))
            {
                return CommandResult.InvalidPhase;
            }

            BattleUnit unit = _machine.FindUnit(unitId);
            if (unit == null)
            {
                return CommandResult.UnitNotFound;
            }

            BattleSide unitSide = BattleGridRules.GetOwnerSide(_machine.Model, unit);
            if (!ReferenceEquals(unitSide, activeSide))
            {
                return CommandResult.NotYourSide;
            }

            if (cellIndex < 0 || cellIndex >= BattleGridRules.CELLS_PER_LINE)
            {
                return CommandResult.InvalidCell;
            }

            bool occupied = activeSide.GetAllUnits()
                .Where(u => u != null && u.HP > 0)
                .Where(u => u.UnitId != unit.UnitId)
                .Any(u => u.Line == line && u.LineCellIndex == cellIndex);
            if (occupied)
            {
                return CommandResult.TargetOccupied;
            }

            CardBattleState card = CardPlayRules.FindCardInHand(activeSide.GetHand(), cardId);
            if (card == null)
            {
                return CommandResult.CardNotFound;
            }

            BattleUnit actor = activeSide.Hero;
            if (!CardPlayRules.CanPlayCard(actor, card))
            {
                return CardPlayRules.GetPlayRejectionReason(actor, card);
            }
            
            bool isMoveCard = card.Config.Effects != null
                              && card.Config.Effects.Any(effect => effect.Type == EEffectType.MoveLine);
            if (!isMoveCard)
            {
                return CommandResult.CardHasNoMoveEffect;
            }

            if (!acceptPlayerInput.TryPlayCard(cardId, unitId))
            {
                return CommandResult.CardApplyRejected;
            }

            bool moved = BattleGridRules.TryMoveUnitToCell(_machine.Model, unit, line, cellIndex);
            if (moved)
            {
                CardPlayedDetailed?.Invoke(new BattleCardPlayedEvent
                {
                    ActorUnitId = actor?.UnitId,
                    TargetUnitId = unit?.UnitId,
                    Card = card,
                    EffectTypes = _collectEffectTypes(card)
                });
                CardPlayed?.Invoke(_machine.Model);
                _tryFinishBattleAfterAction();
                _networkBattle?.SyncFromServer(isCardPlayed: true);
            }

            return moved ? CommandResult.Success : CommandResult.MoveApplyFailed;
        }
        
        public CommandResult TryPlaySummonCardToCellWithResult(string cardId, EBattleLine line, int cellIndex, string requesterUnitId = null)
        {
            if (_networkBattle != null && _networkBattle.IsRemoteClient)
            {
                return _networkBattle.SendSummonToCell(cardId, line, cellIndex, requesterUnitId);
            }

            return _tryPlaySummonCardLocal(cardId, line, cellIndex);
        }

        private CommandResult _tryPlaySummonCardLocal(string cardId, EBattleLine line, int cellIndex)
        {
            if (!(_machine.CurrentState is IAcceptPlayerInput acceptPlayerInput))
            {
                return CommandResult.InvalidState;
            }

            if (!_tryGetActiveSide(out BattleSide activeSide))
            {
                return CommandResult.InvalidPhase;
            }

            if (cellIndex < 0 || cellIndex >= BattleGridRules.CELLS_PER_LINE)
            {
                return CommandResult.InvalidCell;
            }

            bool occupied = activeSide.GetAllUnits()
                .Where(u => u != null && u.HP > 0)
                .Any(u => u.Line == line && u.LineCellIndex == cellIndex);
            if (occupied)
            {
                return CommandResult.TargetOccupied;
            }

            CardBattleState card = CardPlayRules.FindCardInHand(activeSide.GetHand(), cardId);
            if (card == null)
            {
                return CommandResult.CardNotFound;
            }

            if (!CardPlayRules.CanPlayCard(activeSide.Hero, card))
            {
                return CardPlayRules.GetPlayRejectionReason(activeSide.Hero, card);
            }

            bool isSummonCard = card.Config.Effects != null
                                && card.Config.Effects.Any(effect => effect.Type == EEffectType.SummonCompanion);
            if (!isSummonCard)
            {
                return CommandResult.CardApplyRejected;
            }

            int companionsBefore = activeSide.Companions.Count;
            if (!acceptPlayerInput.TryPlayCard(cardId, activeSide.Hero.UnitId))
            {
                return CommandResult.CardApplyRejected;
            }

            BattleUnit summoned = activeSide.Companions.Count > companionsBefore
                ? activeSide.Companions[activeSide.Companions.Count - 1]
                : activeSide.Companions.LastOrDefault();
            if (summoned == null)
            {
                return CommandResult.CardApplyRejected;
            }

            bool moved = BattleGridRules.TryMoveUnitToCell(_machine.Model, summoned, line, cellIndex);
            if (!moved)
            {
                return CommandResult.MoveApplyFailed;
            }

            CardPlayedDetailed?.Invoke(new BattleCardPlayedEvent
            {
                ActorUnitId = activeSide.Hero?.UnitId,
                TargetUnitId = summoned?.UnitId,
                Card = card,
                EffectTypes = _collectEffectTypes(card)
            });
            CardPlayed?.Invoke(_machine.Model);
            _tryFinishBattleAfterAction();
            _networkBattle?.SyncFromServer(isCardPlayed: true);
            return CommandResult.Success;
        }

        public CommandResult EndTurnWithResult(string requesterUnitId = null)
        {
            if (_networkBattle != null && _networkBattle.IsRemoteClient)
            {
                return _networkBattle.SendEndTurn(requesterUnitId);
            }

            if (!(_machine.CurrentState is IAcceptPlayerInput acceptPlayerInput))
            {
                return _resultFromState();
            }
            
            if (!_tryGetActiveSide(out _))
            {
                return CommandResult.InvalidPhase;
            }

            acceptPlayerInput.EndTurn();
            _networkBattle?.SyncFromServer(isTurnStarted: true);
            return CommandResult.Success;
        }

        public BattleUnit FindUnit(string unitId)
        {
            return _machine.FindUnit(unitId);
        }

        private void _tryFinishBattleAfterAction()
        {
            if (_machine.Model == null)
            {
                return;
            }

            if (_machine.Model.Phase.Value == EBattlePhase.Finished || _machine.CurrentState is EndBattleState)
            {
                return;
            }

            if (_machine.Model.IsCoOp)
            {
                bool enemyDead = _machine.Model.EnemySide?.Hero == null || _machine.Model.EnemySide.Hero.HP <= 0;
                bool sideADead = _machine.Model.SideA?.Hero == null || _machine.Model.SideA.Hero.HP <= 0;
                bool sideBDead = _machine.Model.SideB?.Hero == null || _machine.Model.SideB.Hero.HP <= 0;

                if (!enemyDead && !(sideADead && sideBDead))
                {
                    return;
                }
            }
            else
            {
                bool isSideADead = _machine.Model.SideA?.Hero == null || _machine.Model.SideA.Hero.HP <= 0;
                bool isSideBDead = _machine.Model.SideB?.Hero == null || _machine.Model.SideB.Hero.HP <= 0;

                if (!isSideADead && !isSideBDead)
                {
                    return;
                }
            }

            _machine.SwitchState(typeof(EndBattleState));
        }

        private void _onPhaseChanged(EBattlePhase phase)
        {
            switch (phase)
            {
                case EBattlePhase.WaitingBattle:
                    break;
                case EBattlePhase.StartBattle:
                    BattleStarted?.Invoke(_machine.Model);
                    _networkBattle?.SyncFromServer(isBattleStarted: true);
                    break;
                case EBattlePhase.StartTurn:
                    break;
                case EBattlePhase.FirstSideTurn:
                case EBattlePhase.SecondSideTurn:
                case EBattlePhase.EnemyTurn:
                    TurnStarted?.Invoke(_machine.Model);
                    _networkBattle?.SyncFromServer(isTurnStarted: true);
                    break;
                case EBattlePhase.Resolution:
                    _networkBattle?.SyncFromServer(isCardPlayed: true);
                    break;
                case EBattlePhase.Finished:
                    foreach (HeroModel hero in _battleHeroes)
                    {
                        hero.InBattle = false;
                    }
                    _battleHeroes.Clear();
                    _machine.Model.Phase.UnsubscribeProperty(_onPhaseChanged);
                    BattleFinished?.Invoke(_machine.Model);
                    _networkBattle?.SyncFromServer(isBattleFinished: true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(phase), phase, null);
            }
        }

        public void GameExit()
        {
            if (_machine != null)
            {
                _machine.CardPlayedFromStateMachine -= _onCardPlayedFromStateMachine;
            }

            foreach (HeroModel hero in _battleHeroes)
            {
                hero.InBattle = false;
            }
        }

        private void _onCardPlayedFromStateMachine(BattleCardPlayedEvent battleEvent)
        {
            CardPlayedDetailed?.Invoke(battleEvent);
            CardPlayed?.Invoke(_machine?.Model);
            _networkBattle?.SyncFromServer(isCardPlayed: true);
        }

        private static List<EEffectType> _collectEffectTypes(CardBattleState card)
        {
            if (card?.Config?.Effects == null || card.Config.Effects.Count == 0)
            {
                return new List<EEffectType>();
            }

            return card.Config.Effects
                .Where(effect => effect != null)
                .Select(effect => effect.Type)
                .Distinct()
                .ToList();
        }

        private bool _tryGetActiveSide(out BattleSide activeSide)
        {
            activeSide = null;

            if (_machine?.Model == null)
            {
                return false;
            }

            EBattlePhase phase = _machine.Model.Phase.Value;
            if (phase == EBattlePhase.FirstSideTurn)
            {
                activeSide = _machine.Model.SideA;
                return true;
            }

            if (phase == EBattlePhase.SecondSideTurn)
            {
                activeSide = _machine.Model.SideB;
                return true;
            }

            return false;
        }

        private CommandResult _resultFromState()
        {
            return _machine?.Model == null
                ? CommandResult.InvalidState
                : CommandResult.InvalidPhase;
        }

        private static bool _isValidTargetForCard(BattleModel battle, BattleSide actorSide, CardBattleState card, BattleUnit target)
        {
            if (battle == null || actorSide == null || card?.Config?.Effects == null || card.Config.Effects.Count == 0)
            {
                return false;
            }

            bool hasManualTargetEffect = card.Config.Effects.Any(_requiresUnitSelection);
            if (!hasManualTargetEffect)
            {
                return true;
            }

            if (target == null)
            {
                return false;
            }

            BattleSide targetSide = BattleGridRules.GetOwnerSide(battle, target);
            if (targetSide == null)
            {
                return false;
            }

            foreach (CardEffectConfiguration effect in card.Config.Effects)
            {
                if (!_requiresUnitSelection(effect))
                {
                    continue;
                }

                bool isEnemy = _isEnemySide(battle, actorSide, targetSide);
                bool isSelf = target.UnitId == actorSide.Hero.UnitId;
                bool isAlly = ReferenceEquals(targetSide, actorSide);
                bool isCompanion = target.IsCompanion;

                bool validByEffect = effect.Target switch
                {
                    EEffectTarget.Self => isSelf,
                    EEffectTarget.SelectedAlly => isAlly,
                    EEffectTarget.SelectedAnyAllyUnit => isAlly,
                    EEffectTarget.SelectedEnemy => isEnemy,
                    EEffectTarget.EnemyCompanions => isEnemy && isCompanion,
                    _ => false
                };

                if (validByEffect)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool _isEnemySide(BattleModel battle, BattleSide actorSide, BattleSide targetSide)
        {
            if (battle == null || actorSide == null || targetSide == null)
            {
                return false;
            }

            if (ReferenceEquals(actorSide, targetSide))
            {
                return false;
            }

            if (battle.IsCoOp)
            {
                bool actorIsHuman = ReferenceEquals(actorSide, battle.SideA) || ReferenceEquals(actorSide, battle.SideB);
                bool targetIsHuman = ReferenceEquals(targetSide, battle.SideA) || ReferenceEquals(targetSide, battle.SideB);

                if (actorIsHuman && targetIsHuman)
                {
                    return false;
                }

                if (actorIsHuman)
                {
                    return ReferenceEquals(targetSide, battle.EnemySide);
                }

                return targetIsHuman;
            }

            return true;
        }

        private static bool _requiresUnitSelection(CardEffectConfiguration effect)
        {
            if (effect == null)
            {
                return false;
            }

            // Summon/move эффекты не требуют ручного выбора юнита-цели.
            if (effect.Type == EEffectType.SummonCompanion || effect.Type == EEffectType.MoveLine)
            {
                return false;
            }

            EEffectTarget target = effect.Target;
            return target == EEffectTarget.Self
                   || target == EEffectTarget.SelectedAlly
                   || target == EEffectTarget.SelectedAnyAllyUnit
                   || target == EEffectTarget.SelectedEnemy
                   || target == EEffectTarget.EnemyCompanions;
        }
    }
}