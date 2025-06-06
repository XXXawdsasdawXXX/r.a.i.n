using System;
using Core.Data.RangeFloat;
using Core.GameLoop;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CoreGame.Entities.Animation
{
    public class AnimationSpeedRandomizer : Essential.Mono, IStartListener
    {
        [SerializeField] private Animator _animator;
        [SerializeField, MinMaxRangeFloat(0, 3)] private RangedFloat _animationSpeed = new(1f, 1f);

        public UniTask GameStart()
        {
            if (Math.Abs(_animationSpeed.MinValue - _animationSpeed.MaxValue) > 0)
            {
                _animator.SetFloat(AnimatorKey.PARAM_ANIMATION_SPEED, _animationSpeed.GetRandomValue());
            }
            
            return UniTask.CompletedTask;
        }

#if UNITY_EDITOR

        private void OnValidate()
        {
            if (_animator == null)
            {
                _animator = GetComponent<Animator>();
            }
        }
#endif
    }
}