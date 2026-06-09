using System.Collections.Generic;
using System.Linq;
using Core.GameLoop;
using Core.Save;
using Core.ServiceLocator;
using CoreGame.Card.Data;
using CoreGame.Card.Logic;
using CoreGame.Entities.Characters.Hero;
using Cysharp.Threading.Tasks;
using FishNet;
using FishNet.Connection;
using UnityEngine;

namespace CoreGame.InteractionObjects.Activators
{
    public class MultiplayerBattleActivator : ActivatorObject, IInitializeListener
    {
        public bool IsInitialized { get; set; }

        [SerializeField] private EBattleMode _mode = EBattleMode.PvP;
        [SerializeField] private HeroModel _aiDefenderModel;
        [SerializeField] private EEnemyAIDifficulty _enemyDifficulty = EEnemyAIDifficulty.Normal;
        [SerializeField] private EnemyDeckProfile _enemyDeckProfile;
        [SerializeField] private float _participantSearchRadius = 8f;

        private NetworkBattleService _networkBattleService;
        private HeroSpawner _heroSpawner;

        public UniTask Initialize()
        {
            _networkBattleService = Container.Instance.GetService<NetworkBattleService>();
            _heroSpawner = Container.Instance.GetService<HeroSpawner>();

            return UniTask.CompletedTask;
        }

        public override void StartInteraction()
        {
            base.StartInteraction();

            if (!InstanceFinder.IsServerStarted || _networkBattleService.IsNetworkBattle)
            {
                return;
            }

            List<(NetworkConnection connection, Hero hero)> participants = _collectParticipants();
            if (participants.Any(pair => pair.hero.Model.InBattle))
            {
                return;
            }
            if (participants.Count < 2)
            {
                Debug.LogWarning("[MultiplayerBattleActivator] Not enough players nearby to start multiplayer battle.");
                return;
            }

            Dictionary<string, NetworkConnection> participantConnections = participants
                .Where(pair => !string.IsNullOrEmpty(pair.hero.Model.HeroId))
                .ToDictionary(pair => pair.hero.Model.HeroId, pair => pair.connection);

            if (_mode == EBattleMode.CoOpPvE)
            {
                if (_aiDefenderModel == null)
                {
                    Debug.LogWarning("[MultiplayerBattleActivator] AI defender model is not configured.");
                    return;
                }

                _networkBattleService.ServerStartBattle(
                    participants[0].hero.Model,
                    _aiDefenderModel,
                    participants[1].hero.Model,
                    EBattleMode.CoOpPvE,
                    _enemyDifficulty,
                    _enemyDeckProfile,
                    participantConnections);
                return;
            }

            _networkBattleService.ServerStartBattle(
                participants[0].hero.Model,
                participants[1].hero.Model,
                null,
                EBattleMode.PvP,
                _enemyDifficulty,
                null,
                participantConnections);
        }

        private List<(NetworkConnection connection, Hero hero)> _collectParticipants()
        {
            List<(NetworkConnection connection, Hero hero)> participants = new();
            Vector3 origin = transform.position;

            foreach ((NetworkConnection connection, Hero hero) pair in _heroSpawner.GetAllHeroes())
            {
                if (pair.hero == null || pair.hero.Model == null)
                {
                    continue;
                }

                float distance = Vector3.Distance(origin, pair.hero.transform.position);
                if (distance > _participantSearchRadius)
                {
                    continue;
                }

                participants.Add(pair);
            }

            return participants
                .OrderBy(pair => Vector3.Distance(pair.hero.transform.position, origin))
                .ThenBy(pair => pair.hero.Model.HeroId)
                .ToList();
        }
    }
}
