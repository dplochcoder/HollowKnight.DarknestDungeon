using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DarknestDungeon
{
    public class AssetBundleLoader
    {
        public static readonly AssetBundleLoader Instance = new();

        public static void Load() => DarknestDungeon.Log("Loaded AssetBundles");

        private Dictionary<string, AssetBundle> bundles = new();

        private const string PREFIX = "DarknestDungeon.Unity.Assets.AssetBundles.";

        // TODO: Load scenes lazily?
        private AssetBundleLoader()
        {
            foreach (var str in typeof(AssetBundleLoader).Assembly.GetManifestResourceNames())
            {
                if (!str.StartsWith(PREFIX) || str.EndsWith(".manifest")) continue;
                string name = str.Substring(PREFIX.Length);

                if (name == "AssetBundles") continue;
                if (name == "objectsbundle")
                {
                    var ab = LoadAsset(str);
                    bundles[name] = ab;
                    ab.LoadAllAssets();
                }
                else
                {
                    bundles[name] = LoadAsset(str);
                }
            }
        }

        private AssetBundle LoadAsset(string name)
        {
            using StreamReader sr = new(typeof(AssetBundleLoader).Assembly.GetManifestResourceStream(name));
            return AssetBundle.LoadFromStream(sr.BaseStream);
        }

    }
}
