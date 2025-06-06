using Core.GameLoop;
using CoreGame.Harvest;
using UI.Components;
using UnityEngine;

namespace UI.Game
{
    public class UIResourceValueText : Essential.Mono, ISubscriber
    {
        [SerializeField] private Resource _resource;
        [SerializeField] private UIText _text;

        public void Subscribe()
        {
            _resource.Changed += _onChanged;
        }

        public void Unsubscribe()
        {
            _resource.Changed -= _onChanged;
        }

        private void _onChanged(Resource _)
        {
            _text.SetText($"{_resource.CurrentValue}/{_resource.Config.MaxValue}");
        }
    }
}