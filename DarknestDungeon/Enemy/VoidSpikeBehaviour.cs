using DarknestDungeon.EnemyLib;
using DarknestDungeon.UnityExtensions;
using System.Collections.Generic;
using UnityEngine;

namespace DarknestDungeon.Enemy
{
    using ArmorTimerModule = TimerModule<ArmorControl.StateId, ArmorControl.State, ArmorControl.StateMachine, ArmorControl>;
    using SpikeTimerModule = TimerModule<VoidSpikeBehaviour.StateId, VoidSpikeBehaviour.State, VoidSpikeBehaviour.StateMachine, VoidSpikeBehaviour>;

    public class ArmorControl
    {
        public delegate void ArmorChanged(int armor);
        public event ArmorChanged? OnArmorChanged;

        public enum StateId
        {
            FullArmor,
            HalfArmor,
            Armorless,
        }

        private static readonly float _CONST_HALF_RECOVERY = 1.5f;
        private static readonly float _CONST_FULL_RECOVERY = 2.5f;
        private static readonly float _CONST_RECOVERY_DELAY = 0.75f;

        public abstract class State : EnemyState<StateId, State, StateMachine, ArmorControl>
        {
            public State(StateMachine mgr) : base(mgr) { }

            protected override void Init() => Parent.TriggerOnArmorChanged(Armor());

            public abstract int Armor();

            public virtual void NailHit() { }
        }

        public class FullArmorState : State
        {
            public FullArmorState(StateMachine mgr) : base(mgr) { }
            public override int Armor() => 2;

            public override void NailHit() => Mgr.ChangeState(StateId.HalfArmor);
        }

        public class HalfArmorState : State
        {
            public HalfArmorState(StateMachine mgr) : base(mgr)
            {
                AddMod(new ArmorTimerModule(mgr, _CONST_HALF_RECOVERY, StateId.FullArmor));
            }

            public override int Armor() => 1;

            public override void NailHit() => Mgr.ChangeState(StateId.Armorless);
        }

        public class ArmorlessState : State
        {
            private ArmorTimerModule timer;

            public ArmorlessState(StateMachine mgr) : base(mgr)
            {
                timer = AddMod(new ArmorTimerModule(mgr, _CONST_FULL_RECOVERY, StateId.FullArmor));
                Parent.tinkEffect.enabled = false;
                Parent.healthManager.IsInvincible = false;
            }

            protected override void Stop()
            {
                Parent.tinkEffect.enabled = true;
                Parent.healthManager.IsInvincible = true;
            }
            public override int Armor() => 0;

            public override void NailHit() => timer.Remaining += _CONST_RECOVERY_DELAY;
        }

        public class StateMachine : EnemyStateMachine<StateId, State, StateMachine, ArmorControl>
        {
            public StateMachine(ArmorControl ac) : base(ac, StateId.FullArmor, new()
            {
                { StateId.FullArmor, mgr => new FullArmorState(mgr) },
                { StateId.HalfArmor, mgr => new HalfArmorState(mgr) },
                { StateId.Armorless, mgr => new ArmorlessState(mgr) },
            })
            { }

            public override StateMachine AsTyped() => this;
        }

        private StateMachine stateMachine;
        private TinkEffect tinkEffect;
        private HealthManager healthManager;
        private IFrames iFrames;

        public ArmorControl(GameObject go)
        {
            this.tinkEffect = go.GetComponent<TinkEffect>();
            this.healthManager = go.GetComponent<HealthManager>();
            this.iFrames = go.AddComponent<IFrames>();
            stateMachine = new(this);
        }

        public void Update() => stateMachine.Update();

        public void NailHit()
        {
            if (iFrames.AcceptHit()) stateMachine.CurrentState.NailHit();
        }

        public bool Vulnerable => stateMachine.CurrentStateId == StateId.Armorless;

        public int Armor() => stateMachine.CurrentState.Armor();

        public void TriggerOnArmorChanged(int armor) => OnArmorChanged?.Invoke(armor);

        public void Reset() => stateMachine.ChangeState(StateId.FullArmor);
    }

    public class VoidSpikeBehaviour : MonoBehaviour, IHitResponder
    {
        public enum StateId
        {
            Idle,
            Awakening,
            Targeting,
            PreLaunch,
            Launching,
            Landing,
            Burrow,
        }

        public static float _CONST_MINIMUM_LAUNCH_DISTANCE = 2.0f;

        public class State : EnemyState<StateId, State, StateMachine, VoidSpikeBehaviour>
        {
            public State(StateMachine mgr) : base(mgr) { }

            protected override void Init()
            {
                base.Init();
                Parent.UpdateVisuals();
            }

            public Vector3 ToHero => Parent.knight.transform.position - Parent.transform.position;

            private const int TERRAIN_MASK = 1 << (int)GlobalEnums.PhysLayers.TERRAIN;

