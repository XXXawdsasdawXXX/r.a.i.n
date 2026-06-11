using CoreGame.Card.Data;
using CoreGame.Card.Logic;
using FishNet.Broadcast;

namespace CoreGame.Card.Logic.Network
{
    public enum EBattleNetworkAction
    {
        PlayCard,
        EndTurn,
        MoveToCell,
        SummonToCell,
    }

    public enum EBattleLobbyAction
    {
        Leave,
        Start,
    }

    public struct BattleJoinRequestBroadcast : IBroadcast
    {
        public string ActivatorId;
        public EBattleMode Mode;
        public BattleHeroPayload Hero;
        public BattleHeroPayload AiHero;
        public EEnemyAIDifficulty EnemyDifficulty;
        public int MinPlayers;
        public int MaxPlayers;
        public bool AllowEarlyStart;
        public bool AutoStartWhenFull;
    }

    public struct BattleLobbyActionRequestBroadcast : IBroadcast
    {
        public string ActivatorId;
        public EBattleLobbyAction Action;
    }

    public struct BattleActionRequestBroadcast : IBroadcast
    {
        public string BattleId;
        public string RequesterUnitId;
        public EBattleNetworkAction Action;
        public string CardId;
        public string TargetId;
        public string UnitId;
        public EBattleLine Line;
        public int CellIndex;
    }

    public struct BattleStateSyncBroadcast : IBroadcast
    {
        public string SnapshotJson;
        public bool IsBattleStarted;
        public bool IsBattleFinished;
        public bool IsTurnStarted;
        public bool IsCardPlayed;
    }

    public struct BattleHandSyncBroadcast : IBroadcast
    {
        public string BattleId;
        public string PlayerUnitId;
        public string HandJson;
    }

    public struct BattleLobbyUpdateBroadcast : IBroadcast
    {
        public string ActivatorId;
        public bool IsOpen;
        public int PlayersWaiting;
        public int MaxPlayers;
        public int MinPlayers;
        public EBattleMode Mode;
        public bool IsHost;
        public bool AllowEarlyStart;
        public bool ShouldShowLobby;
        public int OnlinePlayersCount;
    }

    public struct BattleActionResultBroadcast : IBroadcast
    {
        public bool Success;
        public CommandResult Result;
    }
}
