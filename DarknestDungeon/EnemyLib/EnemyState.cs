using System.Collections.Generic;

namespace DarknestDungeon.EnemyLib
{
    public abstract class EnemyState<T, S, M> where S : EnemyState<T, S, M> where M : EnemyStateMachine<T, S, M>
    {
        public M Mgr { get; private init; }

        private readonly List<EnemyStateModule<T, S, M>> modules = new();

        public EnemyState(M mgr) { Mgr = mgr; }

        public ModuleT AddMod<ModuleT>(ModuleT m) where ModuleT : EnemyStateModule<T, S, M>
        {
            modules.Add(m);
            return m;
        }

        public void UpdateFinal()
        {
            foreach (var module in modules)
            {
                module.Update(out var changed);
                if (changed) return;
            }
            Update();
        }

        protected void Update() { }

        public void StopFinal()
        {
            modules.ForEach(m => m.Stop());
            Stop();
        }

        protected void Stop() { }
    }
}
