using UnityEngine;

namespace DarknestDungeon.EnemyLib
{
    public class TimerModule<T, S, M, P> : EnemyStateModule<T, S, M, P> where S : EnemyState<T, S, M, P> where M : EnemyStateMachine<T, S, M, P>
    {
        public delegate bool UpdateFilter();

        private readonly M mgr;

        public readonly float Duration;
        public readonly T NextId;
        public readonly UpdateFilter Filter;

        public float Remaining;
        public float RemainingPct
        {
            get
            {
                return Remaining / Duration;
            }
            set
            {
                Remaining = Duration * value;
            }
        }

        public float ProgPct
        {
            get
            {
                return 1.0f - RemainingPct;
            }
            set
            {
                Remaining = Duration - Duration * value;
            }
        }

        public TimerModule(M mgr, float duration, T nextId, UpdateFilter filter)
        {
            this.mgr = mgr;
            this.Duration = duration;
            this.NextId = nextId;
            this.Remaining = duration;
            this.Filter = filter;
        }

        public TimerModule(M mgr, float duration, T nextId) : this(mgr, duration, nextId, () => true) { }

        public override void Update(out bool stateChange)
        {
            if (!Filter())
            {
                stateChange = false;
                return;
            }

            Remaining -= Time.deltaTime;
            if (Remaining <= 0)
            {
                stateChange = true;
                mgr.ChangeState(NextId);
            }
            else
            {
                stateChange = false;
            }
        }
    }
}
