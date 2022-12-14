using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarknestDungeon.Lib
{
    public interface Grid
    {
        int Width();
        int Height();
        bool Filled(int x, int y);
    }

    public class Rect
    {
        public int X;
        public int Y;
        public int W;
        public int H;
    }

    internal class CoverageRuns
    {
        private readonly int width;
        private readonly int height;
        private int[,] xRuns;
        private int[,] yRuns;
        private bool[,] claimed;
        private int cX = 0;
        private int cY = 0;

        public CoverageRuns(Grid g)
        {
            this.width = g.Width();
            this.height = g.Height();
            this.xRuns = new int[width, height];
            this.yRuns = new int[width, height];
            this.claimed = new bool[width, height];

            for (int x = 0; x < width; x++)
            {
                int runSize = 0;
                for (int y = height - 1; y >= 0; y--)
                {
                    if (g.Filled(x, y)) ++runSize;
                    else runSize = 0;
                    yRuns[x, y] = runSize;
                }
            }
            for (int y = 0; y < height; y++)
            {
                int runSize = 0;
                for (int x = width - 1; x >= 0; x--)
                {
                    if (g.Filled(x, y)) ++runSize;
                    else runSize = 0;
                    xRuns[x, y] = runSize;
                }
            }
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    claimed[x, y] = !g.Filled(x, y);
        }

        public bool NextRect(out Rect rect)
        {
            while (cY < height && claimed[cX, cY])
            {
                if (++cX == width)
                {
                    cX = 0;
                    ++cY;
                }
            }

            if (cY == height)
            {
                rect = default;
                return false;
            }

            rect = BuildCurRect();
            return true;
        }

        private Rect BuildCurRect()
        {
            // FIXME
            return null;
        }
    }

    public static class ColliderOptimizer
    {
        public static List<Rect> Covering(Grid grid)
        {
            var runs = new CoverageRuns(grid);
            List<Rect> result = new List<Rect>();
            while (runs.NextRect(out var rect)) result.Add(rect);

            return result;
        }
    }
}
