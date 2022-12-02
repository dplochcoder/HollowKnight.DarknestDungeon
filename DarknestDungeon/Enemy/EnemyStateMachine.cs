using System.Collections.Generic;

namespace DarknestDungeon.Enemy
{
    public class EnemyStateMachine<S, T> where T : EnemyState<S, T>
    {
        public delegate T StateCreator(EnemyStateMachine<S, T> mgr);

        private readonly Dictionary<S, StateCreator> stateCreators;
        private S currentStateId;
        private T currentState;

        public EnemyStateMachine(
            S initialStateId,
            IDictionary<S, StateCreator> stateCreators)
        {
            this.stateCreators = new(stateCreators);

            this.currentStateId = initialStateId;
            this.currentState = stateCreators[initialStateId].Invoke(this);
        }

        public void Update() => currentState.Update();

        public void ChangeState(S newStateId)
        {
            if (EqualityComparer<S>.Default.Equals(currentStateId, newStateId)) return;

            currentState.Stop();
            currentStateId = newStateId;
            currentState = stateCreators[newStateId].Invoke(this);
            currentState.Init();
        }
    }
}
