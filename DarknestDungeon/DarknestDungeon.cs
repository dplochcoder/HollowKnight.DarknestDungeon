using Modding;

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

        public override string GetVersion() => Version.Instance;

        public void OnLoadGlobal(GlobalSettings s) => GS = s ?? new();

        public GlobalSettings OnSaveGlobal() => GS ?? new();
    }
}