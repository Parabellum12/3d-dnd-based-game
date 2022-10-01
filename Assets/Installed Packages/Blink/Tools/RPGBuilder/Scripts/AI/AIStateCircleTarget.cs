using System.Collections;
using UnityEngine;

namespace BLINK.RPGBuilder.AI
{
    public class AIStateCircleTarget : AIStateCombatIdle
    {
        private AIStateCircleTargetTemplate circleTargetTemplate;

        private int currentMovementState = 6;

        private float circlingValue = 0;
        public bool circleRight;
        public float NextDirectionChange;

        private bool _moveForward;
        
        public override void Initialize(AIEntity entity, AIStateTemplate template)
        {
            ThisAIEntity = entity;
            circleTargetTemplate = (AIStateCircleTargetTemplate) template;
            ThisAIEntity.GetActiveStates()[GetName()].StateLoop = ThisAIEntity.StartCoroutine(StateLoop());
        }
        
        public override AIState Execute()
        {
            if (ThisAIEntity.GetChaseState() != null && ThisAIEntity.IsTargetTooFar(ThisAIEntity.BehaviorTemplate.MaxDistanceFromTarget)) return ThisAIEntity.GetChaseState();
            if (ThisAIEntity.GetWalkBackwardState() != null && ThisAIEntity.IsTargetTooClose(ThisAIEntity.BehaviorTemplate.MinDistanceFromTarget)) return ThisAIEntity.GetWalkBackwardState();
            if (ThisAIEntity.GetCombatState() != null && ThisAIEntity.GetCombatState().CanAttack()) return ThisAIEntity.GetCombatState();
            
            Vector3 newPos = ThisAIEntity.transform.TransformPoint(Vector3.forward * ThisAIEntity.GetMovementDirection().y + Vector3.left * ThisAIEntity.GetMovementDirection().x);
            if (Vector3.Distance(newPos, ThisAIEntity.ThisCombatEntity.GetTarget().transform.position) <= ThisAIEntity.BehaviorTemplate.MinDistanceFromTarget)
            {
                newPos = ThisAIEntity.transform.TransformPoint(Vector3.left * ThisAIEntity.GetMovementDirection().x);
            }
            
            ThisAIEntity.MoveAgent(newPos);
            
            if (Time.timeSinceLevelLoad >= NextDirectionChange) CirclingDirectionChange();
            return this;
        }

        public override void Enter()
        {
            IsActive = true;
            MovementStateBlendCompleted = false;
            ThisAIEntity.EntityAgent.enabled = true;
            ThisAIEntity.EntityAgent.updateRotation = false;
            ThisAIEntity.EntityAgent.velocity = Vector3.zero;
            ThisAIEntity.EntityAgent.speed = ThisAIEntity.GetMovementSpeed() * circleTargetTemplate.MovementSpeedModifier;
            ThisAIEntity.EntityAgent.acceleration = ThisAIEntity.EntityAgent.speed;
            ThisAIEntity.EntityAnimator.SetBool(ThisAIEntity.BehaviorTemplate.StrafeParameterName, true);
            
            InitMovement();
        }

        public override void Exit()
        {
            IsActive = false;
            MovementStateBlendCompleted = true;
            NextDirectionChange = 0;
            
            ThisAIEntity.EntityAgent.velocity = Vector3.zero;
            ThisAIEntity.EntityAgent.speed = ThisAIEntity.GetMovementSpeed();
            ThisAIEntity.EntityAgent.acceleration = ThisAIEntity.EntityAgent.speed;
            
            ThisAIEntity.EntityAnimator.SetBool(ThisAIEntity.BehaviorTemplate.StrafeParameterName, false);
        }
        
        public override void InitMovement()
        {
            InitCirclingDirection();
            MovementStateBlendCompleted = false;
            ThisAIEntity.SetMovementSpeedModifier(circleTargetTemplate.MovementSpeedModifier);
        }
        
        protected void InitCirclingDirection()
        {
            int chance = Random.Range(0, 2);
            circleRight = chance == 0;
            ThisAIEntity.SetVerticalMovement(1);
            SetXMovementDirection();
        }

        private void SetXMovementDirection()
        {
            if (circleRight) ThisAIEntity.SetHorizontalMovement(-1);
            else ThisAIEntity.SetHorizontalMovement(1);
        }

        protected virtual void CirclingDirectionChange()
        {
            NextDirectionChange = Time.timeSinceLevelLoad + Random.Range(circleTargetTemplate.minimumTimeBetweenDirectionChange,
                circleTargetTemplate.maximumTimeBetweenDirectionChange);
            ThisAIEntity.EntityAgent.velocity = Vector3.zero;
            circleRight = !circleRight;
            MovementStateBlendCompleted = false;
            SetXMovementDirection();
        }
        
        protected override IEnumerator StateLoop()
        {
            while(true)
            {
                if (IsActive)
                {
                    ThisAIEntity.LookAtTarget(ThisAIEntity.BehaviorTemplate.LookAtTargetSpeed);
                    if (!MovementStateBlendCompleted)
                    {
                        MovementStateBlendCompleted = ThisAIEntity.HandleMovementDirectionsBlend();
                    }
                }

                yield return new WaitForFixedUpdate();
            }
        }
        
    }
}
