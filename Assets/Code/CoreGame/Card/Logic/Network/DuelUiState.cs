namespace CoreGame.Card.Logic.Network
{
    public readonly struct DuelUiState
    {
        public bool IsOpen { get; }
        public EDuelUiRole Role { get; }
        public string OpponentName { get; }
        public string OpponentHeroId { get; }
        public int GoldStake { get; }
        public int MyGold { get; }

        public DuelUiState(
            bool isOpen,
            EDuelUiRole role,
            string opponentName,
            string opponentHeroId,
            int goldStake,
            int myGold)
        {
            IsOpen = isOpen;
            Role = role;
            OpponentName = opponentName;
            OpponentHeroId = opponentHeroId;
            GoldStake = goldStake;
            MyGold = myGold;
        }

        public static DuelUiState Closed => new(false, EDuelUiRole.None, string.Empty, string.Empty, 0, 0);
    }
}
