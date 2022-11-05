using DarknestDungeon.Data;
using Modding;
using System.Collections.Generic;

namespace DarknestDungeon.IC
{
    public class PromptsModule : AbstractDataModule<PromptsModule, SortedDictionary<string, string>>
    {
        public override void Initialize() => ModHooks.LanguageGetHook += OverrideLanguageGet;

        public override void Unload() => ModHooks.LanguageGetHook -= OverrideLanguageGet;

        protected override string JsonName() => "prompts";

        private string OverrideLanguageGet(string key, string sheetTitle, string orig) => Data.TryGetValue(key, out var value) ? value : orig;
    }
}
