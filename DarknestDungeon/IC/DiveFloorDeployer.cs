using ItemChanger;
using UnityEngine;

namespace DarknestDungeon.IC
{
    public record DiveFloorDeployer : Deployer
    {
        public string id;

        public override GameObject Instantiate()
        {
            var obj = Object.Instantiate(Preloader.Instance.DiveFloor);
            var pbd = obj.GetComponent<PersistentBoolItem>().persistentBoolData;
            pbd.sceneName = SceneName;
            pbd.id = id;
            return obj;
        }
    }
}
