using CoreGame.Card.Logic.AI;
using UnityEngine;

namespace CoreGame.Card.Data
{
    [CreateAssetMenu(fileName = "EnemyDeck_", menuName = "Game/Battle/Enemy Deck Profile")]
    public class EnemyDeckProfile : ScriptableObject
    {
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public string DeckName { get; private set; }
        [field: SerializeField] public string[] Cards { get; private set; }
        [field: SerializeField] public EEnemyAIDifficulty Difficulty { get; private set; } = EEnemyAIDifficulty.Normal;
    }
}
