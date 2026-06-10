using System;
using System.Collections.Generic;
using System.Linq;
using Core.GameLoop;
using Core.Save;
using Core.ServiceLocator;
using CoreGame.Card;
using CoreGame.Card;
using CoreGame.Card.Data;
using CoreGame.Card.Logic.Network;
using CoreGame.Card.Logic.StateMachine;
using Cysharp.Threading.Tasks;
using FishNet;
using FishNet.Broadcast;
using FishNet.Connection;
using UnityEngine;
using Channel = FishNet.Transporting.Channel;

namespace CoreGame.Card.Logic
{
    public class NetworkBattleService : IService, IInitializeListener, ISubscriber
    {
        public bool IsInitialized { get; set; }
        public bool IsRemoteClient => InstanceFinder.IsClientStarted && !InstanceFinder.IsServerStarted;

        private BattleService _battleService;
        private BattleStateMachine _stateMachine;
        private CardLibrary _cardLibrary;

        private readonly Dictionary<string, BattleLobby> _lobbies = new();
        private readonly Dictionary<string, NetworkConnection> _participants = new();

        public UniTask Initialize()
        {
            _battleService = Container.Instance.GetService<BattleService>();
            _stateMachine = Container.Instance.GetService<BattleStateMachine>();
            _cardLibrary = Container.Instance.GetSO<CardLibrary>();
            return UniTask.CompletedTask;
        }

        public void Subscribe()
        {
            if (InstanceFinder.ServerManager != null)
            {
                InstanceFinder.ServerManager.RegisterBroadcast<BattleJoinRequestBroadcast>(_onJoinRequest);
                InstanceFinder.ServerManager.RegisterBroadcast<BattleActionRequestBroadcast>(_onActionRequest);
            }

            if (InstanceFinder.ClientManager != null)
            {
                InstanceFinder.ClientManager.RegisterBroadcast<BattleStateSyncBroadcast>(_onStateSync);
                InstanceFinder.ClientManager.RegisterBroadcast<BattleHandSyncBroadcast>(_onHandSync);
                InstanceFinder.ClientManager.RegisterBroadcast<BattleLobbyUpdateBroadcast>(_onLobbyUpdate);
            }
        }

        public void Unsubscribe()
        {
            InstanceFinder.ServerManager?.UnregisterBroadcast<BattleJoinRequestBroadcast>(_onJoinRequest);
            InstanceFinder.ServerManager?.UnregisterBroadcast<BattleActionRequestBroadcast>(_onActionRequest);
            InstanceFinder.ClientManager?.UnregisterBroadcast<BattleStateSyncBroadcast>(_onStateSync);
            InstanceFinder.ClientManager?.UnregisterBroadcast<BattleHandSyncBroadcast>(_onHandSync);
            InstanceFinder.ClientManager?.UnregisterBroadcast<BattleLobbyUpdateBroadcast>(_onLobbyUpdate);
        }

        public bool ShouldDelegateStart => InstanceFinder.IsClientStarted;

        public void RequestJoinBattle(
            string activatorId,
            EBattleMode mode,
            BattleHeroPayload heroPayload,
            BattleHeroPayload aiHeroPayload,
            EEnemyAIDifficulty enemyDifficulty,
            int requiredPlayers)
        {
            if (!InstanceFinder.IsClientStarted)
            {
                return;
            }

            InstanceFinder.ClientManager.Broadcast(new BattleJoinRequestBroadcast
            {
                ActivatorId = activatorId,
                Mode = mode,
                Hero = heroPayload,
                AiHero = aiHeroPayload,
                EnemyDifficulty = enemyDifficulty,
                RequiredPlayers = requiredPlayers
            });
        }

        public CommandResult SendPlayCard(string cardId, string targetId, string requesterUnitId)
        {
            return _sendAction(new BattleActionRequestBroadcast
            {
                BattleId = _stateMachine.Model?.BattleId,
                RequesterUnitId = requesterUnitId,
                Action = EBattleNetworkAction.PlayCard,
                CardId = cardId,
                TargetId = targetId
            });
        }

        public CommandResult SendEndTurn(string requesterUnitId)
        {
            return _sendAction(new BattleActionRequestBroadcast
            {
                BattleId = _stateMachine.Model?.BattleId,
                RequesterUnitId = requesterUnitId,
                Action = EBattleNetworkAction.EndTurn
            });
        }

        public CommandResult SendMoveToCell(string cardId, string unitId, EBattleLine line, int cellIndex, string requesterUnitId)
        {
            return _sendAction(new BattleActionRequestBroadcast
            {
                BattleId = _stateMachine.Model?.BattleId,
                RequesterUnitId = requesterUnitId,
                Action = EBattleNetworkAction.MoveToCell,
                CardId = cardId,
                UnitId = unitId,
                Line = line,
                CellIndex = cellIndex
            });
        }

