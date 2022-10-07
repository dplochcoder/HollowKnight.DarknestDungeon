using DarknestDungeon.UnityExtensions;
using ItemChanger;
using System.Collections.Generic;
using UnityEngine;

namespace DarknestDungeon.IC.Deployers
{
    // TileMap gets destroyed and replaced after scene load for no fucking reason so let's deal with that
    public class TilemapResetCatcher : MonoBehaviour
    {
        public TilemapEdit editor;

        public void OnDestroy() => editor.Reset();
    }

    public class TilemapEdit : MonoBehaviour
    {
        public record Rect
        {
            public int X;
            public int Y;
            public int Width;
            public int Height;
        }
        public List<Rect> ClearRects = new();

        private bool waitingForTilemap = true;

        public void Reset() => waitingForTilemap = true;

        public void FixedUpdate()
        {
            if (!waitingForTilemap) return;

            var go = GameObject.Find("TileMap");
            if (go == null) return;

            go.AddComponent<TilemapResetCatcher>().editor = this;
            var tm = go.GetComponent<tk2dTileMap>();
            foreach (var rect in ClearRects)
            {
                for (int i = 0; i < rect.Width; i++)
                    for (int j = 0; j < rect.Height; j++)
                        tm.ClearTile(rect.X + i, rect.Y + j, 0);
            }
            tm.ForceBuild();
            waitingForTilemap = false;
        }
    }

    public record TileMapEditDeployer : Deployer
    {
        public TilemapEdit.Rect ClearRect;

        public override GameObject Deploy()
        {
            GameObject go = GameObject.Find("_TilemapEdit") ?? new("_TilemapEdit");
            var te = go.GetOrAddComponent<TilemapEdit>();
            te.ClearRects.Add(ClearRect);
            return null;
        }

        public override GameObject Instantiate() => null;
    }
}
