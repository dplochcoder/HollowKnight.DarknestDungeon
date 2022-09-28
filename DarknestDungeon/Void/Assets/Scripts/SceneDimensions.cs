using UnityEngine;

namespace DarknestDungeon.Scripts
{
    class SceneDimensions : MonoBehaviour
    {
        public int Width;
        public int Height;

        public void Awake() => On.GameManager.RefreshTilemapInfo += OnGameManagerRefreshTilemapInfo;

        public void OnDestroy() => On.GameManager.RefreshTilemapInfo -= OnGameManagerRefreshTilemapInfo;

        private void OnGameManagerRefreshTilemapInfo(On.GameManager.orig_RefreshTilemapInfo orig, GameManager self, string targetScene)
        {
            orig(self, targetScene);
            if (targetScene == gameObject.scene.name)
            {
                self.tilemap.width = Width;
                self.tilemap.height = Height;
                self.sceneWidth = Width;
                self.sceneHeight = Height;
                FindObjectOfType<GameMap>().SetManualTilemap(0, 0, Width, Height);
            }
        }
    }
}
