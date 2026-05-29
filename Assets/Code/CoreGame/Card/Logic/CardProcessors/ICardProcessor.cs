using CoreGame.Card.Data;

namespace CoreGame.Card.Logic.CardProcessors
{
    public interface ICardProcessor
    {
        void Process(CardEffectConfiguration effect, BattleUnit actor,BattleUnit target,BattleModel battle);
    }
}