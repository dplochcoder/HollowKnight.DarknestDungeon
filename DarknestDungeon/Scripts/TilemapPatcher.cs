using UnityEngine;
using UnityEngine.Tilemaps;

namespace DarknestDungeon.Scripts
{
    public class TilemapPatcher : MonoBehaviour
    {
        private void Awake()
        {
            var go = new GameObject("TileMap");
            go.tag = "TileMap";
            var newMap = go.AddComponent<tk2dTileMap>();
            var oldMap = gameObject.GetComponent<Tilemap>();
            newMap.width = oldMap.size.x;
            newMap.height = oldMap.size.y;

            GameManager.instance.RefreshTilemapInfo(gameObject.scene.name);
        }
    }
}