using System.Collections;
using BLINK.RPGBuilder.Combat;
using UnityEngine;

namespace BLINK.RPGBuilder.AI
{
    public class AIStateIdle : AIState
    {
        private AIStateIdleTemplate idleTemplate;

        public override void Initialize(AIEntity entity, AIStateTemplate template)
        {
            ThisAIEntity = entity;
            idleTemplate = (AIStateIdleTemplate) template;
            ThisAIEntity.GetActiveStates()[GetName()].StateLoop = ThisAIEntity.StartCoroutine(StateLoop());
        }

        public override AIState Execute()
        {
            if (!(Time.timeSinceLevelLoad >= ThisAIEntity.nextTargetCheck)) return this;
            ThisAIEntity.nextTargetCheck = Time.timeSinceLevelLoad + ThisAIEntity.BehaviorTemplate.CheckTargetInterval;
            CombatEntity newTarget = ThisAIEntity.SearchTarget(idleTemplate.viewDistance, idleTemplate.viewAngle, idleTemplate.AutoAggroDistance);
            if (newTarget == null) return this;
            ThisAIEntity.ThisCombatEntity.SetTarget(newTarget);
            return ThisAIEntity.GetChaseState();
        }

        public override void Enter()
        {
            IsActive = true;
            InitMovement();
            ThisAIEntity.EntityAgent.updateRotation = true;
            MovementStateBlendCompleted = false;
        }
        public override void Exit()
        {
            IsActive = false;
            MovementStateBlendCompleted = true;
        }

        public override void UpdateMovementSpeed()
        {
        }
        
        public override void InitMovement()
        {
            ThisAIEntity.SetHorizontalMovement(0);
            ThisAIEntity.SetVerticalMovement(0);
            MovementStateBlendCompleted = false;
            ThisAIEntity.SetMovementSpeedModifier(idleTemplate.MovementSpeedModifier);
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
