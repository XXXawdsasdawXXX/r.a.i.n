using Core.ServiceLocator;
using UnityEngine;
using UnityEngine.Scripting;

namespace Game.Camera
{
    [Preserve]
    public class CameraView : MonoView
    {
        [field: SerializeField] public UnityEngine.Camera Camera { get; private set; }

        public Vector3 ScreenToWorldPoint(Vector3 worldPosition)
        {
            return Camera.ScreenToWorldPoint(worldPosition);
        }
    }
}