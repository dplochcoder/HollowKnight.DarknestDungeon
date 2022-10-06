using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DarknestDungeon
{
    public class AssetBundleLoader
    {
        private static readonly AssetBundleLoader Instance = new();

        private static readonly List<string> assets = new()
        {
            "mainbundle",
            "scenebundle"
        };

        private Dictionary<string, AssetBundle> bundles = new();

        private AssetBundleLoader()
        {
            foreach (var asset in assets)
            {
                var ab = LoadAsset(asset);
                bundles[asset] = ab;
                ab.LoadAllAssets();
            }
        }

        private AssetBundle LoadAsset(string name)
        {
            using StreamReader sr = new(typeof(AssetBundleLoader).Assembly.GetManifestResourceStream($"DarknestDungeon.Unity.Assets.AssetBundles.{name}"));
            return AssetBundle.LoadFromStream(sr.BaseStream);
        }

    }
}