            public bool HasLineOfSight(out RaycastHit2D hit)
            {
                // Attempt to target the player.
                var origin = Parent.transform.position;
                Vector2 o2d = new(origin.x, origin.y);
                var dest = Parent.knight.transform.position;
                Vector2 dest2d = new(dest.x, dest.y);
                hit = Physics2D.Raycast(o2d, dest2d - o2d, 256f, TERRAIN_MASK);
                var dist = hit.distance;
                return dist >= _CONST_MINIMUM_LAUNCH_DISTANCE && dist + 0.5f >= (dest2d - o2d).magnitude;
            }
        }

        private static readonly float _CONST_AWAKE_RANGE_SQUARED = 10 * 10;
        private static readonly float _CONST_BURROW_TIME_WAIT = 5f;

        public class IdleState : State
        {
            private int lineOfSightTicks = 0;
            private float burrowTime = 0;

            public IdleState(StateMachine mgr) : base(mgr) { }

            protected override void Update()
            {
                if (ToHero.sqrMagnitude <= _CONST_AWAKE_RANGE_SQUARED)
                {
                    burrowTime = 0;
                    if (--lineOfSightTicks <= 0)
                    {
                        if (HasLineOfSight(out var _)) Mgr.ChangeState(StateId.Awakening);
                        else lineOfSightTicks = 10;
                    }
                }
                else if ((Parent.transform.position - Parent.spawnPos).sqrMagnitude > 5 * 5) {
                    burrowTime += Time.deltaTime;
                    if (burrowTime >= _CONST_BURROW_TIME_WAIT) Mgr.ChangeState(StateId.Burrow);
                }
            }
        }

        private static readonly float _CONST_MAX_TARGETING_RANGE_SQUARED = 16 * 16;
        private static readonly float _CONST_SLEEP_RANGE_SQUARED = 14 * 14;
        private static readonly float _CONST_AGGRO_RANGE_SQUARED = 10 * 10;
        private static readonly float _CONST_HYPER_AGGRO_RANGE_SQUARED = 6 * 6;
        private static readonly float _CONST_BASE_RAGE = 2f;
        private static readonly float _CONST_MAX_RAGE = 3f;

        public class AwakeningState : State
        {
            private float rage = _CONST_BASE_RAGE;

            private int sightTicker = 1;
            private bool hasSight = false;

            public AwakeningState(StateMachine mgr) : base(mgr) { }

            protected override void Update()
            {
                if (--sightTicker <= 0)
                {
                    sightTicker = 10;
                    hasSight = HasLineOfSight(out var _);
                }

                float dist = ToHero.sqrMagnitude;
                if (!hasSight || dist >= _CONST_SLEEP_RANGE_SQUARED)
                {
                    rage -= Time.deltaTime;
                    Mgr.ChangeState(StateId.Idle);
                    return;
                }
                else if (dist <= _CONST_HYPER_AGGRO_RANGE_SQUARED) rage = _CONST_MAX_RAGE;
                else if (dist <= _CONST_AGGRO_RANGE_SQUARED) rage += 4 * Time.deltaTime;

                if (rage <= 0) Mgr.ChangeState(StateId.Idle);
                else if (rage >= _CONST_MAX_RAGE) Mgr.ChangeState(StateId.Targeting);
            }
        }

        private static float _CONST_TARGETING_TIME = 0.25f;
        private static int _CONST_MAX_RETARGETS = 5;

        public int retargets = 0;

        public class TargetingState : State
        {
            public SpikeTimerModule timer;

            public TargetingState(StateMachine mgr) : base(mgr) {
                timer = AddMod(new SpikeTimerModule(mgr, _CONST_TARGETING_TIME, StateId.PreLaunch));
            }

            protected override void Init()
            {
                base.Init();
                if (++Parent.retargets > _CONST_MAX_RETARGETS)
                {
                    Parent.retargets = 0;
                    Mgr.ChangeState(StateId.Awakening);
                }
            }

            protected override void Update()
            {
                base.Update();
                if (ToHero.sqrMagnitude > _CONST_MAX_TARGETING_RANGE_SQUARED)
                {
                    Parent.retargets = 0;
                    Mgr.ChangeState(StateId.Idle);
                }
            }
        }

        private static float _CONST_PRE_LAUNCH_TIME = 0.15f;

        public class PreLaunchState : State
        {
            public PreLaunchState(StateMachine mgr) : base(mgr)
            {
                AddMod(new SpikeTimerModule(mgr, _CONST_PRE_LAUNCH_TIME, StateId.Launching));
            }

            protected override void Init()
            {
                base.Init();
                if (!HasLineOfSight(out var hit))
                {
                    var tState = Mgr.ChangeState(StateId.Targeting) as TargetingState;
                    tState.timer.Remaining = 0.1f;
                }
                else
                {
                    Parent.retargets = 0;
                    Parent.launchVector = ToHero;
                    Parent.launchTarget = hit;
                }
            }
        }

        private static float _CONST_LIFT_DIST = 1.2f;
        private static float _CONST_LAUNCH_VELOCITY = 25f;
        private Vector2 launchVector;
        private RaycastHit2D launchTarget;

