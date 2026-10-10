/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Collections.Generic;
using System.Threading;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using ClassicUno.Classes.Assets;
using ClassicUno.Classes.Settings.SettingsData;

namespace ClassicUno.Classes.Logic.Player
{
    [Serializable]
    public class ComputerPlayer : IPlayer
    {
        private Card _discardCard;
        private CardColor _currentColor;

        private bool _hasDrawn;
        private bool _hasRequestedDraw;

        private BackgroundWorker _worker;

        public PlayerNameData NameData { get; set; }

        public int VoiceIndex { get; set; }

        public List<Card> Cards { get; set; }
        public bool Skip { get; set; }

        public event Action<IPlayer> PlayerBeginTurn;
        public event Action<IPlayer> PlayerDraw;
        public event Action<IPlayer, Card> PlayerPlaysCard;
        public event Action<IPlayer> PlayerHasUno;
        public event Action<IPlayer> PlayerPass;
        public event Action<IPlayer> PlayerEndTurn;

        public ComputerPlayer()
        {
            NameData = new PlayerNameData();
            Cards = new List<Card>();
        }

        public void BeginTurn(Card card, CardColor currentColor)
        {
            if (Skip)
            {
                Debug.Print(">>>> " + NameData.Name + " SKIPPED <<<<<");
                Skip = false;
                EndTurn();
                return;
            }
            _discardCard = card;
            _currentColor = currentColor;

            _worker = new BackgroundWorker {WorkerSupportsCancellation = true};
            _worker.DoWork += BackgroundWorkerCallback;
            _worker.RunWorkerCompleted += BackgroundWorkerEndWork;
            _worker.RunWorkerAsync();

            /* This isn't really used for computer players - except to force a UI repaint */
            PlayerBeginTurn?.Invoke(this);
        }

        public void PlayCard(Card card)
        {
            /* This isn't necessary in computer players as PlayerPlaysCard event is raised in the background worker */
        }

        public void DrawCard(Card card)
        {
            Cards.Add(card);
            if (_hasDrawn)
            {
                /* Loop back to original background process */
                _worker.RunWorkerAsync();
            }
        }

        public void Pass()
        {
            PlayerPass?.Invoke(this);
        }

        public void EndTurn()
        {
            _hasDrawn = false;
            PlayerEndTurn?.Invoke(this);
        }

        private void BackgroundWorkerCallback(object sender, DoWorkEventArgs e)
        {
            /* Analyze upper card on discard pile (_discardCard) */
            Thread.Sleep(400);
            //switch (_discardCard.Type)
            //{
            //    case CardType.Skip:
            //        /* Player skipped */
            //        return;

            //    case CardType.DrawTwo:
            //        /* Player needs to draw two cards - then is skipped */
            //        _forceDraw = true;
            //        for (var i = 0; i < 2; i++)
            //        {
            //            PlayerDraw?.Invoke(this);
            //            Thread.Sleep(400);
            //        }
            //        return;

            //    case CardType.DrawFour:
            //        /* Player draws four cards - then is skipped */
            //        Debug.Print("here");
            //        _forceDraw = true;
            //        for (var i = 0; i < 4; i++)
            //        {
            //            Debug.Print("count " + i);
            //            PlayerDraw?.Invoke(this);
            //            Thread.Sleep(400);
            //        }
            //        return;
            //}
            Thread.Sleep(2000);
            //if (_currentColor == CardColor.None && _discardCard.Type == CardType.Wild)
            //{
            //    Debug.Print("HELLO");

            //    /* Computer needs to choose a color */
            //    PlayerChoosesColor?.Invoke(this, ChooseWildColor());
            //    Thread.Sleep(200);
            //}
        }

        private void BackgroundWorkerEndWork(object sender, RunWorkerCompletedEventArgs e)
        {
            if (_hasRequestedDraw)
            {
                _hasRequestedDraw = false;
                return;
            }

            EndTurn();
        }

        private CardColor ChooseWildColor()
        {
            return Cards.Where(c => c.Color != CardColor.None)
                .GroupBy(c => c.Color)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();
        }
    }
}
