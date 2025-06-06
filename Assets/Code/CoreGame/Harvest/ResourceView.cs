using Core.GameLoop;
using UnityEngine;
using UnityEngine.Serialization;

namespace CoreGame.Harvest
{
    public class ResourceView : Essential.Mono, ISubscriber
    {
        private static readonly int RESOURCE_VALUE = Animator.StringToHash("ResourceValue");
        
        [SerializeField] private Animator _animator;
        [SerializeField] private Resource _resource;

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
            _animator.SetFloat(RESOURCE_VALUE, (float)_resource.CurrentValue / _resource.Config.MaxValue);
        }

#if  UNITY_EDITOR
        private void OnValidate()
        {
            if (_resource == null)
            {
                _resource = GetComponentInParent<Resource>(true);
            }

            if (_animator == null)
            {
                _animator = GetComponent<Animator>();
            }
        }
#endif
    }
}