        // TODO: Launch in a random direction defensively on damage
        public class LaunchingState : State
        {
            private SpikeTimerModule timer;
            private Vector2 origPos;
            private Vector2 target;

            public LaunchingState(StateMachine mgr) : base(mgr)
            {
                origPos = Parent.transform.position;
                target = Parent.launchTarget.point + Parent.launchTarget.normal.normalized * _CONST_LIFT_DIST;
                float time = (target - origPos).magnitude / _CONST_LAUNCH_VELOCITY;
                timer = AddMod(new SpikeTimerModule(mgr, time, StateId.Landing));

                Parent.transform.rotation = MathExt.AngleVec(target - origPos, -90);
            }

            protected override void Update()
            {
                base.Update();
                Parent.transform.position = origPos + (target - origPos) * timer.ProgPct;
            }

            protected override void Stop()
            {
                base.Stop();
                Parent.transform.rotation = MathExt.AngleVec(Parent.launchTarget.normal, -90);
            }
        }

        public static float _CONST_LANDING_TIME = 0.45f;

        public class LandingState : State
        {
            public LandingState(StateMachine mgr) : base(mgr) { AddMod(new SpikeTimerModule(mgr, _CONST_LANDING_TIME, StateId.Targeting)); }
        }

        public static float _CONST_BURROW_RESPAWN_TIME = 2f;

        public class BurrowState : State
        {
            public BurrowState(StateMachine mgr) : base(mgr) { AddMod(new SpikeTimerModule(mgr, _CONST_BURROW_RESPAWN_TIME, StateId.Idle)); }

            protected override void Init()
            {
                base.Init();

                // TODO: Poof
                Parent.spriteRenderer.enabled = false;
                Parent.transform.position = new(-50, -50, 0);
            }

            protected override void Stop()
            {
                base.Stop();

                // TODO: Unpoof
                Parent.spriteRenderer.enabled = true;
                Parent.transform.position = Parent.spawnPos;
                Parent.transform.rotation = Quaternion.AngleAxis(0, Vector3.forward);
                Parent.armorControl.Reset();
                Parent.healthManager.hp = 75;
            }
        }

        public class StateMachine : EnemyStateMachine<StateId, State, StateMachine, VoidSpikeBehaviour>
        {
            public StateMachine(VoidSpikeBehaviour parent) : base(parent, StateId.Idle, new()
            {
                { StateId.Idle, mgr => new IdleState(mgr) },
                { StateId.Awakening, mgr => new AwakeningState(mgr) },
                { StateId.Targeting, mgr => new TargetingState(mgr) },
                { StateId.PreLaunch, mgr => new PreLaunchState(mgr) },
                { StateId.Launching, mgr => new LaunchingState(mgr) },
                { StateId.Landing, mgr => new LandingState(mgr) },
                { StateId.Burrow, mgr => new BurrowState(mgr) }
            }) { }

            public override StateMachine AsTyped() => this;
        }

        public HealthManager healthManager;
        public GameObject knight;
        public SpriteRenderer spriteRenderer;
        public BoxCollider2D b2d;
        public Vector3 spawnPos;
        public ArmorControl armorControl;
        public StateMachine stateMachine;

        private List<Sprite> launchSprites;
        private List<Sprite> idleSprites;

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
        public GameObject flashSprite;

        public void Hit(HitInstance damageInstance)
        {
            if (damageInstance.AttackType != AttackTypes.Nail || armorControl.Vulnerable) healthManager.Hit(damageInstance);
            if (damageInstance.AttackType == AttackTypes.Nail) armorControl.NailHit();
        }

        public void UpdateVisuals()
        {
            bool launching = stateMachine.CurrentStateId == StateId.Launching;
            idleHurtbox.SetActive(!launching);
            launchHurtbox.SetActive(launching);

            var targetSprites = launching ? launchSprites : idleSprites;
            spriteRenderer.sprite = targetSprites[armorControl.Armor()];

            var targetB2d = launching ? launchHitbox : idleHitbox;
            b2d.offset = targetB2d.offset;
            b2d.size = targetB2d.size;

            flashSprite.SetActive(stateMachine.CurrentStateId == StateId.Targeting);
        }

        private void Awake()
        {
            this.healthManager = GetComponent<HealthManager>();
            this.knight = GameManager.instance.hero_ctrl.gameObject;
            this.spriteRenderer = GetComponent<SpriteRenderer>();
            this.b2d = gameObject.AddComponent<BoxCollider2D>();
            this.tag = "Spell Vulnerable";
            launchSprites = new() { launchArmorless, launchHalfArmor, launchFullArmor };
            idleSprites = new() { idleArmorless, idleHalfArmor, idleFullArmor };
            this.spawnPos = transform.position;

            this.armorControl = new(gameObject);
            this.stateMachine = new(this);
            this.armorControl.OnArmorChanged += _ => UpdateVisuals();

            UpdateVisuals();
        }

        private void Update()
        {
            armorControl.Update();
            stateMachine.Update();
        }
    }
}
