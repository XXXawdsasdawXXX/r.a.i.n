using System.Collections.Generic;
using System.Linq;
using CoreGame.Card.Data;
using CoreGame.Card.Logic.AI;
using Cysharp.Threading.Tasks;

namespace CoreGame.Card.Logic.StateMachine
{
    public class TurnResolutionState : IBattleState
    {
        public EBattlePhase Phase => EBattlePhase.Resolution;
        public bool IsInitialized { get; set; }

        private readonly BattleStateMachine _machine;

        
        public TurnResolutionState(BattleStateMachine machine)
        {
            _machine = machine;
        }
        
        public UniTask Initialize()
        {
            return  UniTask.CompletedTask;
        }

        public UniTask Enter()
        {
            _processAutoActions(_machine.Model.SideA, _machine.Model.SideB);
            _processAutoActions(_machine.Model.SideB, _machine.Model.SideA);

            _processCompanions(_machine.Model.SideA);
            _processCompanions(_machine.Model.SideB);
            
            _processStatuses(_machine.Model.SideA);
            _processStatuses(_machine.Model.SideB);

            if (_isBattleFinished())
            {
                _machine.SwitchState(typeof(EndBattleState));
                
                return UniTask.CompletedTask;
            }
            
            _machine.SwitchState(typeof(StartTurnState));
            
            return UniTask.CompletedTask;
        }

        public UniTask Exit()
        {
            return UniTask.CompletedTask;
        }

        private static void _processAutoActions(BattleSide side, BattleSide enemySide)
        {
            foreach (BattleUnit unit in side.GetAllUnits())
            {
                if (unit == null || unit.HP <= 0)
                {
                    continue;
                }

                float value = unit.AutoActionValue;
                if (value <= 0f)
                {
                    continue;
                }

                switch (unit.AutoActionType)
                {
                    case EAutoActionType.AttackEnemyHero:
                    {
                        BattleUnit target = enemySide.Hero;
                        if (target == null || target.HP <= 0)
                        {
                            break;
                        }

                        float absorbed = UnityEngine.Mathf.Min(target.Armor, value);
                        target.Armor -= absorbed;
                        target.HP -= (value - absorbed);
                        break;
                    }
                    case EAutoActionType.GiveShieldToOwnerHero:
                    {
                        BattleUnit target = side.Hero;
                        if (target == null || target.HP <= 0)
                        {
                            break;
                        }

                        target.Armor += value;
                        break;
                    }
                }
            }
        }

        private void _processCompanions(BattleSide side)
        {
            foreach (BattleUnit companion in side.Companions)
            {
                if (companion.HP <= 0 || companion.AI == null)
                {
                    continue;
                }

                AIAction action = companion.AI.SelectAction(side, companion, _machine.Model);

                if (action == null)
                {
                    continue;
                }

                BattleUnit target = action.Target;

                if (target != null)
                {
                    _machine.Processor.ApplyCard(companion, action.Card, target, _machine.Model);
                    _machine.NotifyCardPlayed(new BattleCardPlayedEvent
                    {
                        ActorUnitId = companion.UnitId,
                        TargetUnitId = target.UnitId,
                        Card = action.Card,
                        EffectTypes = _collectEffectTypes(action.Card)
                    });
                }
            }
        }
        
        private void _processStatuses(BattleSide side)
        {
            foreach (BattleUnit unit in side.GetAllUnits())
            {
                _machine.Processor.TickStatuses(unit, _machine.Model);
            }

            List<string> removedCompanionIds = side.Companions
                .Where(_shouldRemoveCompanion)
                .Select(c => c.UnitId)
                .ToList();

            //todo тоже вот нужно какой то фитбек во вьюху дать
            side.Companions.RemoveAll(_shouldRemoveCompanion);
            _removeCompanionCards(side, removedCompanionIds);
        }
        
        private static void _removeCompanionCards(BattleSide side, List<string> removedCompanionIds)
        {
            if (side == null || removedCompanionIds == null || removedCompanionIds.Count == 0)
            {
                return;
            }

            side.Hero.Hand.RemoveAll(card => card != null && removedCompanionIds.Contains(card.OwnerId));
            side.Hero.Deck.RemoveAll(card => card != null && removedCompanionIds.Contains(card.OwnerId));
            side.Hero.Discard.RemoveAll(card => card != null && removedCompanionIds.Contains(card.OwnerId));
        }
        
        private static bool _shouldRemoveCompanion(BattleUnit companion)
        {
            if (companion == null)
            {
                return false;
            }

            if (companion.HP <= 0)
            {
                return true;
            }

            return companion.Statuses != null
                   && companion.Statuses.Any(status => status.Type == EStatusType.SummonDuration && status.Duration <= 0);
        }
        
        private bool _isBattleFinished()
        {
            bool sideADead = _machine.Model.SideA.Hero.HP <= 0;
            bool sideBDead = _machine.Model.SideB.Hero.HP <= 0;
            return sideADead || sideBDead;
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
    }
}