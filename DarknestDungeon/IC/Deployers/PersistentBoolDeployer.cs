using ItemChanger;
using System.Security.Permissions;
using UnityEngine;

namespace DarknestDungeon.IC.Deployers
{
    public abstract record PersistentBoolDeployer : Deployer
    {
        public string id;
        public bool? semiPersistent;

        protected abstract GameObject Template();

        protected virtual bool SemiPersistent() => true;

        public override GameObject Instantiate()
        {
            bool isSemiPersistent = semiPersistent ?? SemiPersistent();

            var obj = Object.Instantiate(Template());
            var pbi = obj.GetComponent<PersistentBoolItem>();
            pbi.semiPersistent = isSemiPersistent;
            var pbd = obj.GetComponent<PersistentBoolItem>().persistentBoolData;
            pbd.sceneName = SceneName;
            pbd.id = id;
            pbd.semiPersistent = isSemiPersistent;
            return obj;
        }
    }
}
