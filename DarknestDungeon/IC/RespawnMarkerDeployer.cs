using ItemChanger;
using UnityEngine;

namespace DarknestDungeon.IC
{
    public record RespawnMarkerDeployer : Deployer
    {
        public string name;

        public override GameObject Instantiate()
        {
            GameObject obj = new(name);
            obj.tag = "RespawnPoint";
            obj.AddComponent<RespawnMarker>();
            return obj;
        }

        public override GameObject Deploy()
        {
            // Keep our unique name
            var obj = base.Deploy();
            obj.name = name;
            return obj;
        }
    }
}
