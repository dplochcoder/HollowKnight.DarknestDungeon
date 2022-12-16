using DarknestDungeon.IC;
using System.IO;

namespace DarknestDungeon.Data
{
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
            CopyDll(root, "UnityScriptShims/bin/Debug/DarknestDungeon.dll", "DarknestDungeon/Unity/Assets/Assemblies/DarknestDungeon.dll");
        }

        private static void CopyDll(string root, string src, string dst)
        {
            var inputDll = Path.Combine(root, src);
            var outputDll = Path.Combine(root, dst);
            if (File.Exists(outputDll)) File.Delete(outputDll);
            File.Copy(inputDll, outputDll);
        }
    }
}
