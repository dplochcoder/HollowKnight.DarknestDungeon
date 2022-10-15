using DarknestDungeon.Data;
using System.Collections.Generic;

namespace DarknestDungeon.IC
{
    public class VoidBenchesModule : ItemChanger.Modules.Module
    {
        public override void Initialize()
        {
            Benchwarp.Events.BenchInjectors += EnumVoidBenches;
        }

        public override void Unload()
        {
            Benchwarp.Events.BenchInjectors -= EnumVoidBenches;
        }

        private IEnumerable<Benchwarp.Bench> EnumVoidBenches() => Benches.Data.Values;
    }
}
