using System.Collections.Generic;

namespace DarknestDungeon.EnemyLib
{
    public interface IEnemyStateMachine
    {
        public void Update();

        public void FixedUpdate();
    }

    public abstract class EnemyStateMachine<T, S, M, P> : IEnemyStateMachine where S : EnemyState<T, S, M, P> where M : EnemyStateMachine<T, S, M, P>
    {
        public delegate S StateCreator(M mgr);

        public P Parent { get; private set; }
        private readonly Dictionary<T, StateCreator> stateCreators;
        public T CurrentStateId { get; private set; }
        public S CurrentState { get; private set; }

        public EnemyStateMachine(
            P parent,
            T initialStateId,
            Dictionary<T, StateCreator> stateCreators)
        {
            this.Parent = parent;
            this.stateCreators = new(stateCreators);

            this.CurrentStateId = initialStateId;
            this.CurrentState = stateCreators[initialStateId].Invoke(AsTyped());
        }

        public abstract M AsTyped();

        public void Update() => CurrentState.UpdateFinal();

        public void FixedUpdate() => CurrentState.FixedUpdateFinal();

        public S ChangeState(T newStateId)
        {
            if (EqualityComparer<T>.Default.Equals(CurrentStateId, newStateId)) return CurrentState;

            CurrentState.StopFinal();
            CurrentStateId = newStateId;
            CurrentState = stateCreators[newStateId].Invoke(AsTyped());
            CurrentState.InitFinal();
            return CurrentState;
        }
    }
}
