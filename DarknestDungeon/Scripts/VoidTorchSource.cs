using DarknestDungeon.IC;
using DarknestDungeon.UnityExtensions;
using ItemChanger;
using UnityEngine;

namespace DarknestDungeon.Scripts
{
    public class VoidTorchSource : MonoBehaviour, IHitResponder
    {
        private VoidFlameModule Vfm;

        public string flameId;
        public GameObject voidFlameObj;

        private void Awake()
        {
            Vfm = ItemChangerMod.Modules.Get<VoidFlameModule>();

            VoidFlameModule.OnTemporaryFlameObtained += OnTemporaryFlameObtained;
            VoidFlameModule.OnTemporaryFlameLost += OnTemporaryFlameLost;

            SetFlameActive(!Vfm.IsFlameUsed(flameId));
        }

        private void OnDestroy()
        {
            VoidFlameModule.OnTemporaryFlameObtained -= OnTemporaryFlameObtained;
            VoidFlameModule.OnTemporaryFlameLost -= OnTemporaryFlameLost;
        }

        public void Hit(HitInstance hitInstance)
        {
            if (!voidFlameObj.activeSelf) return;
            if (hitInstance.AttackType != AttackTypes.Nail) return;

            Vfm.ClaimTemporaryFlame(flameId);
        }

        private void SetFlameActive(bool active)
        {
            voidFlameObj.SetActive(active);
            gameObject.GetOrAddComponent<NonBouncer>().SetActive(!active);
        }

        private void OnTemporaryFlameObtained(string id)
        {
            if (id != flameId) return;
            SetFlameActive(false);
        }

        private void OnTemporaryFlameLost(string id)
        {
            if (id != flameId) return;
            SetFlameActive(true);
        }
    }
}
