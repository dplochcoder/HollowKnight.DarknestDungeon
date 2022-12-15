using UnityEngine;

namespace DarknestDungeon.Scripts
{
    public class PatchSceneParticleManager : MonoBehaviour
    {
        public enum Type
        {
            ABYSS,
            VOID,
            DEEP_VOID
        }
        public Type type;
    }
}
