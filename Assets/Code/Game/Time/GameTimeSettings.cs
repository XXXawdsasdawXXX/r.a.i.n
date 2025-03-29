using UnityEngine;

namespace Game.Time
{
    [CreateAssetMenu(fileName = "Settings_GameTime", menuName = "Game/Settings/GameTime")]
    public class GameTimeSettings : ScriptableObject
    {
        [field: SerializeField] public float TimeScale { get; private set; }
        [field: SerializeField, Range(0, 24)] public float SunriseTime { get; private set; }
        [field: SerializeField, Range(0, 24)] public float SunsetTime { get; private set; }
    }
}