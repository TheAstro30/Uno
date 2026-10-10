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
using ClassicUno.Classes.Helpers.UI;
using ClassicUno.Classes.Logic.Player;
using ClassicUno.Classes.Settings.SettingsData;

namespace ClassicUno.Classes.Logic
{
    [Serializable]
    public enum PlayDirection
    {
        Clockwise = 0,
        AntiClockwise = 1
    }

    [Serializable]
    public class Game : IDisposable
    {
        private IGame _ui;

        private BackgroundWorker _worker;
        private bool _stop;

        private int _cardsToDraw;
        private IPlayer _playerToForceDraw;
        private bool _forceDrawChooseColor;
        
        public bool GameRunning { get; set; }

        public List<IPlayer> Players { get; set; }

        public List<Card> Deck { get; set; }

        public List<Card> DiscardPile { get; set; }

        public CardColor CurrentColor { get; set; }

        public IPlayer CurrentPlayer { get; set; }

        public Card HighlightCard { get; set; }

        public PlayDirection CurrentDirection { get; set; }


        #region Constructor
        public Game(IGame ui, GameOptionData data)
        {
            _ui = ui;

            Players = new List<IPlayer>();
            Deck = new List<Card>();
            DiscardPile = new List<Card>();

            CurrentDirection = PlayDirection.Clockwise;

            BuildDeck();
            CreatePlayers(data);
            BeginDeal();
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            /* If a new game is requested while this one is still dealing - we need to stop the dealing loop */
            if (_worker?.IsBusy == true)
            {
                _stop = true;
                _worker.CancelAsync();
            }

            _worker?.Dispose();

            /* Remove handlers */
            foreach (var p in Players)
            {
                p.PlayerDraw -= PlayerDraw;
                p.PlayerPlaysCard -= PlayerPlaysCard;
                p.PlayerHasUno -= PlayerHasUno;
                p.PlayerPass -= PlayerPasses;
                p.PlayerBeginTurn -= PlayerBeginTurn;
                p.PlayerEndTurn -= PlayerEndTurn;
            }
        }
        #endregion

        #region Public methods
        public void PlayerDrawCard()
        {
            if (!(CurrentPlayer is HumanPlayer))
            {
                /* Not your turn */
                return;
            }

            var c = Draw();
            CurrentPlayer.DrawCard(c);
            _ui.EnableDraw(false);
            _ui.EnablePass(true);
            _ui.Invalidate();
        }

        public void PlayerPlayCard()
        {
            if (!(CurrentPlayer is HumanPlayer))
            {
                /* Not your turn */
                return;
            }
            /* Now, we evaluate the selected card can even be played */
            if (IsCardPlayable(DiscardPile[DiscardPile.Count - 1], HighlightCard, CurrentColor))
            {
                /* Play the card */
                CurrentPlayer.PlayCard(HighlightCard);
                HighlightCard = null;
            }
        }

        public void PlayerChoosesColor(IPlayer player, CardColor color)
        {
            /* Change color and continue to next player */
            Debug.Print("COLOR CHANGE> " + player.NameData.Name + " " + color);
            CurrentColor = color;
            if (_forceDrawChooseColor && player is HumanPlayer)
            {
                CurrentPlayer = GetNextPlayer(_playerToForceDraw, CurrentDirection);
                _playerToForceDraw = null;
                _cardsToDraw = 0;
            }
            player.EndTurn();
        }

        public void PlayerPass()
        {
            if (!(CurrentPlayer is HumanPlayer))
            {
                /* Not your turn */
                return;
            }
            CurrentPlayer.Pass();
            CurrentPlayer.EndTurn();
        }
        #endregion

        #region Player callbacks
        private void PlayerDraw(IPlayer player)
        {
            var c = Draw();
            player.DrawCard(c);
        }

