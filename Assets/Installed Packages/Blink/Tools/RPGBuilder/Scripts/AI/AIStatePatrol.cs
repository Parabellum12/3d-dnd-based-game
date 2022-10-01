using System;
using System.Collections;
using BLINK.RPGBuilder.Combat;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BLINK.RPGBuilder.AI
{
    public class AIStatePatrol : AIStateIdle
    {
        private AIStatePatrolTemplate patrolTemplate;
        
        protected float NextPointCheck;

        protected float PointPauseTimeLeft;
        protected bool IsPointPaused;

        protected PatrolPath patrolPath;
        protected int pointIndex;

        protected bool reversed;
        
        public override void Initialize(AIEntity entity, AIStateTemplate template)
        {
            ThisAIEntity = entity;
            patrolTemplate = (AIStatePatrolTemplate) template;
            ThisAIEntity.GetActiveStates()[GetName()].StateLoop = ThisAIEntity.StartCoroutine(StateLoop());

            patrolPath = patrolTemplate.RandomPath ? GameObject.Find(patrolTemplate.PatrolPathNames[Random.Range(0, patrolTemplate.PatrolPathNames.Count)]).GetComponent<PatrolPath>()
                : GameObject.Find(patrolTemplate.PatrolPathName).GetComponent<PatrolPath>();
        }

        public override AIState Execute()
        {
            CombatEntity newTarget = ThisAIEntity.SearchTarget(patrolTemplate.viewDistance, patrolTemplate.viewAngle, patrolTemplate.AutoAggroDistance);
            if (newTarget != null)
            {
                ThisAIEntity.ThisCombatEntity.SetTarget(newTarget);
                return ThisAIEntity.GetChaseState();
            }

            if (ThisAIEntity.IsPlayerInteractionState()) return this;
            HandlePatrol();

            if(!IsPointPaused) ThisAIEntity.MoveAgent(patrolPath.Points[pointIndex].position);
            return this;
        }

        public override void Enter()
        {
            IsActive = true;
            InitMovement();
            ThisAIEntity.EntityAgent.enabled = true;
            ThisAIEntity.EntityAgent.speed = ThisAIEntity.GetMovementSpeed() * patrolTemplate.MovementSpeedModifier;
            ThisAIEntity.EntityAgent.acceleration = ThisAIEntity.EntityAgent.speed;
            ThisAIEntity.EntityAgent.updateRotation = true;
            MovementStateBlendCompleted = false;
            NextPointCheck = 0;
        }

        public override void Exit()
        {
            IsActive = false;
            MovementStateBlendCompleted = true;
            
            ThisAIEntity.EntityAgent.velocity = Vector3.zero;
            ThisAIEntity.EntityAgent.speed = ThisAIEntity.GetMovementSpeed();
            ThisAIEntity.EntityAgent.acceleration = ThisAIEntity.EntityAgent.speed;
        }

        public override void UpdateMovementSpeed()
        {
            ThisAIEntity.EntityAgent.speed = ThisAIEntity.GetMovementSpeed() * patrolTemplate.MovementSpeedModifier;
            ThisAIEntity.EntityAgent.acceleration = ThisAIEntity.EntityAgent.speed;
        }
        
        public override void InitMovement()
        {
            ThisAIEntity.SetHorizontalMovement(0);
            ThisAIEntity.SetVerticalMovement(1);
            MovementStateBlendCompleted = false;
            ThisAIEntity.SetMovementSpeedModifier(patrolTemplate.MovementSpeedModifier);
        }

        protected virtual void HandlePatrol()
        {
            PointPauseTimeLeft -= Time.deltaTime;
            if (!(Time.timeSinceLevelLoad >= NextPointCheck)) return;
            NextPointCheck = Time.timeSinceLevelLoad + patrolTemplate.PointCheckInterval;

            if (!IsPointPaused && NearPoint(patrolTemplate.PointTreshold))
            {
                if (!HasPointPause()) GetNextPoint();
            }

            if (!IsPointPaused || !(PointPauseTimeLeft < 0)) return;
            PointPauseTimeLeft = 0;
            IsPointPaused = false;
            GetNextPoint();
        }

        protected virtual void GetNextPoint()
        {
            if (patrolPath.Looping)
            {
                if (pointIndex < patrolPath.Points.Count - 1) pointIndex++;
                else pointIndex = 0;
            } else
            {
                if (!reversed)
                {
                    if (pointIndex < patrolPath.Points.Count - 1) pointIndex++;
                    else
                    {
                        reversed = true;
                        pointIndex--;
                    }
                }
                else
                {
                    if (pointIndex > 0) pointIndex--;
                    else
                    {
                        reversed = false;
                        pointIndex++;
                    }
                }
            }
            ThisAIEntity.StartMovement();
            MovementStateBlendCompleted = false;
        }

        protected virtual bool HasPointPause()
        {
            float duration;
            
            if (pointIndex == 0) duration = patrolTemplate.PauseAtFirstPointDuration;
            else if (pointIndex == patrolPath.Points.Count - 1) duration = patrolTemplate.PauseAtLastPointDuration;
            else duration = patrolTemplate.PauseAtPointDuration;

            if (!(duration > 0)) return false;
            IsPointPaused = true;
            PointPauseTimeLeft = duration;
            ThisAIEntity.ResetMovement();
            return true;
        }
        

        protected virtual bool NearPoint(float treshold)
        {
            return Vector3.Distance(ThisAIEntity.transform.position,
                new Vector3(patrolPath.Points[pointIndex].position.x, ThisAIEntity.transform.position.y,patrolPath.Points[pointIndex].position.z)) <= treshold;
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
        
    }
}