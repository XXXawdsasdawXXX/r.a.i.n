using System;
using Core.Libraries;

namespace CoreGame.Card
{
    [Serializable]
    public class CardLibrary : Library<string, CardConfiguration>
    {
        protected override bool ThisIs(CardConfiguration value, string key)
        {
            return value.Id.Equals(key);
        }
    }
}