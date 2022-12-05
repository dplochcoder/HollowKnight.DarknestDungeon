using GlobalEnums;
using HutongGames.PlayMaker;
using ItemChanger;
using Modding;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace DarknestDungeon.IC.Deployers
{
    public class BloodGateBehaviour : MonoBehaviour
    {
        private GameObject knight;
        private HeroController hc;
        public Vector2 origPos;

        private float leftX => origPos.x - 0.25f;
        private float rightX => origPos.x + 0.25f;
        private float botY => origPos.y - 2.75f;
        private float topY => origPos.y + 2.75f;

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

            var pos = gameObject.transform.position;
            gameObject.transform.position = new(pos.x + 0.75f, pos.y - 2.29f, 2.51f);
        }

        private void Start() => gameObject.transform.SetPositionZ(2.51f);

        private Loc prevPrevLoc = Loc.Unknown;
        private Loc prevLoc = Loc.Unknown;

        private MethodInfo dieMethod = typeof(HeroController).GetMethod("Die", BindingFlags.NonPublic | BindingFlags.Instance);
        private MethodInfo onTakenDamageMethod = typeof(HeroController).GetMethod("OnTakenDamage", BindingFlags.NonPublic | BindingFlags.Instance);

        private void Update()
        {
            Loc newLoc = GetHeroLoc();
            if (newLoc != prevLoc)
            {
                if (IsLeftOrRight(newLoc) && ((prevLoc == Loc.Mid && IsLeftOrRight(prevPrevLoc) && newLoc != prevPrevLoc)
                    || IsLeftOrRight(prevLoc)))
                {
                    // Force damage to be taken, without any recoil.
                    On.HeroController.CanTakeDamage += OverrideCanTakeDamage;
                    ModHooks.AfterTakeDamageHook += OverrideAfterTakeDamage;
                    On.HeroController.DieFromHazard += OverrideDieFromHazard;

                    hc.TakeDamage(gameObject, CollisionSide.other, 1, 5);

                    On.HeroController.CanTakeDamage -= OverrideCanTakeDamage;
                    ModHooks.AfterTakeDamageHook -= OverrideAfterTakeDamage;
                    On.HeroController.DieFromHazard -= OverrideDieFromHazard;
                }

                prevPrevLoc = prevLoc;
                prevLoc = newLoc;
            }
        }

        private bool OverrideCanTakeDamage(On.HeroController.orig_CanTakeDamage orig, HeroController self) => true;

        private int OverrideAfterTakeDamage(int hazardType, int damage) => 1;

        private IEnumerator OverrideDieFromHazard(On.HeroController.orig_DieFromHazard orig, HeroController self, HazardType type, float angle)
        {
            yield break;
        }
    }

    public record BloodGateDeployer : Deployer
    {
        public override GameObject Instantiate() {
            var obj = Object.Instantiate(Preloader.Instance.ShadowGate);
            var bgb = obj.AddComponent<BloodGateBehaviour>();
            bgb.origPos = new(X, Y);
            return obj;
        }
    }
}
