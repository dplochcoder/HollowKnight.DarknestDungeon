using System.Collections.Generic;
using UnityEngine;

namespace DarknestDungeon.Scripts
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class FlameSeaCompiler : SceneDataOptimizer
    {
        public Sprite flame;

        public override void OptimizeScene()
        {
#if UNITY_EDITOR
            CompileFlameSea();
#endif
        }

        private static Vector2 Snap(Vector2 p) => new Vector2(Snap(p.x), Snap(p.y));

        private static float Snap(float f) => Mathf.Round(f * 2) / 2.0f;

#if UNITY_EDITOR
        [ContextMenu("Compile Flame Sea")]
        void CompileFlameSea()
        {
            gameObject.layer = 11;  // Enemies
            var bc2d = GetComponent<BoxCollider2D>();
            bc2d.offset = new Vector2(0, 0);
            bc2d.size = GOUtils.Snap(bc2d.size, 2.0f);

            var sdiff = GOUtils.SnapAndGetDiff(transform, 1.0f);
            GOUtils.DeleteChildren(gameObject);

            var compiled = new GameObject("Compiled");
            compiled.transform.SetParent(transform);
            compiled.transform.position = transform.position;

            int index = 1;
            for (int x = 0; x < bc2d.size.x; x += 2)
                for (int y = 0; y < bc2d.size.y; y += 2)
                {
                    var go = new GameObject($"Flame {index++}");
                    go.AddComponent<SpriteRenderer>().sprite = flame;
                    go.transform.SetParent(compiled.transform);
                    go.transform.localPosition = new Vector3(x + 1 - bc2d.size.x / 2, y + 1 - bc2d.size.y / 2);
                    go.transform.localScale = new Vector3(1.5f, 1.5f, 1);
                }

            var damage = gameObject.GetOrAddComponent<DamageHero>();
            damage.hazardType = 2;
            damage.shadowDashHazard = true;
        }
#endif
    }

}
