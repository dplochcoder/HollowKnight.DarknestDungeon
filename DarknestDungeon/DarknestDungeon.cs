using Modding;
using PurenailCore.ModUtil;
using System.Collections.Generic;
using UnityEngine;

namespace DarknestDungeon
{
    public class DarknestDungeon : Mod, IGlobalSettings<GlobalSettings>
    {
        public static GlobalSettings GS;

        public DarknestDungeon() : base("Darknest Dungeon") { }

        public override List<(string, string)> GetPreloadNames() => IC.Preloader.Instance.GetPreloadNames();

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            IC.Preloader.Instance.Initialize(preloadedObjects);

            GS ??= new();
            ModuleInstaller.Setup();
        }

        private static readonly string Version = VersionUtil.ComputeVersion<DarknestDungeon>();

        public override string GetVersion() => Version;

        public void OnLoadGlobal(GlobalSettings s) => GS = s ?? new();

        public GlobalSettings OnSaveGlobal() => GS ?? new();
    }
}