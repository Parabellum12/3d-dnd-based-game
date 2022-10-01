using System;
using System.Collections;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.Managers;
using UnityEngine;

namespace BLINK.Controller
{
    public class RPGBThirdPersonCharacterControllerEssentials : RPGBCharacterControllerEssentials
    {
        public RPGBThirdPersonController controller;

        protected static readonly int moveSpeedModifier = Animator.StringToHash("MoveSpeedModifier");

        /*
        -- EVENT FUNCTIONS --
        */
        private void OnMovementSpeedChange(CombatEntity combatEntity, float newSpeed)
        {
            if (combatEntity != GameState.playerEntity) return;
            controller.SetSpeed(newSpeed);
        }

        /*
        -- INIT --
        */
        public override void Awake()
        {
            anim = GetComponent<Animator>();
            controller = GetComponent<RPGBThirdPersonController>();
            charController = GetComponent<CharacterController>();
            
            CombatEvents.MovementSpeedChanged += OnMovementSpeedChange;
        }
        private void OnDisable()
        {
            CombatEvents.MovementSpeedChanged -= OnMovementSpeedChange;
        }

        public override IEnumerator InitControllers()
        {
            yield return new WaitForFixedUpdate();
            controller._playerCamera.InitCameraPosition(new Vector2(15, transform.eulerAngles.y));
            controller.SetControlRotation(new Vector2(15, transform.eulerAngles.y));
            SetCameraMouseLook(true);
            controllerIsReady = true;
        }

        /*
        -- DEATH --
        */
        public override void InitDeath()
        {
            anim.Rebind();
            anim.SetBool("Dead", true);
            ResetCameraAiming();
            ResetCameraMouseLook();
        }

        public override void CancelDeath()
        {
            anim.Rebind();
            anim.SetBool("Dead", false);
            if (controller.CameraSettings.CameraType == CameraSettings.CameraOptions.AimOnly)
            {
                SetCameraAiming(true);
                controller.cameraCanRotate = true;
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                SetCameraMouseLook(true);
            }
        }


        /*
        -- GROUND LEAP FUNCTIONS --
        Ground leaps are mobility abilities. Configurable inside the editor under Combat > Abilities > Ability Type=Ground Leap
        They allow to quickly dash or leap to a certain ground location.
        */
        public override void InitGroundLeap()
        {
            isLeaping = true;
        }

        public override void EndGroundLeap()
        {
            isLeaping = false;
        }
        
        /*
        -- FLYING FUNCTIONS --
        */
        
        public override void InitFlying()
        {
            isFlying = true;
            controller.isFlying = true;
            anim.SetBool("isFlying", true);
        }

        public override void EndFlying()
        {
            isFlying = false;
            controller.isFlying = false;
            anim.SetBool("isFlying", false);
        }

        /*
        -- STAND TIME FUNCTIONS --
        Stand time is an optional mechanic for abilities. It allows to root the caster for a certain duration after using the ability.
        */
        public override void InitStandTime(float max)
        {
            standTimeActive = true;
            currentStandTimeDur = 0;
            maxStandTimeDur = max;
        }

        protected override void HandleStandTime()
        {
            currentStandTimeDur += Time.deltaTime;
            if (currentStandTimeDur >= maxStandTimeDur) ResetStandTime();
        }

        protected override void ResetStandTime()
        {
            standTimeActive = false;
            currentStandTimeDur = 0;
            maxStandTimeDur = 0;
        }

        /* KNOCKBACK FUNCTIONS
         */
        public bool knockbackActive;
        private Vector3 knockBackTarget;
        private float cachedKnockbackDistance;

        public override void InitKnockback(float knockbackDistance, Transform attacker)
        {
            knockbackDistance *= 5;
            cachedKnockbackDistance = knockbackDistance;
            knockBackTarget = (transform.position - attacker.position).normalized * knockbackDistance;
            knockbackActive = true;
        }

        protected override void HandleKnockback()
        {
            if (knockBackTarget.magnitude > (cachedKnockbackDistance * 0.15f))
            {
                controller.getCharController().Move(knockBackTarget * Time.deltaTime);
                knockBackTarget = Vector3.Lerp(knockBackTarget, Vector3.zero, 5 * Time.deltaTime);
            }
            else
            {
                ResetKnockback();
            }
        }

        protected override void ResetKnockback()
        {
            knockbackActive = false;
            cachedKnockbackDistance = 0;
            knockBackTarget = Vector3.zero;
        }
        
