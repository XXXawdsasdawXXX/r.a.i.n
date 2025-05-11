using System;
using Code.CoreGame.Common.Collisions;
using Code.CoreGame.Entities.Characters.Controllers;
using Core.Extensions;
using Core.GameLoop;
using Core.Network;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Code.CoreGame.Harvest
{
    [Serializable]
    public class Resource : Essential.Mono, IInitializeListener ,ISubscriber
    {
        public event Action<Resource> Mained;
        public bool IsInitialized { get; set; }
        [field: SerializeField] public EResource Type { get; private set; }
        [field: SerializeField] public float2 Position { get; private set; }

        [SerializeField] private InteractionTrigger _interactionTrigger;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        private UserProvider _userProvider;
        
        public UniTask Initialize()
        {
            _userProvider = Container.Instance.GetService<UserProvider>();
       
            return UniTask.CompletedTask;
        }
        
        public void Subscribe()
        {
            _interactionTrigger.InteractionPerformed += OnInteractionPerformed;
        }

        public void Unsubscribe()
        {
            _interactionTrigger.InteractionPerformed -= OnInteractionPerformed;
        }

        private void OnInteractionPerformed()
        {
            Mained?.Invoke(this);
        }
        
        
#if UNITY_EDITOR
        public void Validate(ResourceConfig resourceConfig)
        {
            Position = transform.position.AsFloat2();
        }
#endif
    
    }
}