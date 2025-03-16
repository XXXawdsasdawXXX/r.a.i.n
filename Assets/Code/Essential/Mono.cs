using System;
using UnityEngine;

namespace Essential
{
    public class Mono : MonoBehaviour
    {
        public static event Action<Mono> Started;
        public static event Action<Mono> Destroyed;
        
        private void Start()
        {
            Started?.Invoke(this);
        }
        
        private void OnDestroy()
        {
            Destroyed?.Invoke(this);
        }
    }
}