        /* MOTION FUNCTIONS
         */
        private float curMotionSpeed;
        public override void InitMotion(float motionDistance, Vector3 motionDirection, float motionSpeed, bool immune)
        {
            if (GameState.playerEntity.IsShapeshifted()) return;
            if (knockbackActive) return;
            cachedMotionSpeed = motionSpeed;
            curMotionSpeed = cachedMotionSpeed;
            cachedPositionBeforeMotion = transform.position;
            cachedMotionDistance = motionDistance;
            motionTarget = transform.TransformDirection(motionDirection) * motionDistance;
            GameState.playerEntity.SetMotionImmune(immune);
            motionActive = true;
        }

        protected override void HandleMotion()
        {
            float distance = Vector3.Distance(cachedPositionBeforeMotion, transform.position);
            if (distance < cachedMotionDistance)
            {
                lastPosition = transform.position;
                controller.getCharController().Move(motionTarget * (Time.deltaTime * curMotionSpeed));
                
                if (IsInMotionWithoutProgress(0.05f))
                {
                    ResetMotion();
                    return;
                }
                
                if (!(distance < cachedMotionDistance * 0.75f)) return;
                curMotionSpeed = Mathf.Lerp(curMotionSpeed, 0, Time.deltaTime * 5f);
                if(curMotionSpeed < (cachedMotionSpeed * 0.2f))
                {
                    curMotionSpeed = cachedMotionSpeed * 0.2f;
                }
            }
            else
            {
                ResetMotion();
            }
        }

        public override bool IsInMotionWithoutProgress(float treshold)
        {
            float speed = (transform.position - lastPosition).magnitude;
            return speed > -treshold && speed < treshold;
        }

        protected override void ResetMotion()
        {
            motionActive = false;
            GameState.playerEntity.SetMotionImmune(false);
            cachedMotionDistance = 0;
            motionTarget = Vector3.zero;
        }
        
        /* CAMERA AIMING
         * 
         */

        public bool isAimingTransition;
        public override void SetCameraAiming(bool isAiming)
        {
            if (GameState.playerEntity.IsShapeshifted() && !RPGBuilderUtilities.CanActiveShapeshiftCameraAim(GameState.playerEntity)) return;
            if (GameState.playerEntity.IsMounted() && !GameState.playerEntity.GetMountEffectRank().mountCanAim) return;
            controller.CameraSettings.isAiming = isAiming;
            controller.RotationSettings.UseControlRotation = isAiming;
            controller.RotationSettings.OrientRotationToMovement = !isAiming;
            isAimingTransition = true;
            
            anim.SetBool("isAiming", isAiming);
            if(GameState.playerEntity.IsMounted()) GameState.playerEntity.GetMountAnimator().SetBool("isAiming", isAiming);
            
            if (isAiming) GameEvents.Instance.OnEnterAimMode();
            else GameEvents.Instance.OnExitAimMode();
        }
        
        public override void ResetCameraAiming()
        {
            controller.CameraSettings.isAiming = false;
            controller.RotationSettings.UseControlRotation = false;
            controller.RotationSettings.OrientRotationToMovement = true;
            isAimingTransition = true;
            
            anim.SetBool("isAiming", false);
            if(GameState.playerEntity.IsMounted()) GameState.playerEntity.GetMountAnimator().SetBool("isAiming", false);
            GameEvents.Instance.OnExitAimMode();
        }

        /*
        -- CAST SLOWED FUNCTIONS --
        Cast slow is an optional mechanic for abilities. It allows the player to be temporarily slowed while
        casting an ability. I personally use it to increase the risk of certain ability use, to increase the chance of being hit
        by enemies attacks while casting it. Of course this is targetting abilities that can be casted while moving.
        */
        public override void InitCastMoveSlow(float speedPercent, float castSlowDuration, float castSlowRate)
        {
            curSpeedPercentage = 1;
            speedPercentageTarget = speedPercent;
            currentCastSlowDur = 0;
            maxCastSlowDur = castSlowDuration;
            speedCastSlowRate = castSlowRate;
            isCastingSlowed = true;
        }

        protected override void HandleCastSlowed()
        {
            curSpeedPercentage -= speedCastSlowRate;
            if (curSpeedPercentage < speedPercentageTarget) curSpeedPercentage = speedPercentageTarget;

            currentCastSlowDur += Time.deltaTime;

            float newMoveSpeed = RPGBuilderUtilities.getCurrentMoveSpeed(GameState.playerEntity);
            newMoveSpeed *= curSpeedPercentage;
            newMoveSpeed = (float) Math.Round(newMoveSpeed, 2);
            OnMovementSpeedChange(GameState.playerEntity, newMoveSpeed);

            if (currentCastSlowDur >= maxCastSlowDur) ResetCastSlow();
        }

