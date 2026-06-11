using CoreGame.Card.Logic.Network;
using FishNet.Broadcast;

namespace CoreGame.Card.Logic.Network
{
    public enum EDuelNetworkAction
    {
        Challenge,
        Accept,
        Decline,
        Cancel,
    }

    public enum EDuelUiRole
    {
        None,
        Setup,
        Waiting,
        Invite,
    }

    public struct DuelActionRequestBroadcast : IBroadcast
    {
        public EDuelNetworkAction Action;
        public string TargetHeroId;
        public int GoldStake;
        public BattleHeroPayload Hero;
    }

    public struct DuelUiUpdateBroadcast : IBroadcast
    {
        public bool IsOpen;
        public EDuelUiRole Role;
        public string OpponentName;
        public string OpponentHeroId;
        public int GoldStake;
        public int MyGold;
    }

    public struct HeroGoldSyncBroadcast : IBroadcast
    {
        public int Gold;
    }
}
