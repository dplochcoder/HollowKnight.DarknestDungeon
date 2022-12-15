using System;
using System.Collections.Generic;
using UnityEditor;
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

        public static bool IsFilled(this TileAdjacency adj, Tilemap tilemap, int x, int y)
        {
            int nx = x + adj.DeltaX();
            int ny = y + adj.DeltaY();
            return nx < 0 || ny < 0 || nx >= tilemap.size.x || ny >= tilemap.size.y || tilemap.GetTile(new Vector3Int(nx, ny, 0)) != null;
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
    }

    public class Tileset
    {
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

        public (List<(int, int)>, int) GetTileIds(IEnumerable<TileAdjacency> adjacency)
        {
            int key = 0;
            foreach (var adj in adjacency) key += (int)adj;
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
                tilemap.SetTile(new Vector3Int(x, y, 0), AssetDatabase.LoadAssetAtPath($"Assets/Sprites/{spriteFolder}/{chosenId}.asset", typeof(Tile)) as TileBase);
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

            var path = AssetDatabase.GetAssetPath(sprite);
            var folder = path.Substring(ASSET_PREFIX.Length, path.LastIndexOf("/") - ASSET_PREFIX.Length);
            if (tilesets.TryGetValue(folder, out var tileset)) tileset.UpdateTile(r, tilemap, folder, x, y);
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
            // FIXME: Do more

            tilesets.Add("TempleBrick", templeBrick);
        }
    }
}
