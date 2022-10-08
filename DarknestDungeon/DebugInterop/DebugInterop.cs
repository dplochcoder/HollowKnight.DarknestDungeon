using DarknestDungeon.IC;
using DebugMod;
using ItemChanger;
using Modding;
using UnityEngine;

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
    }
}
