using DarknestDungeon.Hero;
using ItemChanger;
using System.Collections.Generic;
using UnityEngine;
using static DarknestDungeon.UnityExtensions.Extensions;

namespace DarknestDungeon.IC
{
    public class VoidFlameModule : ItemChanger.Modules.Module
    {
        // ID for the flame currently on the player. Cleared when warping.
        public string? TemporaryFlameId;

        public HashSet<string> ClaimedFlameIds = new();
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

        public bool HeroHasVoidFlame => TemporaryFlameId != null;

        public bool IsFlameClaimed(string id) => TemporaryFlameId == id || ClaimedFlameIds.Contains(id);

        public bool IsTorchLit(string id) => LitTorchIds.Contains(id);

        private GMHookReference? tempFlameHook;

        public void ClaimTemporaryFlame(string flameId)
        {
            if (TemporaryFlameId != null || ClaimedFlameIds.Contains(flameId)) return;

            TemporaryFlameId = flameId;
            tempFlameHook ??= GameManager.instance.AddSelfDeletingHook(() => LoseTemporaryFlame());

            OnTemporaryFlameObtained?.Invoke(flameId);
            OnVoidFlameStateChange?.Invoke(true);
        }

        public void LoseTemporaryFlame()
        {
            if (TemporaryFlameId == null) return;

            var oldFlameId = TemporaryFlameId;
            TemporaryFlameId = null;
            tempFlameHook?.Unhook();
            tempFlameHook = null;

            OnTemporaryFlameLost?.Invoke(oldFlameId);
            OnVoidFlameStateChange?.Invoke(false);
        }

        public void LightTorch(string torchId)
        {
            if (TemporaryFlameId == null || LitTorchIds.Contains(torchId)) return;

            var oldFlameId = TemporaryFlameId;
            TemporaryFlameId = null;
            LitTorchIds.Add(torchId);
            tempFlameHook?.Unhook();
            tempFlameHook = null;

            OnTorchLit?.Invoke(oldFlameId, torchId);
            OnVoidFlameStateChange?.Invoke(false);
        }
    }
}
