using DarknestDungeon.Data;
using DarknestDungeon.IC;
using ItemChanger;

namespace DarknestDungeon
{
    public static class ModuleInstaller
    {

        public static void Setup()
        {
            On.UIManager.StartNewGame += InstallModules;
        }

        private static void InstallModules(On.UIManager.orig_StartNewGame orig, UIManager self, bool permaDeath, bool bossRush)
        {
            if (!DarknestDungeon.GS.Enabled)
            {
                orig(self, permaDeath, bossRush);
                return;
            }

            ItemChangerMod.Modules.Add<DarknestDungeonSceneEdits>();
            ItemChangerMod.Modules.Add<VoidCloakModule>();
            Deployers.Install();
            orig(self, permaDeath, bossRush);
        }
    }
}
