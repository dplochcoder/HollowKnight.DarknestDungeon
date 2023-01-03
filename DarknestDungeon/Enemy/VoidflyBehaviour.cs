using DarknestDungeon.EnemyLib;
using DarknestDungeon.Scripts.Lib;
using DarknestDungeon.UnityExtensions;
using UnityEngine;

namespace DarknestDungeon.Enemy
{
    using VoidflyTimerModule = TimerModule<VoidflyBehaviour.StateId, VoidflyBehaviour.State, VoidflyBehaviour.StateMachine, VoidflyBehaviour>;
    using ExplosionTimerModule = TimerModule<VoidflyExplosionBehaviour.StateId, VoidflyExplosionBehaviour.State, VoidflyExplosionBehaviour.StateMachine, VoidflyExplosionBehaviour>;

    internal class VoidflyBehaviour : GameplayMonoBehaviour
    {
        public enum StateId
        {
            Idle,
            Activated,
            Flying
        }

        public class State : EnemyState<StateId, State, StateMachine, VoidflyBehaviour>
        {
            public State(StateMachine mgr) : base(mgr) { }
        }

        private static float _CONST_CONE_WIDTH = 60;
        private static float _CONST_DETECTION_RANGE = 9.5f;

        public class IdleState : State
        {
            private int lineOfSightTicker = 1;

            public IdleState(StateMachine mgr) : base(mgr) { }

            private const int TERRAIN_MASK = 1 << (int)GlobalEnums.PhysLayers.TERRAIN;

            protected override void Update()
            {
                base.Update();

                var self = Parent.transform.position;
                var hero = Parent.knight.transform.position;
                var delta = hero - self;
                if (delta.sqrMagnitude > _CONST_DETECTION_RANGE * _CONST_DETECTION_RANGE) return;

                float sightAngle = delta.To2d().VecToAngle();
                float baseAngle = Parent.transform.rotation.ToAngle() + 270;
                if (!MathExt.IsAngleBetween(sightAngle, baseAngle - _CONST_CONE_WIDTH, baseAngle + _CONST_CONE_WIDTH)) return;

                if (--lineOfSightTicker <= 0)
                {
                    lineOfSightTicker = 10;
                    var hit = Physics2D.Raycast(self.To2d(), delta, 256f, TERRAIN_MASK);
                    if (hit.distance + 0.5f >= delta.magnitude) ChangeState(StateId.Activated);
                }
            }
        }

        private static float _CONST_ATTACK_DELAY = 0.225f;

        public class ActivatedState : State
        {
            public ActivatedState(StateMachine mgr) : base(mgr) { AddMod(new VoidflyTimerModule(mgr, _CONST_ATTACK_DELAY, StateId.Flying)); }
        }

        private static float _CONST_FLYING_VELOCITY = 30f;
        private static float _CONST_TURNING_RADIUS = 9f;

        // Degrees/sec
        private static float _CONST_TURNING_SPEED => 360 * _CONST_FLYING_VELOCITY / (2 * Mathf.PI * _CONST_TURNING_RADIUS);

        public class FlyingState : State
        {
            private float currentAngle;

            public FlyingState(StateMachine mgr) : base(mgr) { currentAngle = Parent.transform.rotation.ToAngle() + 270; }

            protected override void FixedUpdate()
            {
                base.FixedUpdate();

                var self = Parent.transform.position;
                var hero = Parent.knight.transform.position;
                var delta = hero - self;

                float sightAngle = delta.To2d().VecToAngle();
                float turnRange = Time.fixedDeltaTime * _CONST_TURNING_SPEED;
                currentAngle = MathExt.Clamp(sightAngle, currentAngle - turnRange, currentAngle + turnRange);

                Parent.transform.localRotation = Quaternion.AngleAxis(currentAngle - 270, Vector3.forward);
                Parent.rigidbody2d.velocity = currentAngle.AsAngleToVec().To3d() * _CONST_FLYING_VELOCITY;
            }
        }

        public class StateMachine : EnemyStateMachine<StateId, State, StateMachine, VoidflyBehaviour>
        {
            public StateMachine(VoidflyBehaviour parent) : base(parent, StateId.Idle, new()
            {
                { StateId.Idle, mgr => new IdleState(mgr) },
                { StateId.Activated, mgr => new ActivatedState(mgr) },
                { StateId.Flying, mgr => new FlyingState(mgr) },
            })
            { }

