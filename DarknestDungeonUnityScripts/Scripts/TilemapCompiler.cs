using UnityEngine.Tilemaps;
using UnityEngine;

namespace DarknestDungeon.Scripts
{
    [RequireComponent(typeof(Tilemap))]
    public class TilemapCompiler : MonoBehaviour
    {
        [ContextMenu("Compile Tilemap")]
        void CompileTilemap() {
            var oldMap = gameObject.GetComponent<Tilemap>();
            var width = oldMap.size.x;
            var height = oldMap.size.y;

            GameObject prevCompiled = gameObject.transform.Find("Compiled")?.gameObject;
            if (prevCompiled != null) DestroyImmediate(prevCompiled, true);

            GameObject compiled = new GameObject();
            compiled.name = "Compiled";
            compiled.transform.SetParent(gameObject.transform);
        }
    }
}
