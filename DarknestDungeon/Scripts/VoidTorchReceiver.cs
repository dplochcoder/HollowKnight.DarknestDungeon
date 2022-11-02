using DarknestDungeon.IC;
using DarknestDungeon.UnityExtensions;
using ItemChanger;
using UnityEngine;
using static DarknestDungeon.IC.VoidFlameModule;

namespace DarknestDungeon.Scripts
{
    public class VoidTorchReceiver : MonoBehaviour, IHitResponder
    {
        private VoidFlameModule Vfm;

        public string torchId;
        public GameObject voidFlameObj;

        private void Awake()
        {
            Vfm = ItemChangerMod.Modules.Get<VoidFlameModule>();

            VoidFlameModule.OnTorchLit += OnTorchLit;

            SetFlameActive(Vfm.IsTorchLit(torchId));
        }

        private void OnDestroy()
        {
            VoidFlameModule.OnTorchLit -= OnTorchLit;
        }

        public void Hit(HitInstance hitInstance)
        {
            if (voidFlameObj.activeSelf) return;
            if (hitInstance.AttackType != AttackTypes.Nail) return;

            Vfm.LightTorch(torchId);
        }

        private void SetFlameActive(bool active)
        {
            voidFlameObj.SetActive(active);
            gameObject.GetOrAddComponent<NonBouncer>().SetActive(!active);
        }

        private void OnTorchLit(string flameId, string torchId)
        {
            if (torchId != this.torchId) return;
            SetFlameActive(true);
        }
    }
}
