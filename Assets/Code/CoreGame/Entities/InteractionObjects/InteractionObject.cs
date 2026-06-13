using System;
using CoreGame.Common.Collisions;
using UnityEngine;

namespace CoreGame.Entities.InteractionObjects
{
    public abstract class InteractionObject : Essential.Mono
    {
        public event Action<InteractionObject> InteractionStarted;
       
        [field: SerializeField] 
        public InteractionTrigger Trigger { get; private set; }
        
        public EInteractionObjectType Type { get; private set; }
        

        public virtual void StartInteraction()
        {
            InteractionStarted?.Invoke(this);
        }
    }
}