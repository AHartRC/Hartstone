using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HartStone.Models;

namespace HartStone.Classes
{
    public class Game
    {
        public bool IsRunning { get; set; }
        public int CurrentTurn { get; set; }
        public int MaxTurns { get; set; }
        public int MaxCardsInHand { get; set; }
        public int StartingHandSize { get; set; }

        public Player[] Players { get; set; }

        private Player _playerOne;
        public Player PlayerOne
        {
            get { return _playerOne; }
            set { _playerOne = value; }
        }

        private Player _playerTwo;
        public Player PlayerTwo
        {
            get { return _playerTwo; }
            set { _playerTwo = value; }
        }

        private Player _winner;
        public Player Winner
        {
            get { return _winner; }
            set { _winner = value; }
        }

        private List<HSCard> _playerOneHand;
        public List<HSCard> PlayerOneHand
        {
            get
            {
                return _playerOneHand;
            }
            set
            {
                _playerOneHand = value;
            }
        }

        private List<HSCard> _playerOneDeck;
        public List<HSCard> PlayerOneDeck
        {
            get
            {
                return _playerOneDeck;
            }
            set
            {
                _playerOneDeck = value;
            }
        }


        private List<HSCard> _playerTwoHand;
        public List<HSCard> PlayerTwoHand
        {
            get
            {
                return _playerTwoHand;
            }
            set
            {
                _playerTwoHand = value;
            }
        }

        private List<HSCard> _playerTwoDeck;

        public List<HSCard> PlayerTwoDeck
        {
            get
            {
                return _playerTwoDeck;
            }
            set
            {
                _playerTwoDeck = value;
            }
        }



        public Game(Player[] players, bool randomizePlayerOrder = true)
        {
            if (players == null || players.Length <= 1 || players.Any(a => a.CurrentDeck.Count() != 30))
            {
                throw new Exception("Unable to start game. The players provided are ineligible!");
            }


            if (!randomizePlayerOrder)
            {
                Players = players;
                PlayerOne = Players[0];
                PlayerTwo = Players[1];
            }
            else
            {
                Players = players.Shuffle().ToArray();
                PlayerOne = Players[0];
                PlayerTwo = Players[1];
            }

            Console.WriteLine("\r\n{0} has been chosen as Player One\r\n{1} has been chosen as Player Two.\r\n\r\n{0} will go first.", PlayerOne.Name, PlayerTwo.Name);

            Console.WriteLine("\r\n{0}'s deck consists of the following cards:", PlayerOne.Name);
            foreach (HSCard card in PlayerOne.CurrentDeck)
                Console.WriteLine(card.ToString());

            Console.WriteLine("\r\n{0}'s deck consists of the following cards:", PlayerTwo.Name);
            foreach (HSCard card in PlayerTwo.CurrentDeck)
                Console.WriteLine(card.ToString());
        }

        public void Play()
        {
            if (InitialDraws())
            {
                Console.WriteLine("\r\nInitial draws are complete. Let the battle commence!");
                while (CurrentTurn < MaxTurns && ProcessTurn())
                {
                    Console.WriteLine("\r\nTurn {0} has completed!\r\n{1} has {2} health, {3} cards in hand, and {4} cards left in their deck.\r\n{5} has {6} health, {7} cards in hand, and {8} cards left in their deck.",
                        CurrentTurn, PlayerOne.Name, PlayerOne.CurrentHealth, _playerOneHand.Count(), _playerOneDeck.Count(), PlayerTwo.Name, PlayerTwo.CurrentHealth, _playerTwoHand.Count(), _playerTwoDeck.Count());
                }

                Console.WriteLine("\r\nThe game has ended.");

                if (CurrentTurn >= MaxTurns)
                {
                    Console.WriteLine("The game has reached it's maximum number of turns.");
                }
            }
            else
            {
                throw new Exception("A problem occurred while trying to complete the initial draw phase. Unable to continue!");
            }
        }

        public bool Setup()
        {
            IsRunning = true;
            CurrentTurn = 0;
            MaxTurns = 50;
            MaxCardsInHand = 10;
            StartingHandSize = 4;

            PlayerOne.CurrentHealth = PlayerTwo.CurrentHealth = 30;

            Console.WriteLine("\r\nPerforming initial shuffling of decks!");
            _playerOneDeck = PlayerOne.CurrentDeck.Shuffle().ToList();
            _playerTwoDeck = PlayerOne.CurrentDeck.Shuffle().ToList();
            Thread.Sleep(1000);
            Console.WriteLine("The decks have been shuffled!");

            return _playerOneDeck.Count() == 30 && _playerTwoDeck.Count() == 30;
        }

        private bool InitialDraws()
        {
            Console.WriteLine("Beginning Initial Draw Phase");
            return InitialDraw(ref _playerOne, ref _playerOneHand, ref _playerOneDeck)
                && InitialDraw(ref _playerTwo, ref _playerTwoHand, ref _playerTwoDeck);
        }

