using System;
using FishNet.Object;
using FishNet.Object.Synchronizing;

namespace CoreGame.Entities.Params
{
    public class Health : NetworkBehaviour
    {
        public event Action Changed;
        public event Action TookDamage;
        public int Current => _health.Value;
        public int Max => 100;

        private readonly SyncVar<int> _health = new();

        public override void OnStartClient()
        {
            enabled = IsOwner;

            _health.Value = Max;

            _health.OnChange += _onHealthChange;
        }

        protected  override void OnDestroy()
        {
            _health.OnChange -= _onHealthChange;
        }

        [ServerRpc(RequireOwnership = false)]
        public void Set(int value)
        {
            _health.Value = value;

            Changed?.Invoke();
        }

        [ServerRpc(RequireOwnership = false)]
        public void UpdateHealth(int value)
        {
            _health.Value += value;

            Changed?.Invoke();
        }

        public float GetNormalize()
        {
            return (float)_health.Value / Max;
        }

        private void _onHealthChange(int prev, int next, bool asserver)
        {
            Changed?.Invoke();

            if (prev > next)
            {
                TookDamage?.Invoke();
            }
        }
    }
}