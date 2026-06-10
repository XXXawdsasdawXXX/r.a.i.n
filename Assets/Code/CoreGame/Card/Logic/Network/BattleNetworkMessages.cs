using CoreGame.Card.Data;
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

    public struct BattleJoinRequestBroadcast : IBroadcast
    {
        public string ActivatorId;
        public EBattleMode Mode;
        public BattleHeroPayload Hero;
        public BattleHeroPayload AiHero;
        public EEnemyAIDifficulty EnemyDifficulty;
        public int RequiredPlayers;
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
        public int PlayersWaiting;
        public int PlayersRequired;
    }
}
