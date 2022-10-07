using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            // Normalize deployers
            var deployersPath = Path.Combine(root, "DarknestDungeon/Resources/Data/deployers.json");
            File.Delete(deployersPath);
            JsonUtil.Serialize(Deployers.Data, deployersPath);

            // Copy DLL
            var outputDll = Path.Combine(root, "DarknestDungeon/UnityScripts/bin/Debug/DarknestDungeon.dll");
            var inputDll = Path.Combine(root, "DarknestDungeon/Unity/Assets/Assemblies/DarknestDungeon.dll");
            File.Delete(inputDll);
            File.Copy(outputDll, inputDll);
        }
    }
}
