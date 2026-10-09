/* Solitaire
 * Version 1.0.0
 * Written by: Jason James Newland
 * ©2025 Kangasoft Software */
/* Classic Uno 2026
* Version: 1.0
* By: Jason James Newland
* ©2026 - Kangasoft Software */
using System;
using System.Xml.Serialization;

namespace ClassicUno.Classes.Settings.SettingsData
{
    [Serializable]
    public sealed class SoundData
    {
        [XmlAttribute("voice")]
        public bool EnableVoice { get; set; }

        [XmlAttribute("voiceVolume")]
        public int VoiceVolume { get; set; }

        [XmlAttribute("effects")]
        public bool EnableEffects { get; set; }

        [XmlAttribute("effectsVolume")]
        public int EffectsVolume { get; set; }

        [XmlAttribute("music")]
        public bool EnableMusic { get; set; }

        [XmlAttribute("musicVolume")]
        public int MusicVolume { get; set; }
    }
}