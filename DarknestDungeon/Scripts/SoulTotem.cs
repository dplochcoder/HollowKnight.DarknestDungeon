using DarknestDungeon.UnityExtensions;
using ItemChanger.Deployers;
using UnityEngine;

namespace DarknestDungeon.Scripts
{
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

        public override void ModifyDeployer(ref SoulTotemDeployer deployer) => deployer = deployer with
        {
            SoulTotemSubtype = (ItemChanger.SoulTotemSubtype)TotemType
        };
    }
}
