using Core.GameLoop;
using CoreGame.Harvest;
using UI.Components;
using UnityEngine;

namespace UI.Game
{
    public class UIResourceValueTab : Essential.Mono, ISubscriber
    {
        [SerializeField] private Resource _resource;
        [SerializeField] private UIText _text;
        [SerializeField] private UIImage _field;

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
            _field.SetFillAmount((float)_resource.CurrentValue/_resource.Config.MaxValue);
        }
    }
}