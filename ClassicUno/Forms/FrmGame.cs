/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using ClassicUno.Classes.Helpers;
using ClassicUno.Classes.Helpers.Management;
using ClassicUno.Classes.Helpers.UI;
using ClassicUno.Classes.Logic;
using ClassicUno.Classes.Settings.SettingsData;
using ClassicUno.Controls;

namespace ClassicUno.Forms
{
    public sealed partial class FrmGame : FeltForm
    {
        private Game _currentGame;
        private readonly Timer _tmrNew;

        private readonly UiSynchronize _sync;

        public FrmGame()
        {
            InitializeComponent();

            /* Get current assembly */
            var assembly = Assembly.GetExecutingAssembly();

            /* Get version information */
            var version = assembly.GetName().Version;

            Text = $@"Classic UNO! 2026 - v{version.Major}.{version.Minor}";
            Icon = Icon.ExtractAssociatedIcon(assembly.Location);

            _tmrNew = new Timer {Interval = 10, Enabled = true};
            _tmrNew.Tick += ShowNewGameDialog;

            _sync = new UiSynchronize(this);

            SettingsManager.Load();
            GraphicsManager.Initialize();
            AudioManager.Initialize();
        }

        #region Form overrides
        protected override void OnLoad(EventArgs e)
        {
            /* Set window position and size */
            var loc = SettingsManager.Settings.Location;
            if (loc == Point.Empty)
            {
                /* Scale form to less than the screen width/height */
                var screen = Utils.GetCurrentMonitor(this);
                var x = screen.Bounds.Width - 100;
                var y = screen.Bounds.Height - 100;
                Size = new Size(x, y);
                /* Set location to center screen */
                Location = new Point((screen.Bounds.Width / 2) - (Size.Width / 2), (screen.Bounds.Height / 2) - (Size.Height / 2));
            }
            else
            {
                Location = loc;
                Size = SettingsManager.Settings.Size;
                if (SettingsManager.Settings.Maximized)
                {
                    WindowState = FormWindowState.Maximized;
                }
            }
            OnResize(e);
            base.OnLoad(e);
        }

        protected override void OnResize(EventArgs e)
        {
            GraphicsManager.Rescale(ClientSize);
            base.OnResize(e);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                SettingsManager.Settings.Location = Location;
                SettingsManager.Settings.Size = Size;
            }
            SettingsManager.Settings.Maximized = WindowState == FormWindowState.Maximized;
            SettingsManager.Save();
            base.OnFormClosing(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            GraphicsManager.DrawGame(_currentGame, e.Graphics, ClientRectangle);
            base.OnPaint(e);
        }
        #endregion

        #region Mouse

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_currentGame == null)
            {
                return;
            }

            var c = HitTest.Compare(_currentGame.Players[0].Cards, e.Location);
            if (c != null)
            {
                Debug.Print("I have a card: " + c.Color + " " + c.Value + " " + c.Type);
            }
            base.OnMouseMove(e);
        }

        #endregion

        #region Sound callbacks
        private void SoundEffectRequest(SoundType type)
        {
            if (InvokeRequired)
            {
                _sync.Execute(() => SoundEffectRequest(type));
                return;
            }
            AudioManager.PlayEffect(type);
        }
        #endregion

        #region Timer callback
        private void ShowNewGameDialog(object sender, EventArgs e)
        {
            /* This allows the form to initialize and load properly before displaying the dialog */
            _tmrNew.Enabled = false;
            _tmrNew.Tick -= ShowNewGameDialog;
            _tmrNew.Dispose();

            var d = new FrmNew
            {
                NameData = SettingsManager.Settings.Options.NameData,
                NumberOfPlayers = SettingsManager.Settings.Options.NumberOfPlayers
            };
            if (d.ShowDialog(this) == DialogResult.OK)
            {
                SettingsManager.Settings.Options.NameData = d.NameData;
                SettingsManager.Settings.Options.NumberOfPlayers = d.NumberOfPlayers;
                NewGame(SettingsManager.Settings.Options);
            }
        }
        #endregion

        #region Private methods
        private void NewGame(GameOptionData data)
        {
            _currentGame = new Game(this, data);
            _currentGame.SoundEffect += SoundEffectRequest;
            AudioManager.PlayEffect(SoundType.Shuffle);
            Invalidate();
        }
        #endregion
    }
}
