/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System.Collections.Generic;
using System.Drawing;
using ClassicUno.Classes.Assets;
using ClassicUno.Classes.Logic;
using ClassicUno.Properties;

namespace ClassicUno.Classes.Helpers.Management
{
    public static class GraphicsManager
    {
        #region CardKey
        /* KeyPair class for Dictionary object */
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
        #endregion

        #region Fields
        /* UNO! card images - this separates current game play card data from the actual bitmaps. This makes it easier
         * to create a smaller "game-save" file without including the images in the binary data file. */
        private static readonly Dictionary<CardKey, Bitmap> CardImages = new Dictionary<CardKey, Bitmap>();

        /* Back of the UNO! cards - this is displayed on the UI when cards are "face-down" */
        public static Bitmap CardBack { get; private set; }

        public static Bitmap Uno { get; private set; }
        #endregion

        static GraphicsManager()
        {
            BuildCardImages();
            BuildAssets();
        }

        #region Card image building
        private static void BuildCardImages()
        {
            var image = Resources.cards;
            var startY = 0;

            /* Loop through all four colors (this is the "Y" direction */
            for (var color = 0; color <= 3; color++)
            {
                /* Loop though all 14 cards of the resource image in the "X" direction "cutting-out" the images,
                 * or cropping, into a single bitmap of each card - paying particular attention to "special" cards;
                 * such as Wild, Wild Draw Four, Skip, UNO! Reverse, etc. */

                var startX = 0; /* This must be reset each new "Y" iteration */

                for (var i = 0; i <= 13; i++)
                {
                    var rect = new Rectangle(startX, startY, 144, 216);
                    var cardImg = image.Clone(rect, image.PixelFormat);

                    /* Set the transparency of the current card image to the predifined ARGB format of RGB(1, 1, 1) */
                    cardImg.MakeTransparent(Color.FromArgb(1, 1, 1));

                    /* Check which "X" position we are currently at for "special" cards, everything else is numeric */
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
                    startX += 144; /* Card image width */
                }
                startY += 216; /* Card image height */
            }
        }
        #endregion

        #region Asset building
        private static void BuildAssets()
        {
            /* Build other graphics related objects */
            Uno = new Bitmap(Resources.uno);
            Uno.MakeTransparent(Color.FromArgb(1, 1, 1));
        }
        #endregion

        #region Drawing methods
        public static void DrawGame(Game game, Graphics g, Rectangle clientRectangle)
        {
            /* Draw the current game in progress */
        }
        #endregion

        #region Private methods
        private static Bitmap GetCardImage(Card card)
        {
            /* This returns the bitmap image related to the UNO! card */
            var key = new CardKey(card.Color, card.Type, card.Value);
            return CardImages.TryGetValue(key, out var image) ? image : null;
        }

        private static void AddCardImage(CardColor color, CardType type, int value, Bitmap image)
        {
            /* Create a new dictionary entry in CardImages based on parameters */
            var key = new CardKey(color, type, value);
            CardImages.Add(key, image);
        }
        #endregion
    }
}
