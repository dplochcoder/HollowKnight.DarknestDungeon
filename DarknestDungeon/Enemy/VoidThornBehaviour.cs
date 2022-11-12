using SFCore.Utils;
using UnityEngine;

namespace DarknestDungeon.Enemy
{
    public class VoidThornBehaviour : MonoBehaviour, IHitResponder
    {
        private HealthManager hm;
        private Rigidbody2D rb;
        private Vector3 origPos;

        private Vector3 destination;
        private float shuffleTimer;

        private void Awake()
        {
            hm = GetComponent<HealthManager>();
            rb = GetComponent<Rigidbody2D>();

            origPos = gameObject.transform.position;
            hm.SetAttr("invincible", true);

            ShuffleDestination();
        }

        private const float SHUFFLE_RADIUS = 1.8f;

        private void ShuffleDestination()
        {
            var oldDestination = destination;
            while (true)
            {
                float radius = Mathf.Sqrt(Random.Range(0, 1)) * SHUFFLE_RADIUS;
                float theta = Random.Range(0, 360);
                destination = origPos + Quaternion.Euler(0, 0, Random.Range(0, Mathf.PI * 2)) * new Vector3(radius, 0, 0);

                if (Vector3.Distance(oldDestination, destination) > SHUFFLE_RADIUS / 2) return;
            }
        }

        private const float IMPULSE_DISTANCE = 0.8f;
        private const float IMPULSE_DURATION_SECONDS = 0.1f;


        public void Hit(HitInstance damageInstance)
        {
            
        }

        private void FixedUpdate()
        {

        }
    }
}
