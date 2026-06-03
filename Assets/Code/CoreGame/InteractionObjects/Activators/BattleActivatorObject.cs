using Core.GameLoop;
using Core.Network;
using Core.Save;
using Core.ServiceLocator;
using CoreGame.Card.Logic;
using CoreGame.Entities.Characters.Hero;
using Cysharp.Threading.Tasks;
using Essential;
using UnityEngine;

namespace CoreGame.InteractionObjects.Activators
{
    public class BattleActivatorObject : ActivatorObject, IInitializeListener
    {
        public bool IsInitialized { get; set; }
        
        [SerializeField] private HeroModel _model;
        
        private BattleService _battleService;
        private UserProvider _userProvider;
        

        public UniTask Initialize()
        {
            _battleService = Container.Instance.GetService<BattleService>();
            _userProvider = Container.Instance.GetService<UserProvider>();
            
            return UniTask.CompletedTask;
        }
        
        public override void StartInteraction()
        {
            base.StartInteraction();

            Log.Info(this, "start interaction");
            _battleService.StartBattle(_userProvider.GetHeroComponent<Hero>().Model, _model);
        }
    }
}