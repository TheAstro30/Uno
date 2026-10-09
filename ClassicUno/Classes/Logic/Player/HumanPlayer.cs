/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Collections.Generic;
using ClassicUno.Classes.Assets;
using ClassicUno.Classes.Settings.SettingsData;

namespace ClassicUno.Classes.Logic.Player
{
    [Serializable]
    public class HumanPlayer : IPlayer
    {
        private Game _game;

        public PlayerNameData NameData { get; set; }

        public int VoiceIndex { get; set; }

        public List<Card> Cards { get; set; }

        public event Action<IPlayer, Card> PlayerPlaysCard;
        public event Action<IPlayer> PlayerPass;
        public event Action<IPlayer> PlayerEndTurn;
        public event Action<IPlayer> PlayerInvalidateRequired;

        public HumanPlayer()
        {
            NameData = new PlayerNameData();
            Cards = new List<Card>();
        }

        public void BeginTurn(Game game)
        {
            _game = game;
        }

        public void Pass()
        {
            PlayerPass?.Invoke(this);
        }

        public void EndTurn()
        {
            PlayerEndTurn?.Invoke(this);
        }
    }
}
