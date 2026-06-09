using System;
using System.Collections.Generic;
using System.Linq;
using Core.GameLoop;
using Core.Network;
using Core.Save;
using Core.ServiceLocator;
using CoreGame.Card.Data;
using CoreGame.Card.Logic.StateMachine;
using Cysharp.Threading.Tasks;
using FishNet;

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
        private NetworkBattleService _network;
        private UserProvider _userProvider;

        private readonly List<HeroModel> _battleHeroes = new List<HeroModel>();

        
        public UniTask Initialize()
        {
            _machine = Container.Instance.GetService<BattleStateMachine>();
            _userProvider = Container.Instance.GetService<UserProvider>();
            _machine.CardPlayedFromStateMachine += _onCardPlayedFromStateMachine;
            
            return UniTask.CompletedTask;
        }

        public void SetNetworkBridge(NetworkBattleService network)
        {
            _network = network;
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
            if (_network != null
                && _network.IsMultiplayerMode(mode)
                && InstanceFinder.IsServerStarted)
            {
                return;
            }

            StartBattleLocal(sideAHero, sideBHero, allyHero, mode, enemyDifficulty, enemyDeckProfile);
        }

        internal void StartBattleLocal(
            HeroModel sideAHero,
            HeroModel sideBHero,
            HeroModel allyHero,
            EBattleMode mode = EBattleMode.PvE,
            EEnemyAIDifficulty enemyDifficulty = EEnemyAIDifficulty.Normal,
            EnemyDeckProfile enemyDeckProfile = null)
        {
            _registerBattleHero(sideAHero);
            _registerBattleHero(sideBHero);
            if (allyHero != null)
            {
                _registerBattleHero(allyHero);
            }

            _machine.StartBattle(sideAHero, sideBHero, allyHero, mode, enemyDifficulty, enemyDeckProfile);
            _machine.Model.Phase.SubscribeProperty(_onPhaseChanged);

            BattleStarted?.Invoke(_machine.Model);
        }

        public void NotifyClientBattleStarted()
        {
            if (_machine?.Model?.Phase == null)
            {
                return;
            }

            _machine.Model.Phase.SubscribeProperty(_onPhaseChanged);
            BattleStarted?.Invoke(_machine.Model);
        }

        public void NotifyClientSynced()
        {
            if (_machine?.Model == null)
            {
                return;
            }

            CardPlayed?.Invoke(_machine.Model);

            if (BattleParticipantResolver.IsMyTurn(_machine.Model, _getLocalHeroId()))
            {
                TurnStarted?.Invoke(_machine.Model);
            }
        }

        public CommandResult TryPlayCardWithResult(string cardId, string targetId, string requesterHeroId = null)
        {
            requesterHeroId ??= _getLocalHeroId();

            if (_shouldDelegateToNetwork(requesterHeroId))
            {
                _network.RequestPlayCard(cardId, targetId, requesterHeroId);
                return CommandResult.Success;
            }

            if (!(_machine.CurrentState is IAcceptPlayerInput acceptPlayerInput))
            {
                return _resultFromState();
            }

            if (!_canPlayerAct(requesterHeroId))
            {
                return CommandResult.InvalidPhase;
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
            return CommandResult.Success;
        }
        
        public CommandResult TryPlayMoveCardToCellWithResult(
            string cardId,
            string unitId,
            EBattleLine line,
            int cellIndex,
            string requesterHeroId = null)
        {
            requesterHeroId ??= _getLocalHeroId();

            if (_shouldDelegateToNetwork(requesterHeroId))
            {
                _network.RequestPlayMoveCard(cardId, unitId, line, cellIndex, requesterHeroId);
                return CommandResult.Success;
            }

            if (!(_machine.CurrentState is IAcceptPlayerInput acceptPlayerInput))
            {
                return CommandResult.InvalidState;
            }

            if (!_canPlayerAct(requesterHeroId))
            {
                return CommandResult.InvalidPhase;
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
            }

            return moved ? CommandResult.Success : CommandResult.MoveApplyFailed;
        }
        
        public CommandResult TryPlaySummonCardToCellWithResult(
            string cardId,
            EBattleLine line,
            int cellIndex,
            string requesterHeroId = null)
        {
            requesterHeroId ??= _getLocalHeroId();

            if (_shouldDelegateToNetwork(requesterHeroId))
            {
                _network.RequestPlaySummonCard(cardId, line, cellIndex, requesterHeroId);
                return CommandResult.Success;
            }

            if (!(_machine.CurrentState is IAcceptPlayerInput acceptPlayerInput))
            {
                return CommandResult.InvalidState;
            }

            if (!_canPlayerAct(requesterHeroId))
            {
                return CommandResult.InvalidPhase;
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
            return CommandResult.Success;
        }

        public CommandResult EndTurnWithResult(string requesterHeroId = null)
        {
            requesterHeroId ??= _getLocalHeroId();

            if (_shouldDelegateToNetwork(requesterHeroId))
            {
                _network.RequestEndTurn(requesterHeroId);
                return CommandResult.Success;
            }

            if (!(_machine.CurrentState is IAcceptPlayerInput acceptPlayerInput))
            {
                return _resultFromState();
            }

            if (!_canPlayerAct(requesterHeroId))
            {
                return CommandResult.InvalidPhase;
            }
            
            if (!_tryGetActiveSide(out _))
            {
                return CommandResult.InvalidPhase;
            }

            acceptPlayerInput.EndTurn();
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

            bool isSideBDead = _machine.Model.SideB?.Hero == null || _machine.Model.SideB.Hero.HP <= 0;

            if (_machine.Model.Mode == EBattleMode.CoOpPvE)
            {
                bool isSideADead = _machine.Model.SideA?.Hero == null || _machine.Model.SideA.Hero.HP <= 0;
                bool isAllyDead = _machine.Model.AllySide?.Hero == null || _machine.Model.AllySide.Hero.HP <= 0;

                if (!isSideBDead && !(isSideADead && isAllyDead))
                {
                    return;
                }
            }
            else
            {
                bool isSideADead = _machine.Model.SideA?.Hero == null || _machine.Model.SideA.Hero.HP <= 0;
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
                    break;
                case EBattlePhase.StartTurn:
                    break;
                case EBattlePhase.FirstSideTurn:
                case EBattlePhase.AllySideTurn:
                case EBattlePhase.SecondSideTurn:
                    TurnStarted?.Invoke(_machine.Model);
                    break;
                case EBattlePhase.Resolution:
                    break;
                case EBattlePhase.Finished:
                    foreach (HeroModel hero in _battleHeroes)
                    {
                        hero.InBattle = false;
                    }
                    _battleHeroes.Clear();
                    _machine.Model.Phase.UnsubscribeProperty(_onPhaseChanged);
                    BattleFinished?.Invoke(_machine.Model);
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
            activeSide = BattleParticipantResolver.GetActiveSide(_machine?.Model);
            return activeSide != null;
        }

        private bool _canPlayerAct(string requesterHeroId)
        {
            return BattleParticipantResolver.IsMyTurn(_machine?.Model, requesterHeroId);
        }

        private bool _shouldDelegateToNetwork(string requesterHeroId)
        {
            return _network != null
                   && _network.IsNetworkBattle
                   && !InstanceFinder.IsServerStarted
                   && requesterHeroId == _getLocalHeroId();
        }

        private string _getLocalHeroId()
        {
            if (!string.IsNullOrEmpty(_userProvider?.Id))
            {
                return _userProvider.Id;
            }

            return null;
        }

        private void _registerBattleHero(HeroModel hero)
        {
            if (hero == null || _battleHeroes.Contains(hero))
            {
                return;
            }

            _battleHeroes.Add(hero);
            hero.InBattle = true;
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

                bool isEnemy = !ReferenceEquals(targetSide, actorSide);
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