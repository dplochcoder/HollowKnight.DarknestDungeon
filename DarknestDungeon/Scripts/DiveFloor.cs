using DarknestDungeon.IC.Deployers;
using DarknestDungeon.UnityExtensions;

namespace DarknestDungeon.Scripts
{
    public class DiveFloor : DeployerMonobehaviour<DiveFloorDeployer>
    {
        public string id;

        public override void ModifyDeployer(ref DiveFloorDeployer deployer) => deployer.id = id;
    }
}
