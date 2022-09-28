using HutongGames.PlayMaker;
using IL;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using Newtonsoft.Json.Linq;
using On;
using System;
using System.EnterpriseServices;
using System.Reflection;
using UnityEngine;

namespace DarknestDungeon.IC
{
    public class VoidCloakBehaviour : MonoBehaviour
    {
        private enum VoidCloakState
        {
            Idle,
            VoidDashing,
            VoidEarlyRelease
        }

        private enum ShadowRechargeAnimState
        {
            Idle,
            AwaitingPause,
            AwaitingUnpause
        }

        private static readonly FieldInfo inputHandlerField = typeof(HeroController).GetField("inputHandler", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo rb2dField = typeof(HeroController).GetField("rb2d", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo dashTimerField = typeof(HeroController).GetField("dash_timer", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo shadowDashTimerField = typeof(HeroController).GetField("shadowDashTimer", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo doubleJumpedField = typeof(HeroController).GetField("doubleJumped", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo origDashVectorMethod = typeof(HeroController).GetMethod("OrigDashVector", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo finishedDashingMethod = typeof(HeroController).GetMethod("FinishedDashing", BindingFlags.NonPublic | BindingFlags.Instance);

        private static float VOID_DASH_EXTENSION_RATIO = 2.1f;
        private static float VOID_DASH_LIMIT = 0.7f;
        private static float FULL_REVERSAL_PERIOD = 0.1f;
        private static float DASH_VELOCITY = 20.0f;
        private static float SHARP_SHADOW_VELOCITY = 28.0f;
        private static float SHARP_SHADOW_TAPER_VELOCITY = 22.0f;

        // Must be set before Start().
        public VoidCloakModule Vcm;

        private HeroController hc;
        private InputHandler ih;
        private Rigidbody2D rb2d;
        private HeroControllerStates hcs;

        private VoidCloakState voidCloakState = VoidCloakState.Idle;
        private Vector2 velocity;
        private float voidDashTimer;

        private ShadowRechargeAnimState shadowRechargeAnimState = ShadowRechargeAnimState.Idle;
        private tk2dSpriteAnimator shadowRecharge;
        private float shadowRechargePauseTime;

        public void Start()
        {
            hc = gameObject.GetComponent<HeroController>();
            ih = (InputHandler)inputHandlerField.GetValue(hc);
            rb2d = (Rigidbody2D)rb2dField.GetValue(hc);
            hcs = hc.cState;

            shadowRecharge = gameObject.FindChild("Effects").FindChild("Shadow Recharge").GetComponent<tk2dSpriteAnimator>();

            Vcm.OnTransition += OnSceneTransition;
        }

        private void OnSceneTransition()
        {
            if (voidCloakState == VoidCloakState.VoidDashing) FinishedVoidDashing();
        }

        public void OnDestroy()
        {
            Vcm.OnTransition -= OnSceneTransition;
        }

        private bool SharpShadowEquipped => PlayerData.instance.GetBool("equippedCharm_16");

        private float dash_timer
        {
            get { return (float)dashTimerField.GetValue(hc); }
            set { dashTimerField.SetValue(hc, value); }
        }

        private float shadowDashTimer
        {
            get { return (float)shadowDashTimerField.GetValue(hc); }
            set { shadowDashTimerField.SetValue(hc, value); }
        }

        private bool doubleJumped
        {
            get { return (bool)doubleJumpedField.GetValue(hc); }
            set { doubleJumpedField.SetValue(hc, value);  }
        }

        private static readonly object[] emptyArr = new object[0];

        private Vector2 OrigDashVector() => (Vector2) origDashVectorMethod.Invoke(hc, emptyArr);

        private void FinishedDashing() => finishedDashingMethod.Invoke(hc, emptyArr);

        public void Update()
        {
            if (!Vcm.HasVoidCloak) return;

            UpdateVoidCloak();
            UpdateShadowRechargeAnim();
        }

        private void UpdateVoidCloak()
        {
            switch (voidCloakState)
            {
                case VoidCloakState.Idle:
                    if (hcs.shadowDashing)
                    {
                        StartVoidDash();
                    }
                    break;
                case VoidCloakState.VoidDashing:
                    if (!ih.inputActions.dash.IsPressed && voidDashTimer <= hc.DASH_TIME)
                    {
                        voidCloakState = VoidCloakState.VoidEarlyRelease;
                        dash_timer = voidDashTimer;
                    }
                    else if (!ih.inputActions.dash.IsPressed || voidDashTimer > VOID_DASH_LIMIT)
                    {
                        FinishedVoidDashing();
                    }
                    else
                    {
                        VoidDashUpdate();
                    }
                    break;
                case VoidCloakState.VoidEarlyRelease:
                    if (!hcs.shadowDashing)
                    {
                        FinishedVoidDashing();
                    }
                    break;
            }
        }

        private void UpdateShadowRechargeAnim()
        {
            switch (shadowRechargeAnimState)
            {
                case ShadowRechargeAnimState.Idle:
                    break;
                case ShadowRechargeAnimState.AwaitingPause:
                    if (shadowRecharge.Playing)
                    {
                        shadowRechargeAnimState = ShadowRechargeAnimState.AwaitingUnpause;
                        shadowRecharge.Pause();
                    }
                    break;
                case ShadowRechargeAnimState.AwaitingUnpause:
                    if (shadowRechargePauseTime <= 0f)
                    {
                        shadowRechargeAnimState = ShadowRechargeAnimState.Idle;
                        shadowRecharge.Resume();
                        shadowRechargePauseTime = 0f;
                    }
                    else
                    {
                        shadowRechargePauseTime -= Time.deltaTime;
                    }
                    break;
            }
        }

        private void StartVoidDash()
        {
            voidCloakState = VoidCloakState.VoidDashing;
            dash_timer = 0;
            voidDashTimer = Time.deltaTime;

            velocity = OrigDashVector();
            SetDashVelocity(GetTargetDir() * GetTargetSpeed());

            // TODO: Particle effects
        }

        private Vector2 GetTargetDir()
        {
            int horz = (ih.inputActions.left ? -1 : 0) + (ih.inputActions.right ? 1 : 0);
            int vert = (ih.inputActions.up ? 1 : 0) + (ih.inputActions.down ? -1 : 0);
            return ((horz != 0 || vert != 0) ? new Vector2(horz, vert) : (velocity.magnitude > 0 ? velocity : OrigDashVector())).normalized;
        }

        private float GetTargetSpeed() => SharpShadowEquipped ? (voidDashTimer > hc.DASH_TIME ? SHARP_SHADOW_TAPER_VELOCITY : SHARP_SHADOW_VELOCITY) : DASH_VELOCITY;

        private void SetDashVelocity(Vector2 v)
        {
            velocity = v;
            Vcm.DashVelocityOverride = v;
            rb2d.velocity = v;
        }

        private void IncreaseShadowTime(float delta)
        {
            shadowRechargePauseTime += VOID_DASH_EXTENSION_RATIO * delta;
            shadowDashTimer += VOID_DASH_EXTENSION_RATIO * delta;
        }

        private void VoidDashUpdate()
        {
            dash_timer = 0;
            bool preVoid = voidDashTimer > hc.DASH_TIME;
            voidDashTimer += Time.deltaTime;
            bool postVoid = voidDashTimer > hc.DASH_TIME;

            if (postVoid)
            {
                if (!preVoid)
                {
                    if (doubleJumped) doubleJumped = false;

                    shadowRechargeAnimState = ShadowRechargeAnimState.AwaitingPause;
                    shadowRechargePauseTime = 0;
                    IncreaseShadowTime(voidDashTimer - hc.DASH_TIME);
                }
                else
                {
                    IncreaseShadowTime(Math.Min(Time.deltaTime, VOID_DASH_LIMIT - (voidDashTimer - Time.deltaTime)));
                }
            }

            var targetVelocity = GetTargetDir() * GetTargetSpeed();
            var diff = targetVelocity - velocity;
            var acc = diff.normalized * 2 * GetTargetSpeed() * Time.deltaTime / FULL_REVERSAL_PERIOD;
            SetDashVelocity(acc.magnitude > diff.magnitude ? targetVelocity : velocity + acc);
        }

        private void FinishedVoidDashing()
        {
            Vcm.DashVelocityOverride = null;
            FinishedDashing();
            voidCloakState = VoidCloakState.Idle;

            // TODO: Airborne velocity
        }
    }
}
