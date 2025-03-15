using System;
using Core.GameLoop;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using Essential;
using UnityEngine;

namespace Core.Input
{
    public sealed class InputManager : IService, IInitializeListener, IUpdateListener
    {
        public event Action<EInputAction> ActionStarted; 
        public event Action<EInputAction> ActionEnded; 
        public Vector2 Direction { get; private set; }

        private InputActionKey[] _inputActionKeys;
        
        public UniTask GameInitialize()
        {
            _inputActionKeys = Container.Instance.GetConfig<InputConfig>().InputActionKeys;
                
            Log.Info("initialize" , this);
            return UniTask.CompletedTask;
        }

        public void GameUpdate()
        {
            Direction = new Vector2(
                UnityEngine.Input.GetAxisRaw("Horizontal"),
                UnityEngine.Input.GetAxisRaw("Vertical"));
            
            Log.Info($"update input {Direction}" , this);

            foreach (InputActionKey inputActionKey in _inputActionKeys)
            {
                if (UnityEngine.Input.GetKeyDown(inputActionKey.Key))
                {
                    ActionStarted?.Invoke(inputActionKey.Action);
                }
                
                if (UnityEngine.Input.GetKeyUp(inputActionKey.Key))
                {
                    ActionEnded?.Invoke(inputActionKey.Action);
                }
            }
        }
    }
}