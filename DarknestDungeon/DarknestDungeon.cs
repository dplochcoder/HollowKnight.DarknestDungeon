using ItemChanger.Internal.Menu;
using Modding;
using PurenailCore.ModUtil;
using System.Collections.Generic;
using UnityEngine;
using ILogger = Modding.ILogger;

namespace DarknestDungeon
{
    public class DarknestDungeon : Mod, ICustomMenuMod, IGlobalSettings<GlobalSettings>
    {
        public static DarknestDungeon Instance { get; private set; }

        public static GlobalSettings GS;

        public DarknestDungeon() : base("Darknest Dungeon")
        {
            Instance = this;
        }

        public static new void Log(string msg) => ((ILogger)Instance).Log(msg);

        public override List<(string, string)> GetPreloadNames() => IC.Preloader.Instance.GetPreloadNames();

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            IC.Preloader.Instance.Initialize(preloadedObjects);

            GS ??= new();
            ModuleInstaller.Setup();
            Data.Deployers.Load();
            Data.Benches.Load();
            AssetBundleLoader.Load();

            if (ModHooks.GetMod("DebugMod") is Mod)
            {
                DebugInterop.DebugInterop.Setup();
            }
        }

        private static readonly string Version = VersionUtil.ComputeVersion<DarknestDungeon>();

        public override string GetVersion() => Version;

        public bool ToggleButtonInsideMenu => false;

        public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates)
        {
            ModMenuScreenBuilder builder = new("Darknest Dungeon", modListMenu);
            builder.AddHorizontalOption(new()
            {
                Name = "Enable",
                Description = "Enable DarknestDungeon mod hooks in new saves if enabled",
                Values = new string[] { "No", "Yes" },
                Saver = i => GS.Enabled = i == 1,
                Loader = () => GS.Enabled ? 1 : 0
            });
            return builder.CreateMenuScreen();
        }

        public void OnLoadGlobal(GlobalSettings s) => GS = s ?? new();

        public GlobalSettings OnSaveGlobal() => GS ?? new();
    }
}