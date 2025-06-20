using System;
using Core.GameLoop;
using Essential;
using FishNet.Object;
using FishNet.Object.Synchronizing;

namespace CoreGame.Entities.Params
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

        public void Subscribe()
        {
            _name.OnChange += OnNameChanged;
        }

        public void Unsubscribe()
        {
            _name.OnChange -= OnNameChanged;
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void SetName(string personName)
        {
            _name.Value = personName;
         
            Changed?.Invoke(personName);
        }

        private void OnNameChanged(string prev, string next, bool asserver)
        {
            Log.Info(this,$"OnNameChanged {prev}  {next} ");
            Changed?.Invoke(next);
        }
    }
}