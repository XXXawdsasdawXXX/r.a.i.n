using System;
using System.Collections.Generic;
using System.Linq;
using Core.GameLoop;
using Core.Save;
using Core.ServiceLocator;
using CoreGame.Card.Data;
using CoreGame.Card.Logic.StateMachine;
using Cysharp.Threading.Tasks;
using Essential;

namespace CoreGame.Card.Logic
{
    public class BattleService : IService, IInitializeListener, IExitListener
    {
        public bool IsInitialized { get; set; }
        public event Action<BattleModel> BattleStarted;
        public event Action<BattleModel> TurnStarted;
        public event Action<BattleModel> BattleFinished;
        public event Action<BattleModel> CardPlayed;
        
        private BattleStateMachine _machine;

        private readonly List<HeroModel> _battleHeroes = new List<HeroModel>();

        
        public UniTask Initialize()
        {
            _machine = Container.Instance.GetService<BattleStateMachine>();
            
            return UniTask.CompletedTask;
        }

        public void StartBattle(HeroModel attacker, HeroModel defender, EBattleMode mode = EBattleMode.PvE)
        {
            _battleHeroes.Add(attacker); 
            _battleHeroes.Add(defender); 
            attacker.InBattle = true;
            defender.InBattle = true;
            
            _machine.StartBattle(attacker, defender, mode);
            _machine.Model.Phase.SubscribeProperty(_onPhaseChanged);
            
            BattleStarted?.Invoke(_machine.Model);
       
            Log.Info(this, "Start battle");            
        }

        public CommandResult TryPlayCardWithResult(string cardId, string targetId)
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
                return CommandResult.CardCannotBePlayed;
            }

            if (!acceptPlayerInput.TryPlayCard(cardId, targetId))
            {
                return CommandResult.CardApplyRejected;
            }

            CardPlayed?.Invoke(_machine.Model);
            _tryFinishBattleAfterAction();
            return CommandResult.Success;
        }
        
        public CommandResult TryPlayMoveCardToCellWithResult(string cardId, string unitId, EBattleLine line, int cellIndex)
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
                Log.Info(this, $"[TryPlayMoveCardToCell] reject foreign side unit={unitId}");
                return CommandResult.NotYourSide;
            }

            if (cellIndex < 0 || cellIndex >= BattleGridRules.CELLS_PER_LINE)
            {
                Log.Info(this, $"[TryPlayMoveCardToCell] invalid cell index={cellIndex}");
                return CommandResult.InvalidCell;
            }

            bool occupied = activeSide.GetAllUnits()
                .Where(u => u != null && u.HP > 0)
                .Where(u => u.UnitId != unit.UnitId)
                .Any(u => u.Line == line && u.LineCellIndex == cellIndex);
            if (occupied)
            {
                Log.Info(this, $"[TryPlayMoveCardToCell] target occupied unit={unitId} target={line}/{cellIndex}");
                return CommandResult.TargetOccupied;
            }

            CardBattleState card = CardPlayRules.FindCardInHand(activeSide.GetHand(), cardId);
            if (card == null)
            {
                Log.Info(this, $"[TryPlayMoveCardToCell] card not found card={cardId}");
                return CommandResult.CardNotFound;
            }

            BattleUnit actor = activeSide.Hero;
            if (!CardPlayRules.CanPlayCard(actor, card))
            {
                Log.Info(this, $"[TryPlayMoveCardToCell] card can't be played card={cardId}");
                return CommandResult.CardCannotBePlayed;
            }
            
            bool isMoveCard = card.Config.Effects != null
                              && card.Config.Effects.Any(effect => effect.Type == EEffectType.MoveLine);
            if (!isMoveCard)
            {
                Log.Info(this, $"[TryPlayMoveCardToCell] card has no MoveLine effect card={cardId}");
                return CommandResult.CardHasNoMoveEffect;
            }

            if (!acceptPlayerInput.TryPlayCard(cardId, unitId))
            {
                Log.Info(this, $"[TryPlayMoveCardToCell] play failed card={cardId} unit={unitId}");
                return CommandResult.CardApplyRejected;
            }

            bool moved = BattleGridRules.TryMoveUnitToCell(_machine.Model, unit, line, cellIndex);
            Log.Info(this, $"[TryPlayMoveCardToCell] card={cardId} unit={unitId} target={line}/{cellIndex} moved={moved}");

            if (moved)
            {
                CardPlayed?.Invoke(_machine.Model);
                _tryFinishBattleAfterAction();
            }

            return moved ? CommandResult.Success : CommandResult.MoveApplyFailed;
        }

        public CommandResult EndTurnWithResult()
        {
            if (!(_machine.CurrentState is IAcceptPlayerInput acceptPlayerInput))
            {
                return _resultFromState();
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

            bool isSideADead = _machine.Model.SideA?.Hero == null || _machine.Model.SideA.Hero.HP <= 0;
            bool isSideBDead = _machine.Model.SideB?.Hero == null || _machine.Model.SideB.Hero.HP <= 0;

            if (!isSideADead && !isSideBDead)
            {
                return;
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
                    TurnStarted?.Invoke(_machine.Model);
                    break;
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
            foreach (HeroModel hero in _battleHeroes)
            {
                hero.InBattle = false;
            }
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
    }
}