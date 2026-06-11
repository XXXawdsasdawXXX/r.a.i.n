using System;
using System.Collections.Generic;
using System.Linq;
using Core.GameLoop;
using Core.Save;
using Core.ServiceLocator;
using CoreGame.Card.Data;
using CoreGame.Card.Logic.Network;
using CoreGame.Card.Logic.StateMachine;
using Cysharp.Threading.Tasks;
using FishNet;
using FishNet.Connection;
using UnityEngine;
using Channel = FishNet.Transporting.Channel;

namespace CoreGame.Card.Logic
{
    public class NetworkBattleService : IService, IInitializeListener, ISubscriber
    {
        public bool IsInitialized { get; set; }
        public bool IsRemoteClient => InstanceFinder.IsClientStarted && !InstanceFinder.IsServerStarted;

        public event Action<BattleLobbyState> LobbyStateChanged;

        private BattleService _battleService;
        private BattleStateMachine _stateMachine;
        private CardLibrary _cardLibrary;

        private readonly Dictionary<string, BattleLobby> _lobbies = new();
        private readonly Dictionary<string, NetworkConnection> _participants = new();
        private string _localLobbyActivatorId;

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
                InstanceFinder.ServerManager.RegisterBroadcast<BattleLobbyActionRequestBroadcast>(_onLobbyActionRequest);
                InstanceFinder.ServerManager.RegisterBroadcast<BattleActionRequestBroadcast>(_onActionRequest);
                InstanceFinder.ServerManager.OnRemoteConnectionState += _onRemoteConnectionState;
            }

