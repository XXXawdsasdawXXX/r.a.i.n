using System;
using System.Collections.Generic;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using Essential;
using FishNet;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using UnityEngine;

namespace Core.Scenes
{
    public class ReferenceTriggerSceneLoader : NetworkBehaviour
    {
        public event Action<NetworkObject, EScene> MovedToAnotherScene;

        /// <summary>
        /// True to move the triggering object.
        /// </summary>
        [Tooltip("True to move the triggering object.")] [SerializeField]
        private bool _moveObject = true;

        /// <summary>
        /// True to move all connection objects (clients).
        /// </summary>
        [Tooltip("True to move all connection objects (clients).")] [SerializeField]
        private bool _moveAllObjects;

        /// <summary>
        /// True to replace current scenes with new scenes. First scene loaded will become active scene.
        /// </summary>
        [Tooltip("True to replace current scenes with new scenes. First scene loaded will become active scene.")]
        [SerializeField]
        private ReplaceOption _replaceOption = ReplaceOption.None;

        /// <summary>
        /// Scenes to load.
        /// </summary>
        [Tooltip("Scenes to load.")] [SerializeField]
        private EScene _scene;

        /// <summary>
        /// True to only unload for the connectioning causing the trigger.
        /// </summary>
        [Tooltip("True to only unload for the connectioning causing the trigger.")] [SerializeField]
        private bool _connectionOnly;

        /// <summary>
        /// True to automatically unload the loaded scenes when no more connections are using them.
        /// </summary>
        [Tooltip("True to automatically unload the loaded scenes when no more connections are using them.")]
        [SerializeField]
        private bool _automaticallyUnload = true;

        /// <summary>
        /// True to fire when entering the trigger. False to fire when exiting the trigger.
        /// </summary>
        [Tooltip("True to fire when entering the trigger. False to fire when exiting the trigger.")] [SerializeField]
        private bool _onTriggerEnter = true;

        /// <summary>
        /// Used to prevent excessive triggering when two clients are loaded and server is separate.
        /// Client may enter trigger intentionally then when moved to a new scene will re-enter trigger
        /// since original scene will still be loaded on server due to another client being in it.
        /// This scenario is extremely unlikely in production but keep it in mind.
        /// </summary>
        private Dictionary<NetworkConnection, float> _triggeredTimes = new();

        [SerializeField] private int _stackedSceneHandle;
        [SerializeField] private bool _sceneStack;

        protected override void Start()
        {
            InstanceFinder.SceneManager.OnLoadEnd += SceneManagerOnOnLoadEnd;
            base.Start();
        }

        private void OnDisable()
        {
            if (InstanceFinder.SceneManager != null)
            {
                InstanceFinder.SceneManager.OnLoadEnd -= SceneManagerOnOnLoadEnd;
            }
        }

        private void SceneManagerOnOnLoadEnd(SceneLoadEndEventArgs obj)
        {
            if (!obj.QueueData.AsServer)
            {
                return;
            }

            if (_sceneStack)
            {
                return;
            }

            if (_stackedSceneHandle != 0)
            {
                return;
            }

            if (obj.LoadedScenes.Length > 0)
            {
                _stackedSceneHandle = obj.LoadedScenes[0].handle;
            }
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            NetworkObject networkObject = col.GetComponent<NetworkObject>();

            Log.Info(this, $"{col.gameObject.name} trigger enter {InstanceFinder.NetworkManager.ServerManager.Started}", Color.cyan);
            
            if (networkObject == null)
            {
                return;
            }

            if (_triggeredTimes.TryGetValue(networkObject.Owner, out float time))
            {
                if (Time.time - time < 1f)
                {
                    Log.Info(this, $"{col.gameObject.name} trigger cooldown", Color.cyan);
                    return;
                }
            }

            _triggeredTimes[networkObject.Owner] = Time.time;

            MovedToAnotherScene?.Invoke(networkObject, _scene);

            //Container.Instance.GetService<SceneService>().LoadSceneAsync(_scene).Forget();
            LoadScene(networkObject);
        }
        

       [ServerRpc(RequireOwnership = false)]
        private void LoadScene(NetworkObject triggeringIdentity)
        {
            Log.Info(this, $"{triggeringIdentity.Owner.ClientId} try load scene", Color.cyan);

            /*if (!InstanceFinder.NetworkManager.IsServerStarted)
            {
                Log.Info(this, $"{triggeringIdentity.Owner.ClientId} !InstanceFinder.NetworkManager.IsServerStarted",
                    Color.cyan);

                return;
            }*/

            LoadOptions loadOptions = new()
            {
                AutomaticallyUnload = true, // Принудительно включаем, даже если флаг выключен в инспекторе
            };

            // Создание и настройка сцены
            SceneLoadData sceneLoadData = new(_scene.ToString());
            sceneLoadData.PreferredActiveScene = new(sceneLoadData.SceneLookupDatas[0]);
            sceneLoadData.ReplaceScenes = ReplaceOption.All; // Было OnlineOnly
            sceneLoadData.Options = new LoadOptions { AutomaticallyUnload = true };
            sceneLoadData.MovedNetworkObjects = new NetworkObject[] { triggeringIdentity };

    

            Log.Info(this,
                $"[SceneLoader] Loading scene: {_scene} with ReplaceOption: {sceneLoadData.ReplaceScenes}", Color.cyan);

            InstanceFinder.SceneManager.LoadConnectionScenes(triggeringIdentity.Owner, sceneLoadData);
        }

        /*private void LoadScene(NetworkObject triggeringIdentity)
        {
            if (!InstanceFinder.NetworkManager.IsServerStarted)
            {
                return;
            }
            
            List<NetworkObject> movedObjects = new();

            if (_moveAllObjects)
            {
                foreach (NetworkConnection item in InstanceFinder.ServerManager.Clients.Values)
                {
                    foreach (NetworkObject nob in item.Objects)
                        movedObjects.Add(nob);
                }
            }
            else if (_moveObject)
            {
                movedObjects.Add(triggeringIdentity);
            }

            LoadOptions loadOptions = new()
            {
                AutomaticallyUnload = _automaticallyUnload,
            };

            //Make scene data.
            SceneLoadData sceneLoadData = new(_scene.ToString());
            sceneLoadData.PreferredActiveScene = new(sceneLoadData.SceneLookupDatas[0]);
            sceneLoadData.ReplaceScenes = _replaceOption;
            sceneLoadData.Options = loadOptions;
            sceneLoadData.MovedNetworkObjects = movedObjects.ToArray();

            //Load for connection only.
            if (_connectionOnly)
            {
                InstanceFinder.SceneManager.LoadConnectionScenes(triggeringIdentity.Owner, sceneLoadData);
            }
            //Load for all clients.
            else
            {
                InstanceFinder.SceneManager.LoadGlobalScenes(sceneLoadData);
            }
        }*/
    }
}