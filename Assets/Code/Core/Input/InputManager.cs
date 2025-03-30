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

        public string RuntimeListenerName => "InputManager";
        public Vector2 Direction { get; private set; }
        public Vector3 MousePosition { get; private set; }


        private readonly InputActionKey[] _inputActionKeys = 
        {
            new()
            {
                Key = KeyCode.F,
                Action = EInputAction.Interaction
            },
            new()
            {
                Key = KeyCode.Mouse0,
                Action = EInputAction.LeftClick
            }
        };

        public void GameUpdate(float deltaTime)
        {
            Direction = new Vector2(
                UnityEngine.Input.GetAxisRaw("Horizontal"),
                UnityEngine.Input.GetAxisRaw("Vertical"));

            MousePosition = UnityEngine.Input.mousePosition;
            
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