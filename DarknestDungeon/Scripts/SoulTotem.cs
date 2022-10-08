using DarknestDungeon.UnityExtensions;
using ItemChanger.Deployers;
using UnityEngine;

namespace DarknestDungeon.Scripts
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SoulTotem : DeployerMonobehaviour<SoulTotemDeployer>
    {
        public enum SoulTotemSubtype
        {
            A,
            B,
            C,
            D,
            E,
            F,
            G,
            Palace,
            PathOfPain
        }

        public SoulTotemSubtype TotemType;

        protected override void ModifyDeployer(ref SoulTotemDeployer deployer) => deployer = deployer with
        {
            SoulTotemSubtype = (ItemChanger.SoulTotemSubtype)TotemType
        };

        protected override void ModifyDeployment(GameObject obj)
        {
            obj.transform.SetPositionZ(0.01f);
            obj.GetComponent<SpriteRenderer>().flipX = gameObject.GetComponent<SpriteRenderer>().flipX;
        }
    }
}
