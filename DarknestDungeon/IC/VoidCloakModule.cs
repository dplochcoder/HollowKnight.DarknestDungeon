﻿using ItemChanger;
using Modding;
using Newtonsoft.Json;
using UnityEngine;

namespace DarknestDungeon.IC
{
    public class VoidCloakModule : ItemChanger.Modules.Module
    {
        public bool HasVoidCloak = false;

        [JsonIgnore]
        public Vector2? DashVelocityOverride = null;

        public override void Initialize()
        {
            ModHooks.GetPlayerBoolHook += OverrideGetBool;
            ModHooks.SetPlayerBoolHook += OverrideSetBool;
            ModHooks.DashVectorHook += OverrideDashVelocity;
            Events.AddFsmEdit(new FsmID("Knight", "ProxyFSM"), HookVoidCloak);
        }

        public override void Unload()
        {
            ModHooks.GetPlayerBoolHook -= OverrideGetBool;
            ModHooks.SetPlayerBoolHook -= OverrideSetBool;
            ModHooks.DashVectorHook -= OverrideDashVelocity;
            Events.RemoveFsmEdit(new FsmID("Knight", "ProxyFSM"), HookVoidCloak);
        }

        private bool OverrideGetBool(string boolName, bool orig) => boolName == nameof(HasVoidCloak) ? HasVoidCloak : orig;

        private bool OverrideSetBool(string boolName, bool value)
        {
            if (boolName == nameof(HasVoidCloak)) HasVoidCloak = value;
            return value;
        }

        private Vector2 OverrideDashVelocity(Vector2 v) => DashVelocityOverride ?? v;

        private void HookVoidCloak(PlayMakerFSM fsm)
        {
            fsm.gameObject.AddComponent<VoidCloakBehaviour>().Vcm = this;
        }
    }
}
