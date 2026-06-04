using CoreGame.Card.Data;

namespace CoreGame.Card.Logic.Targeting
{
    /// <summary>
    /// Выбор цели для карты: враг, союзник, self, несколько целей, AoE.
    /// Сейчас в UI — заглушка на героя противника; позже — режим выбора после клика по карте.
    /// </summary>
    public interface ICardTargetResolver
    {
        bool TryResolveTarget(BattleModel battle, BattleUnit actor, CardBattleState card, out string targetUnitId);
    }
}
