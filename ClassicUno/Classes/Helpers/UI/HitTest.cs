/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ClassicUno.Classes.Assets;

namespace ClassicUno.Classes.Helpers.UI
{
    public static class HitTest
    {
        public static Card Compare(List<Card> cards, Point location)
        {
            return cards.FirstOrDefault(c =>
                location.X >= c.Region.X && location.X <= c.Region.X + c.Region.Width && location.Y >= c.Region.Y &&
                location.Y <= c.Region.Y + c.Region.Height);
        }
    }
}
