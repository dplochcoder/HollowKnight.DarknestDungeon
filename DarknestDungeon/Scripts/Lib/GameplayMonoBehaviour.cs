using DarknestDungeon.EnemyLib;
using System.Collections.Generic;
using UnityEngine;

namespace DarknestDungeon.Scripts.Lib
{
    internal class GameplayMonoBehaviour : MonoBehaviour
    {
        public delegate void DoUpdate();
        public delegate void DoFixedUpdate();

        private List<DoUpdate> doUpdate = new();
        private List<DoFixedUpdate> doFixedUpdate = new();

        public GameManager gm { get; private set; }

        protected GameplayMonoBehaviour() { gm = GameManager.instance; }

        protected M AddESM<M>(M machine) where M : IEnemyStateMachine
        {
            doUpdate.Add(() => machine.Update());
            doFixedUpdate.Add(() => machine.FixedUpdate());
            return machine;
        }

        protected virtual void Awake() { }

        protected virtual void FixedUpdateImpl() { }

        public void FixedUpdate()
        {
            if (gm.isPaused) return;

            doFixedUpdate.ForEach(a => a.Invoke());
            FixedUpdateImpl();
        }

        protected virtual void UpdateImpl() { }

        public void Update()
        {
            if (gm.isPaused) return;

            doUpdate.ForEach(a => a.Invoke());
            UpdateImpl();
        }
    }
}
