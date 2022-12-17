using UnityEngine;

namespace DarknestDungeon.Enemy
{
    internal class ArmorControl
    {
        
    }

    public class VoidSpikeBehaviour : MonoBehaviour, IHitResponder
    {
        public HealthManager hm;

        // Editor fields
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

        public void Hit(HitInstance damageInstance)
        {
            if (damageInstance.AttackType == AttackTypes.Spell)
            {

            }
        }

        private void Awake()
        {
            this.hm = GetComponent<HealthManager>();
            this.tag = "Spell Vulnerable";
        }
    }
}
