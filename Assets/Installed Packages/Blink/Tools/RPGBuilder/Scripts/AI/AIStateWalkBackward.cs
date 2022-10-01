using System.Collections;
using UnityEngine;

namespace BLINK.RPGBuilder.AI
{
    public class AIStateWalkBackward : AIState
    {
        private AIStateWalkBackwardTemplate walkBackwardTemplate;
        
        public override void Initialize(AIEntity entity, AIStateTemplate template)
        {
            ThisAIEntity = entity;
            walkBackwardTemplate = (AIStateWalkBackwardTemplate) template;
            ThisAIEntity.GetActiveStates()[GetName()].StateLoop = ThisAIEntity.StartCoroutine(StateLoop());
        }

        public override AIState Execute()
        {
            if (ThisAIEntity.GetCombatState() != null && ThisAIEntity.GetCombatState().CanAttack()) return ThisAIEntity.GetCombatState();
            if (ThisAIEntity.IsTargetTooFar(ThisAIEntity.BehaviorTemplate.MaxDistanceFromTarget) && ThisAIEntity.GetCombatIdleState() != null) return ThisAIEntity.GetCombatIdleState();
            
            Vector3 newPos = ThisAIEntity.transform.TransformPoint(Vector3.forward * ThisAIEntity.GetMovementDirection().y);
            ThisAIEntity.MoveAgent(newPos);
            return this;
        }

        public override void Enter()
        {
            IsActive = true;
            InitMovement();
            MovementStateBlendCompleted = false;
            ThisAIEntity.EntityAgent.enabled = true;
            ThisAIEntity.EntityAgent.updateRotation = false;
            ThisAIEntity.EntityAgent.speed = ThisAIEntity.GetMovementSpeed() * walkBackwardTemplate.MovementSpeedModifier;
            ThisAIEntity.EntityAgent.acceleration = ThisAIEntity.EntityAgent.speed;
            ThisAIEntity.EntityAgent.updateRotation = false;
        }

        public override void Exit()
        {
            IsActive = false;
            MovementStateBlendCompleted = true;
            ThisAIEntity.EntityAgent.updateRotation = true;
            ThisAIEntity.EntityAgent.velocity = Vector3.zero;
            ThisAIEntity.EntityAgent.speed = ThisAIEntity.GetMovementSpeed();
            ThisAIEntity.EntityAgent.acceleration = ThisAIEntity.EntityAgent.speed;
        }

        public override void UpdateMovementSpeed()
        {
            ThisAIEntity.EntityAgent.speed = ThisAIEntity.GetMovementSpeed() * walkBackwardTemplate.MovementSpeedModifier;
            ThisAIEntity.EntityAgent.acceleration = ThisAIEntity.EntityAgent.speed;
        }
        
        public override void InitMovement()
        {
            ThisAIEntity.SetHorizontalMovement(0);
            ThisAIEntity.SetVerticalMovement(-1);
            MovementStateBlendCompleted = false;
            ThisAIEntity.SetMovementSpeedModifier(walkBackwardTemplate.MovementSpeedModifier);
        }
        
        protected virtual IEnumerator StateLoop()
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
