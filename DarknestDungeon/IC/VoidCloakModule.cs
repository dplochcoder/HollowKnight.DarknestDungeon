using DarknestDungeon.Hero;
using DarknestDungeon.UnityExtensions;
using ItemChanger;
using Modding;
using Newtonsoft.Json;
using System;
using UnityEngine;

namespace DarknestDungeon.IC
{
    public class VoidCloakModule : ItemChanger.Modules.Module
    {
        public bool HasVoidCloak = false;

        public event Action OnTransition;

        [JsonIgnore]
        public Vector2? DashVelocityOverride = null;

        public override void Initialize()
        {
            ModHooks.GetPlayerBoolHook += OverrideGetBool;
            ModHooks.SetPlayerBoolHook += OverrideSetBool;
            ModHooks.DashVectorHook += OverrideDashVelocity;
            Events.OnBeginSceneTransition += OnSceneTransition;
            Events.AddFsmEdit(HeroUtil.KnightFsmID, HookVoidCloak);
        }

        public override void Unload()
        {
            ModHooks.GetPlayerBoolHook -= OverrideGetBool;
            ModHooks.SetPlayerBoolHook -= OverrideSetBool;
            ModHooks.DashVectorHook -= OverrideDashVelocity;
            Events.OnBeginSceneTransition -= OnSceneTransition;
            Events.RemoveFsmEdit(HeroUtil.KnightFsmID, HookVoidCloak);
        }

        public void HookVoidCloak(GameObject knight)
        {
            if (knight == null) return;
            knight.GetOrAddComponent<VoidCloakBehaviour>().Vcm = this;
        }

        private void HookVoidCloak(PlayMakerFSM fsm) => HookVoidCloak(fsm.gameObject);

        private bool OverrideGetBool(string boolName, bool orig) => boolName == nameof(HasVoidCloak) ? HasVoidCloak : orig;

        private bool OverrideSetBool(string boolName, bool value)
        {
            if (boolName == nameof(HasVoidCloak)) HasVoidCloak = value;
            return value;
        }

        private Vector2 OverrideDashVelocity(Vector2 v) => DashVelocityOverride ?? v;

        private void OnSceneTransition(Transition t) => OnTransition?.Invoke();
    }
}
