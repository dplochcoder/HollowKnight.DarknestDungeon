using Modding;
using System.Collections.Generic;

namespace DarknestDungeon.IC
{
    public record PromptsData
    {
        public SortedDictionary<string, string> Prompts = new();
        public SortedDictionary<string, SortedSet<string>> DreamnailPrompts = new();
    }

    public class PromptsModule : AbstractDataModule<PromptsModule, PromptsData>
    {
        private Dictionary<string, string> FullPrompts = new();

        public override void Initialize()
        {
            foreach (var e in Data.Prompts) FullPrompts[e.Key] = e.Value;
            foreach (var e in Data.DreamnailPrompts)
            {
                int i = 0;
                foreach (var value in e.Value) {
                    var name = $"{e.Key}_{++i}";
                    FullPrompts[name] = value;
                }
            }

            ModHooks.LanguageGetHook += OverrideLanguageGet;
        }

        public override void Unload() => ModHooks.LanguageGetHook -= OverrideLanguageGet;

        protected override string JsonName() => "prompts";

        private string OverrideLanguageGet(string key, string sheetTitle, string orig) => FullPrompts.TryGetValue(key, out var value) ? value : orig;
    }
}
