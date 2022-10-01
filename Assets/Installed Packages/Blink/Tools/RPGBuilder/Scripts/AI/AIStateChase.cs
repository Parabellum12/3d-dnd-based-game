using System.Collections;
using UnityEngine;

namespace BLINK.RPGBuilder.AI
{
    public class AIStateChase : AIState
    {
        private AIStateChaseTemplate chaseTemplate;
        
        public override void Initialize(AIEntity entity, AIStateTemplate template)
        {
            ThisAIEntity = entity;
            chaseTemplate = (AIStateChaseTemplate) template;
            ThisAIEntity.GetActiveStates()[GetName()].StateLoop = ThisAIEntity.StartCoroutine(StateLoop());
        }

        public override AIState Execute()
        {
            if (ThisAIEntity.GetCombatState() != null && ThisAIEntity.GetCombatState().CanAttack()) return ThisAIEntity.GetCombatState();
            if (ThisAIEntity.IsTargetTooClose(ThisAIEntity.BehaviorTemplate.MaxDistanceFromTarget)) return ThisAIEntity.GetCombatIdleState();
            
            Vector3 angle = ThisAIEntity.transform.rotation * Vector3.forward;
            Vector3 pos = ThisAIEntity.ThisCombatEntity.GetTarget().transform.position + angle * (ThisAIEntity.BehaviorTemplate.MinDistanceFromTarget * 0.9f);
            ThisAIEntity.MoveAgent(pos);
            return this;
        }

        public override void Enter()
        {
            IsActive = true;
            InitMovement();
            MovementStateBlendCompleted = false;
            ThisAIEntity.EntityAgent.enabled = true;
            ThisAIEntity.EntityAgent.speed = ThisAIEntity.GetMovementSpeed() * chaseTemplate.MovementSpeedModifier;
            ThisAIEntity.EntityAgent.acceleration = ThisAIEntity.EntityAgent.speed;
            ThisAIEntity.EntityAgent.updateRotation = true;
        }

        public override void Exit()
        {
            IsActive = false;
            MovementStateBlendCompleted = true;
            ThisAIEntity.EntityAgent.velocity = Vector3.zero;
        }

        public override void UpdateMovementSpeed()
        {
            ThisAIEntity.EntityAgent.speed = ThisAIEntity.GetMovementSpeed() * chaseTemplate.MovementSpeedModifier;
            ThisAIEntity.EntityAgent.acceleration = ThisAIEntity.EntityAgent.speed;
        }

        public override void InitMovement()
        {
            ThisAIEntity.SetHorizontalMovement(0);
            ThisAIEntity.SetVerticalMovement(1);
            MovementStateBlendCompleted = false;
            ThisAIEntity.SetMovementSpeedModifier(chaseTemplate.MovementSpeedModifier);
        }

        protected virtual IEnumerator StateLoop()
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
    }
}
