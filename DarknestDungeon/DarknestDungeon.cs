using Modding;
using PurenailCore.ModUtil;

namespace DarknestDungeon
{
    public class DarknestDungeon : Mod, IGlobalSettings<GlobalSettings>
    {
        public static GlobalSettings GS;

        public DarknestDungeon() : base("Darknest Dungeon") { }

        public override void Initialize()
        {
            GS ??= new();
            ModuleInstaller.Setup();
        }

        private static readonly string Version = VersionUtil.ComputeVersion<DarknestDungeon>();

        public override string GetVersion() => Version;

        public void OnLoadGlobal(GlobalSettings s) => GS = s ?? new();

        public GlobalSettings OnSaveGlobal() => GS ?? new();
    }
}