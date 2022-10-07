using ItemChanger;
using System.Collections.Generic;

namespace DarknestDungeon.Data
{
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

        public static void Install()
        {
            foreach (var sceneDeployers in Data.Values)
            {
                foreach (var deployer in sceneDeployers.Values)
                {
                    ItemChangerMod.AddDeployer(deployer);
                }
            }
        }
    }
}
