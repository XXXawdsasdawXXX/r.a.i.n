using System;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace Game.Entities.Params
{
    public class Health : NetworkBehaviour
    {
        public event Action Changed;
        public float Current => _health.Value;
        public float Max => 100;

        private readonly SyncVar<float> _health = new();

        public override void OnStartClient()
        {
            enabled = IsOwner;

            _health.Value = Max;

            _health.OnChange += _onHealthChange;
        }

        private void OnDestroy()
        {
            _health.OnChange -= _onHealthChange;
        }

        [ServerRpc(RequireOwnership = false)]
        public void UpdateHealth(int value)
        {
            _health.Value += value;

            Changed?.Invoke();
        }

        public float GetNormalize()
        {
            return _health.Value / Max;
        }

        private void _onHealthChange(float prev, float next, bool asserver)
        {
            Changed?.Invoke();
        }
    }
}