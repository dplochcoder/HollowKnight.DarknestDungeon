using IL;
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
        private static readonly FieldInfo dashTimerField = typeof(HeroController).GetField("dash_timer", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo origDashVectorMethod = typeof(HeroController).GetMethod("OrigDashVector", BindingFlags.NonPublic | BindingFlags.Instance);

        public VoidCloakModule Vcm;
        private HeroController hc;
        private InputHandler ih;
        private HeroControllerStates hcs;

        private State state = State.Idle;
        private Vector2 velocity = Vector2.zero;
        private float voidDashTimer = 0.0f;
        private const float VOID_DASH_LIMIT = 0.6f;

        public void Start()
        {
            hc = gameObject.GetComponent<HeroController>();
            hcs = hc.cState;
            ih = (InputHandler) inputHandlerField.GetValue(hc);
        }

        private float dash_timer
        {
            get { return (float)dashTimerField.GetValue(hc); }
            set { dashTimerField.SetValue(hc, value); }
        }

        private Vector2 OrigDashVector() => (Vector2) origDashVectorMethod.Invoke(hc, new object[0]);

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
                        dash_timer = 0;
                        state = State.VoidEngaged;
                    }
                    break;
                case State.DashReleased:
                    if (!hcs.shadowDashing)
                    {
                        state = State.Idle;
                    }
                    break;
                case State.VoidEngaged:
                    if (voidDashTimer > VOID_DASH_LIMIT)
                    {
                        FinishVoidDashing();
                        state = State.Idle;
                    }

                    VoidDash();
                    break;
            }
        }

        private void VoidDash()
        {
            // FIXME
            voidDashTimer += Time.fixedDeltaTime;
        }

        private void FinishVoidDashing()
        {
            // FIXME: Airborne velocity
        }
    }
}
