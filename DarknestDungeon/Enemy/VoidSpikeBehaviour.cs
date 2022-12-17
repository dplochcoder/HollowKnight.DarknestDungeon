﻿using DarknestDungeon.EnemyLib;
using System.Collections.Generic;
using UnityEngine;

namespace DarknestDungeon.Enemy
{
    using ArmorTimerModule = TimerModule<ArmorControl.StateId, ArmorControl.State, ArmorControl.StateMachine>;
    using SpikeTimerModule = TimerModule<VoidSpikeBehaviour.StateId, VoidSpikeBehaviour.State, VoidSpikeBehaviour.StateMachine>;

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

        private static readonly float HALF_RECOVERY = 0.75f;
        private static readonly float FULL_RECOVERY = 1.5f;

        public abstract class State : EnemyState<StateId, State, StateMachine>
        {
            public State(StateMachine mgr) : base(mgr) { }

            protected override void Init() => Mgr.ac.TriggerOnArmorChanged(Armor);

            protected int Armor { get; }

            public virtual void NailHit() { }
        }

        public class FullArmorState : State
        {
            public FullArmorState(StateMachine mgr) : base(mgr) { }

            public override void NailHit() => Mgr.ChangeState(StateId.HalfArmor);
        }

        public class HalfArmorState : State
        {
            public HalfArmorState(StateMachine mgr) : base(mgr)
            {
                AddMod(new ArmorTimerModule(mgr, HALF_RECOVERY, StateId.FullArmor));
            }

            public override void NailHit() => Mgr.ChangeState(StateId.Armorless);
        }

        public class ArmorlessState : State
        {
            private ArmorTimerModule mod;

            public ArmorlessState(StateMachine mgr) : base(mgr)
            {
                mod = AddMod(new ArmorTimerModule(mgr, FULL_RECOVERY, StateId.FullArmor));
                mgr.ac.te.enabled = false;
            }

            protected override void Stop() => Mgr.ac.te.enabled = true;

            public override void NailHit() => mod.Remaining += 0.5f;
        }

        public class StateMachine : EnemyStateMachine<StateId, State, StateMachine>
        {
            public readonly ArmorControl ac;

            public StateMachine(ArmorControl ac) : base(StateId.FullArmor, new()
            {
                { StateId.FullArmor, mgr => new FullArmorState(mgr) },
                { StateId.HalfArmor, mgr => new HalfArmorState(mgr) },
                { StateId.Armorless, mgr => new ArmorlessState(mgr) },
            })
            { this.ac = ac; }

            public override StateMachine AsTyped() => this;
        }

        private StateMachine sm;
        private TinkEffect te;

        public ArmorControl(TinkEffect te)
        {
            this.te = te;
            sm = new(this);
        }

        public void Update() => sm.Update();

        public void NailHit() => sm.CurrentState.NailHit();

        public bool Vulnerable => sm.CurrentStateId == StateId.Armorless;

