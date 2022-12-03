﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarknestDungeon.EnemyLib
{
    public class EnemyStateModule<T, S, M> where S : EnemyState<T, S, M> where M : EnemyStateMachine<T, S, M>
    {
        public virtual void Init() { }

        public virtual void Update(out bool stateChange) { stateChange = false; }

        public virtual void Stop() { }
    }
}
