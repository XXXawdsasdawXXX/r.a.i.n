using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Core.Save
{
    [CreateAssetMenu(fileName = "Settings_SaveLoad", menuName = "Game/Settings/Save")]
    public class SaveSettings : ScriptableObject
    {
        [field: SerializeField] public GameModel DefaultModel { get; private set; }

        public JsonSerializerSettings JSONSettings { get; } = new()
        {
            MissingMemberHandling = MissingMemberHandling.Ignore,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            Converters = new List<JsonConverter>()
            {
                new Float2Converter()
            }
        };
    }
}