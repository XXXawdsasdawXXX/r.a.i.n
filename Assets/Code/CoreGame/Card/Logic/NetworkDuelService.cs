using System;
using System.Collections.Generic;
using Core.GameLoop;
using Core.Network;
using Core.Save;
using Core.ServiceLocator;
using CoreGame.Card.Data;
using CoreGame.Card.Logic.Network;
using CoreGame.Card.Logic.StateMachine;
using CoreGame.Entities.Characters.Hero;
using Cysharp.Threading.Tasks;
using FishNet;
using FishNet.Connection;
using UnityEngine;
using Channel = FishNet.Transporting.Channel;

namespace CoreGame.Card.Logic
{
    public class NetworkDuelService : IService, IInitializeListener, ISubscriber
    {
        public bool IsInitialized { get; set; }

        public event Action<DuelUiState> DuelUiStateChanged;

        private NetworkBattleService _networkBattleService;
        private BattleService _battleService;
        private BattleStateMachine _stateMachine;
        private HeroSpawner _heroSpawner;
        private UserProvider _userProvider;

        private readonly Dictionary<string, int> _heroGold = new();
        private PendingDuelInvite _pendingInvite;
        private ActiveDuelBet _activeBet;

        private string _localTargetHeroId;
        private string _localTargetName;

        public UniTask Initialize()
        {
            _networkBattleService = Container.Instance.GetService<NetworkBattleService>();
            _battleService = Container.Instance.GetService<BattleService>();
            _stateMachine = Container.Instance.GetService<BattleStateMachine>();
            _heroSpawner = UnityEngine.Object.FindObjectOfType<HeroSpawner>();
            _userProvider = Container.Instance.GetService<UserProvider>();
            return UniTask.CompletedTask;
        }

        public void Subscribe()
        {
            if (InstanceFinder.ServerManager != null)
            {
                InstanceFinder.ServerManager.RegisterBroadcast<DuelActionRequestBroadcast>(_onDuelActionRequest);
                InstanceFinder.ServerManager.OnRemoteConnectionState += _onRemoteConnectionState;
                _battleService.BattleFinished += _onBattleFinished;
            }

            if (InstanceFinder.ClientManager != null)
            {
                InstanceFinder.ClientManager.RegisterBroadcast<DuelUiUpdateBroadcast>(_onDuelUiUpdate);
                InstanceFinder.ClientManager.RegisterBroadcast<HeroGoldSyncBroadcast>(_onHeroGoldSync);
            }
        }

        public void Unsubscribe()
        {
            if (InstanceFinder.ServerManager != null)
            {
                InstanceFinder.ServerManager.UnregisterBroadcast<DuelActionRequestBroadcast>(_onDuelActionRequest);
                InstanceFinder.ServerManager.OnRemoteConnectionState -= _onRemoteConnectionState;
            }

            _battleService.BattleFinished -= _onBattleFinished;

            if (InstanceFinder.ClientManager != null)
            {
                InstanceFinder.ClientManager.UnregisterBroadcast<DuelUiUpdateBroadcast>(_onDuelUiUpdate);
                InstanceFinder.ClientManager.UnregisterBroadcast<HeroGoldSyncBroadcast>(_onHeroGoldSync);
            }
        }

        public void OpenChallengeSetup(string targetHeroId, string targetName)
        {
            if (!InstanceFinder.IsClientStarted || string.IsNullOrEmpty(targetHeroId))
            {
                return;
            }

            _localTargetHeroId = targetHeroId;
            _localTargetName = targetName;

            int myGold = _getLocalHeroGold();
            _raiseLocalUi(new DuelUiState(
                true,
                EDuelUiRole.Setup,
                targetName,
                targetHeroId,
                Mathf.Min(10, myGold),
                myGold));
        }

        public void SendChallenge(int goldStake)
        {
            if (!InstanceFinder.IsClientStarted || string.IsNullOrEmpty(_localTargetHeroId))
            {
                return;
            }

            BattleHeroPayload heroPayload = _buildLocalHeroPayload();
            if (heroPayload.Gold < goldStake)
            {
                Debug.LogWarning("Not enough gold for duel stake.");
                return;
            }

            InstanceFinder.ClientManager.Broadcast(new DuelActionRequestBroadcast
            {
                Action = EDuelNetworkAction.Challenge,
                TargetHeroId = _localTargetHeroId,
                GoldStake = goldStake,
                Hero = heroPayload
            });
        }

        public void AcceptInvite()
        {
            _sendDuelAction(EDuelNetworkAction.Accept);
        }

        public void DeclineInvite()
        {
            _sendDuelAction(EDuelNetworkAction.Decline);
        }

