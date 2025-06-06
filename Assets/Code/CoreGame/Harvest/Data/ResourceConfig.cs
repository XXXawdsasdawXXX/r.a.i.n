using CoreGame.Entities;
using CoreGame.Entities.Animation;
using UnityEngine;

namespace CoreGame.Harvest
{
    [CreateAssetMenu(fileName = "Resource_", menuName = "Game/Resource/Resource")]
    public class ResourceConfig : ScriptableObject
    {
        [field: SerializeField] public EResource Type { get; private set; }
        
        [field: SerializeField] public AnimatorKey.EHarvestType HarvestType { get; private set; }
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: SerializeField] public Sprite WorldView  { get; private set; }
        [field: SerializeField] public int MaxValue { get; private set; }
        
        [field: SerializeField, Tooltip("Game hours")] 
        public float RemainingTime { get; private set; }

        [field: SerializeField, Tooltip("Real seconds")] 
        public float HarvestTime { get; private set; }

        [field: SerializeField, Tooltip("Character health")] 
        public int HealthPrice { get; private set; }

        [field: SerializeField, Tooltip("Character health")] 
        public int ResourcePerTick { get; private set; }

    }
}