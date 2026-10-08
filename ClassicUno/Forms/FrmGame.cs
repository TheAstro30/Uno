/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using ClassicUno.Classes.Assets;
using ClassicUno.Classes.Helpers;
using ClassicUno.Classes.Logic;
using ClassicUno.Classes.Logic.Player;
using ClassicUno.Controls;

namespace ClassicUno.Forms
{
    public sealed partial class FrmGame : FeltForm
    {
        private readonly Game _currentGame;

        public FrmGame()
        {
            InitializeComponent();

            /* Get current assembly */
            var assembly = Assembly.GetExecutingAssembly();

            /* Get version information */
            var version = assembly.GetName().Version;

            Text = $@"Classic UNO! 2026 - v{version.Major}.{version.Minor}";
            Icon = Icon.ExtractAssociatedIcon(assembly.Location);

            Cards.BuildCardImages();

            _currentGame = new Game();

            /* Add some test players */
            _currentGame.Players.Add(new HumanPlayer() { NameData = new PlayerNameData() { Name = "Penis", Gender = PlayerGender.Male } });

            for (var players = 0; players <= 2; players++)
            {
                var n = Names.GetRandomName();
                _currentGame.Players.Add(new ComputerPlayer() { NameData = n });
            }

            foreach (var p in _currentGame.Players)
            {
                //add seven cards (deal)
                for (var i = 0; i <= 6; i++)
                {
                    var c = _currentGame.Deck[0];
                    p.Cards.Add(c);
                    _currentGame.Deck.RemoveAt(0);
                    Debug.Print("Player " + p.NameData.Name + " " + p.NameData.Gender + " card add: " + c.Type + " " + c.Color + " " + c.Value);
                }

                p.PlayerPlaysCard += PlayerPlaysCard;
                p.PlayerPass += PlayerPass;
                p.PlayerEndTurn += PlayerEndTurn;
                p.PlayerInvalidateRequired += PlayerInvalidateRequired;
            }

            //foreach (var cd in _currentGame.Deck)
            //{
            //    Debug.Print("Current card: " + cd.Type + " " + cd.Color + " " + cd.Value);
            //}
            
            //var d = new FrmAbout();
            //d.ShowDialog(this);
        }
        
        #region Form overrides
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
        }
        #endregion

        /* Callbacks for players */
        private void PlayerPlaysCard(IPlayer player, Card card)
        {

        }

        private void PlayerPass(IPlayer player)
        {
            //placeholder for voice audio playback
            Debug.Print("Player " + player.NameData.Name + " passed - however, PlayerEndTurn is called separately");
        }

        private void PlayerEndTurn(IPlayer player)
        {
            Debug.Print("Player " + player.NameData.Name + " ended their turn");
        }

        private void PlayerInvalidateRequired(IPlayer player)
        {
            /* Call a refresh/repaint */
            Invalidate();
        }

        #region Private methods
        private void NewGame()
        {

        }
        #endregion
    }
}