        public CommandResult SendSummonToCell(string cardId, EBattleLine line, int cellIndex, string requesterUnitId)
        {
            return _sendAction(new BattleActionRequestBroadcast
            {
                BattleId = _stateMachine.Model?.BattleId,
                RequesterUnitId = requesterUnitId,
                Action = EBattleNetworkAction.SummonToCell,
                CardId = cardId,
                Line = line,
                CellIndex = cellIndex
            });
        }

        private CommandResult _sendAction(BattleActionRequestBroadcast request)
        {
            if (!InstanceFinder.IsClientStarted)
            {
                return CommandResult.InvalidState;
            }

            if (IsRemoteClient)
            {
                InstanceFinder.ClientManager.Broadcast(request);
                return CommandResult.Success;
            }

            return CommandResult.InvalidState;
        }

        private void _onJoinRequest(NetworkConnection connection, BattleJoinRequestBroadcast request, Channel channel)
        {
            if (!InstanceFinder.IsServerStarted || string.IsNullOrEmpty(request.ActivatorId))
            {
                return;
            }

            if (!_lobbies.TryGetValue(request.ActivatorId, out BattleLobby lobby))
            {
                lobby = new BattleLobby
                {
                    ActivatorId = request.ActivatorId,
                    Mode = request.Mode,
                    RequiredPlayers = Mathf.Max(1, request.RequiredPlayers),
                    EnemyDifficulty = request.EnemyDifficulty,
                    AiHero = request.AiHero
                };
                _lobbies[request.ActivatorId] = lobby;
            }

            if (lobby.Players.Any(player => player.Connection == connection))
            {
                return;
            }

            lobby.Players.Add(new BattleLobbyPlayer
            {
                Connection = connection,
                Hero = request.Hero
            });

            _broadcastLobbyUpdate(lobby);

            if (lobby.Players.Count < lobby.RequiredPlayers)
            {
                return;
            }

            _startNetworkBattle(lobby);
            _lobbies.Remove(request.ActivatorId);
        }

        private void _startNetworkBattle(BattleLobby lobby)
        {
            _participants.Clear();

            BattleLobbyPlayer first = lobby.Players[0];
            HeroModel playerOne = first.Hero.ToHeroModel();

            switch (lobby.Mode)
            {
                case EBattleMode.CoOpPvE when lobby.Players.Count >= 2:
                {
                    BattleLobbyPlayer second = lobby.Players[1];
                    HeroModel playerTwo = second.Hero.ToHeroModel();
                    HeroModel aiHero = lobby.AiHero.ToHeroModel();

                    _participants[playerOne.HeroId] = first.Connection;
                    _participants[playerTwo.HeroId] = second.Connection;

                    _battleService.StartCoOpBattleInternal(
                        playerOne,
                        playerTwo,
                        aiHero,
                        lobby.EnemyDifficulty,
                        null,
                        playerOne.HeroId,
                        playerTwo.HeroId);
                    break;
                }
                case EBattleMode.PvP:
                case EBattleMode.Duel when lobby.Players.Count >= 2:
                {
                    BattleLobbyPlayer second = lobby.Players[1];
                    HeroModel playerTwo = second.Hero.ToHeroModel();

                    _participants[playerOne.HeroId] = first.Connection;
                    _participants[playerTwo.HeroId] = second.Connection;

                    _battleService.StartBattleInternal(
                        playerOne,
                        playerTwo,
                        EBattleMode.PvP,
                        EEnemyAIDifficulty.Normal,
                        null,
                        playerOne.HeroId,
                        playerTwo.HeroId);
                    break;
                }
                default:
                {
                    HeroModel aiHero = lobby.AiHero.ToHeroModel();
                    _participants[playerOne.HeroId] = first.Connection;

                    _battleService.StartBattleInternal(
                        playerOne,
                        aiHero,
                        EBattleMode.PvE,
                        lobby.EnemyDifficulty,
                        null,
                        playerOne.HeroId,
                        null);
                    break;
                }
            }

            _syncBattleState(isBattleStarted: true);
        }

