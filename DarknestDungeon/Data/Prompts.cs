using DarknestDungeon.IC;
using ItemChanger;
using System.Collections.Generic;

namespace DarknestDungeon.Data
{
    using JsonUtil = PurenailCore.SystemUtil.JsonUtil<DarknestDungeon>;

    public static class Prompts
    {
        public static readonly SortedDictionary<string, string> Data = JsonUtil.DeserializeEmbedded<SortedDictionary<string, string>>("DarknestDungeon.Resources.Data.prompts.json");

        public static void Load()
        {
            if (DarknestDungeon.Instance != null) DarknestDungeon.Log("Prompts loaded");
        }

        public static void Install() => ItemChangerMod.Modules.Add<PromptsModule>();
    }
}
