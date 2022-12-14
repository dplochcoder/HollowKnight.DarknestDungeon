using UnityEngine.Tilemaps;
using UnityEngine;
using DarknestDungeon.Lib;
using System.Collections.Generic;

namespace DarknestDungeon.Scripts
{
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

    [RequireComponent(typeof(Tilemap))]
    public class TilemapCompiler : MonoBehaviour
    {
        [ContextMenu("Compile Tilemap")]
        void CompileTilemap() {
            GameObject prevCompiled = gameObject.transform.Find("Compiled")?.gameObject;
            if (prevCompiled != null) DestroyImmediate(prevCompiled, true);

            GameObject compiled = new GameObject();
            compiled.name = "Compiled";
            compiled.transform.SetParent(gameObject.transform);

            GameObject colliders = new GameObject();
            colliders.name = "Colliders";
            colliders.transform.SetParent(compiled.transform);

            var grid = new TilemapGrid(gameObject.GetComponent<Tilemap>());
            int i = 0;
            foreach (var rect in ColliderOptimizer.Covering(grid))
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
        }
    }
}
