using Code.CoreGame.Entities.GameObjects.Items;
using Core.Network;
using Core.ServiceLocator;
using FishNet.Object;
using UnityEngine;

namespace Code.CoreGame.Entities.Characters.Hero
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