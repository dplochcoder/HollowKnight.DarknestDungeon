using DarknestDungeon.IC;
using UnityEngine;

namespace DarknestDungeon.Scripts
{
    public class PatchPlayMakerManager : MonoBehaviour
    {
        public Transform managerTransform;

        public void Awake()
        {
            GameObject tmpPmu2D = Instantiate(Preloader.Instance.PlayMaker, managerTransform);
            tmpPmu2D.SetActive(true);
            tmpPmu2D.name = "PlayMaker Unity 2D";
        }
    }
}