        private void PlayerPlaysCard(IPlayer player, Card card)
        {
            /* Here is the logic for most of the game play. Player chooses a card in their hand, it goes to the discard pile and card is evaluated here
             * to determine coarse of action. */
            CurrentColor = card.Color;
            DiscardPile.Add(card);
            _ui.SoundEffectRequest(SoundType.Drop);
            /* Does player have UNO!? */
            if (player.Cards.Count == 1)
            {
                _ui.VoiceRequest(VoiceType.Uno, player);
            }
            /* Check if they have no cards, then they've won */
            if (player.Cards.Count == 0)
            {
                Debug.Print("won " + player.NameData.Name);
                return;
            }
            _ui.Invalidate();
            IPlayer nextPlayer;
            switch (card.Type)
            {
                case CardType.Wild:
                    if (CurrentPlayer is HumanPlayer)
                    {
                        /* Human played the card */
                        _ui.EnableDraw(false);
                        _ui.EnablePass(false);
                        _ui.ShowColorButtons(true);
                        return;
                    }
                    break;

                case CardType.Skip:
                    /* Next player in line is skipped */
                    nextPlayer = GetNextPlayer(player, CurrentDirection);
                    nextPlayer.Skip = true;
                    break;

                case CardType.Reverse:
                    /* Change direction */
                    CurrentDirection = CurrentDirection == PlayDirection.Clockwise
                        ? PlayDirection.AntiClockwise
                        : PlayDirection.Clockwise;
                    break;

                case CardType.DrawTwo:
                    nextPlayer = GetNextPlayer(player, CurrentDirection);
                    nextPlayer.Skip = true;
                    BeginForceDraw(nextPlayer, 2, false);
                    return;

                case CardType.DrawFour:
                    nextPlayer = GetNextPlayer(player, CurrentDirection);
                    nextPlayer.Skip = true;
                    BeginForceDraw(nextPlayer, 4, CurrentPlayer is HumanPlayer);
                    return;
            }

            player.EndTurn();
        }

        private void PlayerHasUno(IPlayer player)
        {
            //if (_parent.InvokeRequired)
            //{
            //    Debug.Print("UNO! Invoke required");
            //}
            _ui.Invalidate();
        }

        private void PlayerPasses(IPlayer player)
        {
            //placeholder for voice audio playback
            Debug.Print("Player " + player.NameData.Name + " passed - however, PlayerEndTurn is called separately");
            AudioManager.PlayVoice(VoiceType.Pass, player);
        }

        private void PlayerBeginTurn(IPlayer player)
        {
            Debug.Print("Player begin: " + player.NameData.Name);
            /* Loop back mainly for the human player so the UI can be update by showing the "DRAW" and "PASS" buttons */
            _ui.Invalidate();
            if (player is HumanPlayer)
            {
                /* Evaluted top card on discard pile */
                var c = DiscardPile[DiscardPile.Count - 1];
                if (!GameRunning)
                {
                    GameRunning = true;
                    CurrentColor = c.Color;
                    switch (c.Type)
                    {
                        case CardType.Skip:
                            /* You are skipped */
                            PlayerEndTurn(player);
                            return;

                        case CardType.DrawTwo:
                            /* You have to draw two cards - and are skipped */
                            BeginForceDraw(player, 2, false);
                            return;

                        case CardType.Reverse:
                            /* Change direction */
                            CurrentDirection = CurrentDirection == PlayDirection.Clockwise
                                ? PlayDirection.AntiClockwise
                                : PlayDirection.Clockwise;
                            break;
                    }
                }
                _ui.ShowButtons(true);
                Debug.Print("Human player's turn");

                if (CurrentColor == CardColor.None && c.Type == CardType.Wild)
                {
                    /* Human needs to choose a color */
                    _ui.ShowColorButtons(true);
                }
                else
                {
                    _ui.EnableDraw(true);
                }
            }
        }

        private void PlayerEndTurn(IPlayer player)
        {
            Debug.Print("Player " + player.NameData.Name + " ended their turn");
            /* Play moves to next player - dependant on direction */
            if (player is HumanPlayer)
            {
                _ui.ShowButtons(false);
                _ui.EnableDraw(false);
                _ui.EnablePass(false);
                _ui.ShowColorButtons(false);
            }

            CurrentPlayer = GetNextPlayer(player, CurrentDirection);
            CurrentPlayer.BeginTurn(DiscardPile[DiscardPile.Count - 1], CurrentColor);

            _ui.Invalidate();
        }
        #endregion

