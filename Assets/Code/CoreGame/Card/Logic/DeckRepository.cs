using System.Collections.Generic;
using System.Linq;
using Core.Save;
using CoreGame.Card.Data;

namespace CoreGame.Card.Logic
{
    public interface IDeckRepository
    {
        DeckDefinition ResolvePlayerDeck(HeroModel hero, CardLibrary cardLibrary);
        DeckDefinition ResolveEnemyDeck(HeroModel hero, EnemyDeckProfile enemyDeckProfile, CardLibrary cardLibrary);
    }

    public class DeckRepository : IDeckRepository
    {
        private const string DEFAULT_DECK_ID = "player_default";

        public DeckDefinition ResolvePlayerDeck(HeroModel hero, CardLibrary cardLibrary)
        {
            if (hero == null)
            {
                return new DeckDefinition();
            }

            if (hero.Decks != null && hero.Decks.Count > 0)
            {
                SavedDeckDefinition selectedDeck = hero.Decks
                    .FirstOrDefault(deck => deck != null && deck.Id == hero.SelectedDeckId)
                    ?? hero.Decks.FirstOrDefault(deck => deck != null);

                if (selectedDeck != null && selectedDeck.Cards != null && selectedDeck.Cards.Count > 0)
                {
                    return new DeckDefinition
                    {
                        Id = string.IsNullOrEmpty(selectedDeck.Id) ? DEFAULT_DECK_ID : selectedDeck.Id,
                        Name = string.IsNullOrEmpty(selectedDeck.Name) ? "Player Deck" : selectedDeck.Name,
                        Cards = selectedDeck.Cards.ToList()
                    };
                }
            }

            if (hero.Deck != null && hero.Deck.Count > 0)
            {
                return new DeckDefinition
                {
                    Id = DEFAULT_DECK_ID,
                    Name = "Player Deck",
                    Cards = hero.Deck.ToList()
                };
            }

            return new DeckDefinition
            {
                Id = DEFAULT_DECK_ID,
                Name = "Default Deck",
                Cards = cardLibrary?.DefaultCardsDeck?.ToList() ?? new List<string>()
            };
        }

        public DeckDefinition ResolveEnemyDeck(HeroModel hero, EnemyDeckProfile enemyDeckProfile, CardLibrary cardLibrary)
        {
            if (enemyDeckProfile != null && enemyDeckProfile.Cards != null && enemyDeckProfile.Cards.Length > 0)
            {
                return new DeckDefinition
                {
                    Id = string.IsNullOrEmpty(enemyDeckProfile.Id) ? "enemy_profile" : enemyDeckProfile.Id,
                    Name = string.IsNullOrEmpty(enemyDeckProfile.DeckName) ? "Enemy Deck" : enemyDeckProfile.DeckName,
                    Cards = enemyDeckProfile.Cards.ToList()
                };
            }

            if (hero != null && hero.Deck != null && hero.Deck.Count > 0)
            {
                return new DeckDefinition
                {
                    Id = "enemy_hero_deck",
                    Name = "Enemy Hero Deck",
                    Cards = hero.Deck.ToList()
                };
            }

            return new DeckDefinition
            {
                Id = "enemy_default",
                Name = "Default Enemy Deck",
                Cards = cardLibrary?.DefaultCardsDeck?.ToList() ?? new List<string>()
            };
        }
    }
}