        private bool InitialDraw(ref Player player, ref List<HSCard> playerHand, ref List<HSCard> playerDeck)
        {
            Console.WriteLine("\r\n\r\nShuffling {0}'s deck", player.Name);
            playerDeck = new List<HSCard>();
            playerHand = new List<HSCard>();

            bool isHappy = false;
            int counter = 0;

            while (!isHappy && counter < 3)
            {
                counter++;
                playerDeck = player.CurrentDeck.Shuffle().ToList();
                playerHand.Clear();
                playerHand.AddRange(Extensions.Draw(ref playerDeck, StartingHandSize));

                Console.WriteLine("\r\n{0} has drawn the following cards for their opening hand:", player.Name);

                foreach (HSCard card in playerHand)
                    Console.WriteLine(card.ToString());

                if (counter < 3)
                {
                    Console.WriteLine("\r\nWould you like to keep this hand?\r\n(You can mulligan {0} more times)", 3 - counter);
                    Console.WriteLine("Press Y for Yes | Press N for No");

                    ConsoleKeyInfo confirmation = Console.ReadKey(true);

                    while (confirmation.Key != ConsoleKey.Y && confirmation.Key != ConsoleKey.N)
                        confirmation = Console.ReadKey(true);

                    if (confirmation.Key == ConsoleKey.Y)
                        isHappy = true;
                }
                else
                {
                    Console.WriteLine("\r\nYou've run out of mulligans. You must use the hand you currently have!");
                    isHappy = true;
                }
            }
            return isHappy;
        }

        private bool ProcessTurn()
        {
            Console.WriteLine("\r\nBeginning Turn {0}!", ++CurrentTurn);
            return TakeTurn(ref _playerOne, ref _playerTwo, ref _playerOneHand, ref _playerOneDeck)
                && TakeTurn(ref _playerTwo, ref _playerOne, ref _playerTwoHand, ref _playerTwoDeck);
        }

        private bool TakeTurn(ref Player player, ref Player opponent, ref List<HSCard> playerHand, ref List<HSCard> playerDeck)
        {
            if (opponent.CurrentHealth <= 0)
            {
                Console.WriteLine("\r\n{0} is dead. This game is over!", opponent.Name);
                Winner = player;
                return false;
            }
            if (player.CurrentHealth <= 0)
            {
                Console.WriteLine("\r\n{0} is dead. This game is over!", player.Name);
                Winner = opponent;
                return false;
            }

            player.CurrentMana = CurrentTurn <= 0
                                    ? 1
                                    : CurrentTurn >= 10
                                        ? 10
                                        : CurrentTurn;

            Console.WriteLine("{0}, you have {1} health remaining, {2} cards in your hand, and {3} cards in your deck remaining.",
                                player.Name, player.CurrentHealth, playerHand.Count(), playerDeck.Count());
            if (DrawCards(1, ref player, ref opponent, ref playerHand, ref playerDeck))
            {
                while (TakeAction(ref player, ref opponent, ref playerHand, ref playerDeck))
                {
                    Console.WriteLine("It's {0}'s turn to take action!", player.Name);
                }
            }
            return true;
        }

        private bool DrawCards(int amount, ref Player player, ref Player opponent, ref List<HSCard> playerHand, ref List<HSCard> playerDeck)
        {
            if (!ProcessFatigue(amount, playerDeck.Count, ref player, ref opponent))
                return false;

            var drawnCards = Extensions.Draw(ref playerDeck, amount).ToList();

            while (playerHand.Count + drawnCards.Count > MaxCardsInHand)
            {
                var last = drawnCards.Last();
                Console.WriteLine("\r\n{0} has too many cards in their hand!\r\n{1} has been destroyed!", player.Name, last.Name);
                drawnCards.Remove(last);
            }

            if (drawnCards.Any())
            {
                playerHand.AddRange(drawnCards);

                Console.WriteLine("\r\n{0} has drawn the following card{1} and now has {2} card{3} in their hand and {4} card{5} left in their deck.", player.Name, drawnCards.Count() > 1 ? "s" : "", playerHand.Count, playerHand.Count() != 1 ? "s" : "", playerDeck.Count, playerDeck.Count() != 1 ? "s" : "");

                foreach (HSCard card in drawnCards)
                    Console.WriteLine(card.ToString());
            }
            else
                Console.WriteLine("No cards were drawn.");

            return true;
        }

        private bool ProcessFatigue(int drawAmount, int deckCount, ref Player player, ref Player opponent)
        {
            if (drawAmount > deckCount)
            {
                int cycles = drawAmount - deckCount;
                for (int i = 0; i < cycles; i++)
                {
                    player.CurrentHealth -= ++player.CurrentFatigue;
                    Console.WriteLine("{0} has taken {1} points of fatigue damage!", player.Name, player.CurrentFatigue);

                    if (player.CurrentHealth <= 0)
                    {
                        Console.WriteLine("{0} has died from fatigue damage!", player.Name);
                        Winner = opponent;
                        return false;
                    }
                }
            }
            return true;
        }

