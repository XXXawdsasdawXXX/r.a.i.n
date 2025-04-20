using UnityEngine;

namespace Core.Libraries.Configs
{
    [CreateAssetMenu(fileName = "Library_Config", menuName = "Game/Library/Config")]
    public class ConfigLibrary : ScriptableObject
    {
        [field: SerializeField] public ScriptableObject[] Configs { get; private set; }
    }
}