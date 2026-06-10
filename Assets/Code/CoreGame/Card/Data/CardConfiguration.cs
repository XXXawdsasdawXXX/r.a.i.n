using System.Collections.Generic;
using UnityEngine;

namespace CoreGame.Card.Data
{
    [CreateAssetMenu(fileName = "Card_", menuName = "Game/Battle/Card")]
    public class CardConfiguration : ScriptableObject
    {
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: SerializeField] public ECardType Type { get; private set; }
        [field: SerializeField] public int BaseEnergyCost { get; private set; }
        [field: SerializeField] public int Charges { get; private set; } = 0; // 0 = без зарядов
        [field: SerializeField] public List<CardEffectConfiguration> Effects { get; private set; }
    }
}