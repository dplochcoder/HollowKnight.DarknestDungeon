using UnityEngine;

namespace DarknestDungeon.Scripts
{
    public class PatchHazardRespawnTrigger : MonoBehaviour
    {
        public PatchHazardRespawnMarker RespawnMarker;

        private void Awake()
        {
            var hrt = gameObject.AddComponent<HazardRespawnTrigger>();
            hrt.respawnMarker = RespawnMarker.GetHRM();
            gameObject.AddComponent<DeactivateInDarknessWithoutLantern>();
        }
    }
}
