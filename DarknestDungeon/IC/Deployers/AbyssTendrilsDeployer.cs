using ItemChanger;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DarknestDungeon.IC.Deployers
{
    public record AbyssTendrilsDeployer : Deployer
    {
        public override GameObject Instantiate() => Object.Instantiate(Preloader.Instance.AbyssTendrils);
    }
}
