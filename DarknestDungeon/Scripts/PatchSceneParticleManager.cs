using GlobalEnums;
using SFCore.Utils;
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

        private void Awake()
        {
            var scp = FindObjectOfType<SceneParticlesController>();
            scp.DisableParticles();

            // TODO: Create void particle fields
            scp.SetAttr("sceneParticleZoneType", type switch
            {
                Type.ABYSS => MapZone.ABYSS_DEEP,
                Type.VOID => MapZone.ABYSS_DEEP,
                Type.DEEP_VOID => MapZone.ABYSS_DEEP,
                _ => MapZone.ABYSS_DEEP
            });

            Destroy(this);
        }
    }
}
