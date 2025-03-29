using Core.Network;
using Core.ServiceLocator;
using FishNet.Object;
using UnityEngine;

namespace Game.Entities
{
    public class HeroItemController :  NetworkBehaviour
    {
        private NetworkItemSpawner _networkItemSpawner;
        
        public override void OnStartClient()
        {
            base.OnStartClient();
            
            enabled = IsOwner;

            _networkItemSpawner = Container.Instance.GetService<NetworkItemSpawner>();
            
            Debug.Log($"spawn hero {_networkItemSpawner != null}");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                _networkItemSpawner.SpawnItem(transform.position + Vector3.right);
            }

            if (Input.GetKeyDown(KeyCode.F2))
            {
                _networkItemSpawner.DespawnItem();
            }
        }
    }
}