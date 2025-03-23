using Core.Data;
using Core.GameLoop;
using Cysharp.Threading.Tasks;
using FishNet.Object;
using UnityEngine;

namespace Game.Entities
{
    public class HeroAnimation : NetworkBehaviour, IInitializeListener, IUpdateListener
    {
        private static readonly int _speedHash = Animator.StringToHash("Speed");
        
        [SerializeField] private Animator _animator;
        [SerializeField] private Transform _viewBody;
        [SerializeField] private Rigidbody2D _rigidbody2D;

        private Cache<Vector3> _velocityCache;

        public UniTask GameInitialize()
        {
            _velocityCache = new Cache<Vector3>();
            
            return UniTask.CompletedTask;
        }

        public void GameUpdate(float deltaTime)
        {
            if (_velocityCache.Update(_rigidbody2D.velocity))
            {
                _animator.SetFloat(_speedHash, _rigidbody2D.velocity.magnitude);
                if (_rigidbody2D.velocity.x != 0)
                {
                    float forward = _rigidbody2D.velocity.x > 0 ? -1 : 1;
                    _viewBody.localScale = new Vector3(forward, 1, 1);
                }
            }
        }
    }
}