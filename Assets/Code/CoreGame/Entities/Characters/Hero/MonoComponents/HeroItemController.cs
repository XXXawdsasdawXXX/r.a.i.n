using Core.Network;
using Core.ServiceLocator;
using CoreGame.Entities.Items;
using FishNet.Object;
using UnityEngine;

namespace CoreGame.Entities.Characters.Hero
{
    public class HeroItemController :  NetworkBehaviour
    {
        private NetworkPool _networkItemPool;
        
        public override void OnStartClient()
        {
            base.OnStartClient();
            
            enabled = IsOwner;

            _networkItemPool = Container.Instance.GetService<ItemPool>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                _networkItemPool.ServerSpawn(transform.position + Vector3.right);
            }

            if (Input.GetKeyDown(KeyCode.F2))
            {
                _networkItemPool.Despawn();
            }
        }
    }
}