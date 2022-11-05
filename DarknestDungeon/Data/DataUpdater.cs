using DarknestDungeon.IC;
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
            var root = InferGitRoot(Directory.GetCurrentDirectory());

            // Normalize json
            BenchesModule.UpdateJson(root);
            DeployersModule.UpdateJson(root);
            PromptsModule.UpdateJson(root);

            // Copy DLLs
            CopyDll(root, "DarknestDungeonUnityScripts/bin/Debug/DarknestDungeon.dll", "DarknestDungeon/Unity/Assets/Assemblies/DarknestDungeon.dll");
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
