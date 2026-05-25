using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

namespace CoreGame.Card
{
    [CreateAssetMenu(fileName = "Companion_", menuName = "Game/Battle/Companion")]
    public class CompanionConfiguration : ScriptableObject
    {
        [field: SerializeField] public string Name { get; private set; }
        
        [field: SerializeField] public int Health { get; private set; }
        
        [field: SerializeField] public int Armor { get; private set; }
        
        [field: SerializeField] public List<CardConfiguration> Cards { get; private set; }

        [field: SerializeField] public AnimatorController AnimatorController { get; private set; }
    }
}