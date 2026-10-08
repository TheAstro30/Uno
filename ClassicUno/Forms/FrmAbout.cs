/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System.Reflection;
using ClassicUno.Controls;

namespace ClassicUno.Forms
{
    public partial class FrmAbout : FormEx
    {
        public FrmAbout()
        {
            InitializeComponent();

            /* Get current assembly */
            var assembly = Assembly.GetExecutingAssembly();

            /* Get version information */
            var version = assembly.GetName().Version;

            lblVersion.Text = $@"Version: v{version.Major}.{version.Minor}";
        }
    }
}
