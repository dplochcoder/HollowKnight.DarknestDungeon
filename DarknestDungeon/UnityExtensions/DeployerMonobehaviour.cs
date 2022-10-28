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
            ModifyDeployment(deployer.Deploy());
            Destroy(gameObject);
        }

        protected virtual void ModifyDeployer(ref T deployer) { }

        protected virtual void ModifyDeployment(GameObject obj) { }
    }
}
