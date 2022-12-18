using UnityEngine;

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
        // FIXME
    }
#endif
}
