using UnityEngine;

namespace DarknestDungeon.UnityExtensions
{
    public static class MathExt
    {
        public static Quaternion Angle(float x, float y, float degOffset)
        {
            var angle = Mathf.Atan2(y, x) * Mathf.Rad2Deg + degOffset;
            return Quaternion.AngleAxis(angle, Vector3.forward);
        }

        public static Quaternion Angle(float x, float y) => Angle(x, y, 0);

        public static Quaternion AngleVec(Vector2 vec) => Angle(vec.x, vec.y);

        public static Quaternion AngleVec(Vector2 vec, float degOffset) => Angle(vec.x, vec.y, degOffset);
    }
}
