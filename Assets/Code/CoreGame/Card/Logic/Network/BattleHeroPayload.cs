using System;
using System.Collections.Generic;
using Core.Save;

namespace CoreGame.Card.Logic.Network
{
    [Serializable]
    public struct BattleHeroPayload
    {
        public string HeroId;
        public string Name;
        public int Health;
        public int Gold;
        public HeroStats Stats;
        public string SelectedDeckId;
        public List<string> Deck;
        public List<SavedDeckDefinition> Decks;

        public static BattleHeroPayload FromHeroModel(HeroModel hero)
        {
            if (hero == null)
            {
                return default;
            }

            return new BattleHeroPayload
            {
                HeroId = hero.HeroId,
                Name = hero.Name,
                Health = hero.Health,
                Gold = hero.Gold,
                Stats = hero.Stats,
                SelectedDeckId = hero.SelectedDeckId,
                Deck = hero.Deck != null ? new List<string>(hero.Deck) : new List<string>(),
                Decks = hero.Decks != null ? new List<SavedDeckDefinition>(hero.Decks) : new List<SavedDeckDefinition>()
            };
        }

        public HeroModel ToHeroModel()
        {
            return new HeroModel
            {
                HeroId = HeroId,
                Name = Name,
                Health = Health,
                Gold = Gold,
                Stats = Stats ?? new HeroStats(),
                SelectedDeckId = SelectedDeckId,
                Deck = Deck ?? new List<string>(),
                Decks = Decks ?? new List<SavedDeckDefinition>()
            };
        }
    }
}
