using System;
using UnityEngine;

namespace Core.ServiceLocator
{
    public abstract class Installer : ScriptableObject
    {
        public abstract Type[] GetTypes();
    }
}