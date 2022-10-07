using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DarknestDungeon
{
    public class AssetBundleLoader
    {
        // TODO: Load from json
        private static readonly List<string> objectAssetBundles = new()
        {
            "objectsbundle"
        };

        private static readonly List<string> sceneAssetBundles = new()
        {
            "VoidDescent01"
        };

        public static readonly AssetBundleLoader Instance = new();

        public static void Load() => DarknestDungeon.Log("Loaded AssetBundles");

        private Dictionary<string, AssetBundle> bundles = new();

        private AssetBundleLoader()
        {
            objectAssetBundles.ForEach(asset =>
            {
                var ab = LoadAsset(asset);
                bundles[asset] = ab;
                ab.LoadAllAssets();
            });
            sceneAssetBundles.ForEach(asset => bundles[asset] = LoadAsset(asset));
        }

        private AssetBundle LoadAsset(string name)
        {
            using StreamReader sr = new(typeof(AssetBundleLoader).Assembly.GetManifestResourceStream($"DarknestDungeon.Unity.Assets.AssetBundles.{name}"));
            return AssetBundle.LoadFromStream(sr.BaseStream);
        }

    }
}
