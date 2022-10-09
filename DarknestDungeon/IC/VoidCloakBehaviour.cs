using ItemChanger.Extensions;
using System;
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

        private enum JumpHoldState
        {
            Idle,
            HoldingJump
        }

        private static readonly FieldInfo inputHandlerField = typeof(HeroController).GetField("inputHandler", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo rb2dField = typeof(HeroController).GetField("rb2d", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo dashTimerField = typeof(HeroController).GetField("dash_timer", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo shadowDashTimerField = typeof(HeroController).GetField("shadowDashTimer", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo jumpStepsField = typeof(HeroController).GetField("jump_steps", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo jumpedStepsField = typeof(HeroController).GetField("jumped_steps", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo doubleJumpedField = typeof(HeroController).GetField("doubleJumped", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo origDashVectorMethod = typeof(HeroController).GetMethod("OrigDashVector", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo finishedDashingMethod = typeof(HeroController).GetMethod("FinishedDashing", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo jumpMethod = typeof(HeroController).GetMethod("Jump", BindingFlags.NonPublic | BindingFlags.Instance);

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
        private GameManager gm;

        private VoidCloakState voidCloakState = VoidCloakState.Idle;
        private Vector2 velocity;
        private float voidDashTimer;
        private bool wasAirborne;
        private bool escapedQoL;

        private ShadowRechargeAnimState shadowRechargeAnimState = ShadowRechargeAnimState.Idle;
        private tk2dSpriteAnimator shadowRecharge;
        private float shadowRechargePauseTime;

        private JumpHoldState jumpHoldState = JumpHoldState.Idle;
        private bool absorbingJumps = false;

        public void Start()
        {
            hc = gameObject.GetComponent<HeroController>();
            ih = (InputHandler)inputHandlerField.GetValue(hc);
            rb2d = (Rigidbody2D)rb2dField.GetValue(hc);
            hcs = hc.cState;
            gm = GameManager.instance;

            shadowRecharge = gameObject.FindChild("Effects").FindChild("Shadow Recharge").GetComponent<tk2dSpriteAnimator>();

            Vcm.OnTransition += OnSceneTransition;
            On.HeroController.FixedUpdate += OverrideFixedUpdate;
            On.HeroController.LookForInput += OverrideLookForInput;
            On.HeroController.JumpReleased += OverrideJumpReleased;
        }

        private void OnSceneTransition()
        {
            if (voidCloakState == VoidCloakState.VoidDashing) FinishedVoidDashing();
        }

        private void OverrideFixedUpdate(On.HeroController.orig_FixedUpdate orig, HeroController self)
        {
            orig(self);
            if (hc.hero_state == GlobalEnums.ActorStates.no_input && jumpHoldState == JumpHoldState.HoldingJump)
            {
                // Simulate jump.
                Jump();
            }
        }

        private void OverrideLookForInput(On.HeroController.orig_LookForInput orig, HeroController self)
        {
            absorbingJumps = true;
            orig(self);
            absorbingJumps = false;
        }

        private void OverrideJumpReleased(On.HeroController.orig_JumpReleased orig, HeroController self)
        {
            if (!absorbingJumps || jumpHoldState != JumpHoldState.HoldingJump)
            {
                orig(self);
            }
        }

        private void OnDestroy()
        {
            Vcm.OnTransition -= OnSceneTransition;
            On.HeroController.LookForInput -= OverrideLookForInput;
            On.HeroController.JumpReleased -= OverrideJumpReleased;
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

        private int jump_steps
        {
            get { return (int)jumpStepsField.GetValue(hc); }
            set { jumpStepsField.SetValue(hc, value); }
        }

        private int jumped_steps
        {
            get { return (int)jumpedStepsField.GetValue(hc); }
            set { jumpedStepsField.SetValue(hc, value); }
        }

        private bool doubleJumped
        {
            get { return (bool)doubleJumpedField.GetValue(hc); }
            set { doubleJumpedField.SetValue(hc, value);  }
        }

        private static readonly object[] emptyArr = new object[0];

        private Vector2 OrigDashVector() => (Vector2) origDashVectorMethod.Invoke(hc, emptyArr);

        private void Jump() => jumpMethod.Invoke(hc, emptyArr);

        private void FinishedDashing() => finishedDashingMethod.Invoke(hc, emptyArr);

        public void Update()
        {
            if (!Vcm.HasVoidCloak || gm.isPaused) return;

            UpdateVoidCloak();
            UpdateShadowRechargeAnim();
            UpdateJumpHold();
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
                    if (!ih.inputActions.dash.IsPressed || voidDashTimer > VOID_DASH_LIMIT)
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

        private float? prevJumpHoldY = null;

        private void UpdateJumpHold()
        {
            switch (jumpHoldState)
            {
                case JumpHoldState.Idle:
                    prevJumpHoldY = null;
                    break;
                case JumpHoldState.HoldingJump:
                    float newJumpHoldY = hc.gameObject.transform.position.y;
                    if (hcs.dashing || hcs.touchingWall || (prevJumpHoldY != null && prevJumpHoldY > newJumpHoldY))
                    {
                        DarknestDungeon.Log("WFTWHYAREWEFLYING");
                        jumpHoldState = JumpHoldState.Idle;
                    }
                    prevJumpHoldY = newJumpHoldY;
                    break;
            }
        }

        private void StartVoidDash()
        {
            voidCloakState = VoidCloakState.VoidDashing;
            wasAirborne = !hcs.onGround;
            dash_timer = 0;
            voidDashTimer = Time.deltaTime;
            escapedQoL = false;

            velocity = OrigDashVector();
            SetDashVelocity(GetTargetDir() * GetTargetSpeed());

            // TODO: Particle effects
        }

        private bool InputDown => ih.inputActions.down.IsPressed && !ih.inputActions.up.IsPressed;
        private bool InputDownOnly => InputDown && !ih.inputActions.left.IsPressed && !ih.inputActions.right.IsPressed;

        private Vector2 GetTargetDir()
        {
            int horz = (ih.inputActions.left.IsPressed ? -1 : 0) + (ih.inputActions.right.IsPressed ? 1 : 0);
            int vert = (ih.inputActions.up.IsPressed ? 1 : 0) + (ih.inputActions.down.IsPressed ? -1 : 0);

            // Force horizontal if grounded
            if (horz == 0 && vert == -1 && hcs.onGround)
            {
                horz = hcs.facingRight ? 1 : -1;
            }
            else if (!escapedQoL)
            {
                if (voidDashTimer > hc.DASH_TIME || (vert == 1) || (vert == -1 && horz == 0))
                {
                    escapedQoL = true;
                }
                else if (vert == -1 && horz != 0)
                {
                    // Suppress down-angle until a little later, to allow standard QoL spike tunnels
                    vert = 0;
                }
            }

            return ((horz != 0 || vert != 0) ? new Vector2(horz, vert) : (velocity.magnitude > 0 ? velocity : OrigDashVector())).normalized;
        }

        private float GetTargetSpeed()
        {
            if (SharpShadowEquipped)
            {
                return voidDashTimer > hc.DASH_TIME ? SHARP_SHADOW_TAPER_VELOCITY : SHARP_SHADOW_VELOCITY;
            }
            return DASH_VELOCITY;
        }

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
            SetDashVelocity(acc.sqrMagnitude > diff.sqrMagnitude ? targetVelocity : velocity + acc);

            // We cancel the dash if the down-input is pressed and either:
            //   a) A regular dash timer has elapsed, or
            //   b) The dash started airborne, and the player is aiming straight down
            if (hcs.onGround && InputDown && (postVoid || (wasAirborne && InputDownOnly)))
            {
                // Cancel the dash.
                FinishedVoidDashing();
            }
        }

        private void FinishedVoidDashing()
        {
            if (voidCloakState == VoidCloakState.VoidDashing && voidDashTimer <= hc.DASH_TIME)
            {
                dash_timer = voidDashTimer;
                voidCloakState = VoidCloakState.VoidEarlyRelease;
            }
            else
            {
                if (velocity.y > 0)
                {
                    // Fake-jump
                    jumpHoldState = JumpHoldState.HoldingJump;
                    hcs.jumping = true;
                    jump_steps = 3;
                    jumped_steps = 3;
                }

                Vcm.DashVelocityOverride = null;
                FinishedDashing();
                voidCloakState = VoidCloakState.Idle;
            }
        }
    }
}
