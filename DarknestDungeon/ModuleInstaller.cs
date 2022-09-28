using DarknestDungeon.IC;
using ItemChanger;

namespace DarknestDungeon
{
    public static class ModuleInstaller
    {

        public static void Setup()
        {
            On.UIManager.StartNewGame += InstallModule;
        }

        private static void InstallModule(On.UIManager.orig_StartNewGame orig, UIManager self, bool permaDeath, bool bossRush)
        {
            if (DarknestDungeon.GS.Enabled)
            {
                var mod = ItemChangerMod.Modules.Add<VoidCloakModule>();
                if (DarknestDungeon.GS.GiveVoidCloak)
                {
                    mod.HasVoidCloak = true;
                }
            }

            orig(self, permaDeath, bossRush);
        }
    }
}
