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
            newObj.transform.localScale = gameObject.transform.localScale;
            newObj.transform.SetPositionZ(gameObject.transform.GetPositionZ());
            ModifyDeployment(newObj);

            Destroy(gameObject);
        }

        protected virtual void ModifyDeployer(ref T deployer) { }

        protected virtual void ModifyDeployment(GameObject obj) { }
    }
}
