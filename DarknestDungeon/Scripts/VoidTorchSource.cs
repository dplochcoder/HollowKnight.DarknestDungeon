using DarknestDungeon.IC;
using ItemChanger;
using UnityEngine;

namespace DarknestDungeon.Scripts
{
    public class VoidTorchSource : MonoBehaviour
    {
        public string flameId;
        public GameObject voidFlameObj;
        public GameObject nailZoneObj;

        private void Awake()
        {
            VoidFlameModule.OnTemporaryFlameLost += OnTempFlameLost;
            // TODO: On nail hit

            var vfm = ItemChangerMod.Modules.Get<VoidFlameModule>();
            if (vfm.IsFlameUsed(flameId))
            {
                MakeFlameActive(false);
            }
        }
        
        private void OnDestroy()
        {
            VoidFlameModule.OnTemporaryFlameLost -= OnTempFlameLost;
        }

        private void MakeFlameActive(bool active)
        {
            voidFlameObj.SetActive(active);
            nailZoneObj.SetActive(active);
        }

        private void OnTempFlameLost(string id)
        {
            if (id != flameId) return;

            MakeFlameActive(true);
        }
    }
}
