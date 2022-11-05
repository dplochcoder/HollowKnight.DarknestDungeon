using DarknestDungeon.IC;
using ItemChanger;
using System.Collections.Generic;

namespace DarknestDungeon.Data
{
    using JsonUtil = PurenailCore.SystemUtil.JsonUtil<DarknestDungeon>;

    public static class Deployers
    {
        public static readonly SortedDictionary<string, SortedDictionary<string, IDeployer>> Data = JsonUtil.DeserializeEmbedded<SortedDictionary<string, SortedDictionary<string, IDeployer>>>("DarknestDungeon.Resources.Data.deployers.json");

        public static void Load()
        {
            if (DarknestDungeon.Instance != null)
            {
                DarknestDungeon.Log("Deployers loaded");
            }
        }

        public static void Install() => ItemChangerMod.Modules.Add<DeployersModule>();
    }
}
