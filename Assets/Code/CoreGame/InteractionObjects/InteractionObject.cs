using System;
using Code.CoreGame.Collisions;
using UnityEngine;

namespace Code.CoreGame.InteractionObjects
{
    public abstract class InteractionObject : Essential.Mono
    {
        public event Action<InteractionObject> InteractionStarted;
        
        [field: SerializeField] public InteractionTrigger Trigger { get; private set; }
        
        public EInteractionObjectType Type { get; private set; }



        public virtual void StartInteraction()
        {
            InteractionStarted?.Invoke(this);
        }
    }
}