using DarknestDungeon.Data;
using Modding;

namespace DarknestDungeon.IC
{
    public class PromptsModule : ItemChanger.Modules.Module
    {
        public override void Initialize() => ModHooks.LanguageGetHook += OverrideLanguageGet;

        public override void Unload() => ModHooks.LanguageGetHook -= OverrideLanguageGet;

        private string OverrideLanguageGet(string key, string sheetTitle, string orig) => Prompts.Data.TryGetValue(key, out var value) ? value : orig;
    }
}
