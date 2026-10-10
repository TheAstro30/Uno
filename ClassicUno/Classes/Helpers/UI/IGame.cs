using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClassicUno.Classes.DirectSound;
using ClassicUno.Classes.Helpers.Management;
using ClassicUno.Classes.Logic.Player;

namespace ClassicUno.Classes.Helpers.UI
{
    public interface IGame
    {
        void SoundEffectRequest(SoundType type);

        void VoiceRequest(VoiceType type, IPlayer player);

        void ShowButtons(bool visible);

        void EnableDraw(bool enabled);

        void EnablePass(bool enabled);

        void ShowColorButtons(bool visible);

        void Invalidate();
    }
}
