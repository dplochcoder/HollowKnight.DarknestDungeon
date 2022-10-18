using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using SMA = UnityEngine.SceneManagement.SceneManagerAPI;

namespace DarknestDungeon
{
    public class DarknestDungeonSceneManagerAPI : SMA
    {
        public static void Load() => overrideAPI = new DarknestDungeonSceneManagerAPI();

        private AssetBundle shared;
        private Dictionary<string, AssetBundle?> sceneBundles = new();

        private DarknestDungeonSceneManagerAPI()
        {
            shared = LoadAsset("objectsbundle");
            shared.LoadAllAssets();

            foreach (var str in typeof(DarknestDungeonSceneManagerAPI).Assembly.GetManifestResourceNames())
            {
                if (!str.StartsWith(PREFIX) || str.EndsWith(".manifest") || str.EndsWith(".meta")) continue;
                string name = str.Substring(PREFIX.Length);
                if (name == "AssetBundles" || name == "objectsbundle" || name == "scenes") continue;

                sceneBundles[name] = null;
            }
        }

        protected override AsyncOperation LoadSceneAsyncByNameOrIndex(string sceneName, int sceneBuildIndex, LoadSceneParameters parameters, bool mustCompleteNextFrame)
        {
            MaybeLoadAsset(sceneName.ToLower());
            return base.LoadSceneAsyncByNameOrIndex(sceneName, sceneBuildIndex, parameters, mustCompleteNextFrame);
        }

        protected override AsyncOperation UnloadSceneAsyncByNameOrIndex(string sceneName, int sceneBuildIndex, bool immediately, UnloadSceneOptions options, out bool outSuccess)
        {
            MaybeUnloadAsset(sceneName.ToLower());
            return base.UnloadSceneAsyncByNameOrIndex(sceneName, sceneBuildIndex, immediately, options, out outSuccess);
        }

        private const string PREFIX = "DarknestDungeon.Unity.Assets.AssetBundles.";

        private AssetBundle LoadAsset(string name)
        {
            using StreamReader sr = new(typeof(DarknestDungeonSceneManagerAPI).Assembly.GetManifestResourceStream($"{PREFIX}{name}"));
            return AssetBundle.LoadFromStream(sr.BaseStream);
        }

        private void MaybeLoadAsset(string name)
        {
            if (sceneBundles.ContainsKey(name))
            {
                sceneBundles[name] ??= LoadAsset(name);
            }
        }

        private void MaybeUnloadAsset(string name)
        {
            if (sceneBundles.ContainsKey(name))
            {
                sceneBundles[name]?.Unload(false);
                sceneBundles[name] = null;
            }
        }

    }
}
