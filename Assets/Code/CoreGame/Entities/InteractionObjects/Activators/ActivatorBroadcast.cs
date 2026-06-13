using FishNet.Broadcast;

namespace CoreGame.Entities.InteractionObjects.Activators
{
    public struct ActivatorBroadcast : IBroadcast
    {
        public string ObjectID;
        public bool IsActive;
    }
}