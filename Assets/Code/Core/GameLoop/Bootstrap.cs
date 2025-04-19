using Core.StateMachine;
using UnityEngine;

namespace Core.GameLoop
{
    internal sealed class Bootstrap : MonoBehaviour
    {
        public void Awake()
        {
            GameStateMachine gameStateMachine = new();
        }
    }
}