using System.Collections.Generic;
using System.Linq;
using CoreGame.Card.Data;

namespace CoreGame.Card.Logic.AI
{
    public class PriorityAI : IEnemyAI
    {
        public EnemyAction SelectAction(BattleUnit self, BattleModel battle)
        {
            BattleSide enemySide = battle.SideA.Hero.OwnerId == self.OwnerId
                ? battle.SideB : battle.SideA;

            // приоритеты по состоянию
            CardBattleState card = _selectByPriority(self, enemySide);
            string targetId = _selectTarget(card, enemySide);

            return new EnemyAction { Card = card, TargetId = targetId };
        }

        private CardBattleState _selectByPriority(BattleUnit self, BattleSide enemy)
        {
            List<CardBattleState> playable = self.Hand
                .Where(c => c.GetEnergyCost(self.Stats) <= self.Energy)
                .ToList();

            if (playable.Count == 0)
            {
                return null;
            }

            // низкое HP — ищем лечение
            if (self.HP / self.MaxHP < 0.3f)
            {
                var heal = playable.FirstOrDefault(c => c.Config.Effects.Any(e => e.Type == EEffectType.Heal));
               
                if (heal != null)
                {
                    return heal;
                }
            }

            // у врага много статусов — добиваем атакой
            if (enemy.Hero.Statuses.Count >= 2)
            {
                CardBattleState attack = playable.FirstOrDefault(c => c.Config.Type.HasFlag(ECardType.Attack));
                if (attack != null)
                {
                    return attack;
                }
            }

            return playable.OrderByDescending(c => c.GetEnergyCost(self.Stats)).First();
        }

        private string _selectTarget(CardBattleState card, BattleSide enemy)
        {
            if (card == null)
            {
                return null;
            }

            //todo тут нужно поумнее придумать
            return enemy.Hero.UnitId;
        }
    }
}