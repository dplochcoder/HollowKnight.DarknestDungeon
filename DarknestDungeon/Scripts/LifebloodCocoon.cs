using DarknestDungeon.IC.Deployers;
using DarknestDungeon.UnityExtensions;
using UnityEngine;

namespace DarknestDungeon.Scripts
{
    public class LifebloodCocoon : DeployerMonobehaviour<LifebloodCocoonDeployer>
    {
        [Range(2, 3)]
        public int lifeblood = 2;

        public string id;

        protected override void ModifyDeployer(ref LifebloodCocoonDeployer deployer)
        {
            deployer.id = id;
            deployer.lifebloodType = lifeblood == 2 ? LifebloodCocoonDeployer.LifebloodType.TWO_LIFEBLOOD : LifebloodCocoonDeployer.LifebloodType.THREE_LIFEBLOOD;
        }

        protected override void ModifyDeployment(GameObject obj) => obj.transform.SetPositionY(obj.transform.GetPositionY() - 1.5f);
    }
}
