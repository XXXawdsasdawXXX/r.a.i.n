using CoreGame.Card.Data;

namespace CoreGame.Card.Logic.AI
{
    public interface IEnemyAI
    {
        EnemyAction SelectAction(BattleUnit self, BattleModel battle);
    }

    public class EnemyAction
    {
        public CardBattleState Card;
        public string TargetId;
    }
}