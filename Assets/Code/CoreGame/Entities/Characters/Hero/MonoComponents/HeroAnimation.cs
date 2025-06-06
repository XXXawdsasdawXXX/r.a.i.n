using Core.Data;
using Core.GameLoop;
using CoreGame.Entities.Animation;
using CoreGame.Entities.Characters.Interfaces;
using Cysharp.Threading.Tasks;
using Essential;
using FishNet.Object;
using UnityEngine;

namespace CoreGame.Entities.Characters.Hero
{
    public class HeroAnimation : NetworkBehaviour, IHarvestAnimator, IAnimationStateReader,
    IInitializeListener, IUpdateListener
    {
        public bool IsInitialized { get; set; }
        public string RuntimeListenerName => "HeroAnimation";

        public AnimatorKey.ECharacterAnimationState CurrentState { get; private set; }
        
        [SerializeField] private Animator _animator;
        [SerializeField] private Transform _viewBody;
        [SerializeField] private Rigidbody2D _rigidbody2D;

        private Cache<Vector3> _velocityCache;

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
                _animator.SetFloat(AnimatorKey.PARAM_SPEED, _rigidbody2D.velocity.magnitude);
       
                if (_rigidbody2D.velocity.x != 0)
                {
                    _rotateServerRpc(_rigidbody2D.velocity.x);
                }
            }
        }
        
        [ServerRpc] public void StartEat() => 
            _animator.SetBool(AnimatorKey.PARAM_EAT, true);
        [ServerRpc] public void StopEat() => 
            _animator.SetBool(AnimatorKey.PARAM_EAT, false);
        [ServerRpc] public void StartMine(AnimatorKey.EHarvestType harvestType) => 
            _animator.SetInteger(AnimatorKey.PARAM_HARVEST_TYPE, (int)harvestType);

        [ServerRpc] public void StopMine() => 
            _animator.SetInteger(AnimatorKey.PARAM_HARVEST_TYPE, 0);

        [ServerRpc]
        private void _rotateServerRpc(float velocityX)
        {
            float forward = velocityX < 0 ? -1 : 1;
            _rotateObserversRpc(forward);
        }

        [ObserversRpc]
        private void _rotateObserversRpc(float forward)
        {
            _viewBody.localScale = new Vector3(forward, 1, 1);
        }

        public void EnteredState(int stateHash)
        {
            if (AnimatorKey.CHARACTER_STATES.ContainsKey(stateHash))
            {
                CurrentState = AnimatorKey.CHARACTER_STATES[stateHash];
                
                Log.Info(this, $"ENTER {CurrentState}");
            }
        }

        public void ExitedState(int stateHash)
        {
            if (AnimatorKey.CHARACTER_STATES.ContainsKey(stateHash))
            {
                CurrentState = AnimatorKey.CHARACTER_STATES[stateHash];
                
                Log.Info(this, $"EXIT {CurrentState}");
            }
        }
    }
}