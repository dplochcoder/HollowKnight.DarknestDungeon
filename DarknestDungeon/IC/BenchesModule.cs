using Newtonsoft.Json;
using PurenailCore.SystemUtil;
using System.Collections.Generic;
using System.Linq;

namespace DarknestDungeon.IC
{
    public record BenchData
    {
        public Benchwarp.Bench Bench;
        public BenchRando.IC.BenchDeployer Deployer;

        [JsonIgnore]
        public string RespawnMarker => $"BenchDeployer-{Deployer.SceneName}-({(int)Deployer.X},{(int)Deployer.Y})";

        public void Update()
        {
            Bench = new(Bench.name, Bench.areaName, Deployer.SceneName, RespawnMarker,
                Bench.respawnType, Bench.mapZone, Bench.style, Bench.specificOffset);
        }
    }

    public class BenchesModule : AbstractDataModule<BenchesModule, SortedDictionary<string, BenchData>>
    {
        private Benchwarp.Bench waterwaysCopy;

        protected override string JsonName() => "benches";

        public override void Initialize()
        {
            base.Initialize();
            var b = Benchwarp.Bench.baseBenches.Where(b => b.name == "Waterways").Single();
            waterwaysCopy = new(b.name, "City", b.sceneName, b.respawnMarker, b.respawnType, b.mapZone, b.style, b.specificOffset);

            Benchwarp.Events.BenchInjectors += YieldVoidBenches;
            Benchwarp.Events.BenchInjectors += YieldWaterwaysBench;
            Benchwarp.Events.BenchSuppressors += IsWaterwaysAreaBench;

            foreach (var benchData in Data.Values)
            {
                ItemChanger.Events.AddSceneChangeEdit(benchData.Deployer.SceneName, benchData.Deployer.OnSceneChange);
            }
        }

        protected override void Update(SortedDictionary<string, BenchData> data) => data.Values.ForEach(bd => bd.Update());

        public override void Unload()
        {
            Benchwarp.Events.BenchInjectors -= YieldVoidBenches;
            Benchwarp.Events.BenchInjectors -= YieldWaterwaysBench;
            Benchwarp.Events.BenchSuppressors -= IsWaterwaysAreaBench;

            foreach (var benchData in Data.Values)
            {
                ItemChanger.Events.RemoveSceneChangeEdit(benchData.Deployer.SceneName, benchData.Deployer.OnSceneChange);
            }
            base.Unload();
        }

        private IEnumerable<Benchwarp.Bench> YieldVoidBenches() => Data.Values.Select(bd => bd.Bench);

        private IEnumerable<Benchwarp.Bench> YieldWaterwaysBench()
        {
            yield return waterwaysCopy;
        }

        private bool IsWaterwaysAreaBench(Benchwarp.Bench bench) => bench.areaName == "Waterways";
    }
}
