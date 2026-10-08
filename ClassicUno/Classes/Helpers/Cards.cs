/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System.Collections.Generic;
using System.Drawing;
using ClassicUno.Classes.Assets;
using ClassicUno.Properties;

namespace ClassicUno.Classes.Helpers
{
    public static class Cards
    {
        private class CardKey
        {
            private readonly CardColor _color;
            private readonly CardType _type;
            private readonly int _value;

            public CardKey(CardColor color, CardType type, int value)
            {
                _color = color;
                _type = type;
                _value = value;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is CardKey other))
                {
                    return false;
                }
                return _color == other._color && _type == other._type && _value == other._value;
            }

            public override int GetHashCode()
            {
                return (int)_color * 10000 + (int)_type * 100 + _value;
            }
        }

        private static readonly Dictionary<CardKey, Bitmap> CardImages = new Dictionary<CardKey, Bitmap>();

        public static Bitmap CardBack { get; private set; }

        public static void BuildCardImages()
        {
            var image = Resources.cards;
            CardKey card;
            Rectangle rect;
            var startY = 0;

            for (var color = 0; color <= 3; color++)
            {
                var startX = 0;
                for (var i = 0; i <= 13; i++)
                {
                    rect = new Rectangle(startX, startY, 144, 216);
                    var cardImg = image.Clone(rect, image.PixelFormat);
                    cardImg.MakeTransparent(Color.FromArgb(1, 1, 1));

                    switch (i)
                    {
                        case 10:
                            /* Skip */
                            AddCardImage((CardColor)color + 1, CardType.Skip, 0, cardImg);
                            break;

                        case 11:
                            /* Reverse */
                            AddCardImage((CardColor)color + 1, CardType.Reverse, 0, cardImg);
                            break;

                        case 12:
                            /* Draw two */
                            AddCardImage((CardColor)color + 1, CardType.DrawTwo, 0, cardImg);
                            break;

                        case 13:
                            switch (color)
                            {
                                /* Wild/wild draw four */
                                case 0:
                                    /* Wild */
                                    AddCardImage(CardColor.None, CardType.Wild, 0, cardImg);
                                    break;

                                case 1:
                                    /* Wild draw four */
                                    AddCardImage(CardColor.None, CardType.DrawFour, 0, cardImg);
                                    break;

                                case 2:
                                    /* Card back */
                                    CardBack = cardImg;
                                    break;
                            }
                            break;

                        default:
                            /* Numeric */
                            AddCardImage((CardColor)color + 1, CardType.Numeric, i, cardImg);
                            break;
                    }
                    startX += 144;
                }
                startY += 216;
            }
        }

        public static Bitmap GetCardImage(Card card)
        {
            var key = new CardKey(card.Color, card.Type, card.Value);
            return CardImages.TryGetValue(key, out var image) ? image : null;
        }

        private static void AddCardImage(CardColor color, CardType type, int value, Bitmap image)
        {
            var key = new CardKey(color, type, value);
            CardImages.Add(key, image);
        }
    }
}
