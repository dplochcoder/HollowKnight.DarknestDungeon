using DarknestDungeon.EnemyLib;
using DarknestDungeon.Scripts.Lib;
using SFCore.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace DarknestDungeon.Enemy
{
    using TimerModule = TimerModule<VoidThornBehaviour.StateId, VoidThornBehaviour.State, VoidThornBehaviour.StateMachine, VoidThornBehaviour>;

    internal class VoidThornBehaviour : GameplayMonoBehaviour, IHitResponder
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

        public class State : EnemyState<StateId, State, StateMachine, VoidThornBehaviour>
        {
            public State(StateMachine mgr) : base(mgr) { }

            public virtual bool Mobile => true;

            public virtual float InitScale => 0f;

            protected virtual Sprite InitSprite => Parent.defaultSprite;

            protected virtual void SetInitScale() => Parent.SetScale(InitScale);

            protected override void Init()
            {
                SetInitScale();
                Parent.spriteRenderer.sprite = InitSprite;
            }

            public virtual void Hit() { }
        }

        private class IdleState : State
        {
            public IdleState(StateMachine mgr) : base(mgr) { }

            public override void Hit()
            {
                Parent.chargePending = true;
                Mgr.ChangeState(StateId.Triggered);
            }
        }

        public static float _CONST_TRIGGERED_TIME = 0.05f;

        private class TriggeredState : State
        {
            public TriggeredState(StateMachine mgr) : base(mgr)
            {
                AddMod(new TimerModule(mgr, _CONST_TRIGGERED_TIME, StateId.Expanding));
            }
        }

        public static float _CONST_EXPANDING_TIME = 0.125f;
        public static float _CONST_EXPANDED_SCALE = 2.25f;
        public static float _CONST_ATTACK_DISTANCE = 2f;

        private bool chargePending = false;

        private void SetScale(float pct)
        {
            var scale = Mathf.Pow(_CONST_EXPANDED_SCALE, pct * pct);
            gameObject.transform.localScale = new(scale, scale, scale);
        }

        private Vector3 retractTarget;

        private class ExpandingState : State
        {
            public TimerModule timer;

            public ExpandingState(StateMachine mgr) : base(mgr)
            {
                timer = AddMod(new TimerModule(mgr, _CONST_EXPANDING_TIME, StateId.Expanded));

                var vtb = Parent;
                if (vtb.chargePending)
                {
                    // Launch in the knight's direction.
                    var aVec = vtb.knight.transform.position - vtb.gameObject.transform.position;
                    vtb.retractTarget = aVec.normalized * 0.2f + vtb.gameObject.transform.position;
                    vtb.impulses.Add(new(aVec.normalized * _CONST_ATTACK_DISTANCE / _CONST_EXPANDING_TIME, _CONST_EXPANDING_TIME));
                    vtb.chargePending = false;
                }
            }

            protected override void SetInitScale() { }

            protected override Sprite InitSprite => Parent.bigSprite;

            protected override void Update() => Parent.SetScale(timer.ProgPct);
        }

        public static float _CONST_EXPANDED_TIME = 1.1f;

        private class ExpandedState : State
        {
            private int hits = 0;
            private TimerModule timer;

            public ExpandedState(StateMachine mgr) : base(mgr)
            {
                Parent.gameObject.transform.localScale = new(_CONST_EXPANDED_SCALE, _CONST_EXPANDED_SCALE, _CONST_EXPANDED_SCALE);
                timer = AddMod(new TimerModule(mgr, _CONST_EXPANDED_TIME, StateId.Retracting));
            }
            public override bool Mobile => false;

            public override float InitScale => 1f;

            protected override Sprite InitSprite => Parent.bigSprite;

            public override void Hit()
            {
                timer.Remaining += 0.5f / (++hits);
            }
        }

        public static float _CONST_RETRACTING_TIME = 0.35f;

        private class RetractingState : State
        {
            private TimerModule timer;

            public RetractingState(StateMachine mgr) : base(mgr)
            {
                timer = AddMod(new TimerModule(mgr, _CONST_RETRACTING_TIME, StateId.Idle));

                var vtb = Parent;
                var rVec = vtb.retractTarget - vtb.gameObject.transform.position;
                vtb.impulses.Add(new(rVec / _CONST_RETRACTING_TIME, _CONST_RETRACTING_TIME));
            }

            protected override void Update() => Parent.SetScale(timer.RemainingPct);

            protected override Sprite InitSprite => Parent.bigSprite;

            protected override void SetInitScale() { }

            public override void Hit()
            {
                var foo = new TinkEffect();
                var prog = timer.RemainingPct;
                (Mgr.ChangeState(StateId.Expanding) as ExpandingState).timer.ProgPct = prog;
            }
        }

        public static float _CONST_RESPAWNING_TIME = 2.5f;

        private class RespawningState : State
        {
            public RespawningState(StateMachine mgr) : base(mgr)
            {
                AddMod(new TimerModule(mgr, _CONST_RESPAWNING_TIME, StateId.Idle));
                Parent.boxCollider2D.enabled = false;
            }

            protected override Sprite InitSprite => Parent.splitSprite;

            protected override void Stop()
            {
                Parent.boxCollider2D.enabled = true;
                Parent.healthManager.hp = Parent.origHealth;
            }
        }

        public class StateMachine : EnemyStateMachine<StateId, State, StateMachine, VoidThornBehaviour>
        {
            public StateMachine(VoidThornBehaviour parent) : base(parent, StateId.Idle, new()
            {
                { StateId.Idle, m => new IdleState(m) },
                { StateId.Triggered, m => new TriggeredState(m) },
                { StateId.Expanding, m => new ExpandingState(m) },
                { StateId.Expanded, m => new ExpandedState(m) },
                { StateId.Retracting, m => new RetractingState(m) },
                { StateId.Respawning, m => new RespawningState(m) },
            }) { }

            public override StateMachine AsTyped() => this;
        }

        public HealthManager healthManager;
        public SpriteRenderer spriteRenderer;
        public IFrames iFrames;
        public int origHealth;
        public BoxCollider2D boxCollider2D;
        public GameObject knight;
        public Vector3 origPos;
        public Vector3 destination;
        public Vector2 origPos2d;
        public Vector2 destination2d;

        public float shuffleTimer;
        public StateMachine stateMachine;

        // Settable fields
        public Sprite defaultSprite;
        public Sprite bigSprite;
        public Sprite splitSprite;

        protected override void Awake()
        {
            gameObject.tag = "Spell Vulnerable";
            healthManager = GetComponent<HealthManager>();
            origHealth = healthManager.hp;
            healthManager.OnDeath += () => stateMachine.ChangeState(StateId.Respawning);

            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = defaultSprite;

            iFrames = gameObject.AddComponent<IFrames>();

            boxCollider2D = GetComponent<BoxCollider2D>();
            knight = GameManager.instance.hero_ctrl.gameObject;

            origPos = gameObject.transform.position;
            origPos2d = new(origPos.x, origPos.y);
            healthManager.SetAttr("invincible", true);

            shuffleTimer = (_CONST_SHUFFLE_TIMER + _CONST_SHUFFLE_VARIANCE * Random.Range(0f, 1f)) * Random.Range(0f, 1f) - _CONST_SHUFFLE_TIMER - _CONST_SHUFFLE_VARIANCE;
            ShuffleDestination();
            stateMachine = AddESM<StateMachine>(new(this));
        }

        private static float _CONST_SHUFFLE_TIMER = 2.2f;
        private static float _CONST_DRIFT_SPEED = 0.15f;
        private static float _CONST_DRIFT_ACCEL = 0.4f;
        private static float _CONST_SHUFFLE_VARIANCE = 0.6f;
        private static float _CONST_SHUFFLE_RADIUS = 0.55f;

        private void ShuffleDestination()
        {
            shuffleTimer -= Time.fixedDeltaTime;
            if (shuffleTimer > 0)
            {
                destination += Quaternion.Euler(0, 0, Random.Range(0f, 360f)) * new Vector3(_CONST_DRIFT_SPEED, 0, 0) * Time.fixedDeltaTime;
                return;
            }

            shuffleTimer += _CONST_SHUFFLE_TIMER + _CONST_SHUFFLE_VARIANCE * Random.Range(0f, 1f);
            Vector3 oldDestination = new(destination.x, destination.y, destination.z);
            while (true)
            {
                float radius = Mathf.Sqrt(Random.Range(0f, 1f)) * _CONST_SHUFFLE_RADIUS;
                destination = origPos + Quaternion.Euler(0, 0, Random.Range(0f, 360f)) * new Vector3(radius, 0, 0);
                if (Vector3.Distance(destination, oldDestination) > _CONST_SHUFFLE_RADIUS / 2)
                {
                    destination2d = new(destination.x, destination.y);
                    return;
                }
            }
        }

        private static float _CONST_IMPULSE_DISTANCE = 0.25f;
        private static float _CONST_IMPULSE_DURATION_SECONDS = 0.1f;

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
            if (!iFrames.AcceptHit()) return;

            var dir = (Quaternion.Euler(0, 0, damageInstance.Direction) * new Vector3(1, 0, 0)).normalized;
            impulses.Add(new(dir * (_CONST_IMPULSE_DISTANCE / _CONST_IMPULSE_DURATION_SECONDS), _CONST_IMPULSE_DURATION_SECONDS));

            stateMachine.CurrentState.Hit();
        }

        private Vector2 pos2d => new(gameObject.transform.position.x, gameObject.transform.position.y);

        private Vector2 driftVelocity = new();

        private Vector2 Gravitate()
        {
            var dist = pos2d - destination2d;
            var dir = -dist;
            var target = dir * _CONST_DRIFT_SPEED;
            var vDelta = target - driftVelocity;
            var aDelta = _CONST_DRIFT_ACCEL * Time.fixedDeltaTime;
            if (vDelta.magnitude < aDelta)
            {
                driftVelocity = target;
                return driftVelocity;
            }

            driftVelocity += vDelta.normalized * aDelta;
            return driftVelocity;
        }

        protected override void FixedUpdateImpl()
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

            velocity = stateMachine.CurrentState.Mobile ? velocity : new(0, 0);
            var newPos = pos2d + velocity * Time.fixedDeltaTime;
            gameObject.transform.position = new(newPos.x, newPos.y, gameObject.transform.position.z);
        }
    }
}
