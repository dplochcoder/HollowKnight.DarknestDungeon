using UnityEngine;

namespace DarknestDungeon.Scripts
{
    public class PatchHazardRespawnMarker : MonoBehaviour
    {
        public bool respawnFacingRight = true;

        private bool init = false;

        public HazardRespawnMarker GetHRM()
        {
            Init();
            return gameObject.GetComponent<HazardRespawnMarker>();
        }

        private void Init()
        {
            if (init) return;
            init = true;

            var hrm = gameObject.AddComponent<HazardRespawnMarker>();
            hrm.respawnFacingRight = respawnFacingRight;
        }

        private void Awake() => Init();
    }
}
