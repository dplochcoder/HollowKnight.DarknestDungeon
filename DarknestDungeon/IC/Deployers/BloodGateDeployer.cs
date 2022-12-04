using ItemChanger;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace DarknestDungeon.IC.Deployers
{
    public class BloodGateBehaviour : MonoBehaviour
    {
        private GameObject knight;
        private HeroController hc;

        private float leftX => gameObject.transform.position.x - 0.55f;
        private float rightX => gameObject.transform.position.x + 0.55f;
        private float botY => gameObject.transform.position.y - 2.75f;
        private float topY => gameObject.transform.position.y + 2.75f;

        private enum Loc
        {
            Left,
            Right,
            Mid,
            Unknown
        }

        private bool IsLeftOrRight(Loc loc) => loc == Loc.Left || loc == Loc.Right;

        private Loc GetHeroLoc()
        {
            float x = knight.transform.position.x;
            float y = knight.transform.position.y;
            if (y > topY || y < botY) return Loc.Unknown;
            if (x < leftX) return Loc.Left;
            if (x > rightX) return Loc.Right;
            return Loc.Mid;
        }

        private void Awake()
        {
            knight = GameObject.Find("Knight");
            hc = knight.GetComponent<HeroController>();
        }

        private Loc prevPrevLoc = Loc.Unknown;
        private Loc prevLoc = Loc.Unknown;

        private MethodInfo die = typeof(HeroController).GetMethod("Die", BindingFlags.NonPublic | BindingFlags.Instance);

        private void Update()
        {
            Loc newLoc = GetHeroLoc();
            if (newLoc != prevLoc)
            {
                if (prevLoc == Loc.Mid && IsLeftOrRight(prevPrevLoc) && newLoc != prevPrevLoc)
                {
                    // Take health from player. TODO: Play sound.
                    PlayerData.instance.TakeHealth(PlayerData.instance.GetBool("overcharmed") ? 2 : 1);
                    if (PlayerData.instance.GetInt("health") <= 0)
                    {
                        StartCoroutine((IEnumerator)die.Invoke(hc, new object[] { }));
                    }
                }

                prevPrevLoc = prevLoc;
                prevLoc = newLoc;
            }
        }
    }

    public record BloodGateDeployer : Deployer
    {
        public override GameObject Instantiate() {
            var obj = Object.Instantiate(Preloader.Instance.AbyssTendrils);
            obj.AddComponent<BloodGateBehaviour>();
            return obj;
        }
    }
}
