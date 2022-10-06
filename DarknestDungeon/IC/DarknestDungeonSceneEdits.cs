using ItemChanger;
using SFCore.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DarknestDungeon.IC
{
    public class DarknestDungeonSceneEdits : ItemChanger.Modules.Module
    {
        public override void Initialize()
        {
            On.SceneManager.Start += OnSceneManagerStart;
        }

        public override void Unload()
        {
            On.SceneManager.Start -= OnSceneManagerStart;
        }

        private static void OnSceneManagerStart(On.SceneManager.orig_Start orig, SceneManager sm)
        {
            orig(sm);

            var sceneName = sm.gameObject.scene.name;
            if (sceneName == "Abyss_15")
            {
                EditBirthplaceTilemap(GameObject.Find("TileMap").GetComponent<tk2dTileMap>());
            }
        }

        private static void EditBirthplaceTilemap(tk2dTileMap map)
        {
            // Clear out the dive section
            for (int x = 137; x < 141; x++)
                for (int y = 0; y < 10; y++)
                    map.ClearTile(x, y, 0);

            map.ForceBuild();
        }
    }
}
