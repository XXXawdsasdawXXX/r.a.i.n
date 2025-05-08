using Code.CoreGame.Entities.Params;
using Core.Network;
using Core.ServiceLocator;
using FishNet;
using FishNet.Object;
using UnityEngine;

namespace Code.CoreGame.Entities.Hero
{
    public class Hero : NetworkBehaviour
    {
        [field: Header("Unity components")]
        [field: SerializeField] public Rigidbody2D Rigidbody { get; private set; }
        
        [field: Space]
        
        [field: Header("Net components")]
        [field: SerializeField] public PersonName Name { get; private set; }
        [field: SerializeField] public Health Health { get; private set; }
        [field: SerializeField] public HeroMovement Movement { get; private set; }
        [field: SerializeField] public HeroColor Color { get; private set; }
        [field: SerializeField] public HeroAnimation Animation { get; private set; }
        [field: SerializeField] public HeroItemController ItemController { get; private set; }
        
        
        public override void OnStartClient()
        {
            if (IsOwner)
            {
                UserProvider userProvider = Container.Instance.GetService<UserProvider>();
                userProvider.SetConnection(InstanceFinder.ClientManager.Connection);
                userProvider.SetHero(GetComponent<NetworkObject>());

                Debug.Log($"[HeroClientTracker] Set local hero: {gameObject.name}");
            }
        }
    }
}