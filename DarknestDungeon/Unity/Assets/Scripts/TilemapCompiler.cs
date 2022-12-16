using UnityEngine.Tilemaps;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace DarknestDungeon.Scripts
{

#if UNITY_EDITOR
    internal class TilemapGrid : Lib.Grid
    {
        private readonly Tilemap tilemap;

        public TilemapGrid(Tilemap tilemap) => this.tilemap = tilemap;

        public bool Filled(int x, int y) => tilemap.GetTile(new Vector3Int(x, y, 0)) != null;

        public int Height() => tilemap.size.y;

        public int Width() => tilemap.size.x;
    }

    static class RectExtensions
    {
        public static List<Vector2> Points(this Lib.Rect r) => new List<Vector2>() { new Vector2(-r.W / 2f, -r.H / 2f), new Vector2(r.W / 2f, -r.H / 2f), new Vector2(r.W / 2f, r.H / 2f), new Vector2(-r.W / 2f, r.H / 2f), new Vector2(-r.W / 2f, -r.H / 2f) };

        public static Vector3 Center(this Lib.Rect r) => new Vector3(r.X + r.W / 2.0f, r.Y + r.H / 2.0f);
    }
#endif

    [RequireComponent(typeof(Tilemap))]
    public class TilemapCompiler : MonoBehaviour
    {
#if UNITY_EDITOR
        [ContextMenu("Compile Tilemap")]
        void CompileTilemap() {
            GameObject prevCompiled = GameObject.Find("CompiledTilemap");
            if (prevCompiled != null) DestroyImmediate(prevCompiled, true);

            if (gameObject.GetComponent<TilemapPatcher>() == null) gameObject.AddComponent<TilemapPatcher>();

            GameObject compiled = new GameObject("CompiledTilemap");

            GameObject colliders = new GameObject();
            colliders.name = "Colliders";
            colliders.transform.SetParent(compiled.transform);

            var tilemap = gameObject.GetComponent<Tilemap>();
            var grid = new TilemapGrid(tilemap);
            int i = 0;
            foreach (var rect in Lib.TilemapCovering.ComputeCovering(grid))
            {
                GameObject go = new GameObject();
                go.name = $"Collider {++i}";
                go.layer = 8;  // Terrain
                go.transform.SetParent(colliders.transform);

                var ec2d = go.AddComponent<EdgeCollider2D>();
                ec2d.isTrigger = false;
                ec2d.SetPoints(rect.Points());
                go.transform.position = rect.Center();
            }

            System.Random r = new System.Random();
            var w = tilemap.size.x;
            var h = tilemap.size.y;
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                   Lib.DefaultTilesets.UpdateTile(r, tilemap, x, y);

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
#endif
    }
}
