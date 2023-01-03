using UnityEngine;

namespace DarknestDungeon.Scripts.Lib
{
    internal class GameplayMonoBehaviour : MonoBehaviour
    {
        public GameManager gm { get; private set; }

        protected GameplayMonoBehaviour() { gm = GameManager.instance; }

        protected virtual void Awake() { }

        protected virtual void FixedUpdateImpl() { }

        public void FixedUpdate()
        {
            if (gm.isPaused) return;
            FixedUpdateImpl();
        }

        protected virtual void UpdateImpl() { }

        public void Update()
        {
            if (gm.isPaused) return;
            UpdateImpl();
        }
    }
}
