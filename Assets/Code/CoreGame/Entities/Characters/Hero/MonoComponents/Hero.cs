using System;
using System.Collections.Generic;
using System.Linq;
using Core.GameLoop;
using Core.Input;
using Core.Save;
using Core.ServiceLocator;
using CoreGame.Card;
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
        
        
        public override void InitializeComponents()
        {
            Log.Info(this, $"Initialize hero.\nis owner = {IsOwner}.\n owner id = {OwnerId}", UnityEngine.Color.black);
     
            if (IsOwner)
            {
                InputManager input = Container.Instance.GetService<InputManager>();
                HeroSettings heroSettings = Container.Instance.GetSO<HeroSettings>();
                CardLibrary cardLibrary = Container.Instance.GetSO<CardLibrary>();
                GameModel gameModel = Container.Instance.GetService<GameModel>();
                
                Model = gameModel.Hero;
                Model.HeroId = ObjectId.ToString();
         
                if (Model.Deck == null || Model.Deck.Count == 0)
                {
                    Model.Deck = cardLibrary.DefaultCardsDeck.ToList();
                }
                
                Movement movement = new(Rigidbody, input.Direction, heroSettings.MoveSpeed);
                Components.Add(typeof(Movement), movement);
                
                Mainer mainer = new(Animation, Health);
                Components.Add(typeof(Mainer), mainer);
                
                movement.Condition.Add(() => Animation.CurrentState is not 
                    AnimatorKey.ECharacterAnimationState.EAT and not 
                    AnimatorKey.ECharacterAnimationState.HARVEST);
                
                mainer.Condition.Add(() => input.Direction.Value == Vector2.zero);

                foreach (KeyValuePair<Type, ICharacterComponent> characterComponent in Components)
                {
                    characterComponent.Value.Condition.Add(() => Health.Current > 0);
                    characterComponent.Value.Condition.Add(() => !Model.InBattle);
                }
                
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