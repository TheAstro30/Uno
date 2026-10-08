/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;

namespace ClassicUno.Classes.Assets
{
    public enum CardColor
    {
        None = 0,
        Red = 1,
        Yellow = 2,
        Green = 3,
        Blue = 4
    }

    public enum CardType
    {
        None = 0,
        Numeric = 1,
        Skip = 2,
        Reverse = 3,
        DrawTwo = 4,
        DrawFour = 5,
        Wild = 6
    }

    [Serializable]
    public class Card
    {
        public CardType Type { get; set; }

        public CardColor Color { get; set; }

        public int Value { get; set; }
    }
}
