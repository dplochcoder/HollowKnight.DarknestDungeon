using IL;
using Newtonsoft.Json.Linq;
using System.Reflection;
using UnityEngine;

namespace DarknestDungeon.IC
{
    public class VoidCloakBehaviour : MonoBehaviour
    {
        private enum State
        {
            Idle,
            DashHeld,
            DashReleased,
            VoidEngaged
        }

        private static readonly FieldInfo inputHandlerField = typeof(HeroController).GetField("inputHandler", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo rb2dField = typeof(HeroController).GetField("rb2d", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo dashTimerField = typeof(HeroController).GetField("dash_timer", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo origDashVectorMethod = typeof(HeroController).GetMethod("OrigDashVector", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo finishedDashingMethod = typeof(HeroController).GetMethod("FinishedDashing", BindingFlags.NonPublic | BindingFlags.Instance);

        private const float VOID_DASH_LIMIT = 0.6f;
        private const float DEGREES_ROTATION_PER_SEC = 270 / VOID_DASH_LIMIT;

        // Must be set before Start().
        public VoidCloakModule Vcm;

        private HeroController hc;
        private InputHandler ih;
        private Rigidbody2D rb2d;
        private HeroControllerStates hcs;

        private State state = State.Idle;
        private Vector2 velocity = Vector2.zero;
        private float voidDashTimer = 0.0f;

        public void Start()
        {
            hc = gameObject.GetComponent<HeroController>();
            ih = (InputHandler)inputHandlerField.GetValue(hc);
            rb2d = (Rigidbody2D)rb2dField.GetValue(hc);
            hcs = hc.cState;
        }

        private float dash_timer
        {
            get { return (float)dashTimerField.GetValue(hc); }
            set { dashTimerField.SetValue(hc, value); }
        }

        private static readonly object[] emptyArr = new object[0];

        private Vector2 OrigDashVector() => (Vector2) origDashVectorMethod.Invoke(hc, emptyArr);

        private void FinishedDashing() => finishedDashingMethod.Invoke(hc, emptyArr);

        public void Update()
        {
            if (!Vcm.HasVoidCloak) return;

            switch (state)
            {
                case State.Idle:
                    if (hcs.shadowDashing)
                    {
                        state = ih.inputActions.dash.IsPressed ? State.DashHeld : State.DashReleased;
                    }
                    break;
                case State.DashHeld:
                    if (!hcs.shadowDashing)
                    {
                        state = State.Idle;
                    }
                    else if (!ih.inputActions.dash.IsPressed)
                    {
                        state = State.DashReleased;
                    }
                    else if (dash_timer > hc.DASH_TIME)
                    {
                        state = State.VoidEngaged;
                        StartVoidDash();
                    }
                    break;
                case State.DashReleased:
                    if (!hcs.shadowDashing)
                    {
                        state = State.Idle;
                    }
                    break;
                case State.VoidEngaged:
                    if (!ih.inputActions.dash.IsPressed || voidDashTimer > VOID_DASH_LIMIT)
                    {
                        FinishedVoidDashing();
                        state = State.Idle;
                        break;
                    }

                    VoidDash();
                    break;
            }
        }

        private void StartVoidDash()
        {
            dash_timer = 0;
            voidDashTimer = Time.deltaTime;
            velocity = OrigDashVector();
            // TODO: Particle effects
        }

        private Vector2 GetTargetDir()
        {
            int horz = (ih.inputActions.left ? -1 : 0) + (ih.inputActions.right ? 1 : 0);
            int vert = (ih.inputActions.up ? 1 : 0) + (ih.inputActions.down ? -1 : 0);
            return (horz == 0 && vert == 0) ? velocity.normalized : new Vector2(horz, vert).normalized;
        }

        private void SetDashVelocity(Vector2 v)
        {
            Vcm.DashVelocityOverride = v;
            rb2d.velocity = v;
        }

        private void VoidDash()
        {
            dash_timer = 0;
            voidDashTimer += Time.deltaTime;

            var targetVelocity = GetTargetDir() * velocity.magnitude;
            var thetaDelta = Vector2.SignedAngle(velocity, targetVelocity);

            var absDelta = Mathf.Abs(thetaDelta);
            var range = DEGREES_ROTATION_PER_SEC * Time.deltaTime;
            if (range >= absDelta)
            {
                SetDashVelocity(targetVelocity);
            }
            else
            {
                SetDashVelocity(Quaternion.Euler(0, 0, range * Mathf.Sign(thetaDelta)) * velocity);
            }
        }

        private void FinishedVoidDashing()
        {
            Vcm.DashVelocityOverride = null;
            FinishedDashing();
            // TODO: Airborne velocity
        }
    }
}
