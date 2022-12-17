using DarknestDungeon.EnemyLib;
using System.Collections.Generic;
using UnityEngine;

namespace DarknestDungeon.Enemy
{
    using ArmorTimerModule = TimerModule<ArmorControl.StateId, ArmorControl.State, ArmorControl.StateMachine>;

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
            public HalfArmorState(StateMachine mgr) : base(mgr) {
                AddMod(new ArmorTimerModule(mgr, HALF_RECOVERY, StateId.FullArmor));
            }

            public override void NailHit() => Mgr.ChangeState(StateId.Armorless);
        }

        public class ArmorlessState : State
        {
            private ArmorTimerModule mod;

            public ArmorlessState(StateMachine mgr) : base(mgr) {
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
            Launching,
            Landing,
            Reassessing,
        }

        public class State : EnemyState<StateId, State, StateMachine>
        {
            public State(StateMachine mgr) : base(mgr) { }
        }

        public class IdleState : State
        {
            public IdleState(StateMachine mgr) : base(mgr)
            {
            }
        }
        public class AwakeningState : State
        {
            public AwakeningState(StateMachine mgr) : base(mgr) { }
        }

        public class TargetingState : State
        {
            public TargetingState(StateMachine mgr) : base(mgr) { }
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
            public readonly VoidSpikeBehaviour Vsb;

            public StateMachine(VoidSpikeBehaviour vsb) : base(StateId.Idle, new()
            {

            })
            {
                this.Vsb = vsb;
            }

            public override StateMachine AsTyped() => this;
        }

        public HealthManager hm;
        public ArmorControl ac;
        public StateMachine sm;

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
            if (damageInstance.AttackType != AttackTypes.Nail || ac.Vulnerable) hm.Hit(damageInstance);
            if (damageInstance.AttackType == AttackTypes.Nail) ac.NailHit();
        }

        private void Awake()
        {
            this.hm = GetComponent<HealthManager>();
            this.tag = "Spell Vulnerable";
            launchSprites = new() { launchArmorless, launchHalfArmor, launchFullArmor };
            idleSprites = new() { idleArmorless, idleHalfArmor, idleFullArmor };

            this.ac = new(GetComponent<TinkEffect>());
            this.sm = new(this);
        }
    }
}
