using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.Templates;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace BLINK.RPGBuilder.AI
{
    public class AIEntity : MonoBehaviour
    {
        [HideInInspector] public CombatEntity ThisCombatEntity;
        [HideInInspector] public float nextTargetCheck;

        [HideInInspector] public NavMeshAgent EntityAgent;
        [HideInInspector] public Animator EntityAnimator;

        [HideInInspector] public float DefaultMovementSpeed;

        public AIState currentState;
        public AIBehaviorTemplate BehaviorTemplate;
        
        protected GameObject npcPresetGameObject;

        protected bool PhaseInitialized;
        
        protected bool inCombatState;
        public bool IsInCombatState()
        {
            return inCombatState;
        }
        public void SetInCombatState(bool value)
        {
            inCombatState = value;
        }
        
        protected bool inPlayerInteractionState;
        public bool IsPlayerInteractionState()
        {
            return inPlayerInteractionState;
        }
        public void SetInPlayerInteractionState(bool value)
        {
            inPlayerInteractionState = value;
        }
        
        protected Vector3 spawnPosition;
        public Vector3 GetSpawnPosition()
        {
            return spawnPosition;
        }
        public void SetSpawnPosition(Vector3 pos)
        {
            spawnPosition = pos;
        }

        [HideInInspector] public CombatEntity AlliedEntityTarget;
        
        protected float nextPhaseCheck;
        [HideInInspector] public int currentPhaseIndex = 0;
        protected bool hasNextPhase;
        protected bool inPhaseTransition;
        public bool IsInPhaseTransition()
        {
            return inPhaseTransition;
        }
        public void SetInPhaseTransition(bool value)
        {
            inPhaseTransition = value;
        }

        protected bool HasFleeState;
        protected bool AlreadyFled;
        protected float nextFleeCheck;
        
        protected float movementSpeed;
        public virtual float GetMovementSpeed()
        {
            return movementSpeed;
        }
        public virtual void SetMovementSpeed(float newSpeed)
        {
            movementSpeed = newSpeed;
            EntityAgent.speed = movementSpeed;
            EntityAgent.acceleration = movementSpeed;
            if(currentState != null) currentState.UpdateMovementSpeed();
            if (CanAnimate()) EntityAnimator.SetFloat(movementAnimationSpeed, movementSpeed/DefaultMovementSpeed);
        }
        
        protected Dictionary<CombatEntity, AIData.ThreatTableData> ThreatTable = new Dictionary<CombatEntity, AIData.ThreatTableData>();

        public Dictionary<CombatEntity, AIData.ThreatTableData> GetThreatTable()
        {
            return ThreatTable;
        }
        
        protected Dictionary<string, AIData.ActiveState> ActiveStates = new Dictionary<string, AIData.ActiveState>();
        public Dictionary<string, AIData.ActiveState> GetActiveStates()
        {
            return ActiveStates;
        }

        protected Dictionary<int, AIData.AIActiveAbility> abilities = new Dictionary<int, AIData.AIActiveAbility>();
        public Dictionary<int, AIData.AIActiveAbility> GetAbilities()
        {
            return abilities;
        }

        protected static readonly int horizontalMovementParameter = Animator.StringToHash("HorizontalMovement");
        protected static readonly int verticalMovementParameter = Animator.StringToHash("VerticalMovement");

        protected Vector2 movementDirection = Vector2.zero;
        protected static readonly int inCombat = Animator.StringToHash("InCombat");
        protected static readonly int movementAnimationSpeed = Animator.StringToHash("MovementAnimationSpeed");

        public Vector2 GetMovementDirection()
        {
            return movementDirection;
        }

        #region EVENTS

        private void OnEnable()
        {
            CombatEvents.MovementSpeedChanged += MovementSpeedChanged;
        }

        private void OnDisable()
        {
            CombatEvents.MovementSpeedChanged -= MovementSpeedChanged;
        }

        private void MovementSpeedChanged(CombatEntity entity, float speed)
        {
            if (ThisCombatEntity != entity) return;
            SetMovementSpeed(speed);
        }

        #endregion

        #region INIT

        protected virtual void InitPhaseAbilities()
        {
            abilities.Clear();
            AIPhaseAbilitiesTemplate AbilitiesTemplate = null;

            float rdmTemplate = Random.Range(0f, 100f);
            float offset = 0;
            foreach (var template in BehaviorTemplate.PotentialAbilities)
            {
                if (template.AbilitiesTemplate == null) continue;
                if (ProbabilityUtilities.RandomlyPicked(rdmTemplate, 0 + offset, template.chance))
                {
                    AbilitiesTemplate = template.AbilitiesTemplate;
                    break;
                }
                offset += template.chance;
            }

            if (AbilitiesTemplate == null) return;

            int abilitiesAdded = 0;
            foreach (var ability in AbilitiesTemplate.Abilities)
            {
                if (ability.RequirementsTemplate != null && !RequirementsManager.Instance.RequirementsMet(GameState.playerEntity, ability.RequirementsTemplate.Requirements).Result) continue;

                if (AbilitiesTemplate.CheckMaxAbilities && ability.optional)
                {
                    if (ProbabilityUtilities.RunPercentageRandom(ability.chance)) abilitiesAdded++;
                    else continue;
                }

                AIData.AIActiveAbility newActiveAbility = new AIData.AIActiveAbility
                {
                    ability = GameDatabase.Instance.GetAbilities()[ability.abilityID],
                    abilityRank = ability.abilityRank,
                    nextUse = 0,
                    usedAmount = 0,
                };

                switch (newActiveAbility.ability.ranks[newActiveAbility.abilityRank].targetType)
                {
                    case RPGAbility.TARGET_TYPES.SELF:
                        break;
                    case RPGAbility.TARGET_TYPES.CONE:
                        newActiveAbility.DistanceRequired =
                            newActiveAbility.ability.ranks[newActiveAbility.abilityRank].minRange;
                        break;
                    case RPGAbility.TARGET_TYPES.AOE:
                        newActiveAbility.DistanceRequired =
                            newActiveAbility.ability.ranks[newActiveAbility.abilityRank].AOERadius;
                        break;
                    case RPGAbility.TARGET_TYPES.LINEAR:
                        newActiveAbility.DistanceRequired =
                            newActiveAbility.ability.ranks[newActiveAbility.abilityRank].linearLength;
                        break;
                    case RPGAbility.TARGET_TYPES.PROJECTILE:
                        newActiveAbility.DistanceRequired = newActiveAbility.ability.ranks[newActiveAbility.abilityRank]
                            .projectileDistanceMaxForNPC;
                        break;
                    case RPGAbility.TARGET_TYPES.SQUARE:
                        break;
                    case RPGAbility.TARGET_TYPES.GROUND:
                        newActiveAbility.DistanceRequired =
                            newActiveAbility.ability.ranks[newActiveAbility.abilityRank].groundRange;
                        break;
                    case RPGAbility.TARGET_TYPES.GROUND_LEAP:
                        newActiveAbility.DistanceRequired =
                            newActiveAbility.ability.ranks[newActiveAbility.abilityRank].groundRange;
                        break;
                    case RPGAbility.TARGET_TYPES.TARGET_PROJECTILE:
                        newActiveAbility.DistanceRequired =
                            newActiveAbility.ability.ranks[newActiveAbility.abilityRank].maxRange;
                        break;
                    case RPGAbility.TARGET_TYPES.TARGET_INSTANT:
                        newActiveAbility.DistanceRequired =
                            newActiveAbility.ability.ranks[newActiveAbility.abilityRank].maxRange;
                        newActiveAbility.AllyAbility =
                            newActiveAbility.ability.ranks[newActiveAbility.abilityRank].UsedOnALly;
                        break;

                }

                abilities.Add(ability.abilityID, newActiveAbility);

                if (AbilitiesTemplate.CheckMaxAbilities && abilitiesAdded >= AbilitiesTemplate.MaxAbilities) break;
            }
        }

        #endregion

        #region UPDATES
        private void FixedUpdate()
        {
            if (ThisCombatEntity.IsDead() || inPhaseTransition) return;
            if (hasNextPhase) CheckNextPhase();
            if (ThisCombatEntity.IsInCombat())
            {
                if (HasFleeState && !AlreadyFled)
                {
                    CheckFleeBehavior();
                }

                if (ThisCombatEntity.GetTarget() == null)
                {
                    ThisCombatEntity.ResetCombat();
                }
                else
                {
                    CheckTargetDistance();
                }
            }
            else
            {
                if (inPlayerInteractionState)
                {
                    LookAtPlayer();
                }
            }

            RunState();
        }

        protected void CheckTargetDistance()
        {
            if (ThisCombatEntity.GetSpawner() != null && BehaviorTemplate.ResetTargetAfterDistanceFromSpawner)
            {
                if (Vector3.Distance(transform.position, ThisCombatEntity.GetSpawner().transform.position) >=
                    BehaviorTemplate.SpawnerDistanceMax)
                {
                    ThisCombatEntity.ResetCombat();
                }
            }
            if (BehaviorTemplate.ResetTargetAfterDistanceFromSpawnpoint)
            {
                if (Vector3.Distance(transform.position, spawnPosition) >=
                    BehaviorTemplate.SpawnPointDistanceMax)
                {
                    ThisCombatEntity.ResetCombat();
                }
            }
        }
        
        protected void OnDestroy()
        {
            ClearActiveStates();
            if(ThisCombatEntity.GetNPCData().isNameplateEnabled) UIEvents.Instance.OnResetNameplate(ThisCombatEntity);
        }

        #endregion

        #region STATES

        protected virtual void AddState(AIState state, AIStateTemplate template)
        {
            AIData.ActiveState newState = new AIData.ActiveState {State = Instantiate(state), StateTemplate = template};
            if (newState.State == null)
            {
                Debug.LogError("STATE was missing on NPC " + ThisCombatEntity.GetNPCData().entryName + " so we destroyed it");
                Destroy(gameObject);
            }
            ActiveStates.Add(state.name, newState);
        }

        protected virtual void RunState()
        {
            if (currentState == null || ThisCombatEntity.IsStunned() || ThisCombatEntity.IsKnockedBack()) return;
            AIState nextState = currentState.Execute();
            if (nextState == currentState) return;
            currentState.Exit();
            if (nextState == null) nextState = GetDefaultState();
            NextState(nextState);
        }

        protected virtual void NextState(AIState nextState)
        {
            nextState.Enter();
            currentState = nextState;
        }

        protected virtual void ClearActiveStates()
        {
            foreach (var activeState in ActiveStates.Values)
            {
                if(activeState.StateLoop != null) StopCoroutine(activeState.StateLoop);
                Destroy(activeState.State);
            }
            ActiveStates.Clear();
        }
        
        public AIStateIdle GetDefaultState()
        {
            return ThisCombatEntity.IsPet() ? (AIStateIdle) ActiveStates[BehaviorTemplate.DefaultPetState.name].State : (AIStateIdle) ActiveStates[BehaviorTemplate.DefaultState.name].State;
        }
        public AIStateChase GetChaseState()
        {
            if (BehaviorTemplate.ChaseState == null)
            {
                return null;
            }
            
            return (AIStateChase) ActiveStates[BehaviorTemplate.ChaseState.name].State;
        }
        public AIStateCombatIdle GetCombatIdleState()
        {
            if (BehaviorTemplate.CombatIdleState == null)
            {
                return null;
            }
            
            return (AIStateCombatIdle) ActiveStates[BehaviorTemplate.CombatIdleState.name].State;
        }
        public AIStateCombat GetCombatState()
        {
            if (BehaviorTemplate.CombatState == null)
            {
                return null;
            }
            
            return (AIStateCombat) ActiveStates[BehaviorTemplate.CombatState.name].State;
        }
        public AIStateWalkBackward GetWalkBackwardState()
        {
            if (BehaviorTemplate.WalkBackwardState == null)
            {
                return null;
            }
            
            return (AIStateWalkBackward) ActiveStates[BehaviorTemplate.WalkBackwardState.name].State;
        }
        public AIStateFlee GetFleeState()
        {
            if (BehaviorTemplate.FleeState == null)
            {
                return null;
            }
            
            return (AIStateFlee) ActiveStates[BehaviorTemplate.FleeState.name].State;
        }

        protected virtual void CheckFleeBehavior()
        {
            if (currentState != null && currentState == GetFleeState()) return;
            if (!(Time.timeSinceLevelLoad >= nextFleeCheck)) return;
            nextFleeCheck = Time.timeSinceLevelLoad + BehaviorTemplate.FleeCheckInterval;

            if (BehaviorTemplate.FleeRequirementsTemplate != null && !RequirementsManager.Instance.RequirementsMet(
                ThisCombatEntity, BehaviorTemplate.FleeRequirementsTemplate.Requirements).Result) return;
            currentState.Exit();
            currentState = GetFleeState();
            AlreadyFled = true;
            currentState.Enter();
        }

        public virtual void InitPlayerInteraction()
        {
            inPlayerInteractionState = true;
            ResetMovement();
        }

        public virtual void ResetPlayerInteraction()
        {
            inPlayerInteractionState = false;
            StartMovement();
        }
        
        public virtual void InitKnockback()
        {
            currentState.Exit();
            currentState = GetCombatIdleState();
            if (currentState == null) currentState = GetDefaultState();
        }
        
        public virtual void ResetKnockback()
        {
            if (ThisCombatEntity.IsInCombat() && ThisCombatEntity.GetTarget() != null)
            {
                currentState = GetChaseState();
                if (currentState == null)
                {
                    currentState = ThisCombatEntity.IsPet() ? ActiveStates[BehaviorTemplate.DefaultPetState.name].State : ActiveStates[BehaviorTemplate.DefaultState.name].State;
                }
                currentState.Enter();
            }
            else
            {
                currentState = ThisCombatEntity.IsPet() ? ActiveStates[BehaviorTemplate.DefaultPetState.name].State : ActiveStates[BehaviorTemplate.DefaultState.name].State;
            }
        }
        
        #endregion

        #region PHASES

        public virtual AIData.AIPhase GetCurrentPhase()
        {
            return ThisCombatEntity.GetNPCData().Phases[currentPhaseIndex];
        }
        public virtual NPCPresetTemplate GetCurrentNPCPreset()
        {
            return ThisCombatEntity.GetNPCData().Phases[currentPhaseIndex].Preset;
        }
        
        protected virtual void CheckNextPhase()
        {
            if (!(Time.timeSinceLevelLoad >= nextPhaseCheck)) return;
            nextPhaseCheck = Time.timeSinceLevelLoad + BehaviorTemplate.PhaseCheckInterval;
            
            if (ThisCombatEntity.GetNPCData().Phases[currentPhaseIndex + 1].PhaseTemplate
                .EnterPhaseRequirementsTemplate == null || RequirementsManager.Instance.RequirementsMet(ThisCombatEntity,
                ThisCombatEntity.GetNPCData().Phases[currentPhaseIndex + 1].PhaseTemplate
                    .EnterPhaseRequirementsTemplate.Requirements).Result)
            {
                StartCoroutine(EnterNewPhase(ThisCombatEntity.GetNPCData().Phases[currentPhaseIndex + 1].PhaseTemplate.TransitionDuration, currentPhaseIndex + 1));
            }
        }

        public IEnumerator EnterNewPhase(float transitionTime, int phaseIndex)
        {
            inPhaseTransition = true;
            if (phaseIndex > 0)
            {
                TriggerPhaseTemplates(ActivationType.Completed);
            }
            else
            {
               InitNPCPreset();
            }
            InitPhase(phaseIndex);
            ResetMovement();
            yield return new WaitForSeconds(transitionTime);
            if(ThisCombatEntity.IsDead()) yield break;
            StartMovement();
            if(phaseIndex > 0) InitNPCPreset();
            TriggerPhaseTemplates(ActivationType.Start);
            TriggerPhaseActions(AIData.ActivationType.Enter);
            inPhaseTransition = false;
        }

        public virtual void InitPhase(int phaseIndex)
        {
            if (ThisCombatEntity.GetNPCData().Phases.Count == 0)
            {
                Debug.LogError("PHASE COULD NOT BE FOUND FOR NPC " + ThisCombatEntity.GetNPCData().entryName + " SO IT WAS DESTROYED");
                Destroy(gameObject);
            }

            if (PhaseInitialized)
            {
                TriggerPhaseActions(AIData.ActivationType.Exit);
            }
            currentPhaseIndex = phaseIndex;
            PhaseInitialized = true;
            hasNextPhase = currentPhaseIndex < ThisCombatEntity.GetNPCData().Phases.Count - 1;
            if (ThisCombatEntity.GetNPCData().Phases[phaseIndex].PhaseTemplate.PotentialBehaviors.Count == 0)
            {
                BehaviorTemplate = GameDatabase.Instance.GetCombatSettings().DefaultAIBehaviorTemplate;
            }
            else
            {
                float rdmBehavior = Random.Range(0f, 100f);
                float offset = 0;
                foreach (var t in ThisCombatEntity.GetNPCData().Phases[phaseIndex].PhaseTemplate.PotentialBehaviors)
                {
                    if (t.BehaviorTemplate == null) continue;
                    if (rdmBehavior >= 0 + offset && rdmBehavior <= t.chance + offset)
                    {
                        BehaviorTemplate = t.BehaviorTemplate;
                    }

                    offset += t.chance;
                }

                if (BehaviorTemplate == null)
                {
                    BehaviorTemplate = GameDatabase.Instance.GetCombatSettings().DefaultAIBehaviorTemplate;
                }
            }

            if (BehaviorTemplate == null)
            {
                Debug.LogError("BEHAVIOR TEMPLATE COULD NOT BE FOUND FOR NPC " + ThisCombatEntity.GetNPCData().entryName + " SO IT WAS DESTROYED");
                Destroy(gameObject);
            }

            ClearActiveStates();
            if (ThisCombatEntity.IsPet())
            {
                AddState(BehaviorTemplate.DefaultPetState, BehaviorTemplate.DefaultPetStateTemplate);
            }
            else
            {
                AddState(BehaviorTemplate.DefaultState, BehaviorTemplate.DefaultStateTemplate);
            }

            if (BehaviorTemplate.ChaseState != null)
            {
                AddState(BehaviorTemplate.ChaseState, BehaviorTemplate.ChaseTemplate);
            }
            if (BehaviorTemplate.CombatIdleState != null)
            {
                AddState(BehaviorTemplate.CombatIdleState, BehaviorTemplate.CombatIdleTemplate);
            }
            if (BehaviorTemplate.CombatState != null)
            {
                AddState(BehaviorTemplate.CombatState, BehaviorTemplate.CombatTemplate);
            }
            if (BehaviorTemplate.WalkBackwardState != null)
            {
                AddState(BehaviorTemplate.WalkBackwardState, BehaviorTemplate.WalkBackwardTemplate);
            }
            if (BehaviorTemplate.FleeState != null)
            {
                AddState(BehaviorTemplate.FleeState, BehaviorTemplate.FleeTemplate);
                HasFleeState = true;
            }

            foreach (var activeState in ActiveStates.Values)
            {
                activeState.State.Initialize(this, activeState.StateTemplate);
            }

            if (ThisCombatEntity.IsInCombat() && ThisCombatEntity.GetTarget() != null)
            {
                currentState = GetChaseState();
                if (currentState == null)
                {
                    currentState = ThisCombatEntity.IsPet() ? ActiveStates[BehaviorTemplate.DefaultPetState.name].State : ActiveStates[BehaviorTemplate.DefaultState.name].State;
                }
            }
            else
            {
                currentState = ThisCombatEntity.IsPet() ? ActiveStates[BehaviorTemplate.DefaultPetState.name].State : ActiveStates[BehaviorTemplate.DefaultState.name].State;
            }
            currentState.Enter();
            InitPhaseAbilities();
        }

        public virtual void InitNPCPreset()
        {
            if (currentPhaseIndex > 0 && GetCurrentPhase().Preset == ThisCombatEntity.GetNPCData().Phases[currentPhaseIndex - 1].Preset) return;
            NPCPresetTemplate preset = GetCurrentPhase().Preset;
            if (npcPresetGameObject != null)
            {
                Destroy(npcPresetGameObject);
            }
            else
            {
                EntityAnimator = gameObject.AddComponent<Animator>();
                ThisCombatEntity.SetAnimator(EntityAnimator);
                EntityAgent = gameObject.AddComponent<NavMeshAgent>();

                if (ThisCombatEntity.GetNPCData().SetLayer)
                    gameObject.layer = ThisCombatEntity.GetNPCData().GameObjectLayer;
                if (ThisCombatEntity.GetNPCData().SetTag) gameObject.tag = ThisCombatEntity.GetNPCData().GameObjectTag;

                if (ThisCombatEntity.GetNPCData().isPlayerInteractable)
                    gameObject.layer = GameDatabase.Instance.GetWorldSettings().worldInteractableLayer;
            }

            npcPresetGameObject = Instantiate(preset.Prefab, transform);
            npcPresetGameObject.transform.SetParent(transform);
            npcPresetGameObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
            npcPresetGameObject.transform.localPosition = preset.Position;
            npcPresetGameObject.transform.localScale = preset.Scale;

            ThisCombatEntity.NodeSockets = GetComponentInChildren<NodeSockets>();
            
            if (ThisCombatEntity.GetNPCData().isNameplateEnabled)
            {
                if (string.IsNullOrEmpty(preset.RendererName))
                {
                    ThisCombatEntity.mainRenderer = GetComponentInChildren<Renderer>();
                }
                else
                {
                    GameObject rendererGO =
                        RPGBuilderUtilities.GetChildByName(npcPresetGameObject, preset.RendererName);
                    if (rendererGO != null)
                    {
                        ThisCombatEntity.mainRenderer = rendererGO.GetComponent<Renderer>();
                    }
                    if (ThisCombatEntity.mainRenderer == null) ThisCombatEntity.mainRenderer = GetComponentInChildren<Renderer>();
                }
                UIEvents.Instance.OnUpdateNameplate(ThisCombatEntity);
            }

            Destroy(gameObject.GetComponent<Collider>());

            EntityAnimator.runtimeAnimatorController = preset.AnimatorController;
            EntityAnimator.avatar = preset.AnimatorAvatar;
            EntityAnimator.applyRootMotion = preset.AnimatorUseRootMotion;
            EntityAnimator.updateMode = preset.AnimatorUpdateMode;
            EntityAnimator.cullingMode = preset.AnimatorCullingMode;

            EntityAgent.radius = preset.NavmeshAgentRadius;
            EntityAgent.height = preset.NavmeshAgentHeight;
            EntityAgent.angularSpeed = preset.NavmeshAgentAngularSpeed;
            EntityAgent.obstacleAvoidanceType = preset.NavmeshObstacleAvoidance;

            Collider newCollider = null;
            switch (preset.ColliderType)
            {
                case AIData.NPCColliderType.Capsule:
                    CapsuleCollider capsule = gameObject.AddComponent<CapsuleCollider>();
                    capsule.center = preset.ColliderCenter;
                    capsule.radius = preset.ColliderRadius;
                    capsule.height = preset.ColliderHeight;
                    newCollider = capsule;
                    break;
                case AIData.NPCColliderType.Sphere:
                    SphereCollider sphere = gameObject.AddComponent<SphereCollider>();
                    sphere.center = preset.ColliderCenter;
                    sphere.radius = preset.ColliderRadius;
                    newCollider = sphere;
                    break;
                case AIData.NPCColliderType.Box:
                    BoxCollider box = gameObject.AddComponent<BoxCollider>();
                    box.center = preset.ColliderCenter;
                    box.size = preset.ColliderSize;
                    newCollider = box;
                    break;
            }

            if (!ThisCombatEntity.GetNPCData().isCollisionEnabled) ThisCombatEntity.InitCollisions(newCollider);
        }

        protected virtual void TriggerPhaseTemplates(ActivationType activationType)
        {
            GameEvents.Instance.OnTriggerVisualEffectsList(ThisCombatEntity, ThisCombatEntity.GetNPCData().Phases[currentPhaseIndex].PhaseTemplate.VisualEffectEntries,
                activationType);
            GameEvents.Instance.OnTriggerAnimationsList(ThisCombatEntity, ThisCombatEntity.GetNPCData().Phases[currentPhaseIndex].PhaseTemplate.AnimationEntries,
                activationType);
            GameEvents.Instance.OnTriggerSoundsList(ThisCombatEntity, ThisCombatEntity.GetNPCData().Phases[currentPhaseIndex].PhaseTemplate.SoundEntries,
                activationType, transform);
        }

        protected virtual void TriggerPhaseActions(AIData.ActivationType activationType)
        {
            if (ThisCombatEntity.GetNPCData().Phases[currentPhaseIndex].PhaseTemplate.ActionsTemplate == null) return;
            foreach (var action in ThisCombatEntity.GetNPCData().Phases[currentPhaseIndex].PhaseTemplate.ActionsTemplate.PhaseActions)
            {
                if (action.GameActionsTemplate == null) continue;
                if (action.Activation != activationType) continue;
                if (action.RequirementsTemplate == null || RequirementsManager.Instance.RequirementsMet(action.RequirementsTarget == RPGCombatDATA.TARGET_TYPE.Caster ? ThisCombatEntity : ThisCombatEntity.GetTarget(), action.RequirementsTemplate.Requirements).Result)
                {
                    GameActionsManager.Instance.TriggerGameActions(action.ActionsTarget == RPGCombatDATA.TARGET_TYPE.Caster ? ThisCombatEntity : ThisCombatEntity.GetTarget(), action.GameActionsTemplate.GameActions);
                }
            }
        }

        #endregion
        
        #region INFO

        public bool CanAnimate()
        {
            return EntityAnimator.runtimeAnimatorController != null;
        }

        #endregion

        #region COMBAT

        public virtual void EnterCombat()
        {
            EntityAnimator.SetBool(inCombat, true);
            currentState.Exit();
            currentState = GetChaseState();
            if (currentState == null) currentState = GetDefaultState();
            currentState.Enter();
        }
        
        public virtual void ResetCombat()
        {
            EntityAnimator.SetBool(inCombat, false);
            ClearThreatTable();
            if (!ThisCombatEntity.IsDead() && currentState != null)
            {
                if (ThisCombatEntity.GetNPCData().ResetPhaseAfterCombat && currentPhaseIndex != 0)
                {
                    ResetMovement();
                    InitPhase(0);
                    InitNPCPreset();
                    StartMovement();
                }
                else
                {
                    currentState.Exit();
                    currentState = GetDefaultState();
                    currentState.Enter();
                }
            }
        }
        
        public virtual IEnumerator Death()
        {
            if (currentState != null)
            {
                currentState.Exit();
                currentState = null;
            }
            ClearActiveStates();
            EntityAgent.enabled = false;
            yield return new WaitForSeconds(ThisCombatEntity.GetNPCData().corpseDespawnTime);
            Destroy(gameObject);
        }

        public virtual IEnumerator EndAttackState(float attackTime)
        {
            yield return new WaitForSeconds(attackTime);
            inCombatState = false;
        }
        
        public virtual CombatEntity SearchTarget (float viewDistance, float viewAngle, float autoAggroDistance)
        {
            foreach (var entity in GameState.combatEntities)
            {
                if (entity == null || entity == ThisCombatEntity || entity.IsStealth() || entity.IsDead()) continue;
                if(!CanAggroTarget(entity)) continue;
                float distance = Vector3.Distance(transform.position, entity.transform.position);
                if(distance > viewDistance) continue;
                if (distance <= autoAggroDistance) return entity;
                var pointDirection = entity.transform.position - transform.position;
                var angle = Vector3.Angle(transform.forward, pointDirection);
                if (angle < viewAngle)
                {
                    return entity;
                }
            }

            return null;
        }

        public bool CanAggroTarget(CombatEntity target)
        {
            CombatData.EntityAlignment targetAlignment = FactionManager.Instance.GetCombatNodeAlignment(ThisCombatEntity, target);

            switch (targetAlignment)
            {
                case CombatData.EntityAlignment.Ally when BehaviorTemplate.CanAggroAlly:
                case CombatData.EntityAlignment.Neutral when BehaviorTemplate.CanAggroNeutral:
                case CombatData.EntityAlignment.Enemy when BehaviorTemplate.CanAggroEnemy:
                    return true;
                default:
                    return false;
            }
        }
        

        #endregion

        #region AGENT

        public virtual void MoveAgent(Vector3 position)
        {
            if (inPhaseTransition) return;
            if (!EntityAgent.enabled) return;
            EntityAgent.SetDestination(position);
        }
        
        public virtual void ResetMovement()
        {
            SetHorizontalMovement(0);
            SetVerticalMovement(0);
            HandleMovementDirectionsInstant();
            if (EntityAgent.enabled)
            {
                EntityAgent.ResetPath();
                EntityAgent.velocity = Vector3.zero;
            }
        }
        
        public virtual void StartMovement()
        {
            if(currentState != null) currentState.InitMovement();
        }
        
        public bool IsPathAllowed(Vector3 point)
        {
            NavMeshPath path = new NavMeshPath();
            return NavMesh.CalculatePath(transform.position, point, NavMesh.AllAreas, path);
        }

        #endregion

        #region ROTATION

        public void LookAtPlayer()
        {
            Vector3 targetPostition = new Vector3(GameState.playerEntity.transform.position.x,
                transform.position.y, GameState.playerEntity.transform.position.z);
            transform.LookAt(targetPostition);
        }
        
        public void LookAtTarget()
        {
            Vector3 targetPostition = new Vector3(ThisCombatEntity.GetTarget().transform.position.x,
                transform.position.y, ThisCombatEntity.GetTarget().transform.position.z);
            transform.LookAt(targetPostition);
        }

        public void LookAtTarget(float lookAtTargetSpeed)
        {
            if (ThisCombatEntity.GetTarget() == null) return;
            var targetRotation = Quaternion.LookRotation(ThisCombatEntity.GetTarget().transform.position - transform.position, Vector3.up);
            targetRotation.x = 0;
            targetRotation.z = 0;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lookAtTargetSpeed * Time.deltaTime);
        }

        #endregion

        #region POSITION

        public bool IsTargetTooFar(float maximumDistance)
        {
            return Vector3.Distance(transform.position, ThisCombatEntity.GetTarget().transform.position) > maximumDistance;
        }
        public bool IsTargetTooClose(float minimumDistance)
        {
            return Vector3.Distance(transform.position, ThisCombatEntity.GetTarget().transform.position) < minimumDistance;
        }
        public bool IsTargetTooClose(float distance, float modifier)
        {
            return Vector3.Distance(transform.position, ThisCombatEntity.GetTarget().transform.position) <
                   distance * modifier;
        }
        public bool IsTargetWithinRange(float distance)
        {
            return Vector3.Distance(transform.position, ThisCombatEntity.GetTarget().transform.position) <= distance;
        }

        public void TeleportEntity(Vector3 position)
        {
            transform.position = position;
        }
        #endregion

        #region ANIMATIONS

        public virtual void SetVerticalMovement(float value)
        {
            movementDirection.y = value;
        }
        public virtual void SetHorizontalMovement(float value)
        {
            movementDirection.x = value;
        }
        public virtual Vector2 GetHorizontalMovement()
        {
            return movementDirection;
        }
        
        public virtual bool HandleMovementDirectionsBlend()
        {
            if (!CanAnimate()) return true;
            EntityAnimator.SetFloat(horizontalMovementParameter, movementDirection.x, 0.1f, Time.deltaTime);
            EntityAnimator.SetFloat(verticalMovementParameter, movementDirection.y, 0.1f, Time.deltaTime);
            return Math.Abs(EntityAnimator.GetFloat(horizontalMovementParameter) - movementDirection.x) < 0.01f && Math.Abs(EntityAnimator.GetFloat(verticalMovementParameter) - movementDirection.y) < 0.01f;
        }
        
        public virtual void HandleMovementDirectionsInstant()
        {
            if (!CanAnimate()) return;
            EntityAnimator.SetFloat(horizontalMovementParameter, movementDirection.x);
            EntityAnimator.SetFloat(verticalMovementParameter, movementDirection.y);
        }

        public virtual void SetMovementSpeedModifier(float modifier)
        {
            if (!CanAnimate()) return;
            float MoveSpeedModifier = movementSpeed / DefaultMovementSpeed;
            EntityAnimator.SetFloat(movementAnimationSpeed, MoveSpeedModifier < 1 ? MoveSpeedModifier : modifier);
        }

        #endregion

        #region THREAT TABLE

        public virtual void AlterThreatTable(CombatEntity entity, int amount)
        {
            if (ThreatTable.ContainsKey(entity))
            {
                ThreatTable[entity].threatAmount += amount;
            }
            else
            {
                ThreatTable.Add(entity, new AIData.ThreatTableData {threatAmount = amount});
            }
            
            CombatEntity highestThreatEntity = GetHighestThreatEntity();
            if(highestThreatEntity != ThisCombatEntity.GetTarget()) ThisCombatEntity.SetTarget(highestThreatEntity);
        }

        public virtual CombatEntity GetHighestThreatEntity()
        {
            return ThreatTable.Aggregate((x, y) => x.Value.threatAmount > y.Value.threatAmount ? x : y).Key;
        }

        public virtual void RemoveEntityFromThreatTable(CombatEntity entity)
        {
            if (ThreatTable.ContainsKey(entity))
            {
                ThreatTable.Remove(entity);
            }
        }

        
        public virtual void ClearThreatTable()
        {
            ThreatTable.Clear();
        }

        #endregion

        #region EVENTS

        #endregion
    }
}
