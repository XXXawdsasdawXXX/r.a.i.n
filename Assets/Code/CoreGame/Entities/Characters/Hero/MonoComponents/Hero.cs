using Core.GameLoop;
using Core.Input;
using Core.Save;
using Core.ServiceLocator;
using CoreGame.Entities.Animation;
using CoreGame.Entities.Characters.Controllers;
using CoreGame.Entities.Params;
using Essential;
using UnityEngine;

namespace CoreGame.Entities.Characters.Hero
{
    public class Hero : Character, ISubscriber
    {
        public HeroModel Model { get; private set; }
        
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
                
                Model = Container.Instance.GetService<GameModel>().Hero;
                Model.HeroId = OwnerId.ToString();
       
                Movement movement = new(Rigidbody, input.Direction, heroSettings.MoveSpeed);
                Components.Add(typeof(Movement), movement);
                
                Miner miner = new Miner(Animation, Health);
                Components.Add(typeof(Miner), miner);
                
                movement.Condition.Add(() => Health.Current > 0);
                movement.Condition.Add(() => Animation.CurrentState is not 
                    AnimatorKey.ECharacterAnimationState.EAT and not 
                    AnimatorKey.ECharacterAnimationState.HARVEST);
                
                miner.Condition.Add(() => input.Direction.Value == Vector2.zero);
                miner.Condition.Add(() => Health.Current > 0);
                
                Health.Set(Model.Health);
                Name.SetName(Model.Name);
            }

            IsConstructed = true;
        }

        public void Subscribe()
        {
            Health.Changed += _onHealthChanged;
        }

        public void Unsubscribe()
        {   
            Health.Changed -= _onHealthChanged;
        }

        private void _onHealthChanged()
        {
            if (IsOwner && Model != null)
            {
                Model.Health = Health.Current;
            }
        }
    }
}