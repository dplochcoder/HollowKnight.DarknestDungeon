using System;
using System.Collections.Generic;

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
        public readonly int X;
        public readonly int Y;
        public readonly int W;
        public readonly int H;

        public Rect(int x, int y, int w, int h)
        {
            X = x;
            Y = y;
            W = w;
            H = h;
        }
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
                    bool filled = g.Filled(x, y);
                    claimed[x, y] = !filled;
                    if (filled) ++runSize;
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
            // Expand outwards until we can't anymore.
            int w = 1;
            int h = 1;
            int maxW = xRuns[cX, cY];
            int maxH = yRuns[cX, cY];
            while ((w < maxW && !claimed[cX + w, cY]) || h < maxH)
            {
                if (w < maxW && !claimed[cX + w, cY]) maxH = Math.Min(maxH, yRuns[cX + w, cY]);
                if (h < maxH) maxW = Math.Min(maxW, xRuns[cX, cY + h]);
                if (w < maxW && !claimed[cX + w, cY]) ++w;
                if (h < maxH) ++h;
            }

            for (int dx = 0; dx < w; dx++)
                for (int dy = 0; dy < h; dy++)
                    claimed[cX + dx, cY + dy] = true;
            return new Rect(cX, cY, w, h);
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
