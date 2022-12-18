using UnityEngine;

namespace DarknestDungeon.Scripts
{
    [RequireComponent(typeof(Collider2D))]
    public class SpikeCompiler : SceneDataOptimizer
    {
        public override void OptimizeScene()
        {
#if UNITY_EDITOR
            CompileSpikes();
#endif
        }

#if UNITY_EDITOR
        [ContextMenu("Compile Tilemap")]
        void CompileSpikes()
        {
            var previous = transform.Find("Compiled");
            if (previous != null) DestroyImmediate(previous, true);

            var compiled = new GameObject("Compiled");
            compiled.transform.SetParent(transform);

            var c2d = GetComponent<Collider2D>();
            var bounds = c2d.bounds;

            // Average 4 spikes per unit square
            // FIXME
        }
#endif
    }

}
