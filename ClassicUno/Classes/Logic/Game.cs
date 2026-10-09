/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using ClassicUno.Classes.Assets;
using ClassicUno.Classes.Helpers;
using ClassicUno.Classes.Helpers.Management;
using ClassicUno.Classes.Logic.Player;
using ClassicUno.Classes.Settings.SettingsData;

namespace ClassicUno.Classes.Logic
{
    [Serializable]
    public class Game
    {
        /* Basic game class */
        private Form _parent;
        private BackgroundWorker _worker;

        public List<IPlayer> Players { get; set; }

        public List<Card> Deck { get; set; }

        public List<Card> DisposePile { get; set; }

        public IPlayer CurrentPlayer { get; set; }

        public event Action<SoundType> SoundEffect;

        #region Constructor
        public Game(Form parent, GameOptionData data)
        {
            _parent = parent;

            Players = new List<IPlayer>();
            Deck = new List<Card>();
            DisposePile = new List<Card>();

            BuildDeck();
            CreatePlayers(data);
            BeginDeal();
        }
        #endregion

        #region Player callbacks
        private void PlayerPlaysCard(IPlayer player, Card card)
        {

        }

        private void PlayerPass(IPlayer player)
        {
            //placeholder for voice audio playback
            Debug.Print("Player " + player.NameData.Name + " passed - however, PlayerEndTurn is called separately");
            AudioManager.PlayVoice(VoiceType.Pass, player);
        }

        private void PlayerEndTurn(IPlayer player)
        {
            Debug.Print("Player " + player.NameData.Name + " ended their turn");
        }

        private void PlayerInvalidateRequired(IPlayer player)
        {
            /* Call a refresh/repaint */
            _parent.Invalidate();
        }
        #endregion

        #region Private methods
        private void BuildDeck()
        {
            /* Build the deck of 108 cards (four wilds/four wild draw four's) */
            for (var color = 0; color <= 3; color++)
            {
                /* Colors red, yellow, green, blue (ignore on wilds) */
                for (var i = 0; i <= 12; i++)
                {
                    Card c;
                    switch (i)
                    {
                        case 10:
                            /* Skip */
                            c = new Card
                            {
                                Color = (CardColor)color + 1,
                                Type = CardType.Skip
                            };
                            break;

                        case 11:
                            /* Reverse */
                            c = new Card
                            {
                                Color = (CardColor)color + 1,
                                Type = CardType.Reverse
                            };
                            break;

                        case 12:
                            /* Draw two */
                            c = new Card
                            {
                                Color = (CardColor)color + 1,
                                Type = CardType.DrawTwo
                            };
                            break;

                        default:
                            /* Numeric 0 - 9 */
                            c = new Card
                            {
                                Color = (CardColor)color + 1,
                                Type = CardType.Numeric,
                                Value = i
                            };
                            break;
                    }
                    Deck.Add(c);
                }
            }
            /* Add the four wilds/four wild draw four's */
            for (var i = 0; i <= 3; i++)
            {
                /* Wild card */
                Deck.Add(new Card
                {
                    Color = CardColor.None,
                    Type = CardType.Wild
                });
                /* Wild draw four */
                Deck.Add(new Card
                {
                    Color = CardColor.None,
                    Type = CardType.DrawFour
                });
            }
            /* Shuffle the deck */
            Deck.Shuffle();
        }

        private void CreatePlayers(GameOptionData data)
        {
            /* Add players: min 2 max 4.
             * First add the human player */
            Players.Add(new HumanPlayer {NameData = data.NameData, VoiceIndex = AudioManager.GetRandomVoice(data.NameData.Gender)});

            for (var players = 0; players <= data.NumberOfPlayers - 2; players++)
            {
                var n = ComputerNames.GetRandomName();
                Players.Add(new ComputerPlayer {NameData = n, VoiceIndex = AudioManager.GetRandomVoice(n.Gender)});
            }

            foreach (var p in Players)
            {
                /* Add callbacks */
                p.PlayerPlaysCard += PlayerPlaysCard;
                p.PlayerPass += PlayerPass;
                p.PlayerEndTurn += PlayerEndTurn;
                p.PlayerInvalidateRequired += PlayerInvalidateRequired;
            }
        }

        private void BeginDeal()
        {
            _worker = new BackgroundWorker();
            _worker.DoWork += DealWorkerCallback;
            _worker.RunWorkerAsync();
        }

        private void DealWorkerCallback(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(1000);
            for (var i = 0; i <= 6; i++)
            {
                foreach (var p in Players)
                {
                    var c = Deck[0];
                    p.Cards.Add(c);
                    Deck.RemoveAt(0);
                    Thread.Sleep(400);
                    SoundEffect?.Invoke(SoundType.Deal);
                    _parent.Invalidate();
                }
            }
            Thread.Sleep(400);
            /* Top card */
            var topCard = Deck[0];
            Deck.RemoveAt(0);
            /* We make sure the top card is not a draw four */
            Card startingCard;
            if (topCard.Type == CardType.DrawFour)
            {
                /* Use LINQ to find the first card that is NOT a Wild Draw Four */
                startingCard = Deck.FirstOrDefault(c => c.Type != CardType.DrawFour);
                Debug.Print("DF");
                if (startingCard != null)
                {
                    /* Remove that specific card from the deck directly */
                    Deck.Remove(startingCard);
                    /* Put the original card back at the top */
                    Deck.Add(topCard);
                    /* Reshuffle */
                    Deck.Shuffle();
                }
                else
                {
                    /* Fallback safety case if every card in the deck is a Wild Draw Four */
                    startingCard = topCard;
                }
            }
            else
            {
                /* The top card wasn't a Wild Draw Four, so use it directly */
                startingCard = topCard;
            }
            DisposePile.Add(startingCard);
            SoundEffect?.Invoke(SoundType.Drop);
            _parent.Invalidate();
        }
        #endregion
    }
}
