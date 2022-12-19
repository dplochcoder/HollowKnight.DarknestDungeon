using UnityEngine;

namespace DarknestDungeon.UnityExtensions
{
    public static class MathExt
    {
        public static Quaternion RadialToQuat(float x, float y, float degOffset)
        {
            var angle = VecToAngle(x, y) + degOffset;
            var q = new Quaternion();
            return Quaternion.AngleAxis(angle, Vector3.forward);
        }

        public static float ToAngle(this Quaternion q)
        {
            q.ToAngleAxis(out var angle, out var axis);
            return angle;
        }

        public static float VecToAngle(float x, float y) => Mathf.Atan2(y, x) * Mathf.Rad2Deg;

        public static float VecToAngle(this Vector2 vec) => VecToAngle(vec.x, vec.y);

        public static Vector2 AsAngleToVec(this float angle) => new(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Rad2Deg));

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
    }
}