        public void TriggerOnArmorChanged(int armor) => OnArmorChanged?.Invoke(armor);
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
            Reassessing,
        }

        public class State : EnemyState<StateId, State, StateMachine>
        {
            public State(StateMachine mgr) : base(mgr) { }

            public Vector3 ToHero => Mgr.VoidSpikeBehaviour.knight.transform.position - Mgr.VoidSpikeBehaviour.transform.position;

            public bool HasLineOfSight(out RaycastHit2D hit)
            {
                // Attempt to target the player.
                var origin = Mgr.VoidSpikeBehaviour.transform.position;
                Vector2 o2d = new(origin.x, origin.y);
                var dest = Mgr.VoidSpikeBehaviour.knight.transform.position;
                Vector2 dest2d = new(dest.x, dest.y);
                hit = Physics2D.Raycast(o2d, dest2d - o2d, 256f, (int)GlobalEnums.PhysLayers.TERRAIN);
                return hit.distance + 0.5f >= (dest2d - o2d).magnitude;
            }
        }

        private static readonly float _CONST_AWAKE_RANGE_SQUARED = 10 * 10;

        public class IdleState : State
        {
            private int lineOfSightTicks = 0;

            public IdleState(StateMachine mgr) : base(mgr) { }

            protected override void Update()
            {
                if (ToHero.sqrMagnitude <= _CONST_AWAKE_RANGE_SQUARED)
                {
                    if (lineOfSightTicks <= 0)
                    {
                        if (HasLineOfSight(out var _)) Mgr.ChangeState(StateId.Awakening);
                        else lineOfSightTicks = 10;
                    }
                    else --lineOfSightTicks;
                }
            }
        }

        private static readonly float _CONST_SLEEP_RANGE_SQUARED = 14 * 14;
        private static readonly float _CONST_ALERT_RANGE_SQUARED = 11 * 11;
        private static readonly float _CONST_AGGRO_RANGE_SQUARED = 8 * 8;
        private static readonly float _CONST_HYPER_AGGRO_RANGE_SQUARED = 4 * 4;
        private static readonly float _CONST_HYPER_AGGRO_FACTOR = 3;
        private static readonly float _CONST_BASE_RAGE = 1.5f;
        private static readonly float _CONST_MAX_RAGE = 3.5f;

        public class AwakeningState : State
        {
            private float rage = _CONST_BASE_RAGE;

            public AwakeningState(StateMachine mgr) : base(mgr) { }

            protected override void Update()
            {
                float dist = ToHero.sqrMagnitude;
                if (dist >= _CONST_SLEEP_RANGE_SQUARED)
                {
                    Mgr.ChangeState(StateId.Idle);
                    return;
                }
                else if (dist >= _CONST_ALERT_RANGE_SQUARED) rage -= Time.deltaTime;
                else if (dist <= _CONST_HYPER_AGGRO_RANGE_SQUARED) rage += _CONST_HYPER_AGGRO_FACTOR * Time.deltaTime;
                else if (dist <= _CONST_AGGRO_RANGE_SQUARED) rage += Time.deltaTime;

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
                if (++Mgr.VoidSpikeBehaviour.retargets > _CONST_MAX_RETARGETS)
                {

                }
            }
        }

        private static float _CONST_PRE_LAUNCH_TIME = 0.2f;

        public class PreLaunchState : State
        {
            public PreLaunchState(StateMachine mgr) : base(mgr)
            {
                AddMod(new SpikeTimerModule(mgr, _CONST_PRE_LAUNCH_TIME, StateId.Launching));
            }

            protected override void Init()
            {
                base.Init();
                if (HasLineOfSight(out var hit))
                {
                    // TODO
                }
            }
        }

        public class LaunchingState : State
        {
            public LaunchingState(StateMachine mgr) : base(mgr) { }
        }
        public class LandingState : State
        {
            public LandingState(StateMachine mgr) : base(mgr) { }
        }

        public class ReassessingState : State
        {
            public ReassessingState(StateMachine mgr) : base(mgr) { }
        }

        public class StateMachine : EnemyStateMachine<StateId, State, StateMachine>
        {
            public readonly VoidSpikeBehaviour VoidSpikeBehaviour;

            public StateMachine(VoidSpikeBehaviour vsb) : base(StateId.Idle, new()
            {
                { StateId.Idle, mgr => new IdleState(mgr) },
                { StateId.Awakening, mgr => new AwakeningState(mgr) },
                { StateId.Reassessing, mgr => new ReassessingState(mgr) },
                { StateId.Targeting, mgr => new TargetingState(mgr) },
                { StateId.PreLaunch, mgr => new PreLaunchState(mgr) },
                { StateId.Launching, mgr => new LaunchingState(mgr) },
                { StateId.Landing, mgr => new LandingState(mgr) },
            })
            {
                this.VoidSpikeBehaviour = vsb;
            }

            public override StateMachine AsTyped() => this;
        }

        public HealthManager healthManager;
        public GameObject knight;
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

        public void Hit(HitInstance damageInstance)
        {
            if (damageInstance.AttackType != AttackTypes.Nail || armorControl.Vulnerable) healthManager.Hit(damageInstance);
            if (damageInstance.AttackType == AttackTypes.Nail) armorControl.NailHit();
        }

        private void Awake()
        {
            this.healthManager = GetComponent<HealthManager>();
            this.knight = GameManager.instance.hero_ctrl.gameObject;
            this.tag = "Spell Vulnerable";
            launchSprites = new() { launchArmorless, launchHalfArmor, launchFullArmor };
            idleSprites = new() { idleArmorless, idleHalfArmor, idleFullArmor };

            this.armorControl = new(GetComponent<TinkEffect>());
            this.stateMachine = new(this);
        }
    }
}
