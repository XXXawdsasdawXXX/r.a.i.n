using Code.CoreGame.Entities.Characters.Controllers;
using Code.CoreGame.Entities.Params;
using Core.Input;
using Core.Network;
using Core.ServiceLocator;
using Essential;
using FishNet;
using FishNet.Object;
using UnityEngine;

namespace Code.CoreGame.Entities.Characters.Hero
{
    public class Hero : Character
    {
        [field: Header("Unity components")]
        [field: SerializeField] public Rigidbody2D Rigidbody { get; private set; }
        
        [field: Space]
        
        [field: Header("Net components")]
        [field: SerializeField] public PersonName Name { get; private set; }
        [field: SerializeField] public Health Health { get; private set; }
        [field: SerializeField] public HeroColor Color { get; private set; }
        [field: SerializeField] public HeroAnimation Animation { get; private set; }
        [field: SerializeField] public HeroItemController ItemController { get; private set; }
        
        
        public override void OnStartClient()
        {
            /*Log.Info(this, $"on start client {IsOwner}", UnityEngine.Color.black);
            if (IsOwner)
            {
                UserProvider userProvider = Container.Instance.GetService<UserProvider>();
                userProvider.SetConnection(InstanceFinder.ClientManager.Connection);
                userProvider.SetHero(GetComponent<NetworkObject>());

                Debug.Log($"[HeroClientTracker] Set local hero: {gameObject.name}");
            }*/
        }

        public override void InitializeComponents()
        {
            Log.Info(this, $"Initialize components {IsOwner}", UnityEngine.Color.black);
     
            if (IsOwner)
            {
                InputManager input = Container.Instance.GetService<InputManager>();
                HeroSettings heroSettings = Container.Instance.GetConfig<HeroSettings>();

                Movement movement = new Movement(Rigidbody, input.Direction, heroSettings.MoveSpeed);
                Components.Add(typeof(Movement), movement);
                
                Miner miner = new Miner(Animation, Health);
                Components.Add(typeof(Miner), miner);
                
                
                movement.Condition.Add(() => Health.Current > 0);
                
                miner.Condition.Add(() => Rigidbody.velocity.magnitude == 0);
                miner.Condition.Add(() => Health.Current > 0);

                
                
                // Components.Add(typeof(Miner), new Miner());
            }

            IsConstructed = true;
        }
    }
}