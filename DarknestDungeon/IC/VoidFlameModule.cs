using DarknestDungeon.Hero;
using ItemChanger;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using static DarknestDungeon.UnityExtensions.Extensions;

namespace DarknestDungeon.IC
{
    public class VoidFlameModule : ItemChanger.Modules.Module
    {
        // ID for the flame currently on the player. Cleared when warping.
        public string? TemporaryFlameId;

        public HashSet<string> UsedFlameIds = new();
        public HashSet<string> LitTorchIds = new();

        public delegate void VoidFlameStateChange(bool hasVoidFlame);
        public static event VoidFlameStateChange? OnVoidFlameStateChange;

        public delegate void TemporaryFlameObtained(string id);
        public static event TemporaryFlameObtained? OnTemporaryFlameObtained;

        public delegate void TemporaryFlameLost(string id);
        public static event TemporaryFlameLost? OnTemporaryFlameLost;

        public delegate void TorchLit(string flameId, string torchId);
        public static event TorchLit? OnTorchLit;

        public override void Initialize()
        {
            Events.AddFsmEdit(HeroUtil.KnightFsmID, HookVoidFlame);
        }

        public override void Unload()
        {
            Events.RemoveFsmEdit(HeroUtil.KnightFsmID, HookVoidFlame);

            tempFlameHook?.Unhook();
        }

        public void HookVoidFlame(GameObject knight)
        {
            if (knight == null) return;
            knight.GetOrAddComponent<VoidFlameBehaviour>().Vfm = this;
        }

        public void HookVoidFlame(PlayMakerFSM fsm) => HookVoidFlame(fsm.gameObject);

        [JsonIgnore]
        public bool HeroHasVoidFlame => TemporaryFlameId != null;

        public bool IsFlameUsed(string id) => TemporaryFlameId == id || UsedFlameIds.Contains(id);

        public bool IsTorchLit(string id) => LitTorchIds.Contains(id);

        private GMHookReference? tempFlameHook;

        public bool ClaimTemporaryFlame(string flameId)
        {
            if (TemporaryFlameId != null || UsedFlameIds.Contains(flameId)) return false;

            TemporaryFlameId = flameId;
            tempFlameHook ??= GameManager.instance.AddSelfDeletingHook(() => LoseTemporaryFlame());

            OnTemporaryFlameObtained?.Invoke(flameId);
            OnVoidFlameStateChange?.Invoke(true);
            return true;
        }

        public bool LoseTemporaryFlame()
        {
            if (TemporaryFlameId == null) return false;

            var oldFlameId = TemporaryFlameId;
            TemporaryFlameId = null;
            tempFlameHook?.Unhook();
            tempFlameHook = null;

            OnTemporaryFlameLost?.Invoke(oldFlameId);
            OnVoidFlameStateChange?.Invoke(false);
            return true;
        }

        public bool LightTorch(string torchId)
        {
            if (TemporaryFlameId == null || LitTorchIds.Contains(torchId)) return false;

            var oldFlameId = TemporaryFlameId;
            TemporaryFlameId = null;
            UsedFlameIds.Add(oldFlameId);
            LitTorchIds.Add(torchId);
            tempFlameHook?.Unhook();
            tempFlameHook = null;

            OnTorchLit?.Invoke(oldFlameId, torchId);
            OnVoidFlameStateChange?.Invoke(false);
            return true;
        }
    }
}
