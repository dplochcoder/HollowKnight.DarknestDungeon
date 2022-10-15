using SFCore.Utils;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DarknestDungeon.Scripts
{
    // Converts a modern Unity Tilemap into an old school tk2dTileMap
    [RequireComponent(typeof(Tilemap))]
    public class PatchTilemap : MonoBehaviour
    {
        private void Awake()
        {
            var oldMap = gameObject.GetComponent<Tilemap>();
            var newMap = gameObject.AddComponent<tk2dTileMap>();
            newMap.width = oldMap.size.x;
            newMap.height = oldMap.size.y;

            var tmpl = Preloader.Instance.TileMap.GetComponent<tk2dTileMap>();
            newMap.data = tmpl.data;
            newMap.ColorChannel = tmpl.ColorChannel;
            newMap.PrefabsRoot = tmpl.PrefabsRoot;
            newMap.SetAttr("spriteCollection", tmpl.SpriteCollectionInst);

            // Instantiate appropriate data structures before mapping data.
            newMap.ForceBuild();

            for (int i = 0; i < newMap.width; i++)
            {
                for (int j = 0; j < newMap.height; j++)
                {
                    if (oldMap.GetTile(new(i, j, 0)) != null)
                    {
                        newMap.SetTile(i, j, 0, 0);
                    }
                }
            }
            newMap.ForceBuild();

            var parent = gameObject.transform.parent.gameObject;
            gameObject.transform.SetParent(null);
            gameObject.name = "TileMap";
            gameObject.tag = "TileMap";
            GameManager.instance.RefreshTilemapInfo(gameObject.scene.name);

            Destroy(oldMap);
            Destroy(gameObject.GetComponent<TilemapRenderer>());
            Destroy(parent);
            Destroy(this);
        }
    }
}
