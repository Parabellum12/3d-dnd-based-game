using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.Managers;
using UnityEngine;
namespace BLINK.RPGBuilder.AI
{
    public class AIStateCombat : AIState
    {
        private AIStateCombatTemplate combatTemplate;
        
        public float TimeBeforeNextCombatAction;
        public int NextAbilityID;
        protected bool RotateDuringAttack;
        
        public override void Initialize(AIEntity entity, AIStateTemplate template)
        {
            ThisAIEntity = entity;
            combatTemplate = (AIStateCombatTemplate) template;
            ThisAIEntity.GetActiveStates()[GetName()].StateLoop = ThisAIEntity.StartCoroutine(StateLoop());
        }

        public override AIState Execute()
        {
            if (ThisAIEntity.IsInCombatState()) return this;

            if (ThisAIEntity.IsTargetTooFar(ThisAIEntity.BehaviorTemplate.MaxDistanceFromTarget)) return ThisAIEntity.GetChaseState();
            return ThisAIEntity.GetCombatIdleState();
        }

        public override void Enter()
        {
            ThisAIEntity.SetInCombatState(true);
            TimeBeforeNextCombatAction = Time.timeSinceLevelLoad + Random.Range(combatTemplate.minimumAttackInterval, combatTemplate.maximumAttackInterval);
            IsActive = true;
            InitMovement();
            MovementStateBlendCompleted = false;
            ThisAIEntity.EntityAgent.velocity = Vector3.zero;
            ThisAIEntity.EntityAgent.enabled = false;
            ThisAIEntity.EntityAnimator.SetBool(ThisAIEntity.BehaviorTemplate.CombatParameterName, true);

            if (NextAbilityID != -1)
            {
                if (ThisAIEntity.GetAbilities()[NextAbilityID].ability.ranks[ThisAIEntity.GetAbilities()[NextAbilityID].abilityRank].LookAtTargetOnCombatAction)
                {
                    ThisAIEntity.LookAtTarget();
                }
                ThisAIEntity.GetAbilities()[NextAbilityID].nextUse = Time.timeSinceLevelLoad + ThisAIEntity.GetAbilities()[NextAbilityID].ability.ranks[ThisAIEntity.GetAbilities()[NextAbilityID].abilityRank].cooldown;
                CombatManager.Instance.InitAbility(ThisAIEntity.ThisCombatEntity, ThisAIEntity.GetAbilities()[NextAbilityID].ability, ThisAIEntity.GetAbilities()[NextAbilityID].ability.ranks[ThisAIEntity.GetAbilities()[NextAbilityID].abilityRank],false);
            }
        }

        public override void Exit()
        {
            NextAbilityID = -1;
            RotateDuringAttack = false;
            IsActive = false;
            MovementStateBlendCompleted = true;
            ThisAIEntity.EntityAgent.enabled = true;
            ThisAIEntity.EntityAnimator.SetBool(ThisAIEntity.BehaviorTemplate.CombatParameterName, false);
        }
        
        public override void InitMovement()
        {
            ThisAIEntity.SetHorizontalMovement(0);
            ThisAIEntity.SetVerticalMovement(0);
            MovementStateBlendCompleted = false;
        }

        public override void UpdateMovementSpeed()
        {
            
        }

        public virtual bool CanAttack()
        {
            if (ThisAIEntity.GetAbilities().Count == 0) return false;
            if (!(Time.timeSinceLevelLoad >= TimeBeforeNextCombatAction)) return false;
            int randomAbility = GetRandomAbilityID();
            if (randomAbility == -1) return false;
            if (ThisAIEntity.GetAbilities()[randomAbility].ability.ranks[ThisAIEntity.GetAbilities()[randomAbility].abilityRank].targetType == RPGAbility.TARGET_TYPES.SELF)
            {
                ThisAIEntity.AlliedEntityTarget = ThisAIEntity.ThisCombatEntity;
            }
            else if (ThisAIEntity.GetAbilities()[randomAbility].AllyAbility)
            {
                CombatEntity allyEntity = FindAlly(ThisAIEntity.GetAbilities()[randomAbility].ability
                    .ranks[ThisAIEntity.GetAbilities()[randomAbility].abilityRank].maxRange);
                if (allyEntity == null) return false;
                ThisAIEntity.AlliedEntityTarget = allyEntity;
            }
            else
            {
                if (ThisAIEntity.IsTargetTooFar(ThisAIEntity.GetAbilities()[randomAbility].DistanceRequired))
                    return false;
            }

            RotateDuringAttack = ThisAIEntity.GetAbilities()[randomAbility].ability.ranks[ThisAIEntity.GetAbilities()[randomAbility].abilityRank].LookAtTargetDuringCombatAction;
            NextAbilityID = randomAbility;
            return true;
        }

        protected virtual int GetRandomAbilityID()
        {
            List<int> abIDs = new List<int>();
            foreach (var ab in ThisAIEntity.GetAbilities())
            {
                if (ab.Value.nextUse > Time.timeSinceLevelLoad) continue;
                abIDs.Add(ab.Key);
            }

            if (abIDs.Count == 0) return -1;
            int randomID = Random.Range(0, abIDs.Count);
            return abIDs[randomID];
        }

        protected virtual CombatEntity FindAlly(float maxDistance)
        {
            foreach (var entity in GameState.combatEntities)
            {
                if (entity == ThisAIEntity.ThisCombatEntity || entity.IsDead() || 
                    FactionManager.Instance.GetCombatNodeAlignment(ThisAIEntity.ThisCombatEntity, entity) != CombatData.EntityAlignment.Ally) continue;
                if(Vector3.Distance(ThisAIEntity.transform.position, entity.transform.position) > maxDistance) continue;
                return entity;
            }
            return ThisAIEntity.ThisCombatEntity;
        }
        
        protected virtual IEnumerator StateLoop()
        {
            while(true)
            {
                if (IsActive)
                {
                    if(RotateDuringAttack) ThisAIEntity.LookAtTarget(ThisAIEntity.BehaviorTemplate.LookAtTargetSpeed);
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