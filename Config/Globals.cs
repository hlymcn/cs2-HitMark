using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;
using System.Diagnostics;

namespace CS2_HitMark;

public class Globals
{
    public class PlayerDataClass
    {
        public CCSPlayerController Player { get; set; }
        public string Sound_HeadShot { get; set; }       
        public string Sound_BodyShot { get; set; }
        public int DamageAnimToken { get; set; }
        public bool HitMarkEnabled { get; set; }
        public bool SoundEnabled { get; set; }
    
        public PlayerDataClass(CCSPlayerController player, string sound_HeadShot, string sound_BodyShot)
        {
            Player = player;
            Sound_HeadShot = sound_HeadShot;
            Sound_BodyShot = sound_BodyShot;
            DamageAnimToken = 0;
            HitMarkEnabled = true;
            SoundEnabled = true;
        }
    }
    public Dictionary<CCSPlayerController, PlayerDataClass> Player_Data = new Dictionary<CCSPlayerController, PlayerDataClass>();
}
