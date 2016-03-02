using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Serialization;
using HartStone.Classes;
using HartStone.Models;

namespace HartStone
{
    public class HSEngine
    {
        public const string FilePathTemplate = @"./CardData/XML/{0}.xml";
        public const string DefaultLocale = "enUS";
        public string SelectedLocale = DefaultLocale;

        public static readonly string[] Locales = { "deDE", "enGB", "enUS", "esES", "esMX", "frFR", "itIT", "koKR", "plPL", "ptBR", "ruRU", "zhCN", "zhTW" };

        public HSCard[] Cards;
        public Player[] Players;

        public int CurrentTurn = 0;

        public bool Start()
        {
            // Localization selection - disabled due to text parsing for card data - defaults to enUS cardset
            //bool localizationSelected = false;
            //try
            //{
            //    while (!localizationSelected)
            //        localizationSelected = SelectCardLocalization(out SelectedLocale);

            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e.Message);
            //    Console.WriteLine("A request to close the application has occurred. Press any key to exit the application");
            //    Console.ReadKey(false);
            //    return;
            //}

            if (string.IsNullOrWhiteSpace(SelectedLocale) || !Locales.Contains(SelectedLocale))
            {
                Console.WriteLine("Something went wrong. The selected localization doesn't match any of the available locales. This shouldn't happen! We should probably log this!");
                throw new ArgumentException("The selected locale is invalid", "SelectedLocale");
            }

            Console.WriteLine("\r\nLocalization Detected: {0} | Beginning Card Retrieval", SelectedLocale);

            // Retrieve the cards
            bool cardsRetrieved = RetrieveCardData(SelectedLocale, out Cards);

            // If there was a problem retrieving the cards
            if (!cardsRetrieved)
            {
                // Tell the user
                Console.WriteLine("We were unable to retrieve the cards for the specified locale.");
                // Code to insert some standard base cards?
                Console.WriteLine("Without cards, the game cannot be played. Unable to continue. Press any key to exit the application.");
                // Then close the application
                Console.ReadKey(false);
                return false;
            }

            Console.WriteLine("\r\nCards were successfully retrieved!");

            //Let's take a look at the type counts for the cards retrieved.
            int abilityCount = Cards.Count(w => w.Type == TAG_CARDTYPE.ABILITY);
            int enchantmentCount = Cards.Count(w => w.Type == TAG_CARDTYPE.ENCHANTMENT);
            int gameCount = Cards.Count(w => w.Type == TAG_CARDTYPE.GAME);
            int heroCount = Cards.Count(w => w.Type == TAG_CARDTYPE.HERO);
            int heroPowerCount = Cards.Count(w => w.Type == TAG_CARDTYPE.HERO_POWER);
            int invalidCount = Cards.Count(w => w.Type == TAG_CARDTYPE.INVALID);
            int itemCount = Cards.Count(w => w.Type == TAG_CARDTYPE.ITEM);
            int minionCount = Cards.Count(w => w.Type == TAG_CARDTYPE.MINION);
            int playerCount = Cards.Count(w => w.Type == TAG_CARDTYPE.PLAYER);
            int tokenCount = Cards.Count(w => w.Type == TAG_CARDTYPE.TOKEN);
            int weaponCount = Cards.Count(w => w.Type == TAG_CARDTYPE.WEAPON);
            int legendCount = Cards.Count(w => w.Rarity == TAG_RARITY.LEGENDARY && w.Attack == 4 && w.ManaCrystalAmount >= 1);

            Console.WriteLine("\r\nAbility:\t{0}\tEnchantment:\t{1}\tGame:\t{2}\tHero:\t{3}\r\nHero Power:\t{4}\tInvalid:\t{5}\tItem:\t{6}\tMinion:\t{7}\r\nPlayer:\t\t{8}\tToken:\t\t{9}\tWeapon:\t{10}\tLegends: {11}",
                            abilityCount, enchantmentCount, gameCount, heroCount, heroPowerCount, invalidCount, itemCount, minionCount, playerCount, tokenCount, weaponCount, legendCount);

            int numberOfPlayers = 2;

            // Cards retrieved, retrieve players
            bool playersRetrieved = RetrievePlayers(numberOfPlayers, Cards, out Players);

            // If there was a problem retrieving the players or not enough players were retrieved
            if (!playersRetrieved || Players.Count() < numberOfPlayers)
            {
                // Notify the user
                Console.WriteLine("We were unable to retrieve the number of players specified.");
                Console.WriteLine("Without players, the game cannot be played. Unable to continue. Press any key to exit the application.");
                // Then close the application
                Console.ReadKey(false);
                return false;
                // Should probably redirect the user to earlier in the loop by refatoring each section into it's own method
            }
            Thread.Sleep(2000);
            Console.Write("\r\nWe've got cards. . . ");
            Thread.Sleep(1000);
            Console.Write("We've got players . . . ");
            Thread.Sleep(1000);
            Console.Write("It's time to play!");
            Thread.Sleep(2000);
            Console.WriteLine("\r\n\r\nThe {0}, {1} vs. {2} the {3}!", Players[0].CurrentClass, Players[0].Name, Players[1].Name, Players[1].CurrentClass);
            Thread.Sleep(100);
            Console.WriteLine("\r\nPlayers One and Two will be chosen randomly at the start of the game.");
            return true;
        }

