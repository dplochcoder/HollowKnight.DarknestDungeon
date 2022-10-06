using DarknestDungeon.UnityExtensions;
using ItemChanger;
using System.Collections.Generic;
using UnityEngine;

namespace DarknestDungeon.IC
{
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

        public void Awake()
        {
            var tm = GameObject.Find("TileMap").GetComponent<tk2dTileMap>();
            foreach (var rect in ClearRects)
            {
                for (int i = 0; i < rect.Width; i++)
                    for (int j = 0; j < rect.Height; j++)
                        tm.ClearTile(rect.X + i, rect.Y + j, 0);
            }
            tm.ForceBuild();
            GameObject.Destroy(this);
        }
    }

    public record TileMapEditDeployer : Deployer
    {
        public TilemapEdit.Rect ClearRect;

        public override GameObject Deploy()
        {
            var te = GameObject.Find("TileMap").GetOrAddComponent<TilemapEdit>();
            te.enabled = true;
            te.ClearRects.Add(ClearRect);
            return null;
        }

        public override GameObject Instantiate() => null;
    }
}
