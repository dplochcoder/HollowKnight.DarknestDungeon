using DarknestDungeon.IC;
using DebugMod;
using ItemChanger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DarknestDungeon.DebugInterop
{
    public static class DebugInterop
    {
        public static void Setup()
        {
            DebugMod.DebugMod.AddToKeyBindList(typeof(DebugInterop));
        }

        [BindableMethod(name = "Give Void Cloak", category = "DarknestDungeon")]
        public static void GiveVoidCloak()
        {
            DarknestDungeon.Log("Giving Void Cloak");
            var mod = ItemChangerMod.Modules.GetOrAdd<VoidCloakModule>();
            mod.HookVoidCloak(GameObject.Find("/Knight"));
            mod.HasVoidCloak = true;
        }

        public static void TakeVoidCloak()
        {
            DarknestDungeon.Log("Taking Void Cloak");
            var vcm = ItemChangerMod.Modules.Get<VoidCloakModule>();
            if (vcm != null) vcm.HasVoidCloak = false;
        }
    }
}