        protected override void ResetCastSlow()
        {
            isCastingSlowed = false;
            curSpeedPercentage = 1;
            speedPercentageTarget = 1;
            currentCastSlowDur = 0;
            maxCastSlowDur = 0;
            if (GameDatabase.Instance.GetGeneralSettings().useOldController)
            {
                builtInController.anim.SetFloat(moveSpeedModifier, curSpeedPercentage);
            }

            OnMovementSpeedChange(GameState.playerEntity, RPGBuilderUtilities.getCurrentMoveSpeed(GameState.playerEntity));
        }

        /*
        -- LOGIC UPDATES --
        */
        public override void FixedUpdate()
        {
            if (GameState.playerEntity == null) return;
            if (GameState.playerEntity.IsDead()) return;

            HandleCombatStates();

            if (knockbackActive)
                HandleKnockback();
            
            if (motionActive)
                HandleMotion();

            if (isTeleporting)
                HandleTeleporting();
            
            if(controller.isSprinting)
                HandleSprint();


            if (isResetingSprintCamFOV)
                HandleSprintCamFOVReset();

        }

        private void HandleSprintCamFOVReset()
        {
            controller._playerCamera.GetCamera().fieldOfView = Mathf.Lerp(controller._playerCamera.GetCamera().fieldOfView,
                controller.CameraSettings.NormalFOV, Time.deltaTime * controller.CameraSettings.FOVLerpSpeed);

            if (Mathf.Abs(controller._playerCamera.GetCamera().fieldOfView - controller.CameraSettings.NormalFOV) < 0.25f)
            {
                controller._playerCamera.GetCamera().fieldOfView = controller.CameraSettings.NormalFOV;
                isResetingSprintCamFOV = false;
            }
        }

        private void Update()
        {
            if (controller.CameraSettings.CameraType == CameraSettings.CameraOptions.Both && Input.GetKeyDown(RPGBuilderUtilities.GetCurrentKeyByActionKeyName("TOGGLE_CAMERA_AIM_MODE")))
            {
                SetCameraAiming(!controller.CameraSettings.isAiming);
            }

            if (isAimingTransition)
            {
                HandleAimingTransition();
            }
            else
            {
                controller._playerCamera.SetPivotOffset(controller.CameraSettings.isAiming ? controller.CameraSettings.aimingPivot : controller.CameraSettings.normalPivot);
            }
        }

        private void HandleAimingTransition()
        {
            if (controller.CameraSettings.isAiming)
            {
                if (controller._playerCamera.GetPivotOffset() != controller.CameraSettings.aimingPivot)
                {
                    controller._playerCamera.SetPivotOffset(Vector3.Lerp(
                        controller._playerCamera.GetPivotOffset(), controller.CameraSettings.aimingPivot
                        , controller.CameraSettings.aimInSpeed * Time.deltaTime));
                }
                else
                {
                    isAimingTransition = false;
                }
            }
            else
            {
                if (controller._playerCamera.GetPivotOffset() != controller.CameraSettings.normalPivot)
                {
                    controller._playerCamera.SetPivotOffset(Vector3.Lerp(
                        controller._playerCamera.GetPivotOffset(), controller.CameraSettings.normalPivot
                        , controller.CameraSettings.aimOutSpeed * Time.deltaTime));
                }
                else
                {
                    isAimingTransition = false;
                }
            }
        }

        protected override void HandleTeleporting()
        {
            transform.position = teleportTargetPos;
            isTeleporting = false;
            GameState.Instance.InstantNPCSpawnerCheck();
        }

        protected override void HandleCombatStates()
        {
            if (isCastingSlowed) HandleCastSlowed();
            if (standTimeActive) HandleStandTime();
        }

        /*
        -- TELEPORT FUNCTIONS --
        Easy way to instantly teleport the player to a certain location.
        Called by DevUIManager and CombatManager
        */
        public override void TeleportToTarget(Vector3 pos) // Teleport to the Vector3 Coordinates
        {
            if (Vector3.Distance(transform.position, pos) >= 100)
            {
                foreach (var pet in GameState.playerEntity.GetCurrentPets())
                {
                    pet.GetAIEntity().TeleportEntity(pos);
                }
            }

            isTeleporting = true;
            teleportTargetPos = pos;
        }