        public void CancelDuel()
        {
            if (!InstanceFinder.IsClientStarted)
            {
                return;
            }

            if (_localTargetHeroId != null)
            {
                _sendDuelAction(EDuelNetworkAction.Cancel);
            }

            _clearLocalUi();
        }

        public int GetLocalGold()
        {
            return _getLocalHeroGold();
        }

        private void _sendDuelAction(EDuelNetworkAction action)
        {
            InstanceFinder.ClientManager.Broadcast(new DuelActionRequestBroadcast
            {
                Action = action,
                Hero = _buildLocalHeroPayload()
            });
        }

        private BattleHeroPayload _buildLocalHeroPayload()
        {
            Hero hero = _userProvider.GetHeroComponent<Hero>();
            BattleHeroPayload payload = BattleHeroPayload.FromHeroModel(hero?.Model);
            if (string.IsNullOrEmpty(payload.HeroId) && !string.IsNullOrEmpty(_userProvider.Id))
            {
                payload.HeroId = _userProvider.Id;
            }

            return payload;
        }

        private int _getLocalHeroGold()
        {
            Hero hero = _userProvider.GetHeroComponent<Hero>();
            return hero?.Model?.Gold ?? 0;
        }

        private void _onDuelActionRequest(NetworkConnection connection, DuelActionRequestBroadcast request, Channel channel)
        {
            if (!InstanceFinder.IsServerStarted)
            {
                return;
            }

            if (_stateMachine.HasActiveBattle)
            {
                Debug.LogWarning("Duel rejected: battle already in progress.");
                return;
            }

            switch (request.Action)
            {
                case EDuelNetworkAction.Challenge:
                    _handleChallenge(connection, request);
                    break;
                case EDuelNetworkAction.Accept:
                    _handleAccept(connection, request);
                    break;
                case EDuelNetworkAction.Decline:
                    _handleDecline(connection);
                    break;
                case EDuelNetworkAction.Cancel:
                    _handleCancel(connection);
                    break;
            }
        }

        private void _handleChallenge(NetworkConnection challengerConnection, DuelActionRequestBroadcast request)
        {
            if (_pendingInvite != null || _getOnlinePlayerCount() < 2)
            {
                return;
            }

            if (request.GoldStake <= 0 || string.IsNullOrEmpty(request.TargetHeroId))
            {
                return;
            }

            if (!int.TryParse(request.TargetHeroId, out int targetObjectId)
                || !_heroSpawner.TryGetConnectionForHeroObjectId(targetObjectId, out NetworkConnection targetConnection))
            {
                Debug.LogWarning("Duel target hero not found.");
                return;
            }

            if (targetConnection == challengerConnection)
            {
                return;
            }

            _trackHeroGold(request.Hero);

            if (_getHeroGold(request.Hero.HeroId) < request.GoldStake)
            {
                Debug.LogWarning("Challenger does not have enough gold.");
                return;
            }

            _pendingInvite = new PendingDuelInvite
            {
                ChallengerConnection = challengerConnection,
                ChallengerHero = request.Hero,
                TargetConnection = targetConnection,
                TargetHeroId = request.TargetHeroId,
                GoldStake = request.GoldStake
            };

            _sendDuelUi(challengerConnection, new DuelUiUpdateBroadcast
            {
                IsOpen = true,
                Role = EDuelUiRole.Waiting,
                OpponentName = _getHeroDisplayName(targetConnection),
                OpponentHeroId = request.TargetHeroId,
                GoldStake = request.GoldStake,
                MyGold = _getHeroGold(request.Hero.HeroId)
            });

            _sendDuelUi(targetConnection, new DuelUiUpdateBroadcast
            {
                IsOpen = true,
                Role = EDuelUiRole.Invite,
                OpponentName = request.Hero.Name,
                OpponentHeroId = request.Hero.HeroId,
                GoldStake = request.GoldStake,
                MyGold = _getHeroGoldByConnection(targetConnection)
            });
        }

