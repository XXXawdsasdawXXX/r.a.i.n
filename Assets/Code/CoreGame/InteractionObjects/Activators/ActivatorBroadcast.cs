using FishNet.Broadcast;

namespace Code.CoreGame.InteractionObjects.Activators
{
    public struct ActivatorBroadcast : IBroadcast
    {
        public string ObjectID;
        public bool IsActive;
    }
}