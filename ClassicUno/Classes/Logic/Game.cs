/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Collections.Generic;
using ClassicUno.Classes.Assets;
using ClassicUno.Classes.Helpers;
using ClassicUno.Classes.Logic.Player;

namespace ClassicUno.Classes.Logic
{
    [Serializable]
    public class Game
    {
        /* Basic game class */
        public List<IPlayer> Players { get; set; }

        public List<Card> Deck { get; set; }

        public List<Card> DisposePile { get; set; }

        public Game()
        {
            Players = new List<IPlayer>();

            Deck = new List<Card>();

            DisposePile = new List<Card>();

            /* Build the deck of 108 cards (four wilds/four wild draw four's) */
            for (var color = 0; color <= 3; color++)
            {
                /* Colors red, yellow, green, blue (ignore on wilds) */
                for (var i = 0; i <= 12; i++)
                {
                    Card c;
                    switch (i)
                    {
                        case 10:
                            /* Skip */
                            c = new Card
                            {
                                Color = (CardColor)color + 1,
                                Type = CardType.Skip
                            };
                            break;

                        case 11:
                            /* Reverse */
                            c = new Card
                            {
                                Color = (CardColor)color + 1,
                                Type = CardType.Reverse
                            };
                            break;

                        case 12:
                            /* Draw two */
                            c = new Card
                            {
                                Color = (CardColor)color + 1,
                                Type = CardType.DrawTwo
                            };
                            break;

                        default:
                            /* Numeric 0 - 9 */
                            c = new Card
                            {
                                Color = (CardColor) color + 1,
                                Type = CardType.Numeric,
                                Value = i
                            };
                            break;
                    }
                    Deck.Add(c);
                }
            }
            /* Add the four wilds/four wild draw four's */
            for (var i = 0; i <= 3; i++)
            {
                /* Wild card */
                Deck.Add(new Card
                {
                    Color = CardColor.None,
                    Type = CardType.Wild
                });
                /* Wild draw four */
                Deck.Add(new Card
                {
                    Color = CardColor.None,
                    Type = CardType.DrawFour
                });
            }
            /* Shuffle the deck */
            Deck.Shuffle();
        }
    }
}
