using DarknestDungeon.IC;
using UnityEngine;

namespace DarknestDungeon.Scripts
{
    public class PatchDiveFloor : MonoBehaviour
    {
        public string id;

        public void Awake()
        {
            DiveFloorDeployer deployer = new()
            {
                SceneName = gameObject.scene.name,
                X = gameObject.transform.position.x,
                Y = gameObject.transform.position.y,
                id = id
            };
            deployer.Deploy();
            GameObject.Destroy(this);
        }
    }
}
