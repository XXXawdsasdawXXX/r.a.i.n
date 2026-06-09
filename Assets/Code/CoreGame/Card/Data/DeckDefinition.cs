using System;
using System.Collections.Generic;

namespace CoreGame.Card.Data
{
    [Serializable]
    public class DeckDefinition
    {
        public string Id;
        public string Name;
        public List<string> Cards = new();
    }
}
