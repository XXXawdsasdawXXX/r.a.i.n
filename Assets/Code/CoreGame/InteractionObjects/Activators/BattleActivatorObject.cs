using Core.GameLoop;
using Core.Network;
using Core.Save;
using Core.ServiceLocator;
using CoreGame.Card.Data;
using CoreGame.Card.Logic;
using CoreGame.Card.Logic.Network;
using CoreGame.Entities.Characters.Hero;
using Cysharp.Threading.Tasks;
using FishNet;
using UnityEngine;

namespace CoreGame.InteractionObjects.Activators
{
    public class BattleActivatorObject : ActivatorObject, IInitializeListener
    {
        public bool IsInitialized { get; set; }
        
        [SerializeField] private HeroModel _model;
        [SerializeField] private EBattleMode _battleMode = EBattleMode.PvE;
        [SerializeField] private int _requiredPlayers = 1;
        [SerializeField] private EEnemyAIDifficulty _enemyDifficulty = EEnemyAIDifficulty.Normal;
        [SerializeField] private EnemyDeckProfile _enemyDeckProfile;
        
        private BattleService _battleService;
        private NetworkBattleService _networkBattleService;
        private UserProvider _userProvider;
        

        public UniTask Initialize()
        {
            _battleService = Container.Instance.GetService<BattleService>();
            _networkBattleService = Container.Instance.GetService<NetworkBattleService>();
            _userProvider = Container.Instance.GetService<UserProvider>();
            
            return UniTask.CompletedTask;
        }
        
        public override void StartInteraction()
        {
            base.StartInteraction();

            Hero hero = _userProvider.GetHeroComponent<Hero>();
            if (hero?.Model == null)
            {
                return;
            }

            if (_networkBattleService != null && InstanceFinder.IsClientStarted)
            {
                _networkBattleService.RequestJoinBattle(
                    gameObject.GetInstanceID().ToString(),
                    _battleMode,
                    BattleHeroPayload.FromHeroModel(hero.Model),
                    BattleHeroPayload.FromHeroModel(_model),
                    _enemyDifficulty,
                    _resolveRequiredPlayers());
                return;
            }

            _startLocalBattle(hero.Model);
        }

        private int _resolveRequiredPlayers()
        {
            if (_requiredPlayers > 0)
            {
                return _requiredPlayers;
            }

            return _battleMode switch
            {
                EBattleMode.CoOpPvE => 2,
                EBattleMode.PvP => 2,
                EBattleMode.Duel => 2,
                _ => 1
            };
        }

        private void _startLocalBattle(HeroModel playerHero)
        {
            if (_battleMode == EBattleMode.CoOpPvE)
            {
                Debug.LogWarning("Co-op battles require a second player when playing online.");
                return;
            }

            _battleService.StartBattle(
                playerHero,
                _model,
                _battleMode,
                _enemyDifficulty,
                _enemyDeckProfile);
        }
    }
}