        #region Private methods
        private void BuildDeck()
        {
            /* Build the deck of 108 cards (four wilds/four wild draw four's) */
            for (var x = 0; x <= 1; x++)
            {
                /* There's two sets of each color numbers 1-9 */
                for (var color = 0; color <= 3; color++)
                {
                    /* Colors red, yellow, green, blue */
                    for (var i = 0; i <= 13; i++)
                    {
                        Card c = null;
                        switch (i)
                        {
                            case 10:
                                /* Skip */
                                c = new Card
                                {
                                    Color = (CardColor) color + 1,
                                    Type = CardType.Skip
                                };
                                break;

                            case 11:
                                /* Reverse */
                                c = new Card
                                {
                                    Color = (CardColor) color + 1,
                                    Type = CardType.Reverse
                                };
                                break;

                            case 12:
                                /* Draw two */
                                c = new Card
                                {
                                    Color = (CardColor) color + 1,
                                    Type = CardType.DrawTwo
                                };
                                break;

                            case 13:
                                /* Wild / Wild Draw Four - simplified the code from how I had it.
                                 * Originially, this was in a separate loop executed after this one.
                                 * Streamlining it in to this loop == less CPU time and makes more sense. */
                                if (color < 2 && x == 0)
                                {
                                    /* Add the four wilds/four wild draw four's */
                                    for (var w = 0; w <= 3; w++)
                                    {
                                        c = color == 0
                                            ? new Card {Color = CardColor.None, Type = CardType.Wild}
                                            : new Card {Color = CardColor.None, Type = CardType.DrawFour};
                                        Deck.Add(c);
                                    }
                                    continue;
                                }
                                break;

                            default:
                                /* Numeric 0 - 9 */
                                if (i == 0 && x == 1)
                                {
                                    continue;
                                }
                                c = new Card
                                {
                                    Color = (CardColor) color + 1,
                                    Type = CardType.Numeric,
                                    Value = i
                                };
                                break;
                        }
                        /* Add card to deck */
                        if (c != null)
                        {
                            Deck.Add(c);
                        }
                    }
                }
            }
            /* Shuffle the deck - sanity check it is greater than 0, which it should be... */
            if (Deck.Count > 0)
            {
                Deck.Shuffle();
            }
        }

        private void CreatePlayers(GameOptionData data)
        {
            /* Add players: min 2 max 4. First add the human player */
            Players.Add(new HumanPlayer {NameData = data.NameData, VoiceIndex = AudioManager.GetRandomVoice(data.NameData.Gender)});
            
            /* Now we add the remaining computer AI players */
            for (var players = 0; players <= data.NumberOfPlayers - 2; players++)
            {
                var n = ComputerNames.GetRandomName();
                Players.Add(new ComputerPlayer {NameData = n, VoiceIndex = AudioManager.GetRandomVoice(n.Gender)});
            }

            /* Add callbacks */
            foreach (var p in Players)
            {
                p.PlayerDraw += PlayerDraw;
                p.PlayerPlaysCard += PlayerPlaysCard;
                p.PlayerHasUno += PlayerHasUno;
                p.PlayerPass += PlayerPasses;
                p.PlayerBeginTurn += PlayerBeginTurn;
                p.PlayerEndTurn += PlayerEndTurn;
            }
        }
        #endregion

        #region Deal
        private void BeginDeal()
        {
            _worker = new BackgroundWorker {WorkerSupportsCancellation = true};
            _worker.DoWork += DealWorkerCallback;
            _worker.RunWorkerCompleted += DealEnded;
            _worker.RunWorkerAsync();
        }

        private void DealWorkerCallback(object sender, DoWorkEventArgs e)
        {
            /* Give a slight delay before beginning dealing. This gives time for the WM_PAINT event to update the screen with the pile of pick-up cards,
             * player names, etc. */
            Thread.Sleep(1000);
            /* We loop 7 times (7 cards per hand) */
            for (var i = 0; i <= 6; i++)
            {
                /* During each iteration we loop each player and add a card to their hand */
                if (_stop)
                {
                    return;
                }
                foreach (var p in Players)
                {
                    if (_stop)
                    {
                        return;
                    }
                    var c = Deck[0];
                    p.Cards.Add(c);
                    Deck.RemoveAt(0);
                    /* Play sound and update UI */
                    _ui.SoundEffectRequest(SoundType.Deal);
                    _ui.Invalidate();
                    /* Slight delay to allow WM_PAINT and sound event to complete before next iteration */
                    Thread.Sleep(300);
                }
            }
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
            DiscardPile.Add(startingCard);
            //test code
            //DiscardPile.Add(new Card {Color = CardColor.Red, Type = CardType.DrawTwo});
        }

