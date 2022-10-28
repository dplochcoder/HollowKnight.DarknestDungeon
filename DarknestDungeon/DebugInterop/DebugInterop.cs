using DarknestDungeon.IC;
using DebugMod;
using ItemChanger;
using Modding;
using System.Runtime.CompilerServices;
using UnityEngine;
using Random = System.Random;

namespace DarknestDungeon.DebugInterop
{
    public static class DebugInterop
    {
        public static void Setup()
        {
            DebugMod.DebugMod.AddToKeyBindList(typeof(DebugInterop));
        }

        [BindableMethod(name = "Warp to Birthplace", category = "DarknestDungeon")]
        public static void WarpToBirthplace()
        {
            if (!(ModHooks.GetMod("Benchwarp") is Mod))
            {
                Console.AddLine("Cannot warp; Benchwarp is not installed");
            }

            Console.AddLine("Warping to Birthplace exit");
            WarpToBirthplaceImpl();
        }

        private static void WarpToBirthplaceImpl()
        {
            var pd = PlayerData.instance;
            pd.respawnScene = "Abyss_15";
            pd.respawnMarkerName = "Debug Respawn Marker";
            pd.respawnType = 0;
            pd.mapZone = GlobalEnums.MapZone.ABYSS;
            Benchwarp.ChangeScene.WarpToRespawn();
        }

        [BindableMethod(name = "Give Void Cloak", category = "DarknestDungeon")]
        public static void GiveVoidCloak()
        {
            Console.AddLine("Giving Void Cloak");
            var mod = ItemChangerMod.Modules.GetOrAdd<VoidCloakModule>();
            mod.HookVoidCloak(GameObject.Find("/Knight"));
            mod.HasVoidCloak = true;
        }

        [BindableMethod(name = "Take Void Cloak", category = "DarknestDungeon")]
        public static void TakeVoidCloak()
        {
            Console.AddLine("Taking Void Cloak");
            var vcm = ItemChangerMod.Modules.Get<VoidCloakModule>();
            if (vcm != null) vcm.HasVoidCloak = false;
        }

        private const string DEBUG_FLAME_ID = "DebugFlame";

        [BindableMethod(name = "Give Random Void Flame", category = "DarknestDungeon")]
        public static void GiveRandomVoidFlame()
        {
            Console.AddLine("Giving Void Flame");
            var vfm = ItemChangerMod.Modules.GetOrAdd<VoidFlameModule>();
            if (!vfm.HeroHasVoidFlame)
            {
                vfm.UsedFlameIds.Remove(DEBUG_FLAME_ID);
                vfm.ClaimTemporaryFlame(DEBUG_FLAME_ID);
            }
        }

        [BindableMethod(name = "Take Void Flame", category = "DarknestDungeon")]
        public static void TakeVoidFlame()
        {
            Console.AddLine("Taking Void Flame");
            var vfm = ItemChangerMod.Modules.Get<VoidFlameModule>();
            if (vfm != null) vfm.LoseTemporaryFlame();
        }
    }
}
