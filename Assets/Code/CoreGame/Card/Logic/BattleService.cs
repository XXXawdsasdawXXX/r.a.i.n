using System;
using System.Linq;
using Core.GameLoop;
using Core.Save;
using Core.ServiceLocator;
using CoreGame.Card.Data;
using CoreGame.Card.Logic.AI;
using CoreGame.Card.Logic.StateMachine;
using Cysharp.Threading.Tasks;
using GameKit.Dependencies.Utilities;

namespace CoreGame.Card.Logic
{
    public class BattleService : IService, IInitializeListener
    {
        public event Action<BattleModel> BattleStarted;
        public event Action<BattleModel> TurnStarted;
        public event Action<BattleUnit, CardBattleState> CardPlayed;
        public event Action<BattleModel> BattleFinished;

        public bool IsInitialized { get; set; }
        
        public BattleProcessor Processor { get; } = new();
        public BattleValidator Validator { get; } = new();
        public BattleModel Model { get; private set; }
        
        public CardLibrary CardLibrary;

        private BattleStateMachine _battleStateMachine = new BattleStateMachine();
        

        public UniTask Initialize()
        {
            CardLibrary = Container.Instance.GetConfig<CardLibrary>();

            return UniTask.CompletedTask;
        }

        // --- Старт боя ---

        public BattleModel CreateBattle(HeroModel firstSide, HeroModel secondSide, EBattleMode mode = EBattleMode.PvE)
        {
            Model = new BattleModel
            {
                BattleId = Guid.NewGuid().ToString(),
                Mode = mode,
                TurnNumber = 0,
                TurnTimeRemaining = 60f,
                SideA = new BattleSide(BattleUnit.FromHero(firstSide, CardLibrary.AllCards)),
                SideB = new BattleSide(BattleUnit.FromHero(secondSide, CardLibrary.AllCards))
            };
            
            _battleStateMachine.SwitchState(typeof(StartBattleState));

            return Model;
        }
        
        public bool TryPlayCard(string unitId, int cardIndex, string targetId)
        {
            if (!Validator.CanPlayCard(Model, unitId, cardIndex, targetId))
            {
                return false;
            }

            BattleSide side = _battleStateMachine.Phase.Value == EBattlePhase.FirstSideTurn
                ? Model.SideA
                : Model.SideB;
            
            BattleUnit actor = _findUnit(unitId);
            CardBattleState card = side.Hand[cardIndex];
            BattleUnit target = _findUnit(targetId);

            Processor.ApplyCard(actor, card, target, Model);

            _spendCard(actor, card);

            CardPlayed?.Invoke(actor, card);

            _checkBattleEnd();

            return true;
        }

        /*
        public bool TryMoveLine(string unitId)
        {
            BattleUnit unit = _findUnit(unitId);

            if (!Validator.CanMoveLine(Model, unit))
                return false;

            unit.Energy -= unit.MoveLineCost;
            unit.Line = unit.Line == EBattleLine.Frontline
                ? EBattleLine.Backline
                : EBattleLine.Frontline;

            return true;
        }*/

        public void EndTurn(string ownerId)
        {
            if (Model.ActiveSide.Hero.OwnerId != ownerId)
            {
                return;
            }

            _processCompanions(Model.ActiveSide);
            _processStatuses(Model.ActiveSide);
            _processStatuses(Model.WaitingSide);

            _swapSides();

            _refillHand(Model.ActiveSide);

            Model.TurnNumber++;
            Model.TurnTimeRemaining = 60f;
            Model.Phase = EBattlePhase.SecondSideTurn;

            TurnStarted?.Invoke(Model);
        }

        

        // --- Внутренняя логика ---


        private void _drawStartingHand(BattleSide side)
        {
            _drawCards(side.Hero, side.Hero.HandLimit);
        }

        private void _drawCards(BattleUnit unit, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (unit.Deck.Count == 0)
                    _reshuffleDeck(unit);

                if (unit.Deck.Count == 0)
                    break;

                CardBattleState card = unit.Deck[0];
                unit.Deck.RemoveAt(0);
                unit.Hand.Add(card);
            }
        }

        private void _refillHand(BattleSide side)
        {
            // добираем до лимита руки
            int toDraw = side.Hero.HandLimit - side.Hero.Hand.Count;
            if (toDraw > 0)
                _drawCards(side.Hero, toDraw);
        }

        private void _spendCard(BattleUnit actor, CardBattleState card)
        {
            if (card.Config.Charges > 0)
            {
                card.ChargesLeft--;
                if (card.ChargesLeft <= 0)
                {
                    actor.Hand.Remove(card);
                    actor.Discard.Add(card);
                }
                // если заряды остались - карта остаётся в руке
            }
            else
            {
                actor.Hand.Remove(card);
                actor.Discard.Add(card);
            }
        }

        private void _processCompanions(BattleSide side)
        {
            foreach (BattleUnit companion in side.Companions)
            {
                /*if (companion.HP <= 0 || companion.Behaviour == null)
                continue;

            CardBattleState action = companion.Behaviour
                .SelectAction(companion, _battle);

            if (action == null)
                continue;

            BattleUnit target = _resolveCompanionTarget(action, companion);

            if (target != null)
                _processor.ApplyCard(companion, action, target, _battle);*/
            }
        }

        private void _processStatuses(BattleSide side)
        {
            foreach (BattleUnit unit in side.GetAllUnits())
            {
                Processor.TickStatuses(unit, Model);
            }
        }

        private void _swapSides()
        {
            (Model.ActiveSide, Model.WaitingSide) =
                (Model.WaitingSide, Model.ActiveSide);
        }

        private void _checkBattleEnd()
        {
            bool attackerDead = Model.SideA.Hero.HP <= 0;
            bool defenderDead = Model.SideB.Hero.HP <= 0;

            if (!attackerDead && !defenderDead)
            {
                return;
            }

            Model.Phase = EBattlePhase.Finished;

            BattleFinished?.Invoke(Model);
        }

        private BattleUnit _resolveCompanionTarget(CardBattleState action, BattleUnit companion)
        {
            /*bool isOwnerActive = _battle.ActiveSide.Companions.Contains(companion);

        BattleSide enemySide = isOwnerActive 
            ? _battle.WaitingSide 
            : _battle.ActiveSide;

        BattleSide allySide = isOwnerActive 
            ? _battle.ActiveSide 
            : _battle.WaitingSide;

        EEffectTarget target = action.Config.Effects[0].Target;

        return target switch
        {
            EEffectTarget.SelectedEnemy => enemySide.GetTargetableUnits(action.Config.Type).FirstOrDefault(),
            EEffectTarget.AllAllies     => allySide.Hero,
            _                           => null
        };*/

            return null;
        }

        private BattleUnit _findUnit(string unitId)
        {
            return Model.ActiveSide.GetAllUnits()
                .Concat(Model.WaitingSide.GetAllUnits())
                .FirstOrDefault(u => u.UnitId == unitId);
        }

        private void _processNPCTurn(BattleSide side)
        {
            if (side.Hero.AI == null) return; // это игрок

            while (true)
            {
                EnemyAction action = side.Hero.AI.SelectAction(side.Hero, Model);

                if (action?.Card == null) break;

                int cost = action.Card.GetEnergyCost(side.Hero.Stats);
                if (side.Hero.Energy < cost) break;

                Processor.ApplyCard(side.Hero, action.Card,
                    _findUnit(action.TargetId), Model);
                _spendCard(side.Hero, action.Card);
            }
        }
    }
}