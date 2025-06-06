using System;
using Core.Extensions;
using Core.GameLoop;
using CoreGame.Common.Collisions;
using Essential;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Unity.Mathematics;
using UnityEngine;

namespace CoreGame.Harvest
{
    [Serializable]
    public class Resource : NetworkBehaviour ,ISubscriber
    {
        public event Action<Resource> HarvestStarted;
        public event Action<Resource> Changed;
        public event Action<Resource> HarvestEnded;
        [field: SerializeField] public EResource Type { get; private set; }
        [field: SerializeField] public float2 Position { get; private set; }
        [field: SerializeField] public ResourceConfig Config { get; private set; }
        public bool IsMax => CurrentValue >= Config.MaxValue;
        public bool IsEnded => CurrentValue <= 0;
        public int CurrentValue => _current.Value;
        
        private readonly SyncVar<int> _current = new();

        [SerializeField] private InteractionTrigger _interactionTrigger;
        [SerializeField] private SpriteRenderer _spriteRenderer;

        public override void OnStartClient()
        {
            _current.Value = Config.MaxValue;
            
            base.OnStartClient();
        }

        public void Subscribe()
        {
            _interactionTrigger.InteractionPerformed += _onInteractionPerformed;
            _current.OnChange += _onChange;
        }

        public void Unsubscribe()
        {
            _interactionTrigger.InteractionPerformed -= _onInteractionPerformed;
            _current.OnChange -= _onChange;
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetValue(int value)
        {
            _current.Value = value;
            
            Changed?.Invoke(this);
        }

        [ServerRpc(RequireOwnership = false)]
        public void UpdateValue(int value)
        {
            _current.Value += value;

            if (_current.Value > Config.MaxValue)
            {
                _current.Value = Config.MaxValue;
            }
            else if (_current.Value < 0)
            {
                _current.Value = 0;
            }
            
            Changed?.Invoke(this);
        }

        private void _onInteractionPerformed()
        {
            HarvestStarted?.Invoke(this);
        }

        private void _onChange(int prev, int next, bool asserver)
        {
            Log.Info(this, $"{gameObject.name}: _onChange {prev} => {_current.Value} ");
            Changed?.Invoke(this);
        }

#if UNITY_EDITOR

        public void Validate(ResourceConfig resourceConfig)
        {
            Position = transform.position.AsFloat2();
            
            Config = resourceConfig;
         
            _spriteRenderer.sprite = resourceConfig.WorldView;

            gameObject.name = $"resource_{Type,10} [{Position.x,5}:{Position.x,5}]";
        }
#endif
    }
}