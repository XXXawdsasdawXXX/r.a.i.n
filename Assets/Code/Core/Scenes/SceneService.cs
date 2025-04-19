using System;
using System.Collections.Generic;
using Core.GameLoop;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using FishNet;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Core.Scenes
{
    public class SceneService : IService, ISubscriber
    {
        public event Action<EScene> SceneLoaded;
        public event Action<EScene> SceneUnloaded;
        
        private EScene _currentScene;

        public UniTask Subscribe()
        {
            InstanceFinder.SceneManager.OnLoadEnd += OnSceneManagerLoadEnd;
            
            return UniTask.CompletedTask;
        }

        public void Unsubscribe()
        {
            InstanceFinder.SceneManager.OnLoadEnd -= OnSceneManagerLoadEnd;
        }

        public async UniTask LoadSceneAsync(EScene scene)
        {
            SceneUnloaded?.Invoke(_currentScene);
            
            await SceneManager.LoadSceneAsync(scene.ToString());

            _currentScene = scene;
            
            SceneLoaded?.Invoke(scene);
        }

        private void LoadScene(EScene scene, NetworkObject triggeringIdentity, bool connectionOnly = true, bool moveAll = false)
        {
            if (!InstanceFinder.NetworkManager.IsServerStarted)
            {
                return;
            }
            
            List<NetworkObject> movedObjects = new();

            if (moveAll)
            {
                foreach (NetworkConnection item in InstanceFinder.ServerManager.Clients.Values)
                {
                    foreach (NetworkObject nob in item.Objects)
                    {
                        movedObjects.Add(nob);
                    }
                }
            }
            else 
            {
                movedObjects.Add(triggeringIdentity);
            }

            LoadOptions loadOptions = new()
            {
                AutomaticallyUnload = true,
            };

            //Make scene data.
            SceneLoadData sceneLoadData = new(scene.ToString());
            sceneLoadData.PreferredActiveScene = new(sceneLoadData.SceneLookupDatas[0]);
            sceneLoadData.ReplaceScenes = ReplaceOption.None;
            sceneLoadData.Options = loadOptions;
            sceneLoadData.MovedNetworkObjects = movedObjects.ToArray();

            SceneUnloaded?.Invoke(_currentScene);

            if (moveAll)
            {
                InstanceFinder.SceneManager.LoadGlobalScenes(sceneLoadData);
            }
            else
            {
                InstanceFinder.SceneManager.LoadConnectionScenes(triggeringIdentity.Owner, sceneLoadData);
            }
        }

        private void OnSceneManagerLoadEnd(SceneLoadEndEventArgs args)
        {
            if (Enum.TryParse(args.LoadedScenes[0].name, out EScene loadedScene))
            {
                _currentScene = loadedScene;
                
                SceneLoaded?.Invoke(loadedScene);
            }
        }
    }
}