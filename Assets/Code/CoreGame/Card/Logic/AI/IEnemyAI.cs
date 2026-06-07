using CoreGame.Card.Data;

namespace CoreGame.Card.Logic.AI
{
    public interface IEnemyAI
    {
        AIAction SelectAction(BattleSide selfSide, BattleUnit self, BattleModel battle);
    }

    public class AIAction
    {
        public CardBattleState Card;
        public BattleUnit Target;
    }
}