            public override StateMachine AsTyped() => this;
        }

        public HeroController heroController;
        public GameObject knight;
        public HealthManager healthManager;
        public SpriteRenderer spriteRenderer;
        public Rigidbody2D rigidbody2d;

        private StateMachine stateMachine;

        // Editor fields
        public Sprite idleSprite;
        public Sprite activeSprite;
        public GameObject explosionPrefab;

        protected override void Awake()
        {
            this.heroController = gm.hero_ctrl;
            this.knight = heroController.gameObject;
            this.healthManager = GetComponent<HealthManager>();
            this.spriteRenderer = GetComponent<SpriteRenderer>();
            this.rigidbody2d = GetComponent<Rigidbody2D>();

            healthManager.OnDeath += () => Explode();

            this.stateMachine = AddESM<StateMachine>(new(this));
        }

        private void Explode()
        {
            // TODO: Burst explosion
            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            int layer = collider.gameObject.layer;
            if (layer != (int)GlobalEnums.PhysLayers.HERO_BOX || heroController.cState.shadowDashing) return;

            // TODO: Do a different explosion for mid-air collision
            Object.Instantiate(explosionPrefab, collider.gameObject.transform.position, Quaternion.AngleAxis(0, Vector3.forward));
            Destroy(gameObject);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            int layer = collision.collider.gameObject.layer;
            if (layer != (int)GlobalEnums.PhysLayers.TERRAIN) return;

            // TODO: rotate according to normal vector, and offset.
            Object.Instantiate(explosionPrefab, collision.contacts[0].point.To3d(), Quaternion.AngleAxis(0, Vector3.forward));
            Destroy(gameObject);
        }
    }

    internal class VoidflyExplosionBehaviour : GameplayMonoBehaviour
    {
        public enum StateId
        {
            Expanding,
            Lingering,
            Gone,
        }

        public class State : EnemyState<StateId, State, StateMachine, VoidflyExplosionBehaviour>
        {
            public State(StateMachine mgr) : base(mgr) { }
        }

        private static float _CONST_EXPAND_BASE = 0.4f;
        private static float _CONST_EXPAND_TIME = 0.2f;
        private static float _CONST_SPRITE_SIZE = 6;

        public class ExpandingState : State
        {
            private Vector2 origPos;
            private Vector2 targetPos;
            private ExplosionTimerModule timer;

            public ExpandingState(StateMachine mgr) : base(mgr)
            {
                origPos = Parent.transform.position.To2d();
                var expandingAngle = Parent.transform.rotation.ToAngle() + 90;
                targetPos = expandingAngle.AsAngleToVec() * (1 - _CONST_EXPAND_BASE) * _CONST_SPRITE_SIZE + origPos;
                timer = AddMod(new ExplosionTimerModule(mgr, _CONST_EXPAND_TIME, StateId.Lingering));
            }

            protected override void Update()
            {
                base.Update();

                var scale = _CONST_EXPAND_BASE + (1 - _CONST_EXPAND_BASE) * timer.ProgPct;
                Parent.transform.localScale = new(scale, scale, 1);
                Parent.transform.position = origPos + (targetPos - origPos) * timer.ProgPct;
            }
        }

        private static float _LINGER_TIME = 0.35f;

        public class LingeringState : State
        {
            public LingeringState(StateMachine mgr) : base(mgr) { AddMod(new ExplosionTimerModule(mgr, _LINGER_TIME, StateId.Gone)); }
        }

        public class GoneState : State
        {
            public GoneState(StateMachine mgr) : base(mgr) { }

            protected override void Init()
            {
                base.Init();
                Destroy(Parent.gameObject);
            }
        }

        public class StateMachine : EnemyStateMachine<StateId, State, StateMachine, VoidflyExplosionBehaviour>
        {
            public StateMachine(VoidflyExplosionBehaviour parent) : base(parent, StateId.Expanding, new()
            {
                { StateId.Expanding, mgr => new ExpandingState(mgr) },
                { StateId.Lingering, mgr => new LingeringState(mgr) },
                { StateId.Gone, mgr => new GoneState(mgr) },
            })
            { }

            public override StateMachine AsTyped() => this;
        }

        private StateMachine stateMachine;

        protected override void Awake() => stateMachine = AddESM<StateMachine>(new(this));
    }
}
