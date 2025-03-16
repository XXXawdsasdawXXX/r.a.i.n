using UnityEngine;

namespace Core.Input
{
    [CreateAssetMenu(fileName = "InputConfig", menuName = "Game/Config/InputConfig")]
    public sealed class InputConfig : ScriptableObject
    {
        [field: SerializeField] public InputActionKey[] InputActionKeys { get; private set; }
    }
}