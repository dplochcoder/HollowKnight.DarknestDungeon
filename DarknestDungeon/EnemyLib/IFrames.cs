using UnityEngine;

namespace DarknestDungeon.EnemyLib
{
    public class IFrames : MonoBehaviour
    {
        public const float INVULN_TIME = 0.15f;

        private float invuln = 0f;

        public bool AcceptHit()
        {
            if (invuln <= 0)
            {
                invuln = INVULN_TIME;
                return true;
            }
            return false;
        }

        private void Update() => invuln -= Time.deltaTime;
    }
}
