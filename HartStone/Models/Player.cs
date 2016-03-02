using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using HartStone.Classes;

namespace HartStone.Models
{
    public class Player : IPlayer
    {
        public TAG_CLASS CurrentClass { get; set; }
        public IEnumerable<HSCard> CurrentDeck { get; set; }
        public string Name { get; set; }
        public int CurrentHealth { get; set; }
        public int CurrentFatigue = 0;
        public int CurrentMana = 0;

        public Player(TAG_CLASS playerClass, HSCard[] cards)
        {
            CurrentClass = playerClass;
            HSCard[] currentDeck;

            if (GetDeck(playerClass, cards, out currentDeck))
                CurrentDeck = currentDeck;
            else
                throw new ArgumentException("Unable to retrieve deck from card collection!");

            Name = CurrentDeck.Where(w => w != null && !string.IsNullOrWhiteSpace(w.ArtistName)).Random(1).First().ArtistName;
        }

        public static bool GetDeck(TAG_CLASS playerClass, HSCard[] cards, out HSCard[] deck)
        {

            TAG_CARDTYPE[] validTypes = { TAG_CARDTYPE.ABILITY, TAG_CARDTYPE.MINION, TAG_CARDTYPE.WEAPON, };
            HSCard[] legalCards = cards.Where(w => w.IsCollectible && validTypes.Contains(w.Type)).ToArray();

            for (int i = 1; i <= 2; i++ )
            {
                foreach (HSCard card in legalCards.Where(w => w.IsCollectible && !w.HealTarget && w.ManaCost == i).Random(10))
                {
                    card.HealTarget = true;
                    card.HealMinAmount = card.HealMaxAmount = card.ManaCost;
                }
                foreach (HSCard card in legalCards.Where(w => w.IsCollectible && !w.DamageTarget && w.ManaCost == i).Random(10))
                {
                    card.DamageTarget = true;
                    card.DamageMinAmount = card.DamageMaxAmount = card.ManaCost;
                }
            }

            // If not for this criteria, I'd use my Mage Mech deck =-P
            // http://www.hearthpwn.com/decks/272022-laughingman-mech-mage (also a demo of my programming skills)
            // It's outdated now and loses just about all the time but was fun for a while
            var cost1dmg1 = legalCards.Where(w => w.ManaCost == 1 && (w.Attack == 1 || w.DamageMinAmount == 1)).Random(10); // 10 legalCards that are 1 cost 1 damage
            var cost2dmg2 = legalCards.Where(w => w.ManaCost == 2 && (w.Attack == 2 || w.DamageMinAmount == 2)).Random(4); // 4 legalCards that deal 2 damage and cost 2 mana
            var cost3dmg3 = legalCards.Where(w => w.ManaCost == 3 && (w.Attack == 3 || w.DamageMinAmount == 3)).Random(2); // 2 legalCards that deal 3 damage and cost 3 mana
            var cost4dmg4 = legalCards.Where(w => w.ManaCost == 4 && (w.Attack == 4 || w.DamageMinAmount == 4)).Random(2); // 2 legalCards that deal 4 damage and cost 4 mana
            var cost5dmg5 = legalCards.Where(w => w.ManaCost == 5 && (w.Attack == 5 || w.DamageMinAmount == 5)).Random(2); // 2 legalCards that deal 5 damage and cost 5 mana
            var cost1heal1 = legalCards.Where(w => w.ManaCost == 1 && w.HealMinAmount >= 1).Random(5); // 5 legalCards that heal 1 point and cost 1 mana
            var cost2heal2 = legalCards.Where(w => w.ManaCost == 2 && w.HealMinAmount >= 2).Random(2); // 2 legalCards that heal 2 points and cost 2 mana
            var cost1dmg1draw = legalCards.Where(w => w.ManaCost == 1 && (w.Attack == 1 || w.DamageMinAmount >= 1)).Random(2); // 2 legalCards that are 1 cost 1 damage with draw
            var legendaries = legalCards.Where(w => w.ManaCost == 5 && w.Attack == 4 && w.ManaCrystalAmount >= 1);
            var legendary = legendaries.Random(1); // 1 legendary card that deals 4 damage, costs 5 mana, adds an extra mana crystal, outputs "You will never defeat me!"

            List<HSCard> cardsToAdd = new List<HSCard>();
            cardsToAdd.AddRange(cost1dmg1);
            cardsToAdd.AddRange(cost2dmg2);
            cardsToAdd.AddRange(cost3dmg3);
            cardsToAdd.AddRange(cost4dmg4);
            cardsToAdd.AddRange(cost5dmg5);
            cardsToAdd.AddRange(cost1heal1);
            cardsToAdd.AddRange(cost2heal2);
            cardsToAdd.AddRange(cost1dmg1draw);
            cardsToAdd.Add(legendary.First());

            deck = cardsToAdd.Where(w => w != null).ToArray();
            return deck.Any() && deck.Length == 30;
        }

    }
}
