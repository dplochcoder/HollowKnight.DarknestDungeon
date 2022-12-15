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
        }
    }
}
