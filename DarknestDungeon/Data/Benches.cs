using DarknestDungeon.IC;
using ItemChanger;
using System.Collections.Generic;

namespace DarknestDungeon.Data
{
    using JsonUtil = PurenailCore.SystemUtil.JsonUtil<DarknestDungeon>;

    public static class Benches
    {
        public static readonly SortedDictionary<string, Benchwarp.Bench> Data = JsonUtil.DeserializeEmbedded<SortedDictionary<string, Benchwarp.Bench>>("DarknestDungeon.Resources.Data.benches.json");

        public static void Load()
        {
            if (DarknestDungeon.Instance != null)
            {
                DarknestDungeon.Log("Deployers loaded");
            }
        }

        public static void Install()
        {
            ItemChangerMod.Modules.Add<VoidBenchesModule>();
        }
    }
}
