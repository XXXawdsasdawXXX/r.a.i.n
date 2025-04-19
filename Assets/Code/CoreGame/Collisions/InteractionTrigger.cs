using Core.GameLoop;
using Core.Input;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using Unity.Plastic.Newtonsoft.Json.Serialization;
using UnityEngine;

namespace Code.CoreGame.Collisions
{
    public sealed class InteractionTrigger : Trigger, IInitializeListener
    {
        public event Action InteractionPerformed;  

        private InputManager _inputManager;

        public UniTask GameInitialize()
        {
            _inputManager = Container.Instance.GetService<InputManager>();
            
            return UniTask.CompletedTask;
        }

        protected override void OnTriggerEnter2D(Collider2D col)
        {
            SubscribeToInputEvent(true);
            
            base.OnTriggerEnter2D(col);
        }

        protected override void OnTriggerExit2D(Collider2D other)
        {
            SubscribeToInputEvent(false);
            
            base.OnTriggerExit2D(other);
        }

        private void SubscribeToInputEvent(bool flag)
        {
            if (flag)
            {
                _inputManager.ActionEnded += TryInvokeInteraction;
            }
            else
            {
                _inputManager.ActionEnded -= TryInvokeInteraction;
            }
        }

        private void TryInvokeInteraction(EInputAction action)
        {
            if (action is EInputAction.Interaction)
            {
                InteractionPerformed?.Invoke();
            }
        }
    }
}