        private void _handleAccept(NetworkConnection targetConnection, DuelActionRequestBroadcast request)
        {
            if (_pendingInvite == null || _pendingInvite.TargetConnection != targetConnection)
            {
                return;
            }

            _trackHeroGold(request.Hero);

            if (_getHeroGold(request.Hero.HeroId) < _pendingInvite.GoldStake)
            {
                Debug.LogWarning("Target does not have enough gold.");
                _closePendingInvite("Not enough gold.");
                return;
            }

            PendingDuelInvite invite = _pendingInvite;
            _pendingInvite = null;

            _setHeroGold(invite.ChallengerHero.HeroId, _getHeroGold(invite.ChallengerHero.HeroId) - invite.GoldStake);
            _setHeroGold(request.Hero.HeroId, _getHeroGold(request.Hero.HeroId) - invite.GoldStake);

            _activeBet = new ActiveDuelBet
            {
                ChallengerHeroId = invite.ChallengerHero.HeroId,
                TargetHeroId = request.Hero.HeroId,
                ChallengerConnection = invite.ChallengerConnection,
                TargetConnection = targetConnection,
                GoldStake = invite.GoldStake
            };

            _sendDuelUi(invite.ChallengerConnection, _closedDuelUi());
            _sendDuelUi(targetConnection, _closedDuelUi());

            _syncGold(invite.ChallengerConnection, invite.ChallengerHero.HeroId);
            _syncGold(targetConnection, request.Hero.HeroId);

            BattleHeroPayload challengerPayload = invite.ChallengerHero;
            challengerPayload.Gold = _getHeroGold(invite.ChallengerHero.HeroId);
            BattleHeroPayload targetPayload = request.Hero;
            targetPayload.Gold = _getHeroGold(request.Hero.HeroId);

            _networkBattleService.StartDirectDuel(
                invite.ChallengerConnection,
                challengerPayload,
                targetConnection,
                targetPayload);
        }

        private void _handleDecline(NetworkConnection targetConnection)
        {
            if (_pendingInvite == null || _pendingInvite.TargetConnection != targetConnection)
            {
                return;
            }

            _closePendingInvite("Duel declined.");
        }

        private void _handleCancel(NetworkConnection connection)
        {
            if (_pendingInvite == null)
            {
                return;
            }

            if (_pendingInvite.ChallengerConnection != connection && _pendingInvite.TargetConnection != connection)
            {
                return;
            }

            _closePendingInvite("Duel cancelled.");
        }

        private void _closePendingInvite(string reason)
        {
            if (_pendingInvite == null)
            {
                return;
            }

            Debug.Log(reason);
            _sendDuelUi(_pendingInvite.ChallengerConnection, _closedDuelUi());
            _sendDuelUi(_pendingInvite.TargetConnection, _closedDuelUi());
            _pendingInvite = null;
        }

        private void _onRemoteConnectionState(NetworkConnection connection, FishNet.Transporting.RemoteConnectionStateArgs args)
        {
            if (!InstanceFinder.IsServerStarted
                || args.ConnectionState != FishNet.Transporting.RemoteConnectionState.Stopped
                || _pendingInvite == null)
            {
                return;
            }

            if (_pendingInvite.ChallengerConnection == connection || _pendingInvite.TargetConnection == connection)
            {
                _closePendingInvite("Duel cancelled: player disconnected.");
            }
        }

        private void _onBattleFinished(BattleModel model)
        {
            if (!InstanceFinder.IsServerStarted || model == null || model.Mode != EBattleMode.Duel || _activeBet == null)
            {
                return;
            }

            string winnerHeroId = _resolveWinnerHeroId(model);
            if (string.IsNullOrEmpty(winnerHeroId))
            {
                _refundActiveBet();
                return;
            }

            int payout = _activeBet.GoldStake * 2;
            _setHeroGold(winnerHeroId, _getHeroGold(winnerHeroId) + payout);

            _syncGold(_activeBet.ChallengerConnection, _activeBet.ChallengerHeroId);
            _syncGold(_activeBet.TargetConnection, _activeBet.TargetHeroId);

            _activeBet = null;
        }

        private void _refundActiveBet()
        {
            if (_activeBet == null)
            {
                return;
            }

            _setHeroGold(_activeBet.ChallengerHeroId, _getHeroGold(_activeBet.ChallengerHeroId) + _activeBet.GoldStake);
            _setHeroGold(_activeBet.TargetHeroId, _getHeroGold(_activeBet.TargetHeroId) + _activeBet.GoldStake);
            _syncGold(_activeBet.ChallengerConnection, _activeBet.ChallengerHeroId);
            _syncGold(_activeBet.TargetConnection, _activeBet.TargetHeroId);
            _activeBet = null;
        }

        private static string _resolveWinnerHeroId(BattleModel model)
        {
            bool sideADead = model.SideA?.Hero == null || model.SideA.Hero.HP <= 0;
            bool sideBDead = model.SideB?.Hero == null || model.SideB.Hero.HP <= 0;

            if (sideADead && !sideBDead)
            {
                return model.SideB.Hero.UnitId;
            }

            if (sideBDead && !sideADead)
            {
                return model.SideA.Hero.UnitId;
            }

            return null;
        }

        private void _trackHeroGold(BattleHeroPayload hero)
        {
            if (string.IsNullOrEmpty(hero.HeroId))
            {
                return;
            }

            if (!_heroGold.ContainsKey(hero.HeroId))
            {
                _heroGold[hero.HeroId] = Mathf.Max(0, hero.Gold);
            }
        }

