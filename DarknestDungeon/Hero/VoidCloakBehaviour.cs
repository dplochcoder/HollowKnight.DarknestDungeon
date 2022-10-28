using DarknestDungeon.IC;
using GlobalEnums;
using ItemChanger.Extensions;
using System;
using System.Reflection;
using UnityEngine;

namespace DarknestDungeon.Hero
{
    public class VoidCloakBehaviour : MonoBehaviour
    {
        private enum VoidCloakState
        {
            Idle,
            VoidDashing
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
        private static readonly MethodInfo checkStillTouchingWallMethod = typeof(HeroController).GetMethod("CheckStillTouchingWall", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo finishedDashingMethod = typeof(HeroController).GetMethod("FinishedDashing", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo jumpMethod = typeof(HeroController).GetMethod("Jump", BindingFlags.NonPublic | BindingFlags.Instance);

        private static float VOID_DASH_EXTENSION_RATIO = 2.1f;
        private static float VOID_DASH_LIMIT = 0.7f;
        private static float SHADE_CLOAK_ANIMATION_LEAD = 1.4f;
        private static float WALL_LAUNCH_LIMIT = 0.04f;
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
        private float wallLaunchTimer;
        private bool wasAirborne;
        private bool wasWallClingRight;
        private bool wasWallClingLeft;
        private bool voidEscapedQoL;
        private bool voidEarlyReleased;

        private ShadowRechargeAnimState shadowRechargeAnimState = ShadowRechargeAnimState.Idle;
        private GameObject shadowRecharge;
        private tk2dSpriteAnimator shadowRechargeAnimator;

        private JumpHoldState jumpHoldState = JumpHoldState.Idle;
        private bool absorbingJumps = false;

        private void Start()
        {
            hc = gameObject.GetComponent<HeroController>();
            ih = (InputHandler)inputHandlerField.GetValue(hc);
            rb2d = (Rigidbody2D)rb2dField.GetValue(hc);
            hcs = hc.cState;
            gm = GameManager.instance;

            shadowRecharge = gameObject.FindChild("Effects").FindChild("Shadow Recharge");
            shadowRechargeAnimator = shadowRecharge.GetComponent<tk2dSpriteAnimator>();

            Vcm.OnTransition += OnSceneTransition;
            On.HeroController.FixedUpdate += OverrideFixedUpdate;
            On.HeroController.Dash += OverrideDash;
            On.HeroController.ResetMotion += OverrideResetMotion;
            On.HeroController.LookForInput += OverrideLookForInput;
            On.HeroController.JumpReleased += OverrideJumpReleased;
        }

        private void OnSceneTransition() => FinishedVoidDashing(false);

        private void OverrideFixedUpdate(On.HeroController.orig_FixedUpdate orig, HeroController self)
        {
            orig(self);
            if (hc.hero_state == ActorStates.no_input && jumpHoldState == JumpHoldState.HoldingJump)
            {
                // Simulate jump.
                Jump();
            }
        }

        private void OverrideDash(On.HeroController.orig_Dash orig, HeroController self)
        {
            orig(self);

            if (voidCloakState == VoidCloakState.VoidDashing && hcs.facingRight && CheckStillTouchingWall(CollisionSide.right) || !hcs.facingRight && CheckStillTouchingWall(CollisionSide.left))
            {
                FinishedVoidDashing(false);
            }
        }

        private void OverrideResetMotion(On.HeroController.orig_ResetMotion orig, HeroController self)
        {
            FinishedVoidDashing(false);
            orig(self);
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
            On.HeroController.FixedUpdate -= OverrideFixedUpdate;
            On.HeroController.Dash -= OverrideDash;
            On.HeroController.ResetMotion -= OverrideResetMotion;
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
            set { doubleJumpedField.SetValue(hc, value); }
        }

        private static readonly object[] emptyArr = new object[0];

        private bool CheckStillTouchingWall(CollisionSide side, bool checkTop = false) => (bool)checkStillTouchingWallMethod.Invoke(hc, new object[] { side, checkTop });

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
                    VoidDashUpdate();
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
                    if (shadowRechargeAnimator.Playing)
                    {
                        shadowRechargeAnimState = ShadowRechargeAnimState.AwaitingUnpause;
                        shadowRechargeAnimator.Pause();
                        shadowRecharge.SetActive(false);
                    }
                    break;
                case ShadowRechargeAnimState.AwaitingUnpause:
                    if (shadowDashTimer <= SHADE_CLOAK_ANIMATION_LEAD)
                    {
                        shadowRechargeAnimState = ShadowRechargeAnimState.Idle;
                        shadowRecharge.SetActive(true);
                        shadowRechargeAnimator.Resume();
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
                    if (hcs.dashing || hcs.touchingWall || prevJumpHoldY != null && prevJumpHoldY > newJumpHoldY)
                    {
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
            wallLaunchTimer = 0;
            wasWallClingLeft = hcs.touchingWall && hcs.facingRight;
            wasWallClingRight = hcs.touchingWall && !hcs.facingRight;
            dash_timer = 0;
            voidDashTimer = Time.deltaTime;
            voidEscapedQoL = false;
            voidEarlyReleased = false;

            velocity = new(0, 0);
            SetDashVelocity(GetTargetDir() * GetTargetSpeed());

            // TODO: Particle effects
        }

        private bool InputDown => ih.inputActions.down.IsPressed && !ih.inputActions.up.IsPressed;
        private bool InputDownOnly => InputDown && !ih.inputActions.left.IsPressed && !ih.inputActions.right.IsPressed;

        private Vector2 GetTargetDir()
        {
            int horz = (ih.inputActions.left.IsPressed ? -1 : 0) + (ih.inputActions.right.IsPressed ? 1 : 0);
            int vert = (ih.inputActions.up.IsPressed ? 1 : 0) + (ih.inputActions.down.IsPressed ? -1 : 0);

            // Force horizontal if coming off of wall.
            horz = wallLaunchTimer < WALL_LAUNCH_LIMIT ? wasWallClingLeft ? 1 : wasWallClingRight ? -1 : horz : horz;

            // Force horizontal if grounded
            if (horz == 0 && vert == -1 && hcs.onGround)
            {
                horz = hcs.facingRight ? 1 : -1;
            }
            else if (!voidEscapedQoL)
            {
                if (voidDashTimer > hc.DASH_TIME || vert == 1 || vert == -1 && horz == 0)
                {
                    voidEscapedQoL = true;
                }
                else if (vert == -1 && horz != 0)
                {
                    // Suppress down-angle until a little later, to allow standard QoL spike tunnels
                    vert = 0;
                }
            }

            if (horz != 0 || vert != 0) return new Vector2(horz, vert).normalized;
            if (velocity.magnitude > 0) return velocity.normalized;
            if (hc.dashingDown) return new(0, -1);
            if (hcs.facingRight) return new(1, 0);
            return new(-1, 0);
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

            if (v.x > 0) hc.FaceRight();
            else if (v.x < 0) hc.FaceLeft();
        }

        private void IncreaseShadowTime(float delta) => shadowDashTimer += VOID_DASH_EXTENSION_RATIO * delta;

        private void VoidDashUpdate()
        {
            dash_timer = 0;
            voidDashTimer += Time.deltaTime;

            if (!voidEarlyReleased && (!ih.inputActions.dash.IsPressed || voidDashTimer > VOID_DASH_LIMIT))
            {
                FinishedVoidDashing(true);
                return;
            }
            else if (voidEarlyReleased && voidDashTimer > hc.DASH_TIME)
            {
                FinishedVoidDashing(false);
                return;
            }

            if (voidDashTimer > hc.DASH_TIME)
            {
                if (shadowRechargeAnimState == ShadowRechargeAnimState.Idle)
                {
                    shadowRechargeAnimState = ShadowRechargeAnimState.AwaitingPause;
                    IncreaseShadowTime(voidDashTimer - hc.DASH_TIME);
                }
                else
                {
                    IncreaseShadowTime(Math.Min(Time.deltaTime, VOID_DASH_LIMIT - (voidDashTimer - Time.deltaTime)));
                }
            }

            wallLaunchTimer += Time.deltaTime;

            // Velocity cannot be changed if we did an early release.
            if (!voidEarlyReleased)
            {
                var targetVelocity = GetTargetDir() * GetTargetSpeed();
                var diff = targetVelocity - velocity;
                var acc = diff.normalized * 2 * GetTargetSpeed() * Time.deltaTime / FULL_REVERSAL_PERIOD;
                SetDashVelocity(acc.sqrMagnitude > diff.sqrMagnitude ? targetVelocity : velocity + acc);
            }

            // Cancel the dash in certain down-input situations.
            if (!hcs.shadowDashing || hcs.onGround && InputDown && (voidDashTimer > hc.DASH_TIME || wasAirborne && InputDownOnly))
            {
                // Cancel the dash.
                FinishedVoidDashing(true);
            }
        }

        private void FinishedVoidDashing(bool allowEarlyRelease)
        {
            if (voidCloakState == VoidCloakState.Idle) return;

            if (allowEarlyRelease && !voidEarlyReleased && voidDashTimer <= hc.DASH_TIME)
            {
                voidEarlyReleased = true;
            }
            else
            {
                if (velocity.y > 0 && allowEarlyRelease)
                {
                    // Fake-jump
                    SetDashVelocity(velocity);
                    jumpHoldState = JumpHoldState.HoldingJump;
                    hcs.jumping = true;
                    jump_steps = 3;
                    jumped_steps = 3;
                }

                doubleJumped = false;
                Vcm.DashVelocityOverride = null;
                FinishedDashing();
                voidCloakState = VoidCloakState.Idle;
            }
        }
    }
}