        private bool TakeAction(ref Player player, ref Player opponent, ref List<HSCard> playerHand, ref List<HSCard> playerDeck)
        {
            int playerMana = player.CurrentMana;
            if (player.CurrentMana <= 0 || !playerHand.Any(a => a.ManaCost <= (playerMana)))
            {
                Console.WriteLine("\r\n{0} has no more legal moves this turn.", player.Name);
                return false;
            }

            Console.WriteLine("\r\nChoose a card to play:");
            Dictionary<ConsoleKey, HSCard> validSelections = new Dictionary<ConsoleKey, HSCard>();
            int counter = 0;
            for (int i = 0; i < playerHand.Count; i++)
            {
                HSCard currentCard = playerHand[i];
                if (currentCard.ManaCost <= player.CurrentMana)
                {
                    validSelections.Add(TranslateIndexToKey(counter++), currentCard);
                    Console.WriteLine("{0} | {1}", counter, currentCard);
                }
            }
            Console.WriteLine("\r\nPress the key that matches the card you'd like to play.\r\nPress 0 for the 10th selection if one exists.\r\nPress Escape to end your turn.");
            
            if (!validSelections.Any())
            {
                Console.WriteLine("\r\n{0} has no more legal moves this turn.", player.Name);
                return false;
            }

            var selection = Console.ReadKey(true);

            while (!validSelections.ContainsKey(selection.Key))
            {
                if (selection.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine("Escape pressed. Ending turn!");
                    return false;
                }
                selection = Console.ReadKey(true);
            }

            HSCard selectedCard = validSelections[selection.Key];
            playerHand.Remove(selectedCard);

            return PlayCard(selectedCard, ref player, ref opponent, ref playerHand, ref playerDeck);
        }

        private ConsoleKey TranslateIndexToKey(int value)
        {
            switch (value)
            {
                case 0:
                    return ConsoleKey.D1;
                case 1:
                    return ConsoleKey.D2;
                case 2:
                    return ConsoleKey.D3;
                case 3:
                    return ConsoleKey.D4;
                case 4:
                    return ConsoleKey.D5;
                case 5:
                    return ConsoleKey.D6;
                case 6:
                    return ConsoleKey.D7;
                case 7:
                    return ConsoleKey.D8;
                case 8:
                    return ConsoleKey.D9;
                case 9:
                    return ConsoleKey.D0;
                default:
                    throw new Exception("Number is too high. Not enough numbers on the keyboard for the total number of selections!");
            }
        }

        private bool PlayCard(HSCard card, ref Player player, ref Player opponent, ref List<HSCard> playerHand, ref List<HSCard> playerDeck)
        {
            Console.WriteLine("\r\n{0} has used {1}", player.Name, card.Name);

            player.CurrentMana -= card.ManaCost;

            if (card.CardId == "DIANA")
            {
                var originalColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\r\n\r\n\r\nAgent {0} confidently tells {1}:\r\n'You will never defeat me!'\r\n", card.Name, opponent.Name);
                Console.ForegroundColor = originalColor;
            }
            if (card.GainMana)
            {
                player.CurrentMana += card.ManaAmount;
                Console.WriteLine("{0} has gained {1} mana and now has {2} mana remaining!", player.Name, card.ManaAmount, player.CurrentMana);
            }
            if (card.GainManaCrystal)
            {
                player.CurrentMana += card.ManaCrystalAmount;
                Console.WriteLine("{0} has gained {1} mana and now has {2} mana remaining!", player.Name, card.ManaCrystalAmount, player.CurrentMana);
            }
            if (card.DrawCard)
            {
                int drawAmount = new Random().Next(card.DrawMinAmount, card.DrawMaxAmount);
                Console.WriteLine("{0} is drawing {1} cards!", player.Name, drawAmount);
                if (!DrawCards(drawAmount, ref player, ref opponent, ref playerHand, ref playerDeck))
                {
                    return false;
                }
            }
            if (card.HealTarget)
            {
                int healAmount = new Random().Next(card.HealMinAmount, card.HealMaxAmount);
                Console.WriteLine("Healing {0} for {1} health!", player.Name, healAmount);
                player.CurrentHealth += healAmount;
            }
            if (card.DamageTarget)
            {
                int dmgAmount = new Random().Next(card.DamageMinAmount, card.DamageMaxAmount);
                Console.WriteLine("Dealing {0} damage to {1}!", dmgAmount, opponent.Name);
                opponent.CurrentHealth -= dmgAmount;
                if (opponent.CurrentHealth <= 0)
                {
                    Console.WriteLine("{0} has been slain by {1}'s {2}!", opponent.Name, player.Name, card.Name);
                    Winner = player;
                    return false;
                }
            }
            if (card.Attack > 0)
            {
                Console.WriteLine("{0}'s {1} is attacking {2} and dealing {3} damage!", player.Name, card.Name, opponent.Name, card.Attack);
                opponent.CurrentHealth -= card.Attack;
                if (opponent.CurrentHealth <= 0)
                {
                    Console.WriteLine("{0} has been slain by {1}'s {2}!", opponent.Name, player.Name, card.Name);
                    Winner = player;
                    return false;
                }
            }

            return true;
        }
    }
}
