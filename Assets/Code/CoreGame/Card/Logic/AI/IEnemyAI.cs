using CoreGame.Card.Data;

namespace CoreGame.Card.Logic.AI
{
    public interface IEnemyAI
    {
        AIAction SelectAction(BattleUnit self, BattleModel battle);
    }

    public class AIAction
    {
        public CardBattleState Card;
        public string TargetId;
    }
}