using UnityEngine;

namespace Core.Libraries.Configs
{
    [CreateAssetMenu(fileName = "Storage_Config", menuName = "Game/Storage/Config")]
    public class ConfigStorage : ScriptableObject
    {
        [field: SerializeField] public ScriptableObject[] Configs { get; private set; }

        public T Get<T>() where T : ScriptableObject
        {
            foreach (ScriptableObject config in Configs)
            {
                if (config is T typedConfig)
                {
                    return typedConfig;
                }
            }

            return null;
        }
    }
}