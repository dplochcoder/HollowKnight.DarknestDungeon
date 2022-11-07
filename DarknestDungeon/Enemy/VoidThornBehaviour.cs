using SFCore.Utils;
using UnityEngine;

namespace DarknestDungeon.Enemy
{
    public class VoidThornBehaviour : MonoBehaviour, IHitResponder
    {
        private HealthManager hm;
        private Rigidbody2D rb;
        private Vector3 origPos;

        private void Awake()
        {
            hm = GetComponent<HealthManager>();
            rb = GetComponent<Rigidbody2D>();

            origPos = gameObject.transform.position;
            hm.SetAttr("invincible", true);
        }

        public void Hit(HitInstance damageInstance)
        {
            // FIXME: Knockback, response
        }
    }
}
