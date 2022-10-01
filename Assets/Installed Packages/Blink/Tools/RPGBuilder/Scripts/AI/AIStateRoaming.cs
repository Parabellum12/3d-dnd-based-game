using System.Collections;
using BLINK.RPGBuilder.Combat;
using UnityEngine;
namespace BLINK.RPGBuilder.AI
{
    public class AIStateRoaming : AIStateIdle
    {
        private AIStateRoamingTemplate roamingTemplate;

        protected float NextRoamPositionCheck;
        protected Vector3 RoamTargetPosition;

        protected float RoamingPauseTimeLeft;
        protected bool IsRoamingPaused;
        protected static readonly int walking = Animator.StringToHash("Walking");

        public override void Initialize(AIEntity entity, AIStateTemplate template)
        {
            ThisAIEntity = entity;
            roamingTemplate = (AIStateRoamingTemplate) template;
            ThisAIEntity.GetActiveStates()[GetName()].StateLoop = ThisAIEntity.StartCoroutine(StateLoop());
        }
        
        public override AIState Execute()
        {
            CombatEntity newTarget = ThisAIEntity.SearchTarget(roamingTemplate.viewDistance, roamingTemplate.viewAngle, roamingTemplate.AutoAggroDistance);
            if (newTarget != null)
            {
                ThisAIEntity.ThisCombatEntity.SetTarget(newTarget);
                return ThisAIEntity.GetChaseState();
            }

            if (ThisAIEntity.IsPlayerInteractionState()) return this;
            HandleRoaming();

            if(!IsRoamingPaused) ThisAIEntity.MoveAgent(RoamTargetPosition);
            return this;
        }

        public override void Enter()
        {
            IsActive = true;
            InitMovement();
            ThisAIEntity.EntityAnimator.SetBool(walking, true);
            ThisAIEntity.EntityAgent.enabled = true;
            ThisAIEntity.EntityAgent.speed = ThisAIEntity.GetMovementSpeed() * roamingTemplate.MovementSpeedModifier;
            ThisAIEntity.EntityAgent.acceleration = ThisAIEntity.EntityAgent.speed;
            ThisAIEntity.EntityAgent.updateRotation = true;
            MovementStateBlendCompleted = false;
            NextRoamPositionCheck = 0;
            GetNewRoamingPoint();
        }

        public override void Exit()
        {
            IsActive = false;
            ThisAIEntity.EntityAnimator.SetBool(walking, false);
            MovementStateBlendCompleted = true;
            
            ThisAIEntity.EntityAgent.velocity = Vector3.zero;
            ThisAIEntity.EntityAgent.speed = ThisAIEntity.GetMovementSpeed();
            ThisAIEntity.EntityAgent.acceleration = ThisAIEntity.EntityAgent.speed;
        }

        public override void UpdateMovementSpeed()
        {
            ThisAIEntity.EntityAgent.speed = ThisAIEntity.GetMovementSpeed() * roamingTemplate.MovementSpeedModifier;
            ThisAIEntity.EntityAgent.acceleration = ThisAIEntity.EntityAgent.speed;
        }
        
        public override void InitMovement()
        {
            ThisAIEntity.SetHorizontalMovement(0);
            ThisAIEntity.SetVerticalMovement(1);
            MovementStateBlendCompleted = false;
            ThisAIEntity.SetMovementSpeedModifier(roamingTemplate.MovementSpeedModifier);
        }

        protected virtual void HandleRoaming()
        {
            RoamingPauseTimeLeft -= Time.deltaTime;
            if (!(Time.timeSinceLevelLoad >= NextRoamPositionCheck)) return;
            NextRoamPositionCheck = Time.timeSinceLevelLoad + roamingTemplate.roamTargetCheckInterval;

            if (!IsRoamingPaused && NearRoamTarget(roamingTemplate.RoamPointThreshold))
            {
                if (roamingTemplate.PauseDuration > 0)
                {
                    IsRoamingPaused = true;
                    RoamingPauseTimeLeft = roamingTemplate.PauseDuration;
                    ThisAIEntity.ResetMovement();
                }
                else
                {
                    GetNewRoamingPoint();
                }
            }

            if (!(roamingTemplate.PauseDuration > 0) || !IsRoamingPaused || !(RoamingPauseTimeLeft < 0)) return;
            RoamingPauseTimeLeft = 0;
            IsRoamingPaused = false;
            GetNewRoamingPoint();
        }

        

        protected virtual void GetNewRoamingPoint()
        {
            if (roamingTemplate.roamAroundSpawner && ThisAIEntity.ThisCombatEntity.GetSpawner() != null)
            {
                RoamTargetPosition = GetValidPoint(ThisAIEntity.ThisCombatEntity.GetSpawner().transform.position, ThisAIEntity.ThisCombatEntity.GetSpawner().areaHeight,
                    ThisAIEntity.ThisCombatEntity.GetSpawner().groundLayers);
            }
            else
            {
                RoamTargetPosition = GetValidPoint(ThisAIEntity.transform.position, 40,
                    roamingTemplate.roamGroundLayers);
            }
            ThisAIEntity.StartMovement();
        }
        
        protected virtual Vector3 GetValidPoint(Vector3 basePosition, float height, LayerMask groundLayers)
        {
            bool foundPoint = false;
            Vector3 pos = new Vector3();
            int maxAttempt = 15;
            while (!foundPoint)
            {
                maxAttempt--;
                pos = GetPoint(basePosition, height, groundLayers);
                if (pos != basePosition)
                {
                    foundPoint = ThisAIEntity.IsPathAllowed(pos);
                }

                if (maxAttempt == 0)
                {
                    foundPoint = true;
                }
            }

            return pos;
        }

        private Vector3 GetPoint(Vector3 basePosition, float height, LayerMask groundLayers)
        {
            Vector3 pos = new Vector3(
                Random.Range(basePosition.x - roamingTemplate.RoamDistance, basePosition.x + roamingTemplate.RoamDistance),
                basePosition.y + height,
                Random.Range(basePosition.z - roamingTemplate.RoamDistance, basePosition.z + roamingTemplate.RoamDistance));
            return Physics.Raycast(pos, -ThisAIEntity.transform.up, out var hit, height*2, groundLayers) ? hit.point : basePosition;
        }

        protected virtual bool NearRoamTarget(float treshold)
        {
            return Vector3.Distance(ThisAIEntity.transform.position,
                new Vector3(RoamTargetPosition.x, ThisAIEntity.transform.position.y, RoamTargetPosition.z)) <= treshold;
        }

        protected override IEnumerator StateLoop()
        {
            while(true)
            {
                if (IsActive)
                {
                    if (!MovementStateBlendCompleted)
                    {
                        MovementStateBlendCompleted = ThisAIEntity.HandleMovementDirectionsBlend();
                    }
                }

                yield return new WaitForFixedUpdate();
            }
        }

        private void DebugRoamingPoints()
        {
            Vector3 pos = GetValidPoint(ThisAIEntity.ThisCombatEntity.GetSpawner().transform.position, ThisAIEntity.ThisCombatEntity.GetSpawner().areaHeight,
                ThisAIEntity.ThisCombatEntity.GetSpawner().groundLayers);
            GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinder.transform.position = pos;
        }
    }
}