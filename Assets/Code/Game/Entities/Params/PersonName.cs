using System;
using Core.GameLoop;
using Cysharp.Threading.Tasks;
using Essential;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace Game.Entities.Params
{
    public class PersonName : NetworkBehaviour, ISubscriber
    {
        public event Action<string> Changed;
        public string Name => _name.Value;

        private readonly SyncVar<string> _name = new();

        public override void OnStartClient()
        {
            //enabled = IsOwner;
            
            SetName(GetHashCode().ToString());
        }

        public UniTask Subscribe()
        {
            _name.OnChange += OnNameChanged;

            return UniTask.CompletedTask;
        }

        public void Unsubscribe()
        {
            _name.OnChange -= OnNameChanged;
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void SetName(string personName)
        {
            _name.Value = personName;
        }

        private void OnNameChanged(string prev, string next, bool asserver)
        {
            Changed?.Invoke(next);
        }
    }
}