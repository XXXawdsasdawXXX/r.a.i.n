using Code.CoreGame.Entities.Characters.Controllers;
using Code.CoreGame.Entities.Characters.Interfaces;
using Core.Data;
using Core.GameLoop;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using FishNet.Component.Animating;
using FishNet.Object;
using UnityEngine;

namespace Code.CoreGame.Entities.Characters.Hero
{
    public class HeroAnimation : NetworkBehaviour, IHarvestAnimator, 
        IInitializeListener, IUpdateListener
    {
        public bool IsInitialized { get; set; }
        public string RuntimeListenerName => "HeroAnimation";
        
        [SerializeField] private Animator _animator;
        [SerializeField] private NetworkAnimator _networkAnimator;
        
        [SerializeField] private Transform _viewBody;
        [SerializeField] private Rigidbody2D _rigidbody2D;

        private Cache<Vector3> _velocityCache;
        private Miner _mainer;

        public UniTask Initialize()
        {
            if (!IsOwner)
            {
                return UniTask.CompletedTask;
            }
            
            _velocityCache = new Cache<Vector3>();
            
            return UniTask.CompletedTask;
        }

        public void GameUpdate(float deltaTime)
        {
            if (!IsOwner)
            {
                return;
            }

            if (_velocityCache.Update(_rigidbody2D.velocity))
            {
                _animator.SetFloat(AnimatorKey.SPEED, _rigidbody2D.velocity.magnitude);
       
                if (_rigidbody2D.velocity.x != 0)
                {
                    _rotateServerRpc(_rigidbody2D.velocity.x);
                }
            }
        }

        [ServerRpc]
        public void StartHarvest()
        {
            _animator.SetBool(AnimatorKey.HARVEST, true);
            _networkAnimator.SendAll();
        }
        
        [ServerRpc]
        public void StopHarvest()
        {
            _animator.SetBool(AnimatorKey.HARVEST, false);
            _networkAnimator.SendAll();
        }

        [ServerRpc]
        private void _rotateServerRpc(float velocityX)
        {
            float forward = velocityX > 0 ? -1 : 1;
            _rotateObserversRpc(forward);
        }

        [ObserversRpc]
        private void _rotateObserversRpc(float forward)
        {
            _viewBody.localScale = new Vector3(forward, 1, 1);
        }
    }
}