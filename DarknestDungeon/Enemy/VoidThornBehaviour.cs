using HutongGames.PlayMaker;
using SFCore.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace DarknestDungeon.Enemy
{
    public class VoidThornBehaviour : MonoBehaviour, IHitResponder
    {
        private enum State
        {
            Idle,
            Triggered,
            Expanding,
            Expanded,
            Retracting,
            Respawning
        }
        private State state = State.Idle;
        private float stateDuration = 0;

        private HealthManager hm;
        private Rigidbody2D rb;
        private Vector3 origPos;
        private Vector2 origPos2d;

        private Vector3 destination;
        private float shuffleTimer;

        private void Awake()
        {
            hm = GetComponent<HealthManager>();
            rb = GetComponent<Rigidbody2D>();

            origPos = gameObject.transform.position;
            origPos2d = new(origPos.x, origPos.y);
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

        private const float IMPULSE_DISTANCE = 0.55f;
        private const float IMPULSE_DURATION_SECONDS = 0.1f;

        private record Impulse
        {
            public Vector2 velocity;
            public float remaining;

            public Impulse(Vector2 velocity, float remaining)
            {
                this.velocity = velocity;
                this.remaining = remaining;
            }
        }
        private List<Impulse> impulses = new();

        public void Hit(HitInstance damageInstance)
        {
            var dir = (Quaternion.Euler(0, 0, damageInstance.Direction) * new Vector3(1, 0, 0)).normalized;
            impulses.Add(new(dir * (IMPULSE_DISTANCE / IMPULSE_DURATION_SECONDS), IMPULSE_DURATION_SECONDS));
        }

        private Vector2 pos2d => new(rb.position.x, rb.position.y);

        private const float RECOVERY_TIME = 1.25f;

        private Vector2 Gravitate()
        {
            var dist = pos2d - origPos2d;
            var dir = -dist;
            return dir / RECOVERY_TIME;
        }

        private bool Mobile => state != State.Expanded && state != State.Respawning;

        private void FixedUpdate()
        {
            // Sum velocities, tick them.
            Vector2 velocity = Gravitate();
            foreach (var impulse in impulses)
            {
                velocity += impulse.velocity;
                impulse.remaining -= Time.fixedDeltaTime;
            }
            impulses.RemoveAll(i => i.remaining <= 0);

            rb.velocity = Mobile ? velocity : new(0, 0);
        }

        private void SetState(State s)
        {
            state = s;
            switch (s)
            {

            }
        }

        private void Update()
        {
            stateDuration -= Time.deltaTime;
            if (stateDuration <= 0)
            {
                switch (state)
                {
                    case State.Triggered:
                        SetState(State.Expanding);
                        break;
                    case State.Expanding:
                        SetState(State.Expanded);
                        break;
                    case State.Expanded:
                        SetState(State.Retracting);
                        break;
                    case State.Retracting:
                        SetState(State.Idle);
                        break;
                    case State.Respawning:
                        SetState(State.Idle);
                        break;
                }
            }
            else
            {

            }
        }
    }
}
