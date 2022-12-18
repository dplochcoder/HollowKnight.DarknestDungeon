using UnityEngine;

#if UNITY_EDITOR
[ExecuteAlways]
#endif
public class OptimizerFinder : MonoBehaviour
{
#if UNITY_EDITOR
    private void Awake()
    {
        foreach (var sdo in FindObjectsOfType<SceneDataOptimizer>()) sdo.OptimizeScene();
        foreach (var hrt in FindObjectsOfType<HazardRespawnTrigger>()) FixHRT(hrt);

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    private void FixHRT(HazardRespawnTrigger hrt)
    {
        var bc = hrt.gameObject.GetComponent<BoxCollider2D>();
        if (bc != null) bc.isTrigger = true;

        if (hrt.respawnMarker == null)
        {
            bool isFixed = false;
            foreach (Transform child in hrt.gameObject.transform)
            {
                var hrm = child.gameObject.GetComponent<HazardRespawnMarker>();
                if (hrm != null)
                {
                    hrt.respawnMarker = hrm;
                    isFixed = true;
                    break;
                }
            }

            if (!isFixed)
            {
                Debug.LogError($"{hrt.name} is missing its HazardRespawnMarker");
            }
        }
    }
#endif
}
