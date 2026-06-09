using System;
using System.Linq;
using CoreGame.Card.Data;

namespace CoreGame.Card.Logic.CardProcessors
{
    public class CardApplyStatusProcessor : ICardProcessor
    {
        public void Process(CardEffectConfiguration effect, BattleUnit actor, BattleUnit target, BattleModel battle)
        {
            // если статус уже есть - обновляем
            StatusEffect existing = target.Statuses
                .FirstOrDefault(s => s.Type == effect.StatusType);

            if (existing != null)
            {
                existing.Duration += effect.StatusDuration;
                existing.Value = Math.Max(existing.Value, BattleProcessor.CalculateEffectValue(effect, actor));
            }
            else
            {
                target.Statuses.Add(new StatusEffect
                {
                    Type = effect.StatusType,
                    Duration = effect.StatusDuration,
                    Value = BattleProcessor.CalculateEffectValue(effect, actor),
                    //SourceCardId = effect.id
                });
            }
        }
    }
}