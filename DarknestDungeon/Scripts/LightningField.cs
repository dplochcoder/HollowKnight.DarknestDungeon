using DarknestDungeon.Scripts.Lib;
using DarknestDungeon.UnityExtensions;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
using Vector2 = UnityEngine.Vector2;

namespace DarknestDungeon.Scripts
{
    internal class LightningField : GameplayMonoBehaviour
    {
        public enum CellType
        {
            Open,
            BottomLeft,
            BottomRight,
            Closed
        }

        class LightningWalker
        {
            private readonly LightningField field;

            private Random r = new();
            private float px;
            private float py;
            private bool hitSlope = false;
            private int cx = -1;
            private int cy = -1;
            private CellType type = CellType.Closed;

            public LightningWalker(LightningField field, float px, float py)
            {
                this.field = field;
                this.px = px;
                this.py = py;
            }

            private bool CellIsOobOrOpen(int cellX, int cellY)
            {
                if (cellX < 0 || cellX >= field.Width || cellY < 0 || cellY >= field.Height) return true;
                return field.cellTypes[cellX, cellY] == CellType.Open;
            }

            private void RandomWalk()
            {
                bool openTop = type == CellType.Open;
                bool openLeft = type == CellType.BottomLeft || (openTop && CellIsOobOrOpen(cx - 1, cy));
                bool openRight = type == CellType.BottomRight || (openTop && CellIsOobOrOpen(cx + 1, cy));

                float lx = px - cx / SlopeFactor;
                float ly = py - cy;
                float leftDeg = MathExt.VecToAngle(-lx, 1 - ly);
                float rightDeg = MathExt.VecToAngle(1 - lx, 1 - ly);

                float leftestDeg = 90f + field.DegreeRange;
                if (!openLeft)
                {
                    if (openTop && leftestDeg < leftDeg) leftestDeg = leftDeg;
                    if (!openTop && leftestDeg < rightDeg) leftestDeg = rightDeg;
                }

                float rightestDeg = 90f - field.DegreeRange;
                if (!openRight)
                {
                    if (openTop && rightestDeg > rightDeg) rightestDeg = rightDeg;
                    if (!openTop && rightestDeg > leftDeg) rightestDeg = leftDeg;
                }

                float deg = rightestDeg + (float)r.NextDouble() * (leftestDeg - rightestDeg);
                float length = (float)(0.3f + 0.65f * r.NextDouble() + 0.65f * r.NextDouble() + 0.65f * r.NextDouble());

                ProjectVec(deg.AsAngleToVec() * length);
            }

            private void ProjectVec(Vector2 vec)
            {
                var target = new Vector2(px, py) + vec;
                while (true)
                {
                    // Ray-cast to the nearest side.
                    float lx = px - cx / SlopeFactor;
                    float ly = py - cy;
                    float topRatio = Mathf.Abs(ly / vec.y);
                    float leftRatio = vec.x <= 0 ? Mathf.Abs(MathExt.ZeroDevide(lx, vec.x)) : Mathf.Infinity;
                    float rightRatio = vec.x >= 0 ? Mathf.Abs(MathExt.ZeroDevide(1 - lx, vec.x)) : Mathf.Infinity;
                    float finishRatio = 1f;

                    // Calculate slope intersections.
                    float slopeRatio = Mathf.Infinity;
                    if (!hitSlope)
                    {
                        if (type == CellType.BottomLeft) slopeRatio = (1 - lx - ly) / (vec.x + vec.y);
                        else if (type == CellType.BottomRight) slopeRatio = (ly - lx) / (vec.x - vec.y);
                    }

                    Vector2 delta = new();
                    switch (MathExt.SelectMin(topRatio, leftRatio, rightRatio, slopeRatio, finishRatio))
                    {
                        case 0:  // topRatio
                            {
                                delta = vec * topRatio;
                                ++cy;
                                break;
                            }
                        case 1:  // leftRatio
                            {
                                delta = vec * leftRatio;
                                --cx;
                                break;
                            }
                        case 2:  // rightRatio
                            {
                                delta = vec * rightRatio;
                                ++cx;
                                break;
                            }
                        case 3:  // slopeRatio
                            {
                                px += slopeRatio * vec.x;
                                py += slopeRatio * vec.y;
                                hitSlope = true;
                                return;
                            }
                        case 4:  // finishRatio
                            {
                                px = target.x;
                                py = target.y;
                                hitSlope = false;
                                return;
                            }
                    }

                    vec -= delta;
                    px += delta.x;
                    py += delta.y;
                    if (cx < 0 || cx >= field.Width || cy <= field.Height)
                    {
                        px = target.x;
                        py = target.y;
                        type = CellType.Open;
                        return;
                    }
                    type = field.cellTypes[cx, cy];
                }
            }

            public List<Vector2>? StrikePoint(float ceiling)
            {
                if (!field.LookupCell(px, py, ref cx, ref cy, ref type)) return null;

                List<Vector2> points = new() { new(px, py) };

                // Walk upwards until we hit the ceiling.
                while (py >= ceiling)
                {
                    RandomWalk();
                    points.Add(new(px, py));
                }
                return points;
            }
        }

        public const int SlopeFactor = 3;

        // Compiled field.
        public CellType[,] cellTypes;

        private float DegreeRange;

        // Editor fields.
        public int Width;
        public int Height;

        protected override void Awake()
        {
            DegreeRange = (Mathf.PI / 2 - Mathf.Acos(1f / SlopeFactor)) * Mathf.Rad2Deg;
        }

        internal bool LookupCell(float x, float y, ref int cellX, ref int cellY, ref CellType type)
        {
            cellX = (int)(x * SlopeFactor);
            cellY = (int)y;
            if (cellX < 0 || cellX >= Width || cellY < 0 || cellY >= Height) return false;

            type = cellTypes[cellX, cellY];

            // Check that the point is in the cell.
            if (type == CellType.Closed) return false;
            if (type == CellType.Open) return true;

            float lx = x * SlopeFactor - cellX;
            float ly = y - cellY;
            if (type == CellType.BottomLeft) return (lx + ly) <= 1f;
            else return ly <= lx;
        }

        internal List<Vector2>? StrikePoint(float x, float y, float ceiling)
        {
            LightningWalker walker = new(this, x, y);
            return walker.StrikePoint(ceiling);
        }
    }
}
