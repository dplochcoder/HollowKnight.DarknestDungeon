using ItemChanger;
using UnityEngine;

namespace DarknestDungeon.UnityExtensions
{
    public abstract class DeployerMonobehaviour<T> : MonoBehaviour where T : Deployer, new()
    {
        private void Awake()
        {
            T deployer = new()
            {
                SceneName = gameObject.scene.name,
                X = gameObject.transform.position.x,
                Y = gameObject.transform.position.y
            };
            ModifyDeployer(ref deployer);

            var newObj = deployer.Deploy();
            var oScale = newObj.transform.localScale;
            var mScale = gameObject.transform.localScale;
            newObj.transform.localScale = new(oScale.x * mScale.x, oScale.y * mScale.y, oScale.z * mScale.z);
            newObj.transform.SetPositionZ(gameObject.transform.GetPositionZ());
            ModifyDeployment(newObj);

            Destroy(gameObject);
        }

        protected virtual void ModifyDeployer(ref T deployer) { }

        protected virtual void ModifyDeployment(GameObject obj) { }
    }
}
