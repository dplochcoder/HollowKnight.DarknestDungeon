using ItemChanger;
using UnityEngine;

namespace DarknestDungeon.UnityExtensions
{
    public abstract class DeployerMonobehaviour<T> : MonoBehaviour where T : Deployer, new()
    {
        public void Awake()
        {
            T deployer = new()
            {
                SceneName = gameObject.scene.name,
                X = gameObject.transform.position.x,
                Y = gameObject.transform.position.y
            };
            ModifyDeployer(ref deployer);
            deployer.Deploy();
            Destroy(gameObject);
        }

        public abstract void ModifyDeployer(ref T deployer);
    }
}
