using Mono.Security.Protocol.Tls;
using System.IO;

namespace DarknestDungeon.Data
{
    using JsonUtil = PurenailCore.SystemUtil.JsonUtil<DarknestDungeon>;

    public static class DataUpdater
    {
        public static string InferGitRoot(string path)
        {
            var info = Directory.GetParent(path);
            while (info != null)
            {
                if (Directory.Exists(Path.Combine(info.FullName, ".git")))
                {
                    return info.FullName;
                }
                info = Directory.GetParent(info.FullName);
            }

            return path;
        }

        public static void Run()
        {
            Deployers.Load();
            Benches.Load();
            var root = InferGitRoot(Directory.GetCurrentDirectory());

            // Normalize deployers
            var deployersPath = Path.Combine(root, "DarknestDungeon/Resources/Data/deployers.json");
            UpdateJson(deployersPath, Deployers.Data);

            var benchesPath = Path.Combine(root, "DarknestDungeon/Resources/Data/benches.json");
            UpdateJson(benchesPath, Benches.Data);

            // Copy DLLs
            CopyDll(root, "DarknestDungeonUnityScripts/bin/Debug/DarknestDungeon.dll", "DarknestDungeon/Unity/Assets/Assemblies/DarknestDungeon.dll");
        }

        private static void UpdateJson<T>(string path, T data)
        {
            File.Delete(path);
            JsonUtil.Serialize(data, path);
        }

        private static void CopyDll(string root, string src, string dst)
        {
            var outputDll = Path.Combine(root, src);
            var inputDll = Path.Combine(root, dst);
            File.Delete(inputDll);
            File.Copy(outputDll, inputDll);
        }
    }
}