        private void _onActionRequest(NetworkConnection connection, BattleActionRequestBroadcast request, Channel channel)
        {
            if (!InstanceFinder.IsServerStarted || _stateMachine.Model == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(request.BattleId)
                && request.BattleId != _stateMachine.Model.BattleId)
            {
                return;
            }

            if (!_participants.TryGetValue(request.RequesterUnitId, out NetworkConnection ownerConnection)
                || ownerConnection != connection)
            {
                return;
            }

            if (!BattleParticipantHelper.IsMyTurn(_stateMachine.Model, request.RequesterUnitId))
            {
                return;
            }

            CommandResult result = request.Action switch
            {
                EBattleNetworkAction.PlayCard => _battleService.TryPlayCardWithResult(request.CardId, request.TargetId),
                EBattleNetworkAction.EndTurn => _battleService.EndTurnWithResult(),
                EBattleNetworkAction.MoveToCell => _battleService.TryPlayMoveCardToCellWithResult(
                    request.CardId, request.UnitId, request.Line, request.CellIndex),
                EBattleNetworkAction.SummonToCell => _battleService.TryPlaySummonCardToCellWithResult(
                    request.CardId, request.Line, request.CellIndex),
                _ => CommandResult.InvalidState
            };

            if (result == CommandResult.Success)
            {
                _syncBattleState(isCardPlayed: true);
            }
        }

        public void SyncFromServer(bool isBattleStarted = false, bool isTurnStarted = false, bool isCardPlayed = false, bool isBattleFinished = false)
        {
            if (!InstanceFinder.IsServerStarted)
            {
                return;
            }

            _syncBattleState(isBattleStarted, isTurnStarted, isCardPlayed, isBattleFinished);
        }

        private void _syncBattleState(
            bool isBattleStarted = false,
            bool isTurnStarted = false,
            bool isCardPlayed = false,
            bool isBattleFinished = false)
        {
            if (!InstanceFinder.IsServerStarted || _stateMachine.Model == null)
            {
                return;
            }

            string snapshotJson = BattleSnapshotSerializer.Serialize(_stateMachine.Model);
            InstanceFinder.ServerManager.Broadcast(new BattleStateSyncBroadcast
            {
                SnapshotJson = snapshotJson,
                IsBattleStarted = isBattleStarted,
                IsBattleFinished = isBattleFinished,
                IsTurnStarted = isTurnStarted || isBattleStarted,
                IsCardPlayed = isCardPlayed
            });

            foreach (KeyValuePair<string, NetworkConnection> participant in _participants)
            {
                BattleSide side = BattleParticipantHelper.GetMySide(_stateMachine.Model, participant.Key);
                List<CardBattleState> visibleHand = side?.GetVisibleHand(participant.Key) ?? new List<CardBattleState>();

                InstanceFinder.ServerManager.Broadcast(participant.Value, new BattleHandSyncBroadcast
                {
                    BattleId = _stateMachine.Model.BattleId,
                    PlayerUnitId = participant.Key,
                    HandJson = BattleSnapshotSerializer.SerializeHand(visibleHand)
                });
            }
        }

        private void _onStateSync(BattleStateSyncBroadcast broadcast, Channel channel)
        {
            if (InstanceFinder.IsServerStarted)
            {
                return;
            }

            BattleModel model = BattleSnapshotSerializer.Deserialize(broadcast.SnapshotJson, _cardLibrary);
            if (model == null)
            {
                return;
            }

            _battleService.ApplyNetworkSnapshot(model, broadcast);
        }

        private void _onHandSync(BattleHandSyncBroadcast broadcast, Channel channel)
        {
            if (InstanceFinder.IsServerStarted || _stateMachine.Model == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(broadcast.BattleId)
                && broadcast.BattleId != _stateMachine.Model.BattleId)
            {
                return;
            }

            List<CardBattleState> hand = BattleSnapshotSerializer.DeserializeHand(broadcast.HandJson, _cardLibrary);
            BattleSnapshotSerializer.ApplyPrivateHand(_stateMachine.Model, broadcast.PlayerUnitId, hand);
            _battleService.NotifyHandUpdated();
        }

        private void _broadcastLobbyUpdate(BattleLobby lobby)
        {
            InstanceFinder.ServerManager.Broadcast(new BattleLobbyUpdateBroadcast
            {
                ActivatorId = lobby.ActivatorId,
                PlayersWaiting = lobby.Players.Count,
                PlayersRequired = lobby.RequiredPlayers
            });
        }

        private void _onLobbyUpdate(BattleLobbyUpdateBroadcast broadcast, Channel channel)
        {
            Debug.Log($"Battle lobby {broadcast.ActivatorId}: {broadcast.PlayersWaiting}/{broadcast.PlayersRequired} players");
        }

        private sealed class BattleLobby
        {
            public string ActivatorId;
            public EBattleMode Mode;
            public int RequiredPlayers;
            public EEnemyAIDifficulty EnemyDifficulty;
            public BattleHeroPayload AiHero;
            public List<BattleLobbyPlayer> Players = new();
        }

        private sealed class BattleLobbyPlayer
        {
            public NetworkConnection Connection;
            public BattleHeroPayload Hero;
        }
    }
}