        private int _getHeroGold(string heroId)
        {
            return !string.IsNullOrEmpty(heroId) && _heroGold.TryGetValue(heroId, out int gold) ? gold : 0;
        }

        private int _getHeroGoldByConnection(NetworkConnection connection)
        {
            if (_heroSpawner != null
                && _heroSpawner.TryGetHeroObjectId(connection, out int heroObjectId))
            {
                return _getHeroGold(heroObjectId.ToString());
            }

            return 0;
        }

        private void _setHeroGold(string heroId, int gold)
        {
            _heroGold[heroId] = Mathf.Max(0, gold);
        }

        private void _syncGold(NetworkConnection connection, string heroId)
        {
            _sendGoldSync(connection, _getHeroGold(heroId));
        }

        private void _sendGoldSync(NetworkConnection connection, int gold)
        {
            if (connection == null)
            {
                return;
            }

            HeroGoldSyncBroadcast update = new HeroGoldSyncBroadcast { Gold = gold };
            if (connection.IsLocalClient && InstanceFinder.IsClientStarted)
            {
                _applyGoldSync(update);
                return;
            }

            InstanceFinder.ServerManager.Broadcast(connection, update);
        }

        private void _sendDuelUi(NetworkConnection connection, DuelUiUpdateBroadcast update)
        {
            if (connection == null)
            {
                return;
            }

            if (connection.IsLocalClient && InstanceFinder.IsClientStarted)
            {
                _applyDuelUi(update);
                return;
            }

            InstanceFinder.ServerManager.Broadcast(connection, update);
        }

        private void _onDuelUiUpdate(DuelUiUpdateBroadcast broadcast, Channel channel)
        {
            if (!InstanceFinder.IsClientStarted)
            {
                return;
            }

            _applyDuelUi(broadcast);
        }

        private void _applyDuelUi(DuelUiUpdateBroadcast broadcast)
        {
            if (!broadcast.IsOpen)
            {
                _clearLocalUi();
                return;
            }

            DuelUiState state = new DuelUiState(
                true,
                broadcast.Role,
                broadcast.OpponentName,
                broadcast.OpponentHeroId,
                broadcast.GoldStake,
                broadcast.MyGold);

            if (broadcast.Role == EDuelUiRole.Waiting)
            {
                _localTargetHeroId = broadcast.OpponentHeroId;
            }

            DuelUiStateChanged?.Invoke(state);
        }

        private void _onHeroGoldSync(HeroGoldSyncBroadcast broadcast, Channel channel)
        {
            if (!InstanceFinder.IsClientStarted)
            {
                return;
            }

            _applyGoldSync(broadcast);
        }

        private void _applyGoldSync(HeroGoldSyncBroadcast broadcast)
        {
            Hero hero = _userProvider.GetHeroComponent<Hero>();
            if (hero?.Model != null)
            {
                hero.Model.Gold = broadcast.Gold;
            }
        }

        private void _raiseLocalUi(DuelUiState state)
        {
            DuelUiStateChanged?.Invoke(state);
        }

        private void _clearLocalUi()
        {
            _localTargetHeroId = null;
            _localTargetName = null;
            DuelUiStateChanged?.Invoke(DuelUiState.Closed);
        }

        private static int _getOnlinePlayerCount()
        {
            if (!InstanceFinder.IsServerStarted)
            {
                return InstanceFinder.IsClientStarted ? 1 : 0;
            }

            return InstanceFinder.ServerManager.Clients.Count;
        }

        private string _getHeroDisplayName(NetworkConnection connection)
        {
            if (_heroSpawner != null
                && _heroSpawner.TryGetHero(connection, out Hero hero)
                && hero.Name != null
                && !string.IsNullOrEmpty(hero.Name.Name))
            {
                return hero.Name.Name;
            }

            return "Player";
        }

        private static DuelUiUpdateBroadcast _closedDuelUi()
        {
            return new DuelUiUpdateBroadcast
            {
                IsOpen = false,
                Role = EDuelUiRole.None
            };
        }

        private sealed class PendingDuelInvite
        {
            public NetworkConnection ChallengerConnection;
            public BattleHeroPayload ChallengerHero;
            public NetworkConnection TargetConnection;
            public string TargetHeroId;
            public int GoldStake;
        }

        private sealed class ActiveDuelBet
        {
            public string ChallengerHeroId;
            public string TargetHeroId;
            public NetworkConnection ChallengerConnection;
            public NetworkConnection TargetConnection;
            public int GoldStake;
        }
    }
}
