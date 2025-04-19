using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.AssetManagement
{
    public static class AssetProvider
    {
        public static async UniTask<T> LoadScriptableObject<T>(string path) where T : ScriptableObject
        {
            ResourceRequest request = Resources.LoadAsync<T>(path);
            
            await request;

            return request.asset as T;
        }
        
        public static async UniTask<ScriptableObject> LoadScriptableObject(string path)
        {
            ResourceRequest request = Resources.LoadAsync<ScriptableObject>(path);
            
            await request;

            return request.asset as ScriptableObject;
        }
    }
}