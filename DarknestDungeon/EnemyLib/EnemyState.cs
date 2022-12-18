using System.Collections.Generic;

namespace DarknestDungeon.EnemyLib
{
    public abstract class EnemyState<T, S, M, P> where S : EnemyState<T, S, M, P> where M : EnemyStateMachine<T, S, M, P>
    {
        public M Mgr { get; private init; }

        public P Parent => Mgr.Parent;

        private readonly List<EnemyStateModule<T, S, M, P>> modules = new();

        public EnemyState(M mgr) { Mgr = mgr; }

        public ModuleT AddMod<ModuleT>(ModuleT m) where ModuleT : EnemyStateModule<T, S, M, P>
        {
            modules.Add(m);
            return m;
        }

        protected virtual void Init() { }

        public void InitFinal()
        {
            modules.ForEach(m => m.Init());
            Init();
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

        protected virtual void Update() { }

        public void StopFinal()
        {
            modules.ForEach(m => m.Stop());
            Stop();
        }

        protected virtual void Stop() { }
    }
}
