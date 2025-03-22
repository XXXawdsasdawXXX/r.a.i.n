using System;
using Core.GameLoop;
using Core.ServiceLocator;
using UnityEngine;
using UnityEngine.Scripting;

namespace Core.Input
{
    [Preserve]
    public sealed class InputManager : IService, IUpdateListener
    {
        public event Action<EInputAction> ActionStarted; 
        public event Action<EInputAction> ActionEnded; 
        public Vector2 Direction { get; private set; }

        private readonly InputActionKey[] _inputActionKeys = 
        {
            new()
            {
                Key = KeyCode.F,
                Action = EInputAction.Interaction
            }    
        };

        public void GameUpdate(float deltaTime)
        {
            Direction = new Vector2(
                UnityEngine.Input.GetAxisRaw("Horizontal"),
                UnityEngine.Input.GetAxisRaw("Vertical"));

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