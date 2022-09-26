using ItemChanger;
using Modding;
using UnityEngine;

namespace DarknestDungeon.IC
{
    public class VoidCloakModule : ItemChanger.Modules.Module
    {
        public bool HasVoidCloak = false;

        public override void Initialize()
        {
            ModHooks.GetPlayerBoolHook += OverrideGetBool;
            ModHooks.SetPlayerBoolHook += OverrideSetBool;
            Events.AddFsmEdit(new FsmID("Knight", "ProxyFSM"), HookVoidCloak);
        }

        public override void Unload()
        {
            ModHooks.GetPlayerBoolHook -= OverrideGetBool;
            ModHooks.SetPlayerBoolHook -= OverrideSetBool;
            Events.RemoveFsmEdit(new FsmID("Knight", "ProxyFSM"), HookVoidCloak);
        }

        private bool OverrideGetBool(string boolName, bool orig) => boolName == nameof(HasVoidCloak) ? HasVoidCloak : orig;

        private bool OverrideSetBool(string boolName, bool value)
        {
            if (boolName == nameof(HasVoidCloak)) HasVoidCloak = value;
            return value;
        }

        private void HookVoidCloak(PlayMakerFSM fsm)
        {
            fsm.gameObject.AddComponent<VoidCloakBehaviour>().Vcm = this;
        }
    }
}
