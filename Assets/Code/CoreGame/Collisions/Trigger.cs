using System;
using UnityEngine;

namespace Code.CoreGame.Collisions
{
    public class Trigger : Essential.Mono
    {
        public event Action<GameObject> Enter; 
        public event Action<GameObject> Exit; 
        
        
        protected virtual void OnTriggerEnter2D(Collider2D col)
        {
            Enter?.Invoke(col.gameObject);    
        }
        
        protected virtual void OnTriggerExit2D(Collider2D other)
        {
            Exit?.Invoke(other.gameObject);    
        }
    }
}