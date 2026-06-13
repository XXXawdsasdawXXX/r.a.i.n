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

namespace CoreGame.Entities.InteractionObjects.Activators
{
    public class BattleActivatorObject : ActivatorObject, IInitializeListener
    {
        public bool IsInitialized { get; set; }
        
        [SerializeField] private HeroModel _model;
        [Tooltip("Уникальный id активатора. Должен совпадать у всех клиентов. Не используйте GetInstanceID.")]
        [SerializeField] private string _activatorKey;
        [SerializeField] private EBattleMode _battleMode = EBattleMode.PvE;
        [Tooltip("Максимальный размер команды. 0 = авто по режиму (Co-op/PvP: 2, PvE: 1).")]
        [SerializeField] private int _maxPlayers;
        [Tooltip("Минимум игроков для старта. 0 = авто (Co-op/PvE: 1, PvP/Duel: 2).")]
        [SerializeField] private int _minPlayers;
        [Tooltip("Устаревшее поле — используется как MaxPlayers, если MaxPlayers = 0.")]
        [SerializeField] private int _requiredPlayers = 1;
        [SerializeField] private bool _allowEarlyStart = true;
        [SerializeField] private bool _autoStartWhenFull;
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
                BattleHeroPayload heroPayload = BattleHeroPayload.FromHeroModel(hero.Model);
                if (string.IsNullOrEmpty(heroPayload.HeroId) && !string.IsNullOrEmpty(_userProvider.Id))
                {
                    heroPayload.HeroId = _userProvider.Id;
                }

                _networkBattleService.RequestJoinBattle(
                    _getActivatorId(),
                    _battleMode,
                    heroPayload,
                    BattleHeroPayload.FromHeroModel(_model),
                    _enemyDifficulty,
                    _resolveMinPlayers(),
                    _resolveMaxPlayers(),
                    _allowEarlyStart,
                    _autoStartWhenFull);
                return;
            }

            _startLocalBattle(hero.Model);
        }

        private int _resolveMaxPlayers()
        {
            if (_maxPlayers > 0)
            {
                return _maxPlayers;
            }

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

        private int _resolveMinPlayers()
        {
            if (_minPlayers > 0)
            {
                return _minPlayers;
            }

            return _battleMode switch
            {
                EBattleMode.CoOpPvE => 1,
                EBattleMode.PvP => 2,
                EBattleMode.Duel => 2,
                _ => 1
            };
        }

        private string _getActivatorId()
        {
            if (!string.IsNullOrEmpty(_activatorKey))
            {
                return _activatorKey;
            }

            return $"{gameObject.scene.name}/{gameObject.name}";
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
