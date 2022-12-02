using UnityEngine;

namespace DarknestDungeon.EnemyLib
{
    public class TimerModule<T, S, M> : EnemyStateModule<T, S, M> where S : EnemyState<T, S, M> where M : EnemyStateMachine<T, S, M>
    {
        private readonly M mgr;

        public readonly float Duration;
        public readonly T NextId;

        public float Remaining;

        public TimerModule(M mgr, float duration, T nextId)
        {
            this.mgr = mgr;
            this.Duration = duration;
            this.NextId = nextId;
            this.Remaining = duration;
        }

        public void Update(out bool stateChange)
        {
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

        public void Stop() { }
    }
}
