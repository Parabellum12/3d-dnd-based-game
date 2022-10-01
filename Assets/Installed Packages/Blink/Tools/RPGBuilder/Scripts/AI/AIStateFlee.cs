using System;
using System.Collections;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.Managers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BLINK.RPGBuilder.AI
{
    public class AIStateFlee : AIState
    {
        private AIStateFleeTemplate fleeTemplate;
        
        protected float NextFleePositionCheck;
        protected Vector3 FleeTargetPosition;

        protected float FleeDuration;
        protected float FleePauseTimeLeft;
        protected bool IsFleePaused;
        
        public override void Initialize(AIEntity entity, AIStateTemplate template)
        {
            ThisAIEntity = entity;
            fleeTemplate = (AIStateFleeTemplate) template;
            ThisAIEntity.GetActiveStates()[GetName()].StateLoop = ThisAIEntity.StartCoroutine(StateLoop());
        }

        public override AIState Execute()
        {
            FleeDuration += Time.deltaTime;
            if (FleeDuration >= fleeTemplate.FleeDuration) return ThisAIEntity.GetChaseState();
            
            HandleFleeing();
            if(fleeTemplate.CallForHelp) GetHelp();
            if(!IsFleePaused) ThisAIEntity.MoveAgent(FleeTargetPosition);
            return this;
        }

        public override void Enter()
        {
            IsActive = true;
            InitMovement();
            ThisAIEntity.EntityAgent.enabled = true;
            ThisAIEntity.EntityAgent.speed = ThisAIEntity.GetMovementSpeed() * fleeTemplate.MovementSpeedModifier;
            ThisAIEntity.EntityAgent.acceleration = ThisAIEntity.EntityAgent.speed;
            MovementStateBlendCompleted = false;
            GetNewFleePoint();
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
            ThisAIEntity.EntityAgent.speed = ThisAIEntity.GetMovementSpeed() * fleeTemplate.MovementSpeedModifier;
            ThisAIEntity.EntityAgent.acceleration = ThisAIEntity.EntityAgent.speed;
        }
        
        public override void InitMovement()
        {
            ThisAIEntity.SetHorizontalMovement(0);
            ThisAIEntity.SetVerticalMovement(1);
            MovementStateBlendCompleted = false;
            ThisAIEntity.SetMovementSpeedModifier(fleeTemplate.MovementSpeedModifier);
        }
        
        protected virtual void HandleFleeing()
        {
            FleePauseTimeLeft -= Time.deltaTime;
            if (!(Time.timeSinceLevelLoad >= NextFleePositionCheck)) return;
            NextFleePositionCheck = Time.timeSinceLevelLoad + fleeTemplate.FleeTargetCheckInterval;

            if (!IsFleePaused && NearFleeTarget(fleeTemplate.FleePointTreshold))
            {
                if (fleeTemplate.FleePointPauseDuration > 0)
                {
                    IsFleePaused = true;
                    FleePauseTimeLeft = fleeTemplate.FleePointPauseDuration;
                    ThisAIEntity.ResetMovement();
                }
                else
                {
                    GetNewFleePoint();
                }
            }

            if (!(fleeTemplate.FleePointPauseDuration > 0) || !IsFleePaused || !(FleePauseTimeLeft < 0)) return;
            FleePauseTimeLeft = 0;
            IsFleePaused = false;
            GetNewFleePoint();
        }

        protected virtual void GetHelp()
        {
            foreach (var entity in GameState.combatEntities)
            {
                if (entity.IsPlayer() || entity.IsDead() || entity.GetTarget() != null) continue;
                if (!EntityCanHelp(entity)) continue;
                if(Vector3.Distance(ThisAIEntity.transform.position, entity.transform.position) > fleeTemplate.CallForHelpDistance) continue;
                if(FactionManager.Instance.GetCombatNodeAlignment(ThisAIEntity.ThisCombatEntity, entity) != CombatData.EntityAlignment.Ally) continue;
                entity.SetTarget(ThisAIEntity.ThisCombatEntity.GetTarget());
            }
        }

        protected virtual bool EntityCanHelp(CombatEntity entity)
        {
            return fleeTemplate.HelpingNPCList.Contains(entity.GetNPCData()) || (entity.GetNPCData().npcFamily != null && fleeTemplate.HelpingNPCFamilies.Contains(entity.GetNPCData().npcFamily));
        }

        protected virtual void GetNewFleePoint()
        {
            FleeTargetPosition = GetValidPoint(ThisAIEntity.transform.position, ThisAIEntity.transform.position.y+50,
                fleeTemplate.FleeGroundLayers);
            ThisAIEntity.StartMovement();
            MovementStateBlendCompleted = false;
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
                if (pos != basePosition) foundPoint = true;
                if (maxAttempt == 0) foundPoint = true;
            }

            return pos;
        }
        
        private Vector3 GetPoint(Vector3 basePosition, float height, LayerMask groundLayers)
        {
            Vector3 spawnPos = new Vector3(
                Random.Range(basePosition.x - fleeTemplate.FleeDistanceMax, basePosition.x + fleeTemplate.FleeDistanceMax),
                basePosition.y + height,
                Random.Range(basePosition.z - fleeTemplate.FleeDistanceMax, basePosition.z + fleeTemplate.FleeDistanceMax));
            return Physics.Raycast(spawnPos, -ThisAIEntity.transform.up, out var hit, 100, groundLayers) ? hit.point : basePosition;
        }
        
        protected virtual bool NearFleeTarget(float treshold)
        {
            return Vector3.Distance(ThisAIEntity.transform.position,
                new Vector3(FleeTargetPosition.x, ThisAIEntity.transform.position.y, FleeTargetPosition.z)) <= treshold;
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