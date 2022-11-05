using DarknestDungeon.IC;
using ItemChanger;

namespace DarknestDungeon
{
    public static class ModuleInstaller
    {

        public static void Setup() => On.UIManager.StartNewGame += InstallModules;

        private static void InstallModules(On.UIManager.orig_StartNewGame orig, UIManager self, bool permaDeath, bool bossRush)
        {
            if (!DarknestDungeon.GS.Enabled)
            {
                orig(self, permaDeath, bossRush);
                return;
            }

            ItemChangerMod.CreateSettingsProfile(false);
            var mods = ItemChangerMod.Modules;

            mods.Add<BenchesModule>();
            mods.Add<DeployersModule>();
            mods.Add<LifebloodCocoonFixerModule>();
            mods.Add<PromptsModule>();
            mods.Add<VoidCloakModule>();
            mods.Add<VoidFlameModule>();
            mods.Add<VoidShardsModule>();

            orig(self, permaDeath, bossRush);
        }
    }
}
