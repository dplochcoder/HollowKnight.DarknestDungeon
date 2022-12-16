using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DarknestDungeon.Lib
{
    public enum TileAdjacency
    {
        RIGHT = 1,
        UP_RIGHT = 2,
        UP = 4,
        UP_LEFT = 8,
        LEFT = 16,
        DOWN_LEFT = 32,
        DOWN = 64,
        DOWN_RIGHT = 128
    };

    public static class TileAdjacencyExtensions
    {
        public static int DeltaX(this TileAdjacency adj)
        {
            switch (adj)
            {
                case TileAdjacency.DOWN_RIGHT:
                case TileAdjacency.RIGHT:
                case TileAdjacency.UP_RIGHT:
                    return 1;
                case TileAdjacency.DOWN_LEFT:
                case TileAdjacency.LEFT:
                case TileAdjacency.UP_LEFT:
                    return -1;
                default:
                    return 0;
            }
        }

        public static int DeltaY(this TileAdjacency adj)
        {
            switch (adj)
            {
                case TileAdjacency.DOWN_LEFT:
                case TileAdjacency.DOWN:
                case TileAdjacency.DOWN_RIGHT:
                    return -1;
                case TileAdjacency.UP_LEFT:
                case TileAdjacency.UP:
                case TileAdjacency.UP_RIGHT:
                    return 1;
                default:
                    return 0;
            }
        }

        // Prereq: tile is filled
        public static bool IsFilled(this TileAdjacency adj, Tilemap tilemap, int x, int y)
        {
            int dx = adj.DeltaX();
            int dy = adj.DeltaY();
            int nx = x + dx;
            int ny = y + dy;
            if (nx < 0 || nx >= tilemap.size.x)
            {
                if (dy == 0 || ny < 0 || ny >= tilemap.size.y) return true;
                return tilemap.IsFilled(x, ny);
            }
            else if (ny < 0 || ny >= tilemap.size.y)
            {
                return dx == 0 || tilemap.IsFilled(nx, y);
            }
            else
            {
                return tilemap.IsFilled(nx, ny);
            }
        }

        private static readonly List<TileAdjacency> ALL = new List<TileAdjacency>()
        {
            TileAdjacency.RIGHT,
            TileAdjacency.UP_RIGHT,
            TileAdjacency.UP,
            TileAdjacency.UP_LEFT,
            TileAdjacency.LEFT,
            TileAdjacency.DOWN_LEFT,
            TileAdjacency.DOWN,
            TileAdjacency.DOWN_RIGHT,
        };

        public static IEnumerable<TileAdjacency> FilledAdjacencies(this Tilemap tilemap, int x, int y)
        {
            foreach (var adj in ALL)
            {
                if (adj.IsFilled(tilemap, x, y)) yield return adj;
            }
        }

        public static bool IsFilled(this Tilemap tilemap, int x, int y) => tilemap.GetTile(new Vector3Int(x, y, 0)) != null;
    }

    public class Tileset
    {
        private static int RIGHT = (int)TileAdjacency.RIGHT;
        private static int UP_RIGHT = (int)TileAdjacency.UP_RIGHT;
        private static int UP = (int)TileAdjacency.UP;
        private static int UP_LEFT = (int)TileAdjacency.UP_LEFT;
        private static int LEFT = (int)TileAdjacency.LEFT;
        private static int DOWN_LEFT = (int)TileAdjacency.DOWN_LEFT;
        private static int DOWN = (int)TileAdjacency.DOWN;
        private static int DOWN_RIGHT = (int)TileAdjacency.DOWN_RIGHT;

        private List<List<(int, int)>> tileIds;
        private List<int> tileWeightTotals;

        public Tileset()
        {
            tileIds = new List<List<(int, int)>>(256);
            tileWeightTotals = new List<int>(256);
            for (int i = 0; i < 256; i++)
            {
                tileIds.Add(new List<(int, int)>());
                tileWeightTotals.Add(0);
            }
        }

        public void AddTileId(int tileId, int weight, params TileAdjacency[] adjacency)
        {
            int key = 0;
            foreach (var adj in adjacency) key += (int)adj;
            tileIds[key].Add((tileId, weight));
            tileWeightTotals[key] += weight;
        }

        // Ignore orphaned corners.
        private static void NormalizeKey(ref int key)
        {
            if ((key & (UP + RIGHT)) != (UP + RIGHT)) key &= ~UP_RIGHT;
            if ((key & (UP + LEFT)) != (UP + LEFT)) key &= ~UP_LEFT;
            if ((key & (DOWN + RIGHT)) != (DOWN + RIGHT)) key &= ~DOWN_RIGHT;
            if ((key & (DOWN + LEFT)) != (DOWN + LEFT)) key &= ~DOWN_LEFT;
        }

        public (List<(int, int)>, int) GetTileIds(IEnumerable<TileAdjacency> adjacency)
        {
            int key = 0;
            foreach (var adj in adjacency) key += (int)adj;
            NormalizeKey(ref key);
            return (tileIds[key], tileWeightTotals[key]);
        }

        public void UpdateTile(System.Random r, Tilemap tilemap, string spriteFolder, int x, int y)
        {
            (var list, var total) = GetTileIds(tilemap.FilledAdjacencies(x, y));
            int pick = r.Next(total);

            int chosenId = -1;
            foreach ((var id, var weight) in list)
            {
                if (pick <= weight)
                {
                    chosenId = id;
                    break;
                }
                pick -= weight;
            }

            if (chosenId != -1)
            {
#if UNITY_EDITOR
                tilemap.SetTile(new Vector3Int(x, y, 0), UnityEditor.AssetDatabase.LoadAssetAtPath($"Assets/Sprites/{spriteFolder}/{chosenId}.asset", typeof(Tile)) as TileBase);
#endif
            }
        }
    }

    public static class DefaultTilesets
    {
        private static TileAdjacency RIGHT = TileAdjacency.RIGHT;
        private static TileAdjacency UP_RIGHT = TileAdjacency.UP_RIGHT;
        private static TileAdjacency UP = TileAdjacency.UP;
        private static TileAdjacency UP_LEFT = TileAdjacency.UP_LEFT;
        private static TileAdjacency LEFT = TileAdjacency.LEFT;
        private static TileAdjacency DOWN_LEFT = TileAdjacency.DOWN_LEFT;
        private static TileAdjacency DOWN = TileAdjacency.DOWN;
        private static TileAdjacency DOWN_RIGHT = TileAdjacency.DOWN_RIGHT;

        private static Dictionary<string, Tileset> tilesets = new Dictionary<string, Tileset>();

        private const string ASSET_PREFIX = "Assets/Sprites/";

        public static void UpdateTile(System.Random r, Tilemap tilemap, int x, int y)
        {
            var sprite = tilemap.GetSprite(new Vector3Int(x, y, 0));
            if (sprite == null) return;

#if UNITY_EDITOR
            var path = UnityEditor.AssetDatabase.GetAssetPath(sprite);
            var folder = path.Substring(ASSET_PREFIX.Length, path.LastIndexOf("/") - ASSET_PREFIX.Length);
            if (tilesets.TryGetValue(folder, out var tileset)) tileset.UpdateTile(r, tilemap, folder, x, y);
#endif
        }

        static DefaultTilesets()
        {
            Tileset templeBrick = new Tileset();
            templeBrick.AddTileId(0, 1, DOWN);
            templeBrick.AddTileId(1, 1, DOWN, RIGHT);
            templeBrick.AddTileId(2, 1, LEFT, DOWN, RIGHT);
            templeBrick.AddTileId(3, 1, LEFT, DOWN);
            templeBrick.AddTileId(4, 1, LEFT, UP_LEFT, UP, DOWN, RIGHT);
            templeBrick.AddTileId(5, 1, LEFT, DOWN, DOWN_RIGHT, RIGHT);
            templeBrick.AddTileId(6, 1, LEFT, DOWN_LEFT, DOWN, RIGHT);
            templeBrick.AddTileId(7, 1, LEFT, DOWN, UP, UP_RIGHT, RIGHT);
            templeBrick.AddTileId(8, 1, DOWN, DOWN_RIGHT, RIGHT);
            templeBrick.AddTileId(9, 1, LEFT, DOWN_LEFT, DOWN, DOWN_RIGHT, RIGHT, UP);
            templeBrick.AddTileId(10, 1, LEFT, DOWN_LEFT, DOWN, DOWN_RIGHT, RIGHT);
            templeBrick.AddTileId(11, 1, LEFT, DOWN_LEFT, DOWN);
            templeBrick.AddTileId(12, 1, UP, DOWN);
            templeBrick.AddTileId(13, 1, UP, RIGHT, DOWN);
            templeBrick.AddTileId(14, 1, UP, LEFT, RIGHT, DOWN);
            templeBrick.AddTileId(15, 1, UP, LEFT, DOWN);
            templeBrick.AddTileId(16, 1, UP, UP, RIGHT, DOWN_RIGHT, DOWN);
            templeBrick.AddTileId(17, 1, UP, UP_RIGHT, RIGHT, DOWN_RIGHT, DOWN, DOWN_LEFT, LEFT);
            templeBrick.AddTileId(18, 1, UP, UP_LEFT, LEFT, DOWN_LEFT, DOWN, DOWN_RIGHT, RIGHT);
            templeBrick.AddTileId(19, 1, UP, LEFT, DOWN_LEFT, DOWN);
            templeBrick.AddTileId(20, 1, UP, UP_RIGHT, RIGHT, DOWN_RIGHT, DOWN);
            templeBrick.AddTileId(21, 1, UP, UP_RIGHT, RIGHT, DOWN, DOWN_LEFT, LEFT);
            templeBrick.AddTileId(22, 1, RIGHT, UP, UP_LEFT, LEFT, DOWN_LEFT, DOWN);
            templeBrick.AddTileId(23, 1, UP);
            templeBrick.AddTileId(24, 1, UP, RIGHT);
            templeBrick.AddTileId(25, 1, LEFT, UP, RIGHT);
            templeBrick.AddTileId(26, 1, LEFT, UP);
            templeBrick.AddTileId(27, 1, UP, UP_RIGHT, RIGHT, DOWN);
            templeBrick.AddTileId(28, 1, LEFT, UP_LEFT, UP, UP_RIGHT, RIGHT, DOWN_RIGHT, DOWN);
            templeBrick.AddTileId(29, 1, RIGHT, UP_RIGHT, UP, UP_LEFT, LEFT, DOWN_LEFT, DOWN);
            templeBrick.AddTileId(30, 1, DOWN, UP, UP_LEFT, LEFT);
            templeBrick.AddTileId(31, 1, LEFT, UP, UP_RIGHT, RIGHT, DOWN_RIGHT, DOWN);
            templeBrick.AddTileId(32, 1, RIGHT, UP_RIGHT, UP, UP_LEFT, LEFT, DOWN_LEFT, DOWN, DOWN_RIGHT);
            templeBrick.AddTileId(33, 1, UP, UP_LEFT, LEFT, DOWN, DOWN_RIGHT, RIGHT);
            templeBrick.AddTileId(34, 1, UP, UP_LEFT, LEFT, DOWN_LEFT, DOWN);
            templeBrick.AddTileId(35, 1);
            templeBrick.AddTileId(36, 1, RIGHT);
            templeBrick.AddTileId(37, 1, LEFT, RIGHT);
            templeBrick.AddTileId(38, 1, LEFT);
            templeBrick.AddTileId(39, 1, UP, RIGHT, DOWN, DOWN_LEFT, LEFT);
            templeBrick.AddTileId(40, 1, LEFT, UP, UP_RIGHT, RIGHT);
            templeBrick.AddTileId(41, 1, RIGHT, UP, UP_LEFT, LEFT);
            templeBrick.AddTileId(42, 1, UP, LEFT, DOWN, DOWN_RIGHT, RIGHT);
            templeBrick.AddTileId(43, 1, UP, UP_RIGHT, RIGHT);
            templeBrick.AddTileId(44, 1, LEFT, UP_LEFT, UP, UP_RIGHT, RIGHT);
            templeBrick.AddTileId(45, 1, LEFT, UP_LEFT, UP, UP_RIGHT, RIGHT, DOWN);
            templeBrick.AddTileId(46, 1, UP, UP_LEFT, LEFT);

            // FIXME: These are accent tiles, weight them differently
            templeBrick.AddTileId(47, 1, LEFT, DOWN_LEFT, DOWN, DOWN_RIGHT, RIGHT);
            templeBrick.AddTileId(48, 1, LEFT, DOWN_LEFT, DOWN, DOWN_RIGHT, RIGHT);
            templeBrick.AddTileId(49, 1, LEFT, DOWN_LEFT, DOWN, DOWN_RIGHT, RIGHT);
            templeBrick.AddTileId(50, 1);
            templeBrick.AddTileId(51, 1);
            templeBrick.AddTileId(52, 1, UP, UP_RIGHT, RIGHT, DOWN_RIGHT, DOWN);
            templeBrick.AddTileId(53, 1, UP, UP_LEFT, LEFT, DOWN_LEFT, DOWN);
            templeBrick.AddTileId(54, 1, LEFT, UP_LEFT, UP, UP_RIGHT, RIGHT);

            tilesets.Add("TempleBrick", templeBrick);
        }
    }
}
