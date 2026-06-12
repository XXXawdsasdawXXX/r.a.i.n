using Core.GameLoop;
using Core.Network;
using Core.ServiceLocator;
using CoreGame.Entities.GameObjects.Items;
using Cysharp.Threading.Tasks;
using FishNet.Object;
using UnityEngine;

namespace CoreGame.Entities.Characters.Hero
{
    public class HeroItemController :  NetworkBehaviour, IInitializeListener
    {
        public bool IsInitialized { get; set; }
        private NetworkPool _networkItemPool;
        
        
        public override void OnStartClient()
        {
            base.OnStartClient();
            
            enabled = IsOwner;
        }
        
        public UniTask Initialize()
        {
            _networkItemPool = Container.Instance.GetService<ItemPool>();
            
            return UniTask.CompletedTask;
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