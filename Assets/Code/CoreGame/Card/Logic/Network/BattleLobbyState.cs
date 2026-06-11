using Core.Localization;
using CoreGame.Card.Data;

namespace CoreGame.Card.Logic.Network
{
    public readonly struct BattleLobbyState
    {
        public string ActivatorId { get; }
        public bool IsOpen { get; }
        public int PlayersWaiting { get; }
        public int MaxPlayers { get; }
        public int MinPlayers { get; }
        public EBattleMode Mode { get; }
        public bool IsHost { get; }
        public bool AllowEarlyStart { get; }
        public bool ShouldShowLobby { get; }
        public int OnlinePlayersCount { get; }

        public BattleLobbyState(
            string activatorId,
            bool isOpen,
            int playersWaiting,
            int maxPlayers,
            int minPlayers,
            EBattleMode mode,
            bool isHost,
            bool allowEarlyStart,
            bool shouldShowLobby,
            int onlinePlayersCount)
        {
            ActivatorId = activatorId;
            IsOpen = isOpen;
            PlayersWaiting = playersWaiting;
            MaxPlayers = maxPlayers;
            MinPlayers = minPlayers;
            Mode = mode;
            IsHost = isHost;
            AllowEarlyStart = allowEarlyStart;
            ShouldShowLobby = shouldShowLobby;
            OnlinePlayersCount = onlinePlayersCount;
        }

        public bool CanStart =>
            IsHost
            && IsOpen
            && PlayersWaiting >= MinPlayers
            && (AllowEarlyStart || PlayersWaiting >= MaxPlayers);

        public string GetStatusText(LocalizationService localization)
        {
            if (!IsOpen)
            {
                return string.Empty;
            }

            if (localization == null)
            {
                return $"Waiting for players: {PlayersWaiting}/{MaxPlayers}";
            }

            return localization.Format(
                LocalizationTables.CoreGame,
                LocalizationKeys.CoreGame.BattleLobbyStatus,
                PlayersWaiting,
                MaxPlayers);
        }

        public string GetHintText(LocalizationService localization)
        {
            if (!IsOpen || !IsHost)
            {
                return string.Empty;
            }

            if (localization == null)
            {
                return _getHintFallback();
            }

            if (PlayersWaiting >= MaxPlayers)
            {
                return localization.Get(
                    LocalizationTables.CoreGame,
                    LocalizationKeys.CoreGame.BattleLobbyReady,
                    "Team is ready — you can start the battle.");
            }

            if (!AllowEarlyStart || PlayersWaiting < MinPlayers)
            {
                return localization.Get(
                    LocalizationTables.CoreGame,
                    LocalizationKeys.CoreGame.BattleLobbyWaitPartner,
                    "Wait for a partner or connect a second player to this activator.");
            }

            return Mode switch
            {
                EBattleMode.CoOpPvE => localization.Get(
                    LocalizationTables.CoreGame,
                    LocalizationKeys.CoreGame.BattleLobbySoloPve,
                    "You can start solo — the battle will run as PvE."),
                EBattleMode.PvP or EBattleMode.Duel => localization.Get(
                    LocalizationTables.CoreGame,
                    LocalizationKeys.CoreGame.BattleLobbyNotEnoughPvp,
                    "Not enough players for PvP."),
                _ => localization.Get(
                    LocalizationTables.CoreGame,
                    LocalizationKeys.CoreGame.BattleLobbyStartCurrent,
                    "You can start with the current lineup.")
            };
        }

        private string _getHintFallback()
        {
            if (PlayersWaiting >= MaxPlayers)
            {
                return "Team is ready — you can start the battle.";
            }

            if (!AllowEarlyStart || PlayersWaiting < MinPlayers)
            {
                return "Wait for a partner or connect a second player to this activator.";
            }

            return Mode switch
            {
                EBattleMode.CoOpPvE => "You can start solo — the battle will run as PvE.",
                EBattleMode.PvP or EBattleMode.Duel => "Not enough players for PvP.",
                _ => "You can start with the current lineup."
            };
        }
    }
}