            if (InstanceFinder.ClientManager != null)
            {
                InstanceFinder.ClientManager.RegisterBroadcast<BattleStateSyncBroadcast>(_onStateSync);
                InstanceFinder.ClientManager.RegisterBroadcast<BattleHandSyncBroadcast>(_onHandSync);
                InstanceFinder.ClientManager.RegisterBroadcast<BattleLobbyUpdateBroadcast>(_onLobbyUpdate);
                InstanceFinder.ClientManager.RegisterBroadcast<BattleActionResultBroadcast>(_onActionResult);
            }
        }

        public void Unsubscribe()
        {
            if (InstanceFinder.ServerManager != null)
            {
                InstanceFinder.ServerManager.UnregisterBroadcast<BattleJoinRequestBroadcast>(_onJoinRequest);
                InstanceFinder.ServerManager.UnregisterBroadcast<BattleLobbyActionRequestBroadcast>(_onLobbyActionRequest);
                InstanceFinder.ServerManager.UnregisterBroadcast<BattleActionRequestBroadcast>(_onActionRequest);
                InstanceFinder.ServerManager.OnRemoteConnectionState -= _onRemoteConnectionState;
            }

            if (InstanceFinder.ClientManager != null)
            {
                InstanceFinder.ClientManager.UnregisterBroadcast<BattleStateSyncBroadcast>(_onStateSync);
                InstanceFinder.ClientManager.UnregisterBroadcast<BattleHandSyncBroadcast>(_onHandSync);
                InstanceFinder.ClientManager.UnregisterBroadcast<BattleLobbyUpdateBroadcast>(_onLobbyUpdate);
                InstanceFinder.ClientManager.UnregisterBroadcast<BattleActionResultBroadcast>(_onActionResult);
            }
        }

        public void RequestJoinBattle(
            string activatorId,
            EBattleMode mode,
            BattleHeroPayload heroPayload,
            BattleHeroPayload aiHeroPayload,
            EEnemyAIDifficulty enemyDifficulty,
            int minPlayers,
            int maxPlayers,
            bool allowEarlyStart,
            bool autoStartWhenFull)
        {
            if (!InstanceFinder.IsClientStarted)
            {
                return;
            }

            _localLobbyActivatorId = activatorId;

            InstanceFinder.ClientManager.Broadcast(new BattleJoinRequestBroadcast
            {
                ActivatorId = activatorId,
                Mode = mode,
                Hero = heroPayload,
                AiHero = aiHeroPayload,
                EnemyDifficulty = enemyDifficulty,
                MinPlayers = minPlayers,
                MaxPlayers = maxPlayers,
                AllowEarlyStart = allowEarlyStart,
                AutoStartWhenFull = autoStartWhenFull
            });
        }

        public void RequestLeaveLobby()
        {
            if (string.IsNullOrEmpty(_localLobbyActivatorId) || !InstanceFinder.IsClientStarted)
            {
                return;
            }

            InstanceFinder.ClientManager.Broadcast(new BattleLobbyActionRequestBroadcast
            {
                ActivatorId = _localLobbyActivatorId,
                Action = EBattleLobbyAction.Leave
            });
        }

        public void RequestStartLobby()
        {
            if (string.IsNullOrEmpty(_localLobbyActivatorId) || !InstanceFinder.IsClientStarted)
            {
                return;
            }

            InstanceFinder.ClientManager.Broadcast(new BattleLobbyActionRequestBroadcast
            {
                ActivatorId = _localLobbyActivatorId,
                Action = EBattleLobbyAction.Start
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

        public void StartDirectDuel(
            NetworkConnection challengerConnection,
            BattleHeroPayload challengerHero,
            NetworkConnection targetConnection,
            BattleHeroPayload targetHero)
        {
            if (!InstanceFinder.IsServerStarted || _stateMachine.HasActiveBattle)
            {
                return;
            }

            _participants.Clear();
            _participants[challengerHero.HeroId] = challengerConnection;
            _participants[targetHero.HeroId] = targetConnection;

            _battleService.StartBattleInternal(
                challengerHero.ToHeroModel(),
                targetHero.ToHeroModel(),
                EBattleMode.Duel,
                EEnemyAIDifficulty.Normal,
                null,
                challengerHero.HeroId,
                targetHero.HeroId);

            _syncBattleState(isBattleStarted: true);
        }

        private void _startSoloActivatorBattle(NetworkConnection connection, BattleJoinRequestBroadcast request)
        {
            _participants.Clear();
            _participants[request.Hero.HeroId] = connection;

            HeroModel playerHero = request.Hero.ToHeroModel();
            HeroModel aiHero = request.AiHero.ToHeroModel();

            if (request.Mode == EBattleMode.CoOpPvE)
            {
                Debug.Log("Only one player online — starting activator battle as PvE.");
            }

            _battleService.StartBattleInternal(
                playerHero,
                aiHero,
                EBattleMode.PvE,
                request.EnemyDifficulty,
                null,
                request.Hero.HeroId,
                null);

            _syncBattleState(isBattleStarted: true);
        }

        private static int _getOnlinePlayerCount()
        {
            if (!InstanceFinder.IsServerStarted)
            {
                return InstanceFinder.IsClientStarted ? 1 : 0;
            }

            return InstanceFinder.ServerManager.Clients.Count;
        }

        private static bool _shouldShowLobby()
        {
            return _getOnlinePlayerCount() >= 2;
        }

        private void _onJoinRequest(NetworkConnection connection, BattleJoinRequestBroadcast request, Channel channel)
        {
            if (!InstanceFinder.IsServerStarted || string.IsNullOrEmpty(request.ActivatorId))
            {
                return;
            }

            if (_stateMachine.HasActiveBattle)
            {
                Debug.LogWarning("Battle join rejected: a battle is already in progress.");
                return;
            }

            if (string.IsNullOrEmpty(request.Hero.HeroId))
            {
                Debug.LogWarning("Battle join rejected: hero id is missing in payload.");
                return;
            }

            if (_getOnlinePlayerCount() < 2)
            {
                _startSoloActivatorBattle(connection, request);
                return;
            }

            _removePlayerFromOtherLobbies(connection, request.ActivatorId);

            if (!_lobbies.TryGetValue(request.ActivatorId, out BattleLobby lobby))
            {
                lobby = new BattleLobby
                {
                    ActivatorId = request.ActivatorId,
                    Mode = request.Mode,
                    MinPlayers = Mathf.Max(1, request.MinPlayers),
                    MaxPlayers = Mathf.Max(1, request.MaxPlayers),
                    AllowEarlyStart = request.AllowEarlyStart,
                    AutoStartWhenFull = request.AutoStartWhenFull,
                    EnemyDifficulty = request.EnemyDifficulty,
                    AiHero = request.AiHero,
                    HostConnection = connection
                };
                _lobbies[request.ActivatorId] = lobby;
            }

            if (lobby.Players.Any(player => player.Connection == connection))
            {
                _broadcastLobbyUpdate(lobby);
                return;
            }

            if (lobby.Players.Count >= lobby.MaxPlayers)
            {
                Debug.LogWarning($"Battle lobby '{request.ActivatorId}' is full ({lobby.MaxPlayers}).");
                return;
            }

            lobby.Players.Add(new BattleLobbyPlayer
            {
                Connection = connection,
                Hero = request.Hero
            });

            Debug.Log($"Battle lobby '{request.ActivatorId}': {lobby.Players.Count}/{lobby.MaxPlayers} players.");
            _broadcastLobbyUpdate(lobby);

            if (lobby.AutoStartWhenFull && lobby.Players.Count >= lobby.MaxPlayers)
            {
                _tryStartLobby(lobby);
            }
        }

        private void _onLobbyActionRequest(NetworkConnection connection, BattleLobbyActionRequestBroadcast request, Channel channel)
        {
            if (!InstanceFinder.IsServerStarted || string.IsNullOrEmpty(request.ActivatorId))
            {
                return;
            }

            if (!_lobbies.TryGetValue(request.ActivatorId, out BattleLobby lobby))
            {
                return;
            }

            switch (request.Action)
            {
                case EBattleLobbyAction.Leave:
                    _removePlayerFromLobby(lobby, connection);
                    break;
                case EBattleLobbyAction.Start:
                    if (lobby.HostConnection != connection)
                    {
                        Debug.LogWarning("Only the lobby host can start the battle.");
                        return;
                    }

                    _tryStartLobby(lobby);
                    break;
            }
        }

        private void _onRemoteConnectionState(NetworkConnection connection, FishNet.Transporting.RemoteConnectionStateArgs args)
        {
            if (!InstanceFinder.IsServerStarted || args.ConnectionState != FishNet.Transporting.RemoteConnectionState.Stopped)
            {
                return;
            }

            foreach (BattleLobby lobby in _lobbies.Values.ToList())
            {
                if (lobby.Players.All(player => player.Connection != connection))
                {
                    continue;
                }

                _removePlayerFromLobby(lobby, connection);
            }
        }

        private void _removePlayerFromOtherLobbies(NetworkConnection connection, string exceptActivatorId)
        {
            foreach (KeyValuePair<string, BattleLobby> entry in _lobbies.ToList())
            {
                if (entry.Key == exceptActivatorId)
                {
                    continue;
                }

                if (entry.Value.Players.Any(player => player.Connection == connection))
                {
                    _removePlayerFromLobby(entry.Value, connection);
                }
            }
        }

        private void _removePlayerFromLobby(BattleLobby lobby, NetworkConnection connection)
        {
            bool wasInLobby = lobby.Players.Any(player => player.Connection == connection);
            lobby.Players.RemoveAll(player => player.Connection == connection);

            if (wasInLobby)
            {
                _sendLobbyUpdate(
                    connection,
                    _createLobbyUpdate(lobby, connection, isOpen: false));
            }

            if (lobby.Players.Count == 0)
            {
                _lobbies.Remove(lobby.ActivatorId);
                return;
            }

            if (lobby.HostConnection == connection)
            {
                lobby.HostConnection = lobby.Players[0].Connection;
            }

            _broadcastLobbyUpdate(lobby);
        }

        private void _tryStartLobby(BattleLobby lobby)
        {
            if (lobby.Players.Count < lobby.MinPlayers)
            {
                Debug.LogWarning($"Battle lobby '{lobby.ActivatorId}' needs at least {lobby.MinPlayers} players.");
                return;
            }

            if (!lobby.AllowEarlyStart && lobby.Players.Count < lobby.MaxPlayers)
            {
                Debug.LogWarning($"Battle lobby '{lobby.ActivatorId}' is waiting for a full team ({lobby.MaxPlayers}).");
                return;
            }

            if (!_canStartMode(lobby))
            {
                Debug.LogWarning($"Battle lobby '{lobby.ActivatorId}' cannot start with {lobby.Players.Count} players in mode {lobby.Mode}.");
                return;
            }

            _startNetworkBattle(lobby);
            _closeLobby(lobby);
        }

        private static bool _canStartMode(BattleLobby lobby)
        {
            return lobby.Mode switch
            {
                EBattleMode.PvP or EBattleMode.Duel => lobby.Players.Count >= 2,
                _ => lobby.Players.Count >= 1
            };
        }

        private void _closeLobby(BattleLobby lobby)
        {
            _broadcastLobbyClosed(lobby);
            _lobbies.Remove(lobby.ActivatorId);
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
                        lobby.Mode == EBattleMode.Duel ? EBattleMode.Duel : EBattleMode.PvP,
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

                    if (lobby.Mode == EBattleMode.CoOpPvE && lobby.Players.Count < 2)
                    {
                        Debug.Log($"Co-op lobby started solo — falling back to PvE ({lobby.Players.Count}/{lobby.MaxPlayers}).");
                    }

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
                EBattleNetworkAction.PlayCard => _battleService.TryPlayCardWithResult(
                    request.CardId, request.TargetId, request.RequesterUnitId),
                EBattleNetworkAction.EndTurn => _battleService.EndTurnWithResult(request.RequesterUnitId),
                EBattleNetworkAction.MoveToCell => _battleService.TryPlayMoveCardToCellWithResult(
                    request.CardId, request.UnitId, request.Line, request.CellIndex, request.RequesterUnitId),
                EBattleNetworkAction.SummonToCell => _battleService.TryPlaySummonCardToCellWithResult(
                    request.CardId, request.Line, request.CellIndex, request.RequesterUnitId),
                _ => CommandResult.InvalidState
            };

            if (result == CommandResult.Success)
            {
                _syncBattleState(isCardPlayed: true);
            }
            else
            {
                Debug.LogWarning($"Battle action rejected: {result} from {request.RequesterUnitId}");
                InstanceFinder.ServerManager.Broadcast(connection, new BattleActionResultBroadcast
                {
                    Success = false,
                    Result = result
                });
            }
        }

        private void _onActionResult(BattleActionResultBroadcast broadcast, Channel channel)
        {
            if (InstanceFinder.IsServerStarted || broadcast.Success)
            {
                return;
            }

            Debug.LogWarning($"Battle action failed: {broadcast.Result}");
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

            if (broadcast.IsBattleStarted)
            {
                _clearLocalLobby();
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
            foreach (BattleLobbyPlayer player in lobby.Players)
            {
                _sendLobbyUpdate(
                    player.Connection,
                    _createLobbyUpdate(lobby, player.Connection, isOpen: true));
            }
        }

        private void _broadcastLobbyClosed(BattleLobby lobby)
        {
            foreach (BattleLobbyPlayer player in lobby.Players)
            {
                _sendLobbyUpdate(
                    player.Connection,
                    _createLobbyUpdate(lobby, player.Connection, isOpen: false));
            }
        }

        private void _sendLobbyUpdate(NetworkConnection connection, BattleLobbyUpdateBroadcast update)
        {
            if (connection != null && connection.IsLocalClient && InstanceFinder.IsClientStarted)
            {
                _applyLobbyUpdate(update);
                return;
            }

            if (connection != null)
            {
                InstanceFinder.ServerManager.Broadcast(connection, update);
            }
        }

        private static BattleLobbyUpdateBroadcast _createLobbyUpdate(BattleLobby lobby, NetworkConnection recipient, bool isOpen)
        {
            return new BattleLobbyUpdateBroadcast
            {
                ActivatorId = lobby.ActivatorId,
                IsOpen = isOpen,
                PlayersWaiting = lobby.Players.Count,
                MaxPlayers = lobby.MaxPlayers,
                MinPlayers = lobby.MinPlayers,
                Mode = lobby.Mode,
                IsHost = lobby.HostConnection == recipient,
                AllowEarlyStart = lobby.AllowEarlyStart,
                ShouldShowLobby = _shouldShowLobby(),
                OnlinePlayersCount = _getOnlinePlayerCount()
            };
        }

        private void _onLobbyUpdate(BattleLobbyUpdateBroadcast broadcast, Channel channel)
        {
            if (!InstanceFinder.IsClientStarted)
            {
                return;
            }

            _applyLobbyUpdate(broadcast);
        }

        private void _applyLobbyUpdate(BattleLobbyUpdateBroadcast broadcast)
        {
            if (!broadcast.IsOpen)
            {
                if (_localLobbyActivatorId == broadcast.ActivatorId)
                {
                    _clearLocalLobby();
                }

                LobbyStateChanged?.Invoke(_toLobbyState(broadcast));
                return;
            }

            _localLobbyActivatorId = broadcast.ActivatorId;
            LobbyStateChanged?.Invoke(_toLobbyState(broadcast));
        }

        private void _clearLocalLobby()
        {
            _localLobbyActivatorId = null;
        }

        private static BattleLobbyState _toLobbyState(BattleLobbyUpdateBroadcast broadcast)
        {
            return new BattleLobbyState(
                broadcast.ActivatorId,
                broadcast.IsOpen,
                broadcast.PlayersWaiting,
                broadcast.MaxPlayers,
                broadcast.MinPlayers,
                broadcast.Mode,
                broadcast.IsHost,
                broadcast.AllowEarlyStart,
                broadcast.ShouldShowLobby,
                broadcast.OnlinePlayersCount);
        }

        private sealed class BattleLobby
        {
            public string ActivatorId;
            public EBattleMode Mode;
            public int MinPlayers;
            public int MaxPlayers;
            public bool AllowEarlyStart;
            public bool AutoStartWhenFull;
            public EEnemyAIDifficulty EnemyDifficulty;
            public BattleHeroPayload AiHero;
            public NetworkConnection HostConnection;
            public List<BattleLobbyPlayer> Players = new();
        }

        private sealed class BattleLobbyPlayer
        {
            public NetworkConnection Connection;
            public BattleHeroPayload Hero;
        }
    }
}