        private void DealEnded(object sender, RunWorkerCompletedEventArgs e)
        {
            /* Dealing ended - game came begin */
            _ui.SoundEffectRequest(SoundType.Drop);

            _worker.DoWork -= DealWorkerCallback;
            _worker.RunWorkerCompleted -= DealEnded;
            _worker.Dispose();
            _worker = null;

            /*At some point I may add a bit of code to include who the dealer is and who the next player is; for now, we'll start with the human player */
            var c = DiscardPile[DiscardPile.Count - 1];
            CurrentPlayer = Players[0];

            if (c.Type != CardType.Wild)
            {
                CurrentColor = c.Color;
            }
            CurrentPlayer.BeginTurn(c, CurrentColor);
        }
        #endregion

        private void BeginForceDraw(IPlayer player, int cards, bool chooseColor)
        {
            _playerToForceDraw = player;
            _cardsToDraw = cards;
            _forceDrawChooseColor = chooseColor;
            
            _worker = new BackgroundWorker {WorkerSupportsCancellation = true};
            _worker.DoWork += BackgroundForceDraw;
            _worker.RunWorkerCompleted += BackgroundForceDrawEnd;
            _worker.RunWorkerAsync();
        }

        private void BackgroundForceDraw(object sender, DoWorkEventArgs e)
        {
            for (var i = 0; i < _cardsToDraw; i++)
            {
                var c = Draw();
                _playerToForceDraw.DrawCard(c);
                Thread.Sleep(400);
            }
        }

        private void BackgroundForceDrawEnd(object sender, RunWorkerCompletedEventArgs e)
        {
            _worker.DoWork -= BackgroundForceDraw;
            _worker.RunWorkerCompleted -= BackgroundForceDrawEnd;
            _worker.Dispose();
            _worker = null;

            if (_forceDrawChooseColor)
            {
                _ui.EnableDraw(false);
                _ui.EnablePass(false);
                _ui.ShowColorButtons(true);
                return;
            }

            _playerToForceDraw.Skip = true;
            _playerToForceDraw = null;
            _cardsToDraw = 0;

            CurrentPlayer.EndTurn();
        }

        #region Private helpers
        private IPlayer GetNextPlayer(IPlayer currentPlayer, PlayDirection direction)
        {
            var index = Players.IndexOf(currentPlayer);
            return Players[(index + (direction == PlayDirection.Clockwise ? 1 : -1) + Players.Count) % Players.Count];
        }

        private Card Draw()
        {
            /* This method ensures during game play that once the deck get's down to one card, it is refreshed from the
             * discard pile and shuffled */
            if (Deck.Count == 1)
            {
                Debug.Print("refresh deck");
                /* Keep the top card */
                var topCard = DiscardPile[DiscardPile.Count - 1];
                DiscardPile.RemoveAt(DiscardPile.Count - 1);

                /* Copy the rest of the discard pile*/
                Deck.AddRange(DiscardPile);
                DiscardPile.Clear();
                DiscardPile.Add(topCard);

                Deck.Shuffle();
            }

            _ui.SoundEffectRequest(SoundType.Deal);
            _ui.Invalidate();

            var c = Deck[0];
            Deck.RemoveAt(0);
            return c;
        }
        #endregion

        private bool IsCardPlayable(Card topCard, Card selectedCard, CardColor currentColor)
        {
            if (selectedCard == null)
            {
                return false;
            }

            // Wild cards can always be played.
            if (selectedCard.Type == CardType.Wild ||
                selectedCard.Type == CardType.DrawFour)
            {
                return true;
            }

            // Match the colour.
            if (selectedCard.Color == topCard.Color || selectedCard.Color == currentColor)
            {
                return true;
            }

            // Match the number on numeric cards.
            if (selectedCard.Type == CardType.Numeric &&
                topCard.Type == CardType.Numeric &&
                selectedCard.Value == topCard.Value)
            {
                return true;
            }

            // Match the action type (Skip, Reverse, Draw Two).
            if (selectedCard.Type == topCard.Type &&
                selectedCard.Type != CardType.Numeric)
            {
                return true;
            }

            return false;
        }
    }
}
