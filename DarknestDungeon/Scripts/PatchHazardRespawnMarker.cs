using UnityEngine;

namespace DarknestDungeon.Scripts
{
    public class PatchHazardRespawnMarker : MonoBehaviour
    {
        public bool respawnFacingRight = true;

        private bool init = false;

        public void Init()
        {
            if (init) return;
            init = true;

            var hrm = gameObject.AddComponent<HazardRespawnMarker>();
            hrm.respawnFacingRight = respawnFacingRight;
            Destroy(this);
        }

        private void Awake() => Init();
    }
}