        public static bool SelectCardLocalization(out string selectedLocale)
        {
            Console.WriteLine("\r\n\r\nBefore we can begin, we need to know what\r\n  localization you would like to  utilize for the card data.");

            Console.WriteLine("Type 0 and press enter to use the default {0} localized card data", DefaultLocale);

            int counter = 0;
            foreach (string locale in Locales)
            {
                Console.WriteLine("Type {0} and press enter to use {1} localized card data", ++counter, locale);
            }

            Console.WriteLine("Type Q and press enter to close the application");

            Console.Write("Please make your selection at this time: ");

            string selection = Console.ReadLine();

            int selectionValue = 0;

            if (selection != null && selection.ToLower().Contains("q"))
                throw new Exception("The user has requested that the application be closed.");

            if (string.IsNullOrWhiteSpace(selection) || !int.TryParse(selection, out selectionValue) || selectionValue < 0 || (selectionValue - 1) > Locales.Length)
            {
                Console.WriteLine("\r\nThe value you have selected is not a valid option. Please try again.\r\nTo quit, put any value containing the letter Q or close the application.");
                selectedLocale = DefaultLocale;
                return false;
            }

            selectedLocale = selectionValue > 0 ? Locales[selectionValue - 1] : DefaultLocale;

            Console.WriteLine("\r\nYou have selected to use the {0} localized card data. Is this correct?", selectedLocale);
            Console.WriteLine("Press Y for Yes | Press N for No");

            ConsoleKeyInfo confirmation = Console.ReadKey(true);

            while (confirmation.Key != ConsoleKey.Y && confirmation.Key != ConsoleKey.N)
                confirmation = Console.ReadKey(true);

            return confirmation.Key == ConsoleKey.Y;
        }

        public static bool RetrieveCardData(string selectedLocale, out HSCard[] cards)
        {
            if (string.IsNullOrWhiteSpace(selectedLocale) || !Locales.Any(a => a.Contains(selectedLocale)))
            {
                throw new ArgumentException(string.Format("The selected locale of {0} is invalid", selectedLocale), "selectedLocale");
            }

            string cardListPath = string.Format(FilePathTemplate, "CARD");
            string cardBackPath = string.Format(FilePathTemplate, "CARD_BACK");
            string cardDefPath = string.Format(FilePathTemplate, selectedLocale);

            FileInfo cardListFile = new FileInfo(cardListPath);
            FileInfo cardDefFile = new FileInfo(cardDefPath);
            //FileInfo cardBackFile = new FileInfo(cardBackPath);

            if (cardListFile == null || !cardListFile.Exists || cardDefFile == null || !cardDefFile.Exists)
            {
                throw new Exception("The file does not exist or was not able to be opened. Please check the path and try again!");
            }

            Stream cardListStream = cardListFile.OpenRead();
            XmlSerializer listSerializer = new XmlSerializer(typeof(Dbf));
            Dbf cardList = (Dbf)listSerializer.Deserialize(cardListStream);

            Stream cardDefStream = cardDefFile.OpenRead();
            XmlSerializer defSerializer = new XmlSerializer(typeof(CardDefs));
            CardDefs cardDefs = (CardDefs)defSerializer.Deserialize(cardDefStream);

            Console.WriteLine("{0} ({1} definitions) cards found in the listings", cardList.Record.Count, cardDefs.Entity.Count);

            Console.WriteLine("Processing raw card data");

            List<HSCard> newCards = new List<HSCard>() {new HSCard()
            {
                Name = "47",
                Rarity = TAG_RARITY.LEGENDARY,
                Class = 0,
                ManaCost = 5,
                Health = 15,
                Attack = 4,
                ArtistName = "Kaiser Soze",
                CardId = "DIANA",
                Charge = true,
                Taunt = true,
                IsCollectible = true,
                DrawCard = true,
                DrawMaxAmount = 1,
                DrawMinAmount = 1,
                GainManaCrystal = true,
                ManaCrystalAmount = 1,
                Elite = true,
                Type = TAG_CARDTYPE.MINION,
                Race = TAG_RACE.HUMAN,
                FlavorText = "If you suddenly hear a coin drop, be on high alert!",
                Faction = TAG_FACTION.ALLIANCE
            }};

            newCards.AddRange((from rawCard
                                in cardList.Record
                               let rawCardDef = cardDefs.Entity.First(e => e.CardID == rawCard.GetFieldRows().First(f => f.column == "NOTE_MINI_GUID").Field_Text)
                               select new HSCard(rawCard, rawCardDef)));


            HSCard legendaryCard = newCards.First(f => f.Name == "47");
            Console.WriteLine("\r{0}\t\t\t", legendaryCard.Name);

            // Process card relations
            //newCards = newCards.Select(s => s.ProcessEntourage(s.Entourage)).ToList();
            //newCards = newCards.Select(s => s.ProcessPowers(s)).ToList();
            //newCards = newCards.Select(s => s.ProcessReferencedTags(s)).ToList();
            //newCards = newCards.Select(s => s.ProcessTriggeredPowerHistoryInfo(s)).ToList();

            Console.WriteLine("\r{0} cards were processed", newCards.Count);

            int nullCount = newCards.Count(w => w == null);

            if (nullCount > 0)
            {
                Console.WriteLine("{0} null records were detected.", nullCount);
                newCards = newCards.Where(w => w != null).ToList();
            }

            cards = newCards.ToArray();

            var omitted = cardDefs.Entity.Where(w => !newCards.Select(s => s.CardId).Contains(w.CardID));

            Console.WriteLine("\r\nThe following cards were ommitted:");

            foreach (var omit in omitted)
            {
                var cardID = omit.IsCardIDNull() ? null : omit.CardID;
                var entityID = omit.Entity_Id;
                var masterPower = omit.IsMasterPowerNull() ? (Guid?)null : Guid.Parse(omit.MasterPower);
                var version = omit.IsversionNull() ? null : omit.version;

                Console.WriteLine("{0} | {1} | {2} | {3}", cardID, entityID, version, masterPower);
            }

            return cards.Any();
        }

