using System;
using UnityEngine;

namespace Essential
{
    public class Mono : MonoBehaviour
    {
        public static event Action<Mono> Started;
        public static event Action<Mono> Destroyed;

        [field: SerializeField] public UniqueID ID;


        private void Start()
        {
            Started?.Invoke(this);
        }

        protected virtual void OnEnable()
        {
            if (ID.IsEmpty())
            {
                ID.Validate();
            }
        }

        private void OnDestroy()
        {
            Destroyed?.Invoke(this);
        }

        protected virtual void OnValidate()
        {
            if (ID.IsEmpty())
            {
                ID.Validate();
            }
        }
    }
}