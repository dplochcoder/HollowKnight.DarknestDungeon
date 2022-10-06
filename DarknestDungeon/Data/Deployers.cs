using ItemChanger;
using System.Collections.Generic;

namespace DarknestDungeon.Data
{
    public static class Deployers
    {
        private static readonly SortedDictionary<string, SortedDictionary<string, IDeployer>> data = JsonUtil.DeserializeEmbedded<SortedDictionary<string, SortedDictionary<string, IDeployer>>>("DarknestDungeon.Resources.Data.deployers.json");

        public static void Load()
        {
            DarknestDungeon.Log("Deployers loaded");
        }

        public static void Install()
        {
            foreach (var sceneDeployers in data.Values)
            {
                foreach (var deployer in sceneDeployers.Values)
                {
                    ItemChangerMod.AddDeployer(deployer);
                }
            }
        }
    }
}
