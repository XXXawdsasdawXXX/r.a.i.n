using System.Collections.Generic;
using System.Linq;
using Core.Data.RangeInt;
using CoreGame.Card.Data;
using CoreGame.Card.Logic;

namespace CoreGame.Card.Logic.AI
{
    public class PriorityAI : IEnemyAI
    {
        private readonly EEnemyAIDifficulty _difficulty;

        public PriorityAI(EEnemyAIDifficulty difficulty = EEnemyAIDifficulty.Normal)
        {
            _difficulty = difficulty;
        }

        public AIAction SelectAction(BattleSide selfSide, BattleUnit self, BattleModel battle)
        {
            if (selfSide == null || self == null || battle == null)
            {
                return null;
            }

            BattleSide enemySide = BattleGridRules.GetOpponentSide(battle, selfSide);
            List<BattleUnit> enemyUnits = BattleGridRules.GetEnemyUnits(battle, selfSide).ToList();

            List<CardBattleState> playableCards = selfSide.GetHand()
                .Where(card => card != null)
                .Where(card => _canUnitPlayCard(selfSide, self, card))
                .Where(card => CardPlayRules.CanPlayCard(self, card))
                .ToList();

            if (playableCards.Count == 0)
            {
                return null;
            }

            List<AIAction> actions = _buildCandidateActions(selfSide, self, enemyUnits, playableCards);
            if (actions.Count == 0)
            {
                return null;
            }

            return _pickAction(actions, self, enemySide);
        }

        private static List<AIAction> _buildCandidateActions(
            BattleSide selfSide,
            BattleUnit self,
            List<BattleUnit> enemyUnits,
            List<CardBattleState> playableCards)
        {
            List<BattleUnit> allyUnits = selfSide.GetAllUnits()
                .Where(unit => unit != null && unit.HP > 0)
                .ToList();

            List<AIAction> actions = new List<AIAction>();
            foreach (CardBattleState card in playableCards)
            {
                if (card?.Config?.Effects == null || card.Config.Effects.Count == 0)
                {
                    continue;
                }

                bool needsEnemySelection = card.Config.Effects.Any(effect =>
                    effect != null
                    && (effect.Target == EEffectTarget.SelectedEnemy || effect.Target == EEffectTarget.EnemyCompanions));
                bool needsAllySelection = card.Config.Effects.Any(effect =>
                    effect != null
                    && (effect.Target == EEffectTarget.SelectedAlly || effect.Target == EEffectTarget.SelectedAnyAllyUnit));
                bool needsSelfTarget = card.Config.Effects.Any(effect =>
                    effect != null && effect.Target == EEffectTarget.Self);

                if (needsSelfTarget)
                {
                    actions.Add(new AIAction { Card = card, Target = self });
                    continue;
                }

                if (needsEnemySelection)
                {
                    foreach (BattleUnit target in enemyUnits)
                    {
                        bool companionOnly = card.Config.Effects.Any(effect => effect != null && effect.Target == EEffectTarget.EnemyCompanions);
                        if (companionOnly && !target.IsCompanion)
                        {
                            continue;
                        }

                        actions.Add(new AIAction { Card = card, Target = target });
                    }

                    continue;
                }

                if (needsAllySelection)
                {
                    foreach (BattleUnit target in allyUnits)
                    {
                        actions.Add(new AIAction { Card = card, Target = target });
                    }

                    continue;
                }

                // Для all-target/без-таргета эффектов достаточно передать self.
                actions.Add(new AIAction { Card = card, Target = self });
            }

            return actions;
        }

        private AIAction _pickAction(List<AIAction> actions, BattleUnit self, BattleSide enemySide)
        {
            switch (_difficulty)
            {
                case EEnemyAIDifficulty.Easy:
                    return actions
                        .OrderBy(action => action.Card.GetEnergyCost(self.Stats))
                        .First();
                case EEnemyAIDifficulty.Hard:
                    return actions
                        .OrderByDescending(action => _scoreAction(action, self, enemySide))
                        .First();
                default:
                    return actions
                        .OrderByDescending(action => _scoreAction(action, self, enemySide) + action.Card.GetEnergyCost(self.Stats) * 0.2f)
                        .First();
            }
        }

        private static float _scoreAction(AIAction action, BattleUnit self, BattleSide enemySide)
        {
            if (action?.Card?.Config?.Effects == null)
            {
                return float.MinValue;
            }

            float score = 0f;
            BattleUnit target = action.Target;
            foreach (CardEffectConfiguration effect in action.Card.Config.Effects)
            {
                if (effect == null)
                {
                    continue;
                }

                float effectValue = _getAvgValue(effect.BaseValue);

                switch (effect.Type)
                {
                    case EEffectType.Damage:
                        score += effectValue * 3f;
                        if (target != null && target.HP <= effectValue)
                        {
                            score += 100f;
                        }
                        break;
                    case EEffectType.Heal:
                        score += self.HP < self.MaxHP * 0.4f ? effectValue * 2f : effectValue * 0.4f;
                        break;
                    case EEffectType.AddArmor:
                        score += self.HP < self.MaxHP * 0.5f ? effectValue * 1.5f : effectValue * 0.7f;
                        break;
                    case EEffectType.AddEnergy:
                        score += effectValue;
                        break;
                    case EEffectType.ApplyStatus:
                        score += effectValue > 0 ? 8f : 4f;
                        break;
                    case EEffectType.SummonCompanion:
                        score += 18f;
                        break;
                    case EEffectType.MoveLine:
                        score += 3f;
                        break;
                    case EEffectType.InjectParasiteEnemy:
                        score += 7f;
                        break;
                    case EEffectType.InjectParasite:
                        score -= 5f;
                        break;
                    case EEffectType.DrawCards:
                        score += self.Hand.Count <= 2 ? effectValue * 2f : effectValue * 0.8f;
                        break;
                }
            }

            if (enemySide?.Hero != null && enemySide.Hero.HP <= 25f && action.Card.Config.Type.HasFlag(ECardType.Attack))
            {
                score += 40f;
            }

            return score;
        }

        private static float _getAvgValue(RangedInt range)
        {
            return (range.MinValue + range.MaxValue) * 0.5f;
        }

        private static bool _canUnitPlayCard(BattleSide selfSide, BattleUnit self, CardBattleState card)
        {
            if (self.IsCompanion)
            {
                return card.OwnerId == self.UnitId;
            }
            
            return card.OwnerId == self.UnitId || selfSide.ContainsMandatoryCard(card);
        }
    }
}