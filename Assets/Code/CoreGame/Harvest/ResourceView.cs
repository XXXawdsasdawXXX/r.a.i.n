using System;
using Core.Data.RangeFloat;
using Core.GameLoop;
using CoreGame.Entities.Animation;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CoreGame.Harvest
{
    public class ResourceView : Essential.Mono, ISubscriber
    {
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
            _animator.SetFloat(
                AnimatorKey.PARAM_RESOURCE_VALUE, 
                (float)_resource.CurrentValue / _resource.Config.MaxValue);
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