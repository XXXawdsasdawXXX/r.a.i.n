using System;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing.Logging;
using FishNet.Managing.Scened;
using FishNet.Object;
using UnityEngine;

namespace Core.Scenes
{
    public class ReferenceTriggerSceneLoader : MonoBehaviour
    {
          /// <summary>
        /// True to move the triggering object.
        /// </summary>
        [Tooltip("True to move the triggering object.")]
        [SerializeField]
        private bool _moveObject = true;
        /// <summary>
        /// True to move all connection objects (clients).
        /// </summary>
        [Tooltip("True to move all connection objects (clients).")]
        [SerializeField]
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
        [Tooltip("Scenes to load.")]
        [SerializeField]
        private string[] _scenes = new string[0];
        /// <summary>
        /// True to only unload for the connectioning causing the trigger.
        /// </summary>
        [Tooltip("True to only unload for the connectioning causing the trigger.")]
        [SerializeField]
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
        [Tooltip("True to fire when entering the trigger. False to fire when exiting the trigger.")]
        [SerializeField]
        private bool _onTriggerEnter = true;

        /// <summary>
        /// Used to prevent excessive triggering when two clients are loaded and server is separate.
        /// Client may enter trigger intentionally then when moved to a new scene will re-enter trigger
        /// since original scene will still be loaded on server due to another client being in it.
        /// This scenario is extremely unlikely in production but keep it in mind.
        /// </summary>
        private Dictionary<NetworkConnection, float> _triggeredTimes = new();

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (!_onTriggerEnter)
            {
                return;
            }

            NetworkObject networkObject = col.GetComponent<NetworkObject>();
            
            if (networkObject == null)
            {
                return;
            }
            
            if (_triggeredTimes.TryGetValue(networkObject.Owner, out float time))
            {
                if (Time.time - time < 0.5f)
                {
                    return;
                }
            }
          
            _triggeredTimes[networkObject.Owner] = Time.time;
            
            LoadScene(col.GetComponent<NetworkObject>());
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!_onTriggerEnter)
            {
                return;
            }

            NetworkObject networkObject = other.GetComponent<NetworkObject>();

            if (networkObject == null)
            {
                return;
            }
            
            if (_triggeredTimes.TryGetValue(networkObject.Owner, out float time))
            {
                if (Time.time - time < 0.5f)
                {
                    return;
                }
            }
          
            _triggeredTimes[networkObject.Owner] = Time.time;

            LoadScene(other.GetComponent<NetworkObject>());
        }

        
        private void LoadScene(NetworkObject triggeringIdentity)
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
            SceneLoadData sceneLoadData = new(_scenes);
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
        }

    }
}