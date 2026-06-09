using Core.GameLoop;
using Core.Network;
using Core.Save;
using Core.ServiceLocator;
using CoreGame.Card.Logic;
using CoreGame.Card.Data;
using CoreGame.Entities.Characters.Hero;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CoreGame.InteractionObjects.Activators
{
    public class BattleActivatorObject : ActivatorObject, IInitializeListener
    {
        public bool IsInitialized { get; set; }
        
        [SerializeField] private HeroModel _model;
        [SerializeField] private EEnemyAIDifficulty _enemyDifficulty = EEnemyAIDifficulty.Normal;
        [SerializeField] private EnemyDeckProfile _enemyDeckProfile;
        
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

            _battleService.StartBattle(
                _userProvider.GetHeroComponent<Hero>().Model,
                _model,
                EBattleMode.PvE,
                _enemyDifficulty,
                _enemyDeckProfile);
        }
    }
}