/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using ClassicUno.Classes.Assets;
using ClassicUno.Classes.Logic;
using ClassicUno.Classes.Logic.Player;
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

        private static float _scale;
        private static float _cardWidth;
        private static float _cardHeight;

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

        #region Scaling
        public static void Rescale(Size clientSize)
        {
            const float referenceWidth = 1600f;
            const float referenceHeight = 850f;

            _scale = Math.Min(clientSize.Width / referenceWidth, clientSize.Height / referenceHeight);

            _cardWidth = 110f * _scale;
            _cardHeight = 160f * _scale;
        }
        #endregion

        #region Drawing methods
        public static void DrawGame(Game game, Graphics g, Rectangle clientRectangle)
        {
            /* Draw the current game in progress */
            if (game == null)
            {
                return;
            }

            DrawDeck(game, g, clientRectangle);
            DrawDisposePile(game, g, clientRectangle);
            DrawPlayerCards(game, g, clientRectangle);
            DrawPlayerNames(game, g, clientRectangle);
        }

        private static void DrawDeck(Game game, Graphics g, Rectangle clientRectangle)
        {
            /* Get the center of the clientRectangle */
            var position = new PointF((clientRectangle.Width - _cardWidth) / 2f - (_cardWidth + 10) / 2f, (clientRectangle.Height - _cardHeight) / 2f);

            var totalCards = game.Deck.Count;

            if (totalCards == 0)
            {
                /* Don't draw anything if the list is empty (which it shouldn't be, as the deck
                 * will be reshuffled if it gets down to 4 cards remaining. */
                return;
            }

            /* Map your total list count (up to 108) to a visual layer count between 1 and 10 */
            var maxPossibleCards = 108;
            var ratio = (double)Math.Min(totalCards, maxPossibleCards) / maxPossibleCards;
            var visualLayers = (int)Math.Ceiling(ratio * 10);

            /* Ensures it's always between 1 and 10 */
            visualLayers = Math.Max(1, Math.Min(10, visualLayers));
            
            /* Loop through and draw only the calculated visual layers */
            for (var i = 0; i < visualLayers; i++)
            {
                DrawCard(g, CardBack, clientRectangle, position);
                position.X += 1;
                position.Y += 1;
            }
        }

        private static void DrawDisposePile(Game game, Graphics g, Rectangle clientRectangle)
        {
            if (game.DisposePile.Count == 0)
            {
                return;
            }
            var position = new PointF((_cardWidth + 10) / 2f + (clientRectangle.Width - _cardWidth) / 2f, (clientRectangle.Height - _cardHeight) / 2f);

            DrawCard(g, GetCardImage(game.DisposePile[game.DisposePile.Count - 1]), clientRectangle, position);

        }

        private static void DrawPlayerCards(Game game, Graphics g, Rectangle clientRectangle)
        {
            /* Clockwise: bottom, left, top, right. */
            var angle = 0;
            foreach (var player in game.Players)
            {
                DrawPlayerHand(player, g, clientRectangle, angle);
                angle += 90;
            }
        }

        private static void DrawPlayerHand(IPlayer player, Graphics g, Rectangle clientRectangle, int angle)
        {
            if (player.Cards == null || player.Cards.Count == 0)
            {
                return;
            }

            var count = player.Cards.Count;

            /* Upright cards use their normal width for spacing. Sideways cards are spaced along their rotated width. */
            var sideways = angle == 90 || angle == 270;

            var cardWidth = _cardWidth;
            var cardHeight = _cardHeight;

            var spacing = sideways ? cardHeight * 0.17f : cardWidth * 0.25f ;
            var available = sideways ? clientRectangle.Height - 40f : clientRectangle.Width - 40f;

            if (count > 1)
            {
                spacing = Math.Min(spacing, (available - cardWidth) / (count - 1));
            }

            if (spacing < 0f)
            {
                spacing = 0f;
            }

            var totalWidth = cardWidth + (count - 1) * spacing;
            var totalHeight = cardHeight + (count - 1) * spacing;

            for (var i = 0; i < count; i++)
            {
                /* Show card back if player is not human */
                Image image = player.GetType() == typeof(HumanPlayer) ? GetCardImage(player.Cards[i]) : CardBack;

                if (image == null)
                {
                    continue;
                }

                float x;
                float y;
                if (!sideways)
                {
                    x = clientRectangle.Left + (clientRectangle.Width - totalWidth) / 2f + i * spacing;
                    y = angle == 0 ? clientRectangle.Bottom - cardHeight - 120f : clientRectangle.Top + 50f;
                    DrawCard(g, image, clientRectangle, new PointF(x - clientRectangle.Left, y - clientRectangle.Top));
                }
                else
                {
                    x = angle == 90 ? clientRectangle.Left + cardWidth / 2f + 50f : clientRectangle.Right - cardWidth / 2f - 50f;
                    y = clientRectangle.Top + (clientRectangle.Height - totalHeight) / 2f + i * spacing + cardHeight / 2f - cardWidth / 2f;
                    DrawRotatedCard(g, image, x, y, angle);
                }
            }
        }

        private static void DrawCard(Graphics g, Image cardImage, Rectangle clientRectangle, PointF position)
        {
            /* This draws a card anywhere on screen (related to "position") scaled; and is kept track of on resize */
            g.DrawImage(cardImage, new RectangleF(clientRectangle.Left + position.X, clientRectangle.Top + position.Y, _cardWidth, _cardHeight));
        }

        private static void DrawRotatedCard(Graphics g, Image cardImage, float centerX, float centerY, float angle)
        {
            var state = g.Save();

            try
            {
                g.TranslateTransform(centerX, centerY);
                g.RotateTransform(angle);
                g.DrawImage(cardImage, new RectangleF(-_cardWidth / 2f, -_cardHeight / 2f, _cardWidth, _cardHeight));
            }
            finally
            {
                g.Restore(state);
            }
        }

        private static void DrawPlayerNames(Game game, Graphics g, Rectangle clientRectangle)
        {
            /* Positions relative to the client clientRectangle (bottom, left, top, right) */
            PointF[] positions =
            {
                new PointF(clientRectangle.Left + clientRectangle.Width * 0.50f,
                    clientRectangle.Top + clientRectangle.Height * 0.88f),
                new PointF(clientRectangle.Left + clientRectangle.Width * 0.16f,
                    clientRectangle.Top + clientRectangle.Height * 0.50f),
                new PointF(clientRectangle.Left + clientRectangle.Width * 0.50f,
                    clientRectangle.Top + clientRectangle.Height * 0.24f),
                new PointF(clientRectangle.Left + clientRectangle.Width * 0.85f,
                    clientRectangle.Top + clientRectangle.Height * 0.50f)
            };

            using (var font = new Font("Arial", 14, FontStyle.Bold))
            {
                using (var shadow = new SolidBrush(Color.Black))
                {
                    using (var normalText = new SolidBrush(Color.FromArgb(255, 225, 140)))
                    {
                        using (var highlightText = new SolidBrush(Color.DeepSkyBlue))
                        {
                            using (var sf = new StringFormat())
                            {
                                sf.Alignment = StringAlignment.Center;
                                sf.LineAlignment = StringAlignment.Center;

                                for (var i = 0; i < game.Players.Count; i++)
                                {
                                    /* Draw shadow */
                                    g.DrawString(game.Players[i].NameData.Name, font, shadow, positions[i].X + 2,
                                        positions[i].Y + 2, sf);

                                    /* Draw text; bottom player (index 0) is the human player */
                                    Brush textBrush = game.CurrentPlayer == game.Players[i] ? highlightText : normalText;
                                    g.DrawString(game.Players[i].NameData.Name, font, textBrush, positions[i].X,
                                        positions[i].Y,
                                        sf);
                                }
                            }
                        }
                    }
                }
            }
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
