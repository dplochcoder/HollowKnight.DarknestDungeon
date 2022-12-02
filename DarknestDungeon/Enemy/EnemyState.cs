namespace DarknestDungeon.Enemy
{
    public class EnemyState<S, T> where T : EnemyState<S, T>
    {
        public readonly EnemyStateMachine<S, T> mgr;

        public EnemyState(EnemyStateMachine<S, T> mgr)
        {
            this.mgr = mgr;
        }

        public virtual void Init() { }

        public virtual void Update() { }

        public virtual void Stop() { }
    }
}
