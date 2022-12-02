using DarknestDungeon.EnemyLib;
using HutongGames.PlayMaker;
using SFCore.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace DarknestDungeon.Enemy
{
    using TimerModule = EnemyLib.TimerModule<VoidThornBehaviour.StateId, VoidThornBehaviour.State, VoidThornBehaviour.StateMachine>;

    public class VoidThornBehaviour : MonoBehaviour, IHitResponder
    {
        public enum StateId
        {
            Idle,
            Triggered,
            Expanding,
            Expanded,
            Retracting,
            Respawning
        }

        public class State : EnemyState<StateId, State, StateMachine>
        {
            public State(StateMachine mgr) : base(mgr) { }

            public virtual bool Mobile => true;

            public virtual void Hit() { }
        }

        private class IdleState : State
        {
            public IdleState(StateMachine mgr) : base(mgr) { }

            public override void Hit() => Mgr.ChangeState(StateId.Triggered);
        }

        public static float TRIGGERED_TIME = 0.05f;

        private class TriggeredState : State
        {
            public TriggeredState(StateMachine mgr) : base(mgr)
            {
                AddMod(new TimerModule(mgr, TRIGGERED_TIME, StateId.Expanding));
            }
        }

        public static float EXPANDING_TIME = 0.125f;
        public static float EXPANDED_SCALE = 2.5f;

        private class ExpandingState : State
        {
            public TimerModule timer;

            public ExpandingState(StateMachine mgr) : base(mgr)
            {
                timer = AddMod(new TimerModule(mgr, EXPANDING_TIME, StateId.Expanded));
            }
            public override bool Mobile => false;

            protected override void Update()
            {
                var scale = Mathf.Pow(EXPANDED_SCALE, timer.ProgPct * timer.ProgPct);
                Mgr.Vtb.rb.transform.localScale = new(scale, scale, scale);
            }
        }

        public static float EXPANDED_TIME = 1.1f;

        private class ExpandedState : State
        {
            private int hits = 0;
            private TimerModule timer;

            public ExpandedState(StateMachine mgr) : base(mgr)
            {
                Mgr.Vtb.rb.transform.localScale = new(EXPANDED_SCALE, EXPANDED_SCALE, EXPANDED_SCALE);
                timer = AddMod(new TimerModule(mgr, EXPANDED_TIME, StateId.Retracting));
            }
            public override bool Mobile => false;

            public override void Hit()
            {
                timer.Remaining += 0.5f / (++hits);
            }
        }

        public static float RETRACTING_TIME = 0.225f;

        private class RetractingState : State
        {
            private TimerModule timer;

            public RetractingState(StateMachine mgr) : base(mgr)
            {
                timer = AddMod(new TimerModule(mgr, RETRACTING_TIME, StateId.Idle));
            }

            protected override void Update()
            {
                var scale = Mathf.Pow(EXPANDED_SCALE, Mathf.Sqrt(timer.RemainingPct));
                Mgr.Vtb.rb.transform.localScale = new(scale, scale, scale);
            }

            public override void Hit()
            {
                var prog = timer.RemainingPct;
                (Mgr.ChangeState(StateId.Expanding) as ExpandingState).timer.ProgPct = prog;
            }
        }

        public static float RESPAWNING_TIME = 1.5f;

        // TODO: sprites
        private class RespawningState : State
        {
            public RespawningState(StateMachine mgr) : base(mgr)
            {
                AddMod(new TimerModule(mgr, RESPAWNING_TIME, StateId.Idle));
                Mgr.Vtb.b2d.enabled = false;
            }

            protected override void Stop()
            {
                Mgr.Vtb.b2d.enabled = true;
                Mgr.Vtb.hm.hp = Mgr.Vtb.origHealth;
            }
        }

        public class StateMachine : EnemyStateMachine<StateId, State, StateMachine>
        {
            public readonly VoidThornBehaviour Vtb;

            public StateMachine(VoidThornBehaviour vtb) : base(StateId.Idle, new()
            {
                { StateId.Idle, m => new IdleState(m) },
                { StateId.Triggered, m => new TriggeredState(m) },
                { StateId.Expanding, m => new ExpandingState(m) },
                { StateId.Expanded, m => new ExpandedState(m) },
                { StateId.Retracting, m => new RetractingState(m) },
                { StateId.Respawning, m => new RespawningState(m) },
            }) {
                this.Vtb = vtb;
            }

            public override StateMachine AsTyped() => this;
        }

        public HealthManager hm;
        public int origHealth;
        public Rigidbody2D rb;
        public BoxCollider2D b2d;
        public Vector3 origPos;
        public Vector3 destination;
        public Vector2 origPos2d;
        public Vector2 destination2d;

        public float shuffleTimer;
        public StateMachine sm;

        private void Awake()
        {
            hm = GetComponent<HealthManager>();
            origHealth = hm.hp;
            hm.OnDeath += () => sm.ChangeState(StateId.Respawning);

            rb = GetComponent<Rigidbody2D>();
            b2d = GetComponent<BoxCollider2D>();

            origPos = gameObject.transform.position;
            origPos2d = new(origPos.x, origPos.y);
            hm.SetAttr("invincible", true);

            shuffleTimer = 0;
            ShuffleDestination();
            sm = new(this);
        }

        private static float SHUFFLE_TIMER = 2.5f;
        private static float SHUFFLE_RADIUS = 0.65f;

        private void ShuffleDestination()
        {
            shuffleTimer -= Time.fixedDeltaTime;
            if (shuffleTimer > 0) return;

            shuffleTimer += SHUFFLE_TIMER;
            Vector3 oldDestination = new(destination.x, destination.y, destination.z);
            while (true)
            {
                float radius = Mathf.Sqrt(Random.Range(0, 1)) * SHUFFLE_RADIUS;
                destination = origPos + Quaternion.Euler(0, 0, Random.Range(0, 360)) * new Vector3(radius, 0, 0);
                if (Vector3.Distance(destination, oldDestination) > SHUFFLE_RADIUS / 2)
                {
                    destination2d = new(destination.x, destination.y);
                    return;
                }
            }
        }

        private static float IMPULSE_DISTANCE = 0.55f;
        private static float IMPULSE_DURATION_SECONDS = 0.1f;

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

            sm.CurrentState.Hit();
        }

        private Vector2 pos2d => new(rb.position.x, rb.position.y);

        private const float RECOVERY_TIME = 1.25f;

        private Vector2 Gravitate()
        {
            var dist = pos2d - destination2d;
            var dir = -dist;
            return dir / RECOVERY_TIME;
        }

        private void FixedUpdate()
        {
            ShuffleDestination();

            // Sum velocities, tick them.
            Vector2 velocity = Gravitate();
            foreach (var impulse in impulses)
            {
                velocity += impulse.velocity;
                impulse.remaining -= Time.fixedDeltaTime;
            }
            impulses.RemoveAll(i => i.remaining <= 0);

            rb.velocity = sm.CurrentState.Mobile ? velocity : new(0, 0);
        }

        private void Update() => sm.Update();
    }
}
