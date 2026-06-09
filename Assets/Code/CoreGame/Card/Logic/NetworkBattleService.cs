using System;
using System.Collections.Generic;
using Core.GameLoop;
using Core.Save;
using Core.ServiceLocator;
using CoreGame.Card.Data;
using CoreGame.Card.Logic.StateMachine;
using Cysharp.Threading.Tasks;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

namespace CoreGame.Card.Logic
{
    [Serializable]
    internal class BattleSyncJsonWrapper
    {
        public BattleSyncData Data;
    }

    public class NetworkBattleService : NetworkBehaviour, IService, IInitializeListener, ISubscriber
    {
        public bool IsInitialized { get; set; }
        public bool IsNetworkBattle { get; private set; }

        private BattleService _battleService;
        private BattleStateMachine _machine;
        private CardLibrary _cardLibrary;
        private readonly Dictionary<string, NetworkConnection> _participants = new();

        public UniTask Initialize()
        {
            _battleService = Container.Instance.GetService<BattleService>();
            _machine = Container.Instance.GetService<BattleStateMachine>();
            _cardLibrary = Container.Instance.GetSO<CardLibrary>();
            _battleService.SetNetworkBridge(this);

            return UniTask.CompletedTask;
        }

        public void Subscribe()
        {
            _battleService.BattleStarted += _onBattleEvent;
            _battleService.TurnStarted += _onBattleEvent;
            _battleService.CardPlayed += _onBattleEvent;
            _battleService.BattleFinished += _onBattleFinished;
        }

        public void Unsubscribe()
        {
            _battleService.BattleStarted -= _onBattleEvent;
            _battleService.TurnStarted -= _onBattleEvent;
            _battleService.CardPlayed -= _onBattleEvent;
            _battleService.BattleFinished -= _onBattleFinished;
        }

        public bool IsMultiplayerMode(EBattleMode mode)
        {
            return mode is EBattleMode.PvP or EBattleMode.CoOpPvE or EBattleMode.Duel;
        }

        public bool ShouldHandleLocally(EBattleMode mode)
        {
            if (!IsMultiplayerMode(mode))
            {
                return true;
            }

            return !InstanceFinder.IsServerStarted;
        }

        public void SetParticipants(IReadOnlyDictionary<string, NetworkConnection> participants)
        {
            _participants.Clear();

            if (participants == null)
            {
                return;
            }

            foreach (KeyValuePair<string, NetworkConnection> pair in participants)
            {
                _participants[pair.Key] = pair.Value;
            }
        }

        public void ServerStartBattle(
            HeroModel sideAHero,
            HeroModel sideBHero,
            HeroModel allyHero,
            EBattleMode mode,
            EEnemyAIDifficulty enemyDifficulty,
            EnemyDeckProfile enemyDeckProfile,
            IReadOnlyDictionary<string, NetworkConnection> participants)
        {
            if (!IsServerStarted)
            {
                return;
            }

            SetParticipants(participants);
            IsNetworkBattle = true;
            _battleService.StartBattleLocal(sideAHero, sideBHero, allyHero, mode, enemyDifficulty, enemyDeckProfile);
            _broadcastSnapshots();
        }

        public void RequestPlayCard(string cardId, string targetId, string requesterHeroId)
        {
            if (!IsNetworkBattle || IsServerStarted)
            {
                return;
            }

            ServerPlayCard(cardId, targetId, requesterHeroId);
        }

        public void RequestPlayMoveCard(string cardId, string unitId, EBattleLine line, int cellIndex, string requesterHeroId)
        {
            if (!IsNetworkBattle || IsServerStarted)
            {
                return;
            }

            ServerPlayMoveCard(cardId, unitId, line, cellIndex, requesterHeroId);
        }

        public void RequestPlaySummonCard(string cardId, EBattleLine line, int cellIndex, string requesterHeroId)
        {
            if (!IsNetworkBattle || IsServerStarted)
            {
                return;
            }

            ServerPlaySummonCard(cardId, line, cellIndex, requesterHeroId);
        }