        public static bool RetrievePlayers(int numberOfPlayers, HSCard[] cards, out Player[] players)
        {
            TAG_CLASS[] bannedClasses = { TAG_CLASS.DEATHKNIGHT, TAG_CLASS.DREAM, TAG_CLASS.INVALID };
            TAG_CLASS[] validClasses =
                Enum.GetValues(typeof(TAG_CLASS)).Cast<TAG_CLASS>().Where(w => !bannedClasses.Contains(w)).ToArray();

            if (numberOfPlayers <= 1)
                numberOfPlayers = 2;

            TAG_CLASS[] classes = validClasses.Random(numberOfPlayers).ToArray();
            List<Player> newPlayers = new List<Player>();

            for (int i = 0; i < numberOfPlayers; i++)
                newPlayers.Add(new Player(classes[i], cards));

            players = newPlayers.ToArray();

            return players.Any();
        }

        public void Run()
        {
            if (Players.Length <= 1)
                throw new ArgumentException("Not enough players to play the game!");

            int counter = 0;
            while (Players.Any(a => a.CurrentDeck.Count() != 30) && counter < 10)
            {
                foreach (Player player in Players.Where(w => w.CurrentDeck.Count() != 30))
                {
                    HSCard[] currentDeck;
                    if (Player.GetDeck(player.CurrentClass, Cards, out currentDeck))
                        player.CurrentDeck = currentDeck;
                }
                counter++;
            }

            if (Players.Any(a => a.CurrentDeck.Count() != 30))
            {
                throw new ArgumentException("Not all players have a legal deck!", "Players");
            }

            Thread.Sleep(1000);
            Console.WriteLine("Starting the game!");

            bool playing = true;
            while (playing)
            {
                Console.WriteLine("\r\nWould you like to clear the screen?");
                Console.WriteLine("Press Y for Yes | Press N for No");

                var confirmation = Console.ReadKey(true);

                while (confirmation.Key != ConsoleKey.Y && confirmation.Key != ConsoleKey.N)
                    confirmation = Console.ReadKey(true);

                if (confirmation.Key == ConsoleKey.Y)
                    Console.Clear();

                Game game = new Game(Players);
                if (game.Setup())
                {
                    Console.WriteLine("\r\nSetup complete.");
                    game.Play();
                }
                else
                {
                    throw new Exception("The game failed to initialize!");
                }

                Console.WriteLine("\r\nThe game is now completed.");

                if (game.Winner == null)
                    Console.WriteLine("\r\nThe game has ended in a draw!");
                else
                    Console.WriteLine("\r\n{0} has won the game! Congrats!", game.Winner.Name);

                Console.WriteLine("Would you like to play again?");
                Console.WriteLine("Press Y for Yes | Press N for No");

                confirmation = Console.ReadKey(true);

                while (confirmation.Key != ConsoleKey.Y && confirmation.Key != ConsoleKey.N)
                    confirmation = Console.ReadKey(true);

                playing = confirmation.Key == ConsoleKey.Y;
            }
        }
    }
}
