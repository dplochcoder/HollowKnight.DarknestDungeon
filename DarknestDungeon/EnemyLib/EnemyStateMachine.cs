using System.Collections.Generic;

namespace DarknestDungeon.EnemyLib
{
    public class EnemyStateMachine<T, S, M> where S : EnemyState<T, S, M> where M : EnemyStateMachine<T, S, M>
    {
        public delegate S StateCreator(EnemyStateMachine<T, S, M> mgr);

        private readonly Dictionary<T, StateCreator> stateCreators;
        private T currentStateId;
        private S currentState;

        public EnemyStateMachine(
            T initialStateId,
            IDictionary<T, StateCreator> stateCreators)
        {
            this.stateCreators = new(stateCreators);

            this.currentStateId = initialStateId;
            this.currentState = stateCreators[initialStateId].Invoke(this);
        }

        public void Update() => currentState.Update();

        public void ChangeState(T newStateId)
        {
            if (EqualityComparer<T>.Default.Equals(currentStateId, newStateId)) return;

            currentState.Stop();
            currentStateId = newStateId;
            currentState = stateCreators[newStateId].Invoke(this);
        }
    }
}
