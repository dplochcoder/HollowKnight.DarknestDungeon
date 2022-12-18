namespace DarknestDungeon.EnemyLib
{
    public class EnemyStateModule<T, S, M, P> where S : EnemyState<T, S, M, P> where M : EnemyStateMachine<T, S, M, P>
    {
        public virtual void Init() { }

        public virtual void Update(out bool stateChange) { stateChange = false; }

        public virtual void Stop() { }
    }
}
