using Benchwarp;
using DarknestDungeon.Data;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace DarknestDungeon.IC
{
    public class VoidBenchesModule : ItemChanger.Modules.Module
    {
        Bench waterwaysCopy;

        public override void Initialize()
        {
            var b = Bench.baseBenches.Where(b => b.name == "Waterways").Single();
            waterwaysCopy = new(b.name, "City", b.sceneName, b.respawnMarker, b.respawnType, b.mapZone, b.style, b.specificOffset);

            Events.BenchInjectors += EnumVoidBenches;
            Events.BenchInjectors += ReplaceWaterwaysBench;
            Events.BenchSuppressors += HideWaterwaysBenches;
        }

        public override void Unload()
        {
            Events.BenchInjectors -= EnumVoidBenches;
            Events.BenchInjectors -= ReplaceWaterwaysBench;
            Events.BenchSuppressors -= HideWaterwaysBenches;
        }

        private IEnumerable<Bench> EnumVoidBenches() => Benches.Data.Values;

        private IEnumerable<Bench> ReplaceWaterwaysBench()
        {
            yield return waterwaysCopy;
        }

        private bool HideWaterwaysBenches(Bench bench) => bench.areaName == "Waterways";
    }
}
