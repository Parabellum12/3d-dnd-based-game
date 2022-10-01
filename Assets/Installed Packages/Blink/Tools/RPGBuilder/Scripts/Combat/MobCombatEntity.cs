using System;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.AI;
using BLINK.RPGBuilder.Logic;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.Templates;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BLINK.RPGBuilder.Combat
{
    public class MobCombatEntity : CombatEntity
    {
        private RPGNpc npc;
        private AIEntity AIEntity;

        #region INIT
        
        private void Awake()
        {
            CombatEvents.StatValueChanged += OnStatChange;
            if(ThisAnimator == null) ThisAnimator = GetComponent<Animator>();
        }
        
        private void OnDisable()
        {
            CombatEvents.StatValueChanged -= OnStatChange;
        }

        private List<StatCalculator.CustomStatsGroup> GetCustomStats()
        {
            List<StatCalculator.CustomStatsGroup> CustomStatsGroups = new List<StatCalculator.CustomStatsGroup>();
            StatCalculator.CustomStatsGroup npcStats = new StatCalculator.CustomStatsGroup {level = Level};
            foreach (var customStat in npc.CustomStats)
            {
                npcStats.stats.Add(customStat);
            }

            CustomStatsGroups.Add(npcStats);

            if (species == null) return CustomStatsGroups;
            {
                StatCalculator.CustomStatsGroup speciesStats = new StatCalculator.CustomStatsGroup {level = Level};
                foreach (var customStat in species.CustomStats)
                {
                    speciesStats.stats.Add(customStat);
                }
                CustomStatsGroups.Add(speciesStats);
            }

            return CustomStatsGroups;
        }

        public override void InitStats()
        {
            base.InitStats();

            List<StatCalculator.VitalityStatState> vitalityStatStates = StatCalculator.GetVitalityStatsStates(this);
            foreach (var StatsGroup in GetCustomStats())
            {
                foreach (var stat in StatsGroup.stats)
                {
                    float amount = stat.addedValue;
                    if (stat.valuePerLevel > 0)
                    {
                        amount += stat.valuePerLevel * Level;
                    }

                    if (stat.overrideMinValue)
                    {
                       GetStats()[stat.statID].currentMinValue = stat.minValue;
                    }
                    if (stat.overrideMaxValue)
                    {
                        GetStats()[stat.statID].currentMaxValue = stat.maxValue;
                    }
                    
                    StatCalculator.HandleStat(this, GameDatabase.Instance.GetStats()[stat.statID], GetStats()[stat.statID], amount, stat.Percent, StatCalculator.TemporaryStatSourceType.none);
                    
                    RPGStat statEntry = GameDatabase.Instance.GetStats()[stat.statID];
                    if (statEntry.isVitalityStat)
                    {
                        float startValue = stat.overrideStartPercentage
                            ? stat.startPercentage
                            : statEntry.startPercentage;
                        startValue /= 100f;
                        GetStats()[stat.statID].currentValue = GetStats()[stat.statID].currentMaxValue * startValue;
                    }
                }
            }
            
            StatCalculator.ResetLinkedStats(this, true);
            StatCalculator.ProcessLinkedStats(this, true);
            
            StatCalculator.ApplyVitalityStatsStates(vitalityStatStates, this);
            
            AIEntity.DefaultMovementSpeed = RPGBuilderUtilities.getCurrentMoveSpeed(this);
            AIEntity.SetMovementSpeed(RPGBuilderUtilities.getCurrentMoveSpeed(this));
        }

        public override void InitNPC(RPGNpc npcTemplate, CombatEntity owner)
        {
            ownerEntity = owner == null ? this : owner;
            npc = npcTemplate;
            
            if (Spawner != null)
            {
                if (Spawner.OverrideFaction)
                {
                    faction = Spawner.Faction;  
                }
                else
                {
                    faction = ownerEntity != this ? owner.GetFaction() : GameDatabase.Instance.GetFactions()[npc.factionID];
                }
                
                if (Spawner.OverrideSpecies)
                {
                    species = Spawner.Species;  
                }
                else
                {
                    if (npc.speciesID != -1)
                    {
                        species = GameDatabase.Instance.GetSpecies()[npc.speciesID];
                    }
                }
            }
            else
            {
                faction = ownerEntity != this ? owner.GetFaction() : GameDatabase.Instance.GetFactions()[npc.factionID];
                if (npc.speciesID != -1)
                {
                    species = GameDatabase.Instance.GetSpecies()[npc.speciesID];
                }
            }
            

            AIEntity = gameObject.GetComponent<AIEntity>();
            if (AIEntity == null)
            {
                Debug.LogError("Could not find AI ENTITY on the NPC Template");
                Destroy(gameObject);
            }

            AIEntity.ThisCombatEntity = this;
            AIEntity.SetSpawnPosition(AIEntity.transform.position);
            AIEntity.StartCoroutine(AIEntity.EnterNewPhase(npc.Phases[0].PhaseTemplate.TransitionDuration, 0));
            
            if(npc.isNameplateEnabled) UIEvents.Instance.OnRegisterNameplate(this);
            
            if (npc.Phases[0].PhaseTemplate.SpawnedAnimationTemplate != null)
            {
                GameEvents.Instance.OnTriggerAnimationTemplate(this, npc.Phases[0].PhaseTemplate.SpawnedAnimationTemplate);
            }
            
            InitNPCLevel();
            InitStats();
        }

        public override void InitNPCLevel()
        {
            int finalLevel = 1;
            if (GetSpawner() != null)
            {
                if (GetSpawner().OverrideLevels)
                {
                    finalLevel = GetSpawner().ScaleWithPlayer
                        ? RPGBuilderUtilities.getCurrentPlayerLevel()
                        : Random.Range(GetSpawner().MinLevel, GetSpawner().MaxLevel + 1);
                }
                else
                {
                    finalLevel = npc.isScalingWithPlayer
                        ? RPGBuilderUtilities.getCurrentPlayerLevel()
                        : Random.Range(npc.MinLevel, npc.MaxLevel + 1);
                }
            }
            else
            {
                finalLevel = npc.isScalingWithPlayer
                    ? RPGBuilderUtilities.getCurrentPlayerLevel()
                    : Random.Range(npc.MinLevel, npc.MaxLevel + 1);
            }

            Level = finalLevel;
        }

        public override void InitCollisions(Collider localCollider)
        {
            foreach (var entity in GameState.combatEntities)
            {
                if(entity == null) continue;
                Physics.IgnoreCollision(localCollider,
                    entity == GameState.playerEntity
                        ? entity.gameObject.GetComponent<CharacterController>()
                        : entity.gameObject.GetComponent<Collider>());
            }
        }

        #endregion

        #region EVENTS

        private void OnDestroy()
        {
            if(GameState.Instance == null || GameState.playerEntity == null) return;
            if(GameState.playerEntity.GetTarget() == this) GameState.playerEntity.ResetTarget();
            RemoveFromEntityList();
            OnMouseExit();
        }
        
        private void OnStatChange(CombatEntity entity, RPGStat stat, float cur, float max)
        {
            if(entity != this) return;
            
            // HEALTH CHANGE
            if (stat == GameDatabase.Instance.GetHealthStat())
            {
                UIEvents.Instance.OnUpdateNameplateBar(entity);
            }
        }

        #endregion
        #region COMBAT INFO

        public override RPGAbility.RPGAbilityRankData GetCurrentAbilityRank(RPGAbility ability, bool abMustBeKnown)
        {
            return ability.ranks[0];
        }
        
        public override bool IsAbilityReady(RPGAbility ability)
        {
            return Time.timeSinceLevelLoad >= AIEntity.GetAbilities()[ability.ID].nextUse;
        }

        public override void StartAbilityCooldown(RPGAbility.RPGAbilityRankData rank, int abilityID)
        {
            var finalCD = rank.cooldown;
            float CDRecoverySpeed = CombatUtilities.GetTotalOfStatType(this, RPGStat.STAT_TYPE.CD_RECOVERY_SPEED);

            if (CDRecoverySpeed != 0)
            {
                CDRecoverySpeed /= 100;
                finalCD -= finalCD * CDRecoverySpeed;
            }
            
            AIEntity.GetAbilities()[abilityID].nextUse = finalCD;
        }

        public override CombatEntity GetTarget()
        {
            return CurrentTarget;
        }
        
        public override void SetTarget(CombatEntity target)
        {
            base.SetTarget(target);
            CurrentTarget = target;
            AIEntity.AlterThreatTable(target, 0);
            if(!InCombat) EnterCombat();
        }

        public override void ResetTarget()
        {
            CurrentTarget = null;
        }
        
        public override void EnterCombat()
        {
            base.EnterCombat();
            AIEntity.EnterCombat();
        }
        
        public override bool CanStartCasting()
        {
            return true;
        }

        public override void InitCasting(RPGAbility thisAbility, RPGAbility.RPGAbilityRankData rankREF)
        {
            base.InitCasting(thisAbility, rankREF);
        }
        
        protected override void CastingCompleted()
        {
            EndAbility(CurrentAbilityCastedCurRank.AIAttackTime);
            
            if ((CurrentAbilityCastedCurRank.targetType == RPGAbility.TARGET_TYPES.TARGET_PROJECTILE ||
                 CurrentAbilityCastedCurRank.targetType == RPGAbility.TARGET_TYPES.TARGET_INSTANT)
                && CurrentTargetCasted.IsDead())
            {
                ResetCasting();
                return;
            }

            GameEvents.Instance.OnTriggerVisualEffectsList(this, CurrentAbilityCastedCurRank.VisualEffectEntries, ActivationType.Completed);
            GameEvents.Instance.OnTriggerAnimationsList(this, CurrentAbilityCastedCurRank.AnimationEntries, ActivationType.Completed);
            GameEvents.Instance.OnTriggerSoundsList(this, CurrentAbilityCastedCurRank.SoundEntries, ActivationType.Completed, transform);
            
            CombatManager.Instance.HandleAbilityTypeActions(this, CurrentTargetCasted, CurrentAbilityCasted, CurrentAbilityCastedCurRank, false);

            if (!CurrentAbilityCastedCurRank.startCDOnActivate)
            {
                StartAbilityCooldown(CurrentAbilityCastedCurRank, CurrentAbilityCasted.ID);
            }

            CombatManager.Instance.HandleAbilityCost(this, CurrentAbilityCastedCurRank);
            ResetCasting();
        }

        public override void ResetCasting()
        {
            base.ResetCasting();
            CombatEvents.Instance.OnStoppedCastingAbility(this);
        }

        public override void InterruptCastActions()
        {
            base.InterruptCastActions();
            AIEntity.SetInCombatState(false);
        }

        public override void SetProjectileRotation(GameObject projectile, RPGAbility.RPGAbilityRankData rank,
            float yOffset)
        {
            projectile.transform.LookAt(new Vector3(CurrentTarget.transform.position.x, CurrentTarget.transform.position.y + 1, CurrentTarget.transform.position.z));
        }

        public override List<RPGStat.VitalityActions> GetAllVitalityActions(List<RPGStat.VitalityActions> actions, RPGStat stat, int statIndex)
        {
            foreach (var extraVitAction in npc.CustomStats.Where(npcStat => npcStat.statID == stat.ID).SelectMany(npcStat => npcStat.vitalityActions))
            {
                actions.Insert(0, extraVitAction);
            }

            if (!GameDatabase.Instance.GetSpecies().TryGetValue(npc.speciesID, out var speciesREF)) return actions;
            {
                foreach (var extraVitAction in speciesREF.CustomStats.Where(npcStat => npcStat.statID == stat.ID).SelectMany(npcStat => npcStat.vitalityActions))
                {
                    actions.Insert(0, extraVitAction);
                }
            }

            return actions;
        }
        
        public override bool IsPlayer()
        {
            return false;
        }
        public override bool IsNPC()
        {
            return true;
        }
        
        public override RPGNpc GetNPCData()
        {
            return npc;
        }
        public override NPCSpawner GetSpawner()
        {
            return Spawner;
        }
        public override AIEntity GetAIEntity()
        {
            return AIEntity;
        }

        #endregion
        
        #region Combat Loops


        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            
            if(Knockedback) HandleKnockback();
            if (Casting) HandleCasting();
        }

        protected override void HandleCombatState()
        {
            LastCombatActionTimer += Time.deltaTime;
            if (!(LastCombatActionTimer >= GameDatabase.Instance.GetCombatSettings().ResetCombatDuration)) return;
            if (CurrentTarget == null)
            {
                ResetCombat();
            }
        }
        
        protected override void HandleCasting()
        {
            CurrentCastProgress += Time.deltaTime;

            if (CurrentAbilityCastedCurRank.animationTriggered && EffectTriggered)
            {
                EffectTriggered = false;
                CurrentCastProgress = TargetCastTime;
            }

            if (!(CurrentCastProgress >= TargetCastTime)) return;

            CastingCompleted();
        }

        #endregion

        #region COMBAT EVENTS

        public override void InitAbility(RPGAbility ab, RPGAbility.RPGAbilityRankData rank)
        {
            
        }
        
        public override void EndAbility(float attackTime)
        {
            AIEntity.StartCoroutine(AIEntity.EndAttackState(attackTime));
        }
        
        public override void TakeDamage(CombatCalculations.DamageResult result, RPGAbility.RPGAbilityRankData abilityRank, int alteredStatID)
        {
            base.TakeDamage(result, abilityRank, alteredStatID);

            if (result.caster.IsPlayer() && result.caster.GetTarget() == this &&
                !GameState.playerEntity.IsAutoAttacking())
            {
                GameState.playerEntity.SetAutoAttacking(true);
            }

            UIEvents.Instance.OnEnableNameplate(this);
            
            bool isHealthDamage = alteredStatID == GameDatabase.Instance.GetCombatSettings().HealthStatID;
            
            if (isHealthDamage && !result.caster.IsDead()) AIEntity.AlterThreatTable(result.caster, (int)result.DamageAmount);
            if (npc.isDummyTarget && result.DamageAmount >= GetStats()[alteredStatID].currentValue)
            {
                CombatCalculations.HealResult dummyHealResult = new CombatCalculations.HealResult
                {
                    caster = this, target = this, HealAmount = (int) GetStats()[alteredStatID].currentMaxValue
                };
                Heal(dummyHealResult, alteredStatID);
            }
            else
            {
                CombatUtilities.UpdateCurrentStatValue(this, alteredStatID, -result.DamageAmount);
            }

            if (!GameState.inCombatOverriden && !Dead)
            {
                if (!InCombat)
                {
                    EnterCombat();
                }

                if (!result.caster.IsInCombat() && (!result.caster.IsPlayer() || result.caster.IsPlayer() && GameDatabase.Instance.GetCombatSettings().AutomaticCombatStates))
                {
                    result.caster.EnterCombat();
                }
            }
        }

        public override void EntityDeath()
        {
            base.EntityDeath();

            if (EconomyUtilities.ShouldRewardPlayer(this))
            {
                EconomyUtilities.GenerateDroppedLoot(GetNPCData(), this);
                LevelingManager.Instance.GenerateMobEXP(npc, this);
                FactionManager.Instance.GenerateMobFactionReward(npc);
                CombatEvents.Instance.OnPlayerKilledNPC(npc);
            }
            
            if (ownerEntity != this)
            {
                ownerEntity.GetCurrentPets().Remove(this);
                CombatEvents.Instance.OnPlayerPetUpdate();
            }

            if (Spawner != null)
            {
                Spawner.SpawningCoroutines.Add(Spawner.StartCoroutine(Spawner.ExecuteSpawner(Spawner.OverrideRespawn
                    ? Random.Range(Spawner.MinRespawn, Spawner.MaxRespawn)
                    : Random.Range(npc.MinRespawn, npc.MaxRespawn))));
            }
            
            ThisAnimator.Rebind();
            ThisAnimator.SetTrigger("Death");
            AIEntity.StartCoroutine(AIEntity.Death());
            RemoveFromEntityList();
        }
        
        public override void ResetCombat()
        {
            base.ResetCombat();

            if (npc.InstantlyHealAfterCombat)
            {
                foreach (var t in Stats.Where(t => t.Value.stat.isVitalityStat))
                {
                    t.Value.currentValue = (int) t.Value.currentMaxValue;
                    CombatEvents.Instance.OnStatValueChanged(this, GameDatabase.Instance.GetHealthStat(),
                        CombatUtilities.GetCurrentStatValue(this, t.Value.stat.ID),
                        CombatUtilities.GetMaxStatValue(this, t.Value.stat.ID));
                }
                CombatEvents.Instance.OnStatsChanged(this);
            }

            AIEntity.ResetCombat();
        }
        
        public override void DisableCombat()
        {
            AIEntity.ClearThreatTable();
            for (var x = 0; x < States.Count; x++)
            {
                EndStateEffect(x);
            }
        }
        
        public override void InitStun(CombatEntity attacker)
        {
            base.InitStun(attacker);
            CombatManager.Instance.HandleMobAggro(attacker, this);
            AIEntity.ResetMovement();
        }

        public override void ResetStun()
        {
            AIEntity.StartMovement();
        }
        
        public override void InitStandTime(RPGAbility.RPGAbilityRankData rank)
        {
            
        }
        
        public override void InitCastSlow(RPGAbility.RPGAbilityRankData rank)
        {
            
        }
        
        private Vector3 knockBackTarget;
        private float knockbackDistanceRequired;
        private Vector3 knockbackStartPOS;
        public override void InitKnockback(CombatEntity attacker, float distance)
        {
            AIEntity.ResetMovement();
            AIEntity.InitKnockback();
            knockbackDistanceRequired = distance;
            distance *= 5;
            knockbackStartPOS = transform.position;
            Knockedback = true;
            AIEntity.EntityAgent.angularSpeed = 0;
            knockBackTarget = (transform.position - attacker.transform.position).normalized * distance;
            AIEntity.EntityAgent.velocity = knockBackTarget;
        }
        
        public override void ResetKnockback()
        {
            Knockedback = false;
            AIEntity.EntityAgent.angularSpeed = AIEntity.GetCurrentNPCPreset().NavmeshAgentAngularSpeed;
            AIEntity.ResetMovement();
            AIEntity.ResetKnockback();
        }
        
        public override void HandleKnockback()
        {
            if(Dead) ResetKnockback();
            if (!(Vector3.Distance(knockbackStartPOS, transform.position) >= knockbackDistanceRequired)) return;
            ResetKnockback();
        }
        
        public override void RemoveFromThreatTable(CombatEntity entity)
        {
            AIEntity.RemoveEntityFromThreatTable(entity);
        }
        
        public override void InitAnimation(AnimationEntry visualAnimation, string parameterName)
        {
            StartCoroutine(PlayAnimation(visualAnimation, parameterName));
        }

        public override void RemoveFromEntityList()
        {
            foreach (var npcSpawner in GameState.allNPCSpawners)
            {
                if (npcSpawner.CurrentNPCs.Contains(this))
                {
                    npcSpawner.CurrentNPCs.Remove(this);
                }
                else if (npcSpawner.CurrentPersistentNPCs.Contains(this))
                {
                    npcSpawner.CurrentPersistentNPCs.Remove(this);
                }
            }
            GameState.Instance.UnregisterEntity(this);
        }

        #endregion

        #region INTERACTIONS

        private void HandleInteraction(CombatData.EntityAlignment thisNodeAlignment, bool click)
        {
            if (click && (thisNodeAlignment == CombatData.EntityAlignment.Enemy ||
                          thisNodeAlignment == CombatData.EntityAlignment.Neutral))
            {
                if (npc == null || !npc.isTargetable) return;
                GameState.playerEntity.SetTarget(this);
                return;
            }
            
            if (Vector3.Distance(transform.position, GameState.playerEntity.transform.position) < AIEntity.GetCurrentNPCPreset().InteractionDistanceMax)
            {
                if (!UIEvents.Instance.IsPanelOpen("NPC_Interactions")) 
                    UIEvents.Instance.OnShowInteractionsPanel(this);
                AIEntity.InitPlayerInteraction();
            }
            else
            {
                if (GameState.playerEntity.controllerEssentials.GETControllerType() != RPGBuilderGeneralSettings.ControllerTypes.TopDownClickToMove)
                {
                    UIEvents.Instance.OnShowAlertMessage("This is too far", 3);
                }
            }

        }

        #endregion

        #region ON MOUSE EVENTS

        protected override void OnMouseDown()
        {
            if (UIEvents.Instance.CursorHoverUI) return;
            if (Dead) return;
            if (npc.isTargetable)
            {
                GameState.playerEntity.SetTarget(this);
            }
        }

        protected override void OnMouseOver()
        {
            if (Dead) return;
            if (UIEvents.Instance.CursorHoverUI)
            {
                UIEvents.Instance.OnSetCursorToDefault();
                return;
            }
            
            CombatData.EntityAlignment thisNodeAlignment = FactionManager.Instance.GetCombatNodeAlignment(GameState.playerEntity, this);

            if (Input.GetMouseButtonUp(1))
            {
                HandleInteraction(thisNodeAlignment, true);
                if (FactionManager.Instance.GetCombatNodeAlignment(GameState.playerEntity, this) != CombatData.EntityAlignment.Ally)
                {
                    GameState.playerEntity.SetAutoAttacking(true);
                }
            }

            
            if (npc.isDialogue)
            {
                UIEvents.Instance.OnSetNewCursor(CursorType.InteractiveObject);
            }
            if (npc.isMerchant)
            {
                UIEvents.Instance.OnSetNewCursor(CursorType.Merchant);
            }
            if (npc.isQuestGiver)
            {
                UIEvents.Instance.OnSetNewCursor(CursorType.QuestGiver);
            }

            if (thisNodeAlignment == CombatData.EntityAlignment.Enemy)
            {
                UIEvents.Instance.OnSetNewCursor(CursorType.EnemyEntity);
            }
        }

        protected override void OnMouseExit()
        {
            CombatData.EntityAlignment thisNodeAlignment = FactionManager.Instance.GetCombatNodeAlignment(GameState.playerEntity, this);
            if (npc.isDialogue || npc.isQuestGiver || npc.isMerchant || thisNodeAlignment == CombatData.EntityAlignment.Enemy)
            {
                UIEvents.Instance.OnSetCursorToDefault();
            }
        }


        #endregion
        
        #region IPLAYERINTERACTABLE

        protected override bool HasInteractions()
        {
            return npc != null && (npc.isMerchant ||
                                    npc.isQuestGiver ||
                                    npc.isDialogue);
        }

        public override void Interact()
        {
            if (InCombat)
            {
                UIEvents.Instance.OnShowAlertMessage("This unit is in combat", 3);
                return;
            }
            if (!HasInteractions() || UIEvents.Instance.CursorHoverUI) return;
            if (!(Vector3.Distance(transform.position, GameState.playerEntity.transform.position) <= 3)) return;
            CombatData.EntityAlignment thisNodeAlignment = FactionManager.Instance.GetAlignment(GetFaction(),
                FactionManager.Instance.GetEntityStanceToFaction(this, GetFaction()));
            HandleInteraction(thisNodeAlignment, false);
        }

        public override void ShowInteractableUI()
        {
            var pos = transform;
            Vector3 worldPos = new Vector3(pos.position.x, pos.position.y + AIEntity.GetCurrentNPCPreset().NameplateYOffset + 1f, pos.position.z);
            var screenPos = Camera.main.WorldToScreenPoint(worldPos);
            if (WorldInteractableDisplayManager.Instance != null)
            {
                WorldInteractableDisplayManager.Instance.transform.position = new Vector3(screenPos.x, screenPos.y, screenPos.z);
                WorldInteractableDisplayManager.Instance.Show(this);
            }
        }

        public override string getInteractableName()
        {
            return "";
        }

        public override bool isReadyToInteract()
        {
            return HasInteractions() && !Dead;
        }

        public override RPGCombatDATA.INTERACTABLE_TYPE getInteractableType()
        {
            if (npc == null) return RPGCombatDATA.INTERACTABLE_TYPE.None;
            
            CombatData.EntityAlignment thisNodeAlignment = FactionManager.Instance.GetAlignment(GetFaction(),
                FactionManager.Instance.GetEntityStanceToFaction(this, GetFaction()));
            switch (thisNodeAlignment)
            {
                case CombatData.EntityAlignment.Ally:
                    return RPGCombatDATA.INTERACTABLE_TYPE.AlliedUnit;
                case CombatData.EntityAlignment.Neutral:
                    return RPGCombatDATA.INTERACTABLE_TYPE.NeutralUnit;
                case CombatData.EntityAlignment.Enemy:
                    return RPGCombatDATA.INTERACTABLE_TYPE.EnemyUnit;
                default:
                    return RPGCombatDATA.INTERACTABLE_TYPE.None;
            }
        }

        #endregion

    }
}
