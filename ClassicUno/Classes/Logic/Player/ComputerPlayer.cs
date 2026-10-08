/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Collections.Generic;
using System.Threading;
using System.ComponentModel;
using ClassicUno.Classes.Assets;
using ClassicUno.Classes.Settings.SettingsData;

namespace ClassicUno.Classes.Logic.Player
{
    [Serializable]
    public class ComputerPlayer : IPlayer
    {
        private Game _game;

        public PlayerNameData NameData { get; set; }

        public List<Card> Cards { get; set; }

        public event Action<IPlayer, Card> PlayerPlaysCard;
        public event Action<IPlayer> PlayerPass;
        public event Action<IPlayer> PlayerEndTurn;
        public event Action<IPlayer> PlayerInvalidateRequired;

        public ComputerPlayer()
        {
            NameData = new PlayerNameData();
            Cards = new List<Card>();
        }

        public void BeginTurn(Game game)
        {
            _game = game;
            var worker = new BackgroundWorker();
            worker.DoWork += BackgroundWorkerCallback;
            worker.RunWorkerCompleted += BackgroundWorkerEndWork;
            worker.RunWorkerAsync();
        }

        public void Pass()
        {
            PlayerPass?.Invoke(this);
        }

        public void EndTurn()
        {
            PlayerEndTurn?.Invoke(this);
        }

        private void BackgroundWorkerCallback(object sender, DoWorkEventArgs e)
        {
            /* Analyze upper card on dispose pile */
            Thread.Sleep(500);


        }

        private void BackgroundWorkerEndWork(object sender, RunWorkerCompletedEventArgs e)
        {
            EndTurn();
        }
    }
}
