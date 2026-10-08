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
    public interface IPlayer
    {
        PlayerNameData NameData { get; set; }

        List<Card> Cards { get; set; }

        event Action<IPlayer, Card> PlayerPlaysCard;
        event Action<IPlayer> PlayerPass;
        event Action<IPlayer> PlayerEndTurn;
        event Action<IPlayer> PlayerInvalidateRequired;

        void BeginTurn(Game game);

        void Pass();

        void EndTurn();
    }
}
