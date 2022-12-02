using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarknestDungeon.EnemyLib
{
    public interface EnemyStateModule<T, S, M> where S : EnemyState<T, S, M> where M : EnemyStateMachine<T, S, M>
    {
        public void Update(out bool stateChange);

        public void Stop();
    }
}
