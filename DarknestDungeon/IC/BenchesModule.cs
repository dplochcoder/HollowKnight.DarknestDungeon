﻿using Benchwarp;
using System.Collections.Generic;
using System.Linq;

namespace DarknestDungeon.IC
{
    public class BenchesModule : AbstractDataModule<BenchesModule, SortedDictionary<string, Bench>>
    {
        private Bench waterwaysCopy;

        protected override string JsonName() => "benches";

        public override void Initialize()
        {
            base.Initialize();
            var b = Bench.baseBenches.Where(b => b.name == "Waterways").Single();
            waterwaysCopy = new(b.name, "City", b.sceneName, b.respawnMarker, b.respawnType, b.mapZone, b.style, b.specificOffset);

            Events.BenchInjectors += YieldVoidBenches;
            Events.BenchInjectors += YieldWaterwaysBench;
            Events.BenchSuppressors += IsWaterwaysAreaBench;
        }

        public override void Unload()
        {
            Events.BenchInjectors -= YieldVoidBenches;
            Events.BenchInjectors -= YieldWaterwaysBench;
            Events.BenchSuppressors -= IsWaterwaysAreaBench;
            base.Unload();
        }

        private IEnumerable<Bench> YieldVoidBenches() => Data.Values;

        private IEnumerable<Bench> YieldWaterwaysBench()
        {
            yield return waterwaysCopy;
        }

        private bool IsWaterwaysAreaBench(Bench bench) => bench.areaName == "Waterways";
    }
}