        public override void TeleportToTarget(CombatEntity target) // Teleport to the CombatEntity Coordinates
        {
            isTeleporting = true;
            teleportTargetPos = target.transform.position;
        }

        /*
        -- CHECKING CONDITIONAL FUNCTIONS --
        */
        public override bool HasMovementRestrictions()
        {
            return GameState.playerEntity.IsDead() ||
                   !canMove ||
                   isTeleporting ||
                   standTimeActive ||
                   knockbackActive ||
                   motionActive ||
                   isLeaping ||
                   GameState.playerEntity.IsStunned() ||
                   GameState.playerEntity.IsSleeping();
        }

        public override bool HasRotationRestrictions()
        {
            return GameState.playerEntity.IsDead() ||
                   isLeaping ||
                   knockbackActive ||
                   motionActive ||
                   GameState.playerEntity.IsStunned() ||
                   GameState.playerEntity.IsSleeping();
        }

        /*
        -- UI --
        */
        public override void GameUIPanelAction(bool opened)
        {
            SetCameraMouseLook(!opened);
        }

        /*
        -- CAMERA --
        */
        public override void SetCameraMouseLook(bool state)
        {
            if (GameState.playerEntity.IsDead()) return;
            controller.cameraCanRotate = state;
            Cursor.visible = !state;
            Cursor.lockState = state ? CursorLockMode.Locked : CursorLockMode.None;
        }
        
        public override void ResetCameraMouseLook()
        {
            controller.cameraCanRotate = false;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        public override void ToggleCameraMouseLook()
        {
            if (GameState.playerEntity.IsDead()) return;
            SetCameraMouseLook(!controller.cameraCanRotate);
        }

        /*
         *
         * MOVEMENT
         */

        public override void StartSprint()
        {
            controller.isSprinting = true;
            
        }
        public override void EndSprint()
        {
            controller.isSprinting = false;
            isResetingSprintCamFOV = true;
        }


        public override void HandleSprint()
        {
            if (controller.CameraSettings.NormalFOV != controller.CameraSettings.SprintFOV)
            {
                controller._playerCamera.GetCamera().fieldOfView = Mathf.Lerp(controller._playerCamera.GetCamera().fieldOfView,
                    controller.CameraSettings.SprintFOV, Time.deltaTime * controller.CameraSettings.FOVLerpSpeed);
            }
            
            
            if(GameDatabase.Instance.GetSprintDrainStat() == null) return;

            if (!(Time.time >= nextSprintStatDrain)) return;
            nextSprintStatDrain = Time.time + GameDatabase.Instance.GetCharacterSettings().SprintStatDrainInterval;
            CombatUtilities.UpdateCurrentStatValue(GameState.playerEntity, GameDatabase.Instance.GetCharacterSettings().SprintStatDrainID, -GameDatabase.Instance.GetCharacterSettings().SprintStatDrainAmount);
        }

        public override bool isSprinting()
        {
            return controller.isSprinting;
        }

        /*
        -- CONDITIONS --
        */
        public override bool ShouldCancelCasting()
        {
            return !IsGrounded() || IsMoving();
        }

        public override bool IsGrounded()
        {
            return charController.isGrounded;
        }

        public override bool IsMoving()
        {
            return charController.velocity != Vector3.zero;
        }

        public override bool IsThirdPersonShooter()
        {
            return controller.CameraSettings.isAiming;
        }

        public override RPGBuilderGeneralSettings.ControllerTypes GETControllerType()
        {
            return controller.CameraSettings.isAiming ? RPGBuilderGeneralSettings.ControllerTypes.ThirdPersonShooter : RPGBuilderGeneralSettings.ControllerTypes.ThirdPerson;
        }

        public override void MainMenuInit()
        {
            Destroy(GetComponent<RPGBThirdPersonController>());
            Destroy(GetComponent<CharacterAnimator>());
            Destroy(GetComponent<RPGBThirdPersonCharacterControllerEssentials>());
            Destroy(GetComponent<RPGBCharacterWorldInteraction>());
            Destroy(charController);
        }
        
        public override void AbilityInitActions(RPGAbility.RPGAbilityRankData rankREF)
        {
        }
        public override void AbilityEndCastActions(RPGAbility.RPGAbilityRankData rankREF)
        {
            if (rankREF.standTimeDuration > 0)
            {
                ResetStandTime();
            }
        }
    }
}
