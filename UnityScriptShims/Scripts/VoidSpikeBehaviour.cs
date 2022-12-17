using UnityEngine;

namespace DarknestDungeon.Enemy
{
    public class VoidSpikeBehaviour : MonoBehaviour
    {
        public Sprite idleFullArmor;
        public Sprite idleHalfArmor;
        public Sprite idleArmorless;
        public Sprite launchFullArmor;
        public Sprite launchHalfArmor;
        public Sprite launchArmorless;

        public BoxCollider2D idleHitbox;
        public BoxCollider2D launchHitbox;
        public GameObject idleHurtbox;
        public GameObject launchHurtbox;
    }
}
