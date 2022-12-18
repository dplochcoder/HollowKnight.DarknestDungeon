using UnityEngine;
using System.Collections;

namespace DarknestDungeon.Scripts
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class AbyssTendrilsCompiler : SceneDataOptimizer
    {
        public override void OptimizeScene()
        {
#if UNITY_EDITOR
            CompileTendrils();
#endif
        }

#if UNITY_EDITOR
        [ContextMenu("Compile Tendrils")]
        void CompileTendrils()
        {
            // FIXME
        }
#endif
    }
}
