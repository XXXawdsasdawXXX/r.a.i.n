using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

namespace CoreGame.Card.Data
{
    [CreateAssetMenu(fileName = "Companion_", menuName = "Game/Battle/Companion")]
    public class CompanionConfiguration : ScriptableObject
    {
        [field: SerializeField] public int Health { get; private set; }
        
        [field: SerializeField] public int Armor { get; private set; }
        
        [field: SerializeField] public List<string> Cards { get; private set; }
        
        [field: SerializeField, Min(0)] public int CardsPerTurn { get; private set; } = 1;
        
        [field: SerializeField, Min(0)] public int LifetimeTurns { get; private set; } = 0;

        [field: SerializeField] public AnimatorController AnimatorController { get; private set; }
    }
}