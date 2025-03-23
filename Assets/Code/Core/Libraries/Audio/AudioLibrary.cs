using FMODUnity;
using UnityEngine;

namespace Core.Libraries
{
    [CreateAssetMenu(fileName = "Library_Audio", menuName = "Game/Library/Audio")]
    public class AudioLibrary : ScriptableObject
    {
        [field: SerializeField] public AudioEventLibrary Events { get; private set; }
    }
}