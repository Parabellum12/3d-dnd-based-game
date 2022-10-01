using System.Collections;
using UnityEngine;

namespace BLINK.RPGBuilder.AI
{
    public class AIStateCombatIdle : AIState
    {
        private AIStateCombatIdleTemplate combatIdleTemplate;

        public override void Initialize(AIEntity entity, AIStateTemplate template)
        {
            ThisAIEntity = entity;
            combatIdleTemplate = (AIStateCombatIdleTemplate) template;
            ThisAIEntity.GetActiveStates()[GetName()].StateLoop = ThisAIEntity.StartCoroutine(StateLoop());
        }

        public override AIState Execute()
        {
            if (!ThisAIEntity.IsTargetWithinRange(ThisAIEntity.BehaviorTemplate.MaxDistanceFromTarget)) return ThisAIEntity.GetChaseState();
            if (ThisAIEntity.GetWalkBackwardState() != null && ThisAIEntity.IsTargetTooClose(ThisAIEntity.BehaviorTemplate.MinDistanceFromTarget)) {return ThisAIEntity.GetWalkBackwardState();}
            if (ThisAIEntity.GetCombatState() != null && ThisAIEntity.GetCombatState().CanAttack()) return ThisAIEntity.GetCombatState();
            return this;
        }

        public override void Enter()
        {
            IsActive = true;
            ThisAIEntity.SetHorizontalMovement(0);
            ThisAIEntity.SetVerticalMovement(0);
            MovementStateBlendCompleted = false;
            ThisAIEntity.EntityAgent.enabled = false;
        }

        public override void Exit()
        {
            IsActive = false;
            MovementStateBlendCompleted = true;
            ThisAIEntity.EntityAgent.enabled = true;
        }

        public override void UpdateMovementSpeed()
        {
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
