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
            VoidDashing,
            VoidEarlyRelease
        }

        private static readonly FieldInfo inputHandlerField = typeof(HeroController).GetField("inputHandler", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo rb2dField = typeof(HeroController).GetField("rb2d", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo dashTimerField = typeof(HeroController).GetField("dash_timer", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo origDashVectorMethod = typeof(HeroController).GetMethod("OrigDashVector", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo finishedDashingMethod = typeof(HeroController).GetMethod("FinishedDashing", BindingFlags.NonPublic | BindingFlags.Instance);

        private /*const*/ static float VOID_DASH_TIME = 0.7f;
        private /*const*/ static float FULL_REVERSAL_PERIOD = 0.125f;

        // Must be set before Start().
        public VoidCloakModule Vcm;

        private HeroController hc;
        private InputHandler ih;
        private Rigidbody2D rb2d;
        private HeroControllerStates hcs;

        private State state = State.Idle;
        private Vector2 velocity;
        private float origMagnitude;
        private float voidDashTimer;

        public void Start()
        {
            hc = gameObject.GetComponent<HeroController>();
            ih = (InputHandler)inputHandlerField.GetValue(hc);
            rb2d = (Rigidbody2D)rb2dField.GetValue(hc);
            hcs = hc.cState;

            Vcm.OnTransition += OnSceneTransition;
        }

        private void OnSceneTransition()
        {
            if (state == State.VoidDashing) FinishedVoidDashing();
        }

        public void OnDestroy()
        {
            Vcm.OnTransition -= OnSceneTransition;
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
                        StartVoidDash();
                    }
                    break;
                case State.VoidDashing:
                    if (!ih.inputActions.dash.IsPressed && voidDashTimer <= hc.DASH_TIME)
                    {
                        state = State.VoidEarlyRelease;
                        dash_timer = voidDashTimer;
                    }
                    else if (!ih.inputActions.dash.IsPressed || voidDashTimer > VOID_DASH_TIME)
                    {
                        FinishedVoidDashing();
                    }
                    else
                    {
                        VoidDashUpdate();
                    }
                    break;
                case State.VoidEarlyRelease:
                    if (!hcs.shadowDashing)
                    {
                        FinishedVoidDashing();
                    }
                    break;
            }
        }

        private void StartVoidDash()
        {
            state = State.VoidDashing;
            dash_timer = 0;
            voidDashTimer = Time.deltaTime;

            velocity = OrigDashVector();
            origMagnitude = velocity.magnitude;
            SetDashVelocity(GetTargetDir() * velocity.magnitude);

            // TODO: Particle effects
        }

        private Vector2 GetTargetDir()
        {
            int horz = (ih.inputActions.left ? -1 : 0) + (ih.inputActions.right ? 1 : 0);
            int vert = (ih.inputActions.up ? 1 : 0) + (ih.inputActions.down ? -1 : 0);
            return ((horz != 0 || vert != 0) ? new Vector2(horz, vert) : (velocity.magnitude > 0 ? velocity : OrigDashVector())).normalized;
        }

        private void SetDashVelocity(Vector2 v)
        {
            velocity = v;
            Vcm.DashVelocityOverride = v;
            rb2d.velocity = v;
        }

        private void VoidDashUpdate()
        {
            dash_timer = 0;
            voidDashTimer += Time.deltaTime;

            var targetVelocity = GetTargetDir() * origMagnitude;
            var diff = targetVelocity - velocity;
            var acc = diff.normalized * 2 * origMagnitude * Time.deltaTime / FULL_REVERSAL_PERIOD;
            SetDashVelocity(acc.magnitude > diff.magnitude ? targetVelocity : velocity + acc);
        }

        private void FinishedVoidDashing()
        {
            Vcm.DashVelocityOverride = null;
            FinishedDashing();
            state = State.Idle;

            // TODO: Airborne velocity
        }
    }
}
