#if UNITY_EDITOR
using ParrelSync;
#endif

namespace Core.Editor
{
    public static class ParrelSyncUtility
    {
        public static bool IsClone()
        {
#if UNITY_EDITOR
            return ClonesManager.IsClone();
#endif
            return false;
        }
        
    }
}
