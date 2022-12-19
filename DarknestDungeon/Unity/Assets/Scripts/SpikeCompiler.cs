using System.Collections.Generic;
using UnityEngine;

namespace DarknestDungeon.Scripts
{
    [RequireComponent(typeof(Collider2D))]
    public class SpikeCompiler : SceneDataOptimizer
    {
        public Sprite spike1;
        public Sprite spike2;
        public Sprite spike3;

        public override void OptimizeScene()
        {
#if UNITY_EDITOR
            CompileSpikes();
#endif
        }

        private static Vector2 Snap(Vector2 p) => new Vector2(Snap(p.x), Snap(p.y));

        private static float Snap(float f) => Mathf.Round(f * 2) / 2.0f;

#if UNITY_EDITOR
        [ContextMenu("Compile Spikes")]
        void CompileSpikes()
        {
            var xdiff = Snap(transform.position.x) - transform.position.x;
            var ydiff = Snap(transform.position.y) - transform.position.y;
            transform.position = new Vector3(Snap(transform.position.x), Snap(transform.position.y), 0);

            var children = new List<GameObject>();
            foreach (Transform child in transform) children.Add(child.gameObject);
            foreach (var go in children) DestroyImmediate(go, true);

            var compiled = new GameObject("Compiled");
            compiled.transform.SetParent(transform);
            var sprites = new List<Sprite>() { spike1, spike2, spike3 };

            var c2d = GetComponent<Collider2D>();
            if (c2d is PolygonCollider2D pc2d)
            {
                pc2d.isTrigger = true;
                var oldPoints = pc2d.points;
                for (int i = 0; i < oldPoints.Length; i++) oldPoints[i] = new Vector2(Snap(oldPoints[i].x - xdiff), Snap(oldPoints[i].y - ydiff));
                pc2d.points = oldPoints;
            }
            var diag = c2d.bounds.max - c2d.bounds.min;
            int area = (int)(diag.x * diag.y);

            // Average 4 spikes per unit square
            var r = new System.Random();
            var t2d = new Texture2D((int)diag.x, (int)diag.y);
            int index = 1;
            for (int i = 0; i < 4 * area; i++)
            {
                float x = c2d.bounds.min.x + diag.x * (float)r.NextDouble();
                float y = c2d.bounds.min.y + diag.y * (float)r.NextDouble();
                if (!c2d.OverlapPoint(new Vector2(x, y))) continue;

                var go = new GameObject($"Spike {index++}");
                go.AddComponent<SpriteRenderer>().sprite = sprites[r.Next(sprites.Count)];
                go.transform.position = new Vector3(x, y, 0);
                go.transform.rotation = Quaternion.AngleAxis((float)(360 * r.NextDouble()), Vector3.forward);
                go.transform.SetParent(compiled.transform);
            }

            var damage = gameObject.GetOrAddComponent<DamageHero>();
            damage.hazardType = 2;
            damage.shadowDashHazard = true;
        }
#endif
    }

}
