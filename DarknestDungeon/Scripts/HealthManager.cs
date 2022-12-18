using DarknestDungeon.UnityExtensions;

namespace DarknestDungeon.Scripts
{
    public class HealthManager : global::HealthManager
    {
        private static MonobehaviourPatcher<global::HealthManager> Patcher = new(
            Preloader.Instance.CrystalCrawler.GetComponent<global::HealthManager>(),
            "audioPlayerPrefab",
            "regularInvincibleAudio",
            "blockHitPrefab",
            "strikeNailPrefab",
            "slashImpactPrefab",
            "fireballHitPrefab",
            "sharpShadowImpactPrefab",
            "corpseSplatPrefab",
            "enemyDeathSwordAudio",
            "enemyDamageAudio",
            "smallGeoPrefab",
            "mediumGeoPrefab",
            "largeGeoPrefab");

        private new void Awake()
        {
            base.Awake();
            Patcher.Patch(this);
        }
    }
}
