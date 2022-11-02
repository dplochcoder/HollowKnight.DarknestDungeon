using DarknestDungeon.IC;
using ItemChanger;
using UnityEngine;

namespace DarknestDungeon.Scripts
{
    public class VoidTorchReceiver : MonoBehaviour
    {
        public string torchId;
        public GameObject voidFlameObj;
        public GameObject nailZoneObj;

        private void Awake()
        {
            VoidFlameModule.OnTorchLit += OnTorchLit;

            // TODO: On nail hit

            var vfm = ItemChangerMod.Modules.Get<VoidFlameModule>();
            if (vfm.IsTorchLit(torchId))
            {
                MakeFlameActive(true);
            }
        }
        
        private void OnDestroy()
        {
            VoidFlameModule.OnTorchLit -= OnTorchLit;
        }

        private void MakeFlameActive(bool active)
        {
            voidFlameObj.SetActive(active);
            nailZoneObj.SetActive(active);
        }

        private void OnTorchLit(string flameId, string torchId)
        {
            if (torchId != this.torchId) return;

            MakeFlameActive(true);
        }
    }
}
