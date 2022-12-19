using System.Collections.Generic;
using UnityEngine;

public static class GOUtils
{
    public static T GetOrAddComponent<T>(this GameObject go) where T : MonoBehaviour => go.GetComponent<T>() ?? go.AddComponent<T>();

    public static void DeleteChildren(GameObject go)
    {
        var children = new List<GameObject>();
        foreach (Transform child in go.transform) children.Add(child.gameObject);
        foreach (var child in children) Object.DestroyImmediate(child, true);
    }

    public static Vector2 To2d(this Vector3 vec) => new Vector2(vec.x, vec.y);

    public static Vector3 To3d(this Vector2 vec) => new Vector3(vec.x, vec.y, 0);

    public static float Snap(float f, float unit) => Mathf.RoundToInt(f / unit) * unit;

    public static float SnapDiff(float f, float unit) => Snap(f, unit) - f;

    public static Vector2 Snap(Vector2 v, float unit) => new Vector2(Snap(v.x, unit), Snap(v.y, unit));

    public static Vector2 SnapDiff(Vector2 v, float unit) => Snap(v, unit) - v;

    public static Vector3 Snap(Vector3 v, float unit) => new Vector3(Snap(v.x, unit), Snap(v.y, unit));

    public static Vector3 SnapDiff(Vector3 v, float unit) => Snap(v, unit) - v;

    public static Vector3 SnapAndGetDiff(Transform t, float unit)
    {
        var oldPos = t.position;
        var newPos = Snap(oldPos, unit);
        t.position = newPos;
        return newPos - oldPos;
    }

    public static void UnifyCenter(Transform t, BoxCollider2D bc2d)
    {
        var newPos = t.position + bc2d.offset.To3d();
        bc2d.offset = Vector2.zero;
        t.position = newPos;
    }
}