using System.Collections.Generic;
using UnityEngine;

namespace DarknestDungeon.UnityExtensions
{
    public static class MathExt
    {
        public static Quaternion RadialToQuat(float x, float y, float degOffset)
        {
            var angle = VecToAngle(x, y) + degOffset;
            return Quaternion.AngleAxis(angle, Vector3.forward);
        }

        public static float ToAngle(this Quaternion q)
        {
            q.ToAngleAxis(out var angle, out var axis);
            return angle;
        }

        public static float VecToAngle(float x, float y) => Mathf.Atan2(y, x) * Mathf.Rad2Deg;

        public static float VecToAngle(this Vector2 vec) => VecToAngle(vec.x, vec.y);

        public static float ZeroDevide(float num, float denom) => (Mathf.Abs(denom) >= 0.000001f) ? (num / denom) : Mathf.Infinity;

        public static Vector2 AsAngleToVec(this float angle) => new(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

        public static Quaternion RadialToQuat(float x, float y) => RadialToQuat(x, y, 0);

        public static Quaternion RadialVecToQuat(Vector2 vec) => RadialToQuat(vec.x, vec.y);

        public static Quaternion RadialVecToQuat(Vector2 vec, float degOffset) => RadialToQuat(vec.x, vec.y, degOffset);

        public static bool IsAngleBetween(float angle, float min, float max)
        {
            while (angle < min) angle += 360;
            while (angle > max) angle -= 360;
            return angle >= min && angle <= max;
        }

        public static float Clamp(float angle, float min, float max)
        {
            while (angle < min) angle += 360;
            while (angle > max) angle -= 360;
            if (angle < min - 180) return max;
            else if (angle < min || angle > max + 180) return min;
            else if (angle > max) return max;
            else return angle;
        }

        public static int SelectMin<T>(params T[] items)
        {
            if (items.Length == 0) return -1;

            int index = 0;
            T min = items[0];
            for (int i = 1; i < items.Length; i++)
            {
                if (Comparer<T>.Default.Compare(min, items[i]) > 0)
                {
                    index = i;
                    min = items[i];
                }
            }

            return index;
        }
    }
}
