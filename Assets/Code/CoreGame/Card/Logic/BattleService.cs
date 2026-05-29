using Core.ServiceLocator;

namespace CoreGame.Card.Logic
{
    public class BattleService : IService
    {
        private BattleProcessor _processor;
        private BattleValidator _validator;
    }
}