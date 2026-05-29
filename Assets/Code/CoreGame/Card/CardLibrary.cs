using System;
using Core.Libraries;
using CoreGame.Card.Data;
using UnityEngine;

namespace CoreGame.Card
{
    [CreateAssetMenu(fileName = "Library_Cards", menuName = "Game/Library/Cards")]
    public class CardLibrary : ScriptableObject
    {
        [field: SerializeField] public AllCardCollection AllCards { get; private set; }

        [field: SerializeField] public CardConfiguration[] DefaultCardsDeck { get; private set; }
    }

    [Serializable]
    public class AllCardCollection : Library<string, CardConfiguration>
    {
        protected override bool ThisIs(CardConfiguration value, string key)
        {
            if (value == null || string.IsNullOrEmpty(key))
            {
                return false;
            }
            
            return value.Id.Equals(key);
        }
    }
}