using Benchwarp;
using DarknestDungeon.Data;
using System.Collections.Generic;
using System.Linq;

namespace DarknestDungeon.IC
{
    public class VoidBenchesModule : ItemChanger.Modules.Module
    {
        Bench waterwaysCopy;

        public override void Initialize()
        {
            var b = Bench.baseBenches.Where(b => b.name == "Waterways").Single();
            waterwaysCopy = new(b.name, "City", b.sceneName, b.respawnMarker, b.respawnType, b.mapZone, b.style, b.specificOffset);

            Events.BenchInjectors += YieldVoidBenches;
            Events.BenchInjectors += YieldWaterwaysBench;
            Events.BenchSuppressors += IsWaterwaysBench;
        }

        public override void Unload()
        {
            Events.BenchInjectors -= YieldVoidBenches;
            Events.BenchInjectors -= YieldWaterwaysBench;
            Events.BenchSuppressors -= IsWaterwaysBench;
        }

        private IEnumerable<Bench> YieldVoidBenches() => Benches.Data.Values;

        private IEnumerable<Bench> YieldWaterwaysBench()
        {
            yield return waterwaysCopy;
        }

        private bool IsWaterwaysBench(Bench bench) => bench.areaName == "Waterways";
    }
}
