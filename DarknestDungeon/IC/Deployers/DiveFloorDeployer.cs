using UnityEngine;

namespace DarknestDungeon.IC.Deployers
{
    public record DiveFloorDeployer : PersistentBoolDeployer
    {
        protected override GameObject Template() => Preloader.Instance.DiveFloor;

        protected override bool SemiPersistent() => false;
    }
}