        public void RequestEndTurn(string requesterHeroId)
        {
            if (!IsNetworkBattle || IsServerStarted)
            {
                return;
            }

            ServerEndTurn(requesterHeroId);
        }

        [ServerRpc(RequireOwnership = false)]
        private void ServerPlayCard(string cardId, string targetId, string requesterHeroId)
        {
            if (!_canAct(requesterHeroId))
            {
                return;
            }

            _battleService.TryPlayCardWithResult(cardId, targetId, requesterHeroId);
            _broadcastSnapshots();
        }

        [ServerRpc(RequireOwnership = false)]
        private void ServerPlayMoveCard(string cardId, string unitId, EBattleLine line, int cellIndex, string requesterHeroId)
        {
            if (!_canAct(requesterHeroId))
            {
                return;
            }

            _battleService.TryPlayMoveCardToCellWithResult(cardId, unitId, line, cellIndex, requesterHeroId);
            _broadcastSnapshots();
        }

        [ServerRpc(RequireOwnership = false)]
        private void ServerPlaySummonCard(string cardId, EBattleLine line, int cellIndex, string requesterHeroId)
        {
            if (!_canAct(requesterHeroId))
            {
                return;
            }

            _battleService.TryPlaySummonCardToCellWithResult(cardId, line, cellIndex, requesterHeroId);
            _broadcastSnapshots();
        }

        [ServerRpc(RequireOwnership = false)]
        private void ServerEndTurn(string requesterHeroId)
        {
            if (!_canAct(requesterHeroId))
            {
                return;
            }

            _battleService.EndTurnWithResult(requesterHeroId);
            _broadcastSnapshots();
        }

        public void ApplyClientSnapshot(BattleSyncData data, string viewerHeroId)
        {
            if (IsServerStarted)
            {
                return;
            }

            _machine.EnsureClientModelShell();

            EBattlePhase previousPhase = _machine.Model.Phase.Value;
            BattleSyncSerializer.ApplySnapshot(_machine.Model, data, viewerHeroId, _cardLibrary);

            if (previousPhase == EBattlePhase.WaitingBattle && data.Phase != EBattlePhase.WaitingBattle)
            {
                _battleService.NotifyClientBattleStarted();
            }

            _battleService.NotifyClientSynced();
        }

        private void _onBattleEvent(BattleModel _)
        {
            if (!IsServerStarted || !IsNetworkBattle)
            {
                return;
            }

            _broadcastSnapshots();
        }

        private void _onBattleFinished(BattleModel _)
        {
            if (!IsServerStarted)
            {
                return;
            }

            IsNetworkBattle = false;
            _participants.Clear();
            _broadcastSnapshots();
        }

        private void _broadcastSnapshots()
        {
            if (_machine?.Model == null)
            {
                return;
            }

            foreach (KeyValuePair<string, NetworkConnection> participant in _participants)
            {
                if (participant.Value == null || !participant.Value.IsActive)
                {
                    continue;
                }

                BattleSyncData snapshot = BattleSyncSerializer.CreateSnapshot(
                    _machine.Model,
                    participant.Key,
                    _cardLibrary);
                string json = JsonUtility.ToJson(new BattleSyncJsonWrapper { Data = snapshot });
                TargetSyncBattle(participant.Value, json, participant.Key);
            }
        }

        [TargetRpc]
        private void TargetSyncBattle(NetworkConnection connection, string json, string viewerHeroId)
        {
            if (string.IsNullOrEmpty(json))
            {
                return;
            }

            BattleSyncJsonWrapper wrapper = JsonUtility.FromJson<BattleSyncJsonWrapper>(json);
            ApplyClientSnapshot(wrapper.Data, viewerHeroId);
        }

        private bool _canAct(string requesterHeroId)
        {
            return IsNetworkBattle
                   && _machine?.Model != null
                   && BattleParticipantResolver.IsMyTurn(_machine.Model, requesterHeroId);
        }
    }
}
