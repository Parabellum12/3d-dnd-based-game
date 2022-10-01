using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Logic;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.Templates;
using BLINK.RPGBuilder.Utility;
using BLINK.RPGBuilder.World;
using UnityEngine;

namespace BLINK.RPGBuilder.Combat
{
    [RequireComponent(typeof(RPGBCharacterControllerEssentials))]
    [RequireComponent(typeof(PlayerAppearance))]
    [RequireComponent(typeof(Animator))]
    public class PlayerCombatEntity : CombatEntity
    {
        public RPGBCharacterControllerEssentials controllerEssentials;
        public PlayerAppearance appearance;
        public GroundIndicatorManager indicatorManagerRef;
        public bool GroundCasting;

        
        public class AutoAttackData
        {
            public int CurrentAutoAttackAbilityID = -1;
            public RPGItem WeaponItem;
        }

        public readonly AutoAttackData autoAttackData = new AutoAttackData();
        private float _nextAutoAttack;
        private bool isAutoAttacking;

        public void SetAutoAttacking(bool value)
        {
            isAutoAttacking = value;
        }

        public bool IsAutoAttacking()
        {
            return isAutoAttacking;
        }
        
        private Vector3 _currentProjectileClickPos;

        #region INIT

        private void Awake()
        {
            if(controllerEssentials == null) controllerEssentials = GetComponent<RPGBCharacterControllerEssentials>();
            if(appearance == null) appearance = GetComponent<PlayerAppearance>();
            if(indicatorManagerRef == null) indicatorManagerRef = GetComponentInChildren<GroundIndicatorManager>();
            if(NodeSockets == null) NodeSockets = GetComponent<NodeSockets>();
            if(ThisAnimator == null) ThisAnimator = GetComponent<Animator>();
            ownerEntity = this;
        }

        protected override void Start()
        {
            base.Start();
            faction = GameDatabase.Instance.GetFactions()[GameDatabase.Instance.GetRaces()[Character.Instance.CharacterData.RaceID].factionID];
            NodeSockets = GetComponentInChildren<NodeSockets>();
        }
        
        private void OnEnable()
        {
            GameEvents.NewGameSceneLoaded += InitializeStates;
            GameEvents.NewGameSceneLoaded += InitializePersistentStats;
            GameEvents.StartNewGameSceneLoad += NewSceneLoad;
            CombatEvents.PlayerRequestedRespawned += RespawnPlayerToGraveyard;
            CombatEvents.PlayerPetsFollow += TriggerPetsFollow;
            CombatEvents.PlayerPetsStay += TriggerPetsStay;
            CombatEvents.PlayerPetsAggro += TriggerPetsAggro;
            CombatEvents.PlayerPetsDefend += TriggerPetsDefend;
            CombatEvents.PlayerPetsReset += TriggerPetsReset;
            CombatEvents.PlayerPetsAttack += TriggerPetsAttack;
        }

        private void OnDisable()
        {
            GameEvents.NewGameSceneLoaded -= InitializeStates;
            GameEvents.NewGameSceneLoaded -= InitializePersistentStats;
            GameEvents.StartNewGameSceneLoad -= NewSceneLoad;
            CombatEvents.PlayerRequestedRespawned -= RespawnPlayerToGraveyard;
            CombatEvents.PlayerPetsFollow -= TriggerPetsFollow;
            CombatEvents.PlayerPetsStay -= TriggerPetsStay;
            CombatEvents.PlayerPetsAggro -= TriggerPetsAggro;
            CombatEvents.PlayerPetsDefend -= TriggerPetsDefend;
            CombatEvents.PlayerPetsReset -= TriggerPetsReset;
            CombatEvents.PlayerPetsAttack -= TriggerPetsAttack;
        }

        

        public override void InitStats()
        {
            base.InitStats();
            
            StatCalculator.InitCharacterStats();
        }

        private void InitializePersistentStats()
        {
            InitializeVitalityStats(Character.Instance.CharacterData.VitalityStats);
        }

        private void NewSceneLoad()
        {
            if (Shapeshifted)
            {
                ResetShapeshifting();
            }

            if (Stealth)
            {
                ResetStealth();
            }
        }

        #endregion

        #region EVENTS
        private void TriggerPetsFollow()
        {
            SetCurrentPetsMovementActionType(CombatData.PetMovementActionTypes.Follow);
            CombatEvents.Instance.OnPlayerPetUpdate();
        }

        private void TriggerPetsStay()
        {
            SetCurrentPetsMovementActionType(CombatData.PetMovementActionTypes.Stay);
            CombatEvents.Instance.OnPlayerPetUpdate();
        }

        private void TriggerPetsDefend()
        {
            SetCurrentPetsCombatActionType(CombatData.PetCombatActionTypes.Defend);
            CombatEvents.Instance.OnPlayerPetUpdate();
        }

        private void TriggerPetsAggro()
        {
            SetCurrentPetsCombatActionType(CombatData.PetCombatActionTypes.Aggro);
            CombatEvents.Instance.OnPlayerPetUpdate();
        }

        private void TriggerPetsReset()
        {
            foreach (var pet in GameState.playerEntity.GetCurrentPets())
            {
                pet.ResetTarget();
            }
            CombatEvents.Instance.OnPlayerPetUpdate();
        }
        
        private void TriggerPetsAttack()
        {
            if (CurrentTarget == null) return;
            if (FactionManager.Instance.GetCombatNodeAlignment(this, CurrentTarget) == CombatData.EntityAlignment.Ally) return;
            foreach (var pet in CurrentPets)
            {
                pet.SetTarget(CurrentTarget);
            }
        }

        #endregion
        
        #region COMBAT LOOPS
        
        
        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            
            if(InCombat) HandleCombatState();
            
            HandleStatShifting();
            HandleActiveCombos();
            
            HandleLeaping();

            if (GetTarget() != null && isAutoAttacking)
            {
                HandleAutoAttack();
            }
            
            if (Casting) HandleCasting();
            else if(Channeling) HandleChanneling();
            else if(InteractingWithObject) HandleObjectInteraction();

            if (ActiveBlocking)
            {
                HandleActiveBlocking();
                UpdateActiveBlockingUI();
            }
            
            HandleToggledAbilities();
        }

        private void Update()
        {
            if(GroundCasting) HandleGroundCasting();
        }

        private void HandleGroundCasting()
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                CombatManager.Instance.EXECUTE_GROUND_ABILITY(this, CurrentAbilityCasted, RPGCombatDATA.CombatVisualActivationType.Activate , CurrentAbilityCastedCurRank);
                GroundCasting = false;
            }
            else if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                indicatorManagerRef.HideIndicator();
                GroundCasting = false;
            }

            if (RPGBCharacterControllerEssentials.IsUsingBuiltInControllers())
            {
                controllerEssentials.builtInController.StartCoroutine(
                    controllerEssentials.builtInController.UpdateCachedGroundCasting(false));
            }
        }
        
        protected override void HandleCombatState()
        {
            LastCombatActionTimer += Time.deltaTime;
            if (!(LastCombatActionTimer >= GameDatabase.Instance.GetCombatSettings().ResetCombatDuration)) return;
            if (!GameState.inCombatOverriden && GameDatabase.Instance.GetCombatSettings().AutomaticCombatStates)
            {
                ResetCombat();
            }
        }
        
        protected override void HandleToggledAbilities()
        {
            foreach (var toggledAbility in ActiveToggledAbilities.Where(toggledAbility => Time.time >= toggledAbility.nextTrigger))
            {
                if (!CombatManager.Instance.UseRequirementsMet(this, GetTarget(), toggledAbility.ability, toggledAbility.rank, true))
                {
                    RemoveToggledAbility(toggledAbility.ability);
                    return;
                }
                toggledAbility.nextTrigger = Time.time + toggledAbility.rank.toggledTriggerInterval;
                    
                CombatManager.Instance.HandleAbilityTypeActions(this, this, toggledAbility.ability,
                    toggledAbility.rank, true);
                GameEvents.Instance.OnTriggerVisualEffectsList(this, toggledAbility.rank.VisualEffectEntries, ActivationType.Start);
                GameEvents.Instance.OnTriggerSoundsList(this, toggledAbility.rank.SoundEntries, ActivationType.Start, transform);
                if (toggledAbility.rank.isToggleCostOnTrigger)
                {
                    CombatManager.Instance.HandleAbilityCost(this, toggledAbility.rank);
                }
            }
        }
        
        public override void RemoveToggledAbility(RPGAbility ability)
        {
            foreach (var toggledAbility in ActiveToggledAbilities.Where(toggledAbility => toggledAbility.ability == ability))
            {
                ActiveToggledAbilities.Remove(toggledAbility);
                ActionBarManager.Instance.UpdateToggledAbilities();
                return;
            }
        }

        protected override void HandleCasting()
        {
            CurrentCastProgress += Time.deltaTime;
            if (!CurrentAbilityCastedCurRank.castInRun && controllerEssentials.ShouldCancelCasting())
            {
                ResetCasting();
                return;
            }

            if (CurrentAbilityCastedCurRank.animationTriggered && EffectTriggered)
            {
                EffectTriggered = false;
                CurrentCastProgress = TargetCastTime;
            }

            if (!(CurrentCastProgress >= TargetCastTime)) return;

            CastingCompleted();
        }

        protected override void HandleChanneling()
        {
            CurrentChannelProgress -= Time.deltaTime;
            if (!CurrentAbilityCastedCurRank.castInRun && controllerEssentials.IsMoving())
            {
                ResetChanneling();
                return;
            }

            if (CurrentChannelProgress <= 0) ResetChanneling();
        }

        protected override void HandleObjectInteraction()
        {
            CurrentInteractionProgress += Time.deltaTime;

            if (WorldInteractableDisplayManager.Instance.showInteractionBar)
            {
                WorldInteractableDisplayManager.Instance.UpdateInteractionBar(CurrentInteractionProgress,
                    TargetInteractionTime);
            }

            if (controllerEssentials.IsMoving())
            {
                ResetObjectInteraction();
                return;
            }

            if (!(CurrentInteractionProgress >= TargetInteractionTime)) return;
            NodeObjectInteractionCompleted();
        }


        protected override void NodeObjectInteractionCompleted()
        {
            currentInteractableObject.UseObject();
            ResetObjectInteraction();
        }
        

        protected override bool CanShift(CombatData.CombatEntityStat stat)
        {
            if (controllerEssentials.isSprinting() && !stat.stat.isShiftingInSprint) return false;
            if (stat.currentValue == stat.currentMaxValue)
            {
                return false;
            }
            return !ActiveBlocking || stat.stat.isShiftingInBlock;
        }

        private void HandleAutoAttack()
        {
            if (autoAttackData.CurrentAutoAttackAbilityID == -1) return;
            if (!(Time.time >= _nextAutoAttack)) return;
            var abilityRef = GameDatabase.Instance.GetAbilities()[autoAttackData.CurrentAutoAttackAbilityID];
            if (abilityRef == null) return;
            var rankRef = abilityRef.ranks[0];
            if (rankRef != null) CombatManager.Instance.InitAbility(this, abilityRef, GetCurrentAbilityRank(abilityRef, false),false);
        }

        protected override void UpdateActiveBlockingUI()
        {
        }

        #endregion

        #region COMBAT INFO

        protected bool IsAutoAttackReady()
        {
            return Time.time >= _nextAutoAttack;
        }
        
        public override RPGAbility.RPGAbilityRankData GetCurrentAbilityRank(RPGAbility ability, bool abMustBeKnown)
        {
            int rankIndex = RPGBuilderUtilities.GetCharacterAbilityRank(ability.ID);
            if (!abMustBeKnown || IsShapeshifted()) rankIndex = 0;
            return ability.ranks[rankIndex];
        }
        
        public override bool IsAbilityReady(RPGAbility ability)
        {
            return Character.Instance.isAbilityCDReady(ability);
        }

        public override void StartAbilityCooldown(RPGAbility.RPGAbilityRankData rank, int abilityID)
        {
            var finalCD = rank.cooldown;
            float cdrecoveryspeed = CombatUtilities.GetTotalOfStatType(this, RPGStat.STAT_TYPE.CD_RECOVERY_SPEED);

            if (cdrecoveryspeed != 0)
            {
                cdrecoveryspeed /= 100;
                finalCD -= finalCD * cdrecoveryspeed;
            }
            
            Character.Instance.InitAbilityCooldown(abilityID, finalCD);
                
            if (rank.isSharingCooldown)
            {
                foreach (var ab in GameDatabase.Instance.GetAbilities().Values)
                {
                    if(!CombatUtilities.IsAbilityKnown(ab.ID)) continue;
                    RPGAbility.RPGAbilityRankData thisRankREF = ab.ranks[RPGBuilderUtilities.GetCharacterAbilityRank(ab.ID)];
                    if (!thisRankREF.isSharingCooldown || thisRankREF.abilityCooldownTag.entryName != rank.abilityCooldownTag.entryName) continue;
                    float thisCD = thisRankREF.cooldown - (thisRankREF.cooldown * cdrecoveryspeed);
                    Character.Instance.InitAbilityCooldown(ab.ID, thisCD);
                }
            }
        }
        
        public override void SetProjectileRotation(GameObject projectile, RPGAbility.RPGAbilityRankData rank, float yOffset)
        {
            Ray ray;
            switch (controllerEssentials.GETControllerType())
            {
                case RPGBuilderGeneralSettings.ControllerTypes.ThirdPerson:
                    projectile.transform.rotation = Quaternion.Euler(transform.eulerAngles.x,
                        transform.eulerAngles.y + yOffset, transform.eulerAngles.z);
                    break;
                case RPGBuilderGeneralSettings.ControllerTypes.ThirdPersonShooter:
                    Vector3 v3LookPoint;
                    ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                    RaycastHit hit;

                    v3LookPoint = Physics.Raycast(ray, out hit, 300, GameDatabase.Instance.GetCombatSettings().ProjectileRaycastLayers)
                        ? hit.point
                        : ray.GetPoint(300);

                    projectile.transform.LookAt(v3LookPoint);
                    projectile.transform.rotation = Quaternion.Euler(projectile.transform.eulerAngles.x,
                        projectile.transform.eulerAngles.y + yOffset, projectile.transform.eulerAngles.z);
                    break;
                case RPGBuilderGeneralSettings.ControllerTypes.TopDownClickToMove:
                case RPGBuilderGeneralSettings.ControllerTypes.TopDownWASD:
                    if (!rank.projectileShootOnClickPosition)
                    {
                        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                        var plane = new Plane(Vector3.up,
                            new Vector3(0, transform.position.y + rank.effectPositionOffset.y, 0));
                        if (plane.Raycast(ray, out var distance))
                        {
                            var target = ray.GetPoint(distance);
                            var direction = target - transform.position;
                            var rotation = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                            projectile.transform.rotation = Quaternion.Euler(0, rotation + yOffset, 0);
                        }
                    }
                    else
                    {
                        var target = CurrentProjectileClickPos;
                        var direction = target - transform.position;
                        var rotation = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                        projectile.transform.rotation = Quaternion.Euler(0, rotation + yOffset, 0);
                    }
                    break;
                case RPGBuilderGeneralSettings.ControllerTypes.FirstPerson:
                    break;
            }
        }

        public override void SetProjectileGravity(Rigidbody projectile)
        {
            switch (controllerEssentials.GETControllerType())
            {
                case RPGBuilderGeneralSettings.ControllerTypes.ThirdPersonShooter:
                case RPGBuilderGeneralSettings.ControllerTypes.FirstPerson:
                    projectile.constraints = RigidbodyConstraints.FreezeRotationX |
                                                RigidbodyConstraints.FreezeRotationY |
                                                RigidbodyConstraints.FreezeRotationZ;
                    break;
                case RPGBuilderGeneralSettings.ControllerTypes.TopDownClickToMove:
                case RPGBuilderGeneralSettings.ControllerTypes.TopDownWASD:
                case RPGBuilderGeneralSettings.ControllerTypes.ThirdPerson:
                    projectile.constraints = RigidbodyConstraints.FreezePositionY |
                                                RigidbodyConstraints.FreezeRotationX |
                                                RigidbodyConstraints.FreezeRotationY |
                                                RigidbodyConstraints.FreezeRotationZ;
                    break;
            }
        }
        
        public override bool IsPlayer()
        {
            return true;
        }
        public override bool IsNPC()
        {
            return false;
        }
        
        public override RPGNpc GetNPCData()
        {
            return null;
        }

        #endregion

        #region COMBAT EVENTS

        public override void TakeDamage(CombatCalculations.DamageResult result, RPGAbility.RPGAbilityRankData abilityRank, int alteredStatID)
        {
            base.TakeDamage(result, abilityRank, alteredStatID);

            CombatUtilities.UpdateCurrentStatValue(this, alteredStatID, -result.DamageAmount);

            if (!GameState.inCombatOverriden && GameDatabase.Instance.GetCombatSettings().AutomaticCombatStates)
            {
                EnterCombat();
            }

            if (!result.caster.IsPlayer() && !result.caster.IsInCombat() && !GameState.inCombatOverriden)
            {
                result.caster.EnterCombat();
            }
        }

        public override void EntityDeath()
        {
            base.EntityDeath();

            CombatManager.Instance.HandleOnKillActions(LatestAttacker, this, LatestAbilityRankHit);

            Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex].LastPosition = getClosestGraveyardPosition();

            for (int i = CurrentPets.Count - 1; i >= 0; i--)
            {
                Destroy(CurrentPets[i].gameObject);
                CurrentPets.Remove(CurrentPets[i]);
            }
            
            CombatEvents.Instance.OnPlayerPetUpdate();

            controllerEssentials.InitDeath();
            ResetTarget();
            
            CombatEvents.Instance.OnPlayerDied();
        }

        private void InitializeStates()
        {
            foreach (var state in Character.Instance.CharacterData.States)
            {
                CombatManager.Instance.InitializePersistentState(state, this);
            }
            Character.Instance.CharacterData.States.Clear();
        }
        
        

        private void RespawnPlayerToGraveyard()
        {
            Vector3 respawnPOS = Vector3.zero;
            if (GameState.allGraveyards.Count > 0)
            {
                CharacterGraveyard closestCharacterGraveyard = null;
                var closestDist = Mathf.Infinity;
                foreach (var t in GameState.allGraveyards)
                    if (isGraveyardAccepted(t))
                    {
                        var dist = Vector3.Distance(GameState.playerEntity.transform.position, t.transform.position);
                        if (!(dist < closestDist)) continue;
                        closestDist = dist;
                        closestCharacterGraveyard = t;
                    }

                if (closestCharacterGraveyard != null)
                {
                    respawnPOS = closestCharacterGraveyard.transform.position;
                }
                else
                {
                    respawnPOS = GameDatabase.Instance.GetWorldPositions()[RPGBuilderUtilities
                            .GetGameSceneFromName(RPGBuilderEssentials.Instance.getCurrentScene().name).startPositionID]
                        .position;
                }
            }
            else
            {
                respawnPOS = GameDatabase.Instance.GetWorldPositions()[RPGBuilderUtilities
                        .GetGameSceneFromName(RPGBuilderEssentials.Instance.getCurrentScene().name).startPositionID]
                    .position;
            }

            InitRespawn(respawnPOS);
        }

        private Vector3 getClosestGraveyardPosition()
        {
            Vector3 respawnPOS = Vector3.zero;
            if (GameState.allGraveyards.Count > 0)
            {
                CharacterGraveyard closestCharacterGraveyard = null;
                var closestDist = Mathf.Infinity;
                foreach (var t in GameState.allGraveyards)
                    if (isGraveyardAccepted(t))
                    {
                        var dist = Vector3.Distance(GameState.playerEntity.transform.position, t.transform.position);
                        if (!(dist < closestDist)) continue;
                        closestDist = dist;
                        closestCharacterGraveyard = t;
                    }

                if (closestCharacterGraveyard != null)
                {
                    respawnPOS = closestCharacterGraveyard.transform.position;
                }
                else
                {
                    respawnPOS = GameDatabase.Instance.GetWorldPositions()[RPGBuilderUtilities
                            .GetGameSceneFromName(RPGBuilderEssentials.Instance.getCurrentScene().name).startPositionID]
                        .position;
                }
            }
            else
            {
                respawnPOS = GameDatabase.Instance.GetWorldPositions()[RPGBuilderUtilities
                        .GetGameSceneFromName(RPGBuilderEssentials.Instance.getCurrentScene().name).startPositionID]
                    .position;
            }

            return respawnPOS;
        }
        
        private bool isGraveyardAccepted(CharacterGraveyard characterGraveyard)
        {
            if (characterGraveyard.requiredClasses.Count <= 0)
                return characterGraveyard.requiredRaces.Count <= 0 ||
                       characterGraveyard.requiredRaces.Contains(
                           GameDatabase.Instance.GetRaces()[Character.Instance.CharacterData.RaceID]);
            if (!GameDatabase.Instance.GetCharacterSettings().NoClasses && !characterGraveyard.requiredClasses.Contains(
                GameDatabase.Instance.GetClasses()[Character.Instance.CharacterData.ClassID]))
                return false;
            return characterGraveyard.requiredRaces.Count <= 0 || characterGraveyard.requiredRaces.Contains(GameDatabase.Instance.GetRaces()[Character.Instance.CharacterData.RaceID]);
        }
        
        public override void InitRespawn(Vector3 respawnPosition)
        {
            controllerEssentials.CancelDeath();
            controllerEssentials.TeleportToTarget(respawnPosition);
            
            Dead = false;
            LatestAttacker = null;
            LatestAbilityRankHit = null;
            
            
            StatCalculator.ResetPlayerStatsAfterRespawn();
            CombatManager.Instance.HandleVitalityStatActions(this, GameDatabase.Instance.GetHealthStat());
            
            CombatEvents.Instance.OnPlayerRespawned();
        }

        public override void ResetCombat()
        {
            base.ResetCombat();
            UIEvents.Instance.OnShowAlertMessage("Out of combat", 3);
            appearance.UpdateWeaponStates(InCombat);
            appearance.HandleAnimatorOverride();
        }
        
        public override void EnterCombat()
        {
            base.EnterCombat();
            UIEvents.Instance.OnShowAlertMessage("Entered Combat", 3);
            appearance.UpdateWeaponStates(InCombat);
            appearance.HandleAnimatorOverride();
        }

        public override void InitCasting(RPGAbility thisAbility, RPGAbility.RPGAbilityRankData rankREF)
        {
            base.InitCasting(thisAbility, rankREF);
            
            if (rankREF.startCDOnActivate)
            {
                StartAbilityCooldown(rankREF, thisAbility.ID);
                if (CurrentAbilityCastedCurRank.isGCD)
                    CombatManager.Instance.StartGCD();
            }

            InitCurrentClickPos(rankREF);
        }

        protected override void CastingCompleted()
        {
            if ((CurrentAbilityCastedCurRank.targetType == RPGAbility.TARGET_TYPES.TARGET_PROJECTILE ||
                 CurrentAbilityCastedCurRank.targetType == RPGAbility.TARGET_TYPES.TARGET_INSTANT)
                && CurrentTargetCasted.IsDead())
            {
                UIEvents.Instance.OnShowAlertMessage("The target is dead!", 3);
                ResetCasting();
                return;
            }
            
            CombatManager.Instance.HandleAbilityTypeActions(this, CurrentTargetCasted, CurrentAbilityCasted, CurrentAbilityCastedCurRank, false);

            if (!CurrentAbilityCastedCurRank.startCDOnActivate)
            {
                StartAbilityCooldown(CurrentAbilityCastedCurRank, CurrentAbilityCasted.ID);
                if (CurrentAbilityCastedCurRank.isGCD)
                    CombatManager.Instance.StartGCD();
            }

            CombatManager.Instance.HandleAbilityCost(this, CurrentAbilityCastedCurRank);
            if (CurrentAbilityCastedCurRank.comboStarsAfterCastComplete) CombatManager.Instance.AbilityUsed(this, CurrentAbilityCasted.ID);
            ResetCasting();
        }

        public override void ResetCasting()
        {
            controllerEssentials.AbilityEndCastActions(CurrentAbilityCastedCurRank);
            base.ResetCasting();
            CombatEvents.Instance.OnStoppedCastingAbility(this);
            
        }

        public override void InitChanneling(RPGAbility thisAbility, RPGAbility.RPGAbilityRankData rankREF)
        {
            base.InitChanneling(thisAbility, rankREF);
            CombatEvents.Instance.OnStartedChannelingAbility(this, thisAbility, rankREF, rankREF.channelTime);
        }
        
        protected override void ResetChanneling()
        {
            base.ResetChanneling();
            CombatEvents.Instance.OnStoppedChannelingAbility(this);
        }

        public override void EndStateEffect(int stateIndex)
        {
            switch (States[stateIndex].stateEffect.effectType)
            {
                case RPGEffect.EFFECT_TYPE.Stun:
                case RPGEffect.EFFECT_TYPE.Sleep:
                case RPGEffect.EFFECT_TYPE.Root:
                    ThisAnimator.SetBool("Stunned", false);
                    break;
                case RPGEffect.EFFECT_TYPE.Shapeshifting:
                    if (IsShapeshifted()) ResetShapeshifting();
                    break;

                case RPGEffect.EFFECT_TYPE.Flying:
                    controllerEssentials.EndFlying();
                    break;

                case RPGEffect.EFFECT_TYPE.Stealth:
                    if (IsStealth()) ResetStealth();
                    return;
                case RPGEffect.EFFECT_TYPE.Mount:
                    break;
            }

            bool statEffect = States[stateIndex].stateEffect.effectType == RPGEffect.EFFECT_TYPE.Stat;
            CombatEvents.Instance.OnStateEnded(this, stateIndex);
            States.RemoveAt(stateIndex);
            if (statEffect) StatCalculator.CalculateEffectsStats(this);

        }

        
        public override void InitEffectType(RPGEffect effect, int effectRank)
        {
            switch (effect.effectType)
            {
                case RPGEffect.EFFECT_TYPE.Shapeshifting:
                {
                    InitShapeshifting(effect, effectRank);
                    break;
                }
                case RPGEffect.EFFECT_TYPE.Flying:
                {
                    controllerEssentials.InitFlying();
                    break;
                }
                case RPGEffect.EFFECT_TYPE.Stealth:
                {
                    InitStealth(effect.ranks[effectRank].showStealthActionBar, effect.ranks[effectRank].nestedEffects);
                    break;
                }
                case RPGEffect.EFFECT_TYPE.Mount:
                {
                    InitMount(effect, effectRank);
                    break;
                }
            }
        }
        
        public override void InitStealth(bool showActionBar, List<RPGAbility.AbilityEffectsApplied> nestedEffects)
        {
            Stealth = true;
                CombatManager.Instance.DestroyStealthNodeCombatEntities(this);

            if (showActionBar)
            {
                ActionBarManager.Instance.InitStealthActionBar();
            }
                        
            CombatManager.Instance.HandleNestedEffects(this, nestedEffects);
            UIEvents.Instance.OnShowOverlay("STEALTH");
        }
        public override void ResetStealth()
        {
            base.ResetStealth();
            for (var index = 0; index < States.Count; index++)
            {
                var state = States[index];
                if (state.stateEffect.effectType != RPGEffect.EFFECT_TYPE.Stealth) continue;

                if (state.effectRank.showStealthActionBar)
                {
                    ActionBarManager.Instance.ResetTemporaryActionBar();
                }

                CombatManager.Instance.ResetNestedEffects(this, state.stateEffect, state.effectRank);

                CombatEvents.Instance.OnStateEnded(this, index);
                States.Remove(state);
                break;
            }
            
            UIEvents.Instance.OnHideOverlay("STEALTH");
        }
        public override void InitFlying()
        {
            controllerEssentials.InitFlying();
        }
        public override void InitShapeshifting(RPGEffect effect, int effectRank)
        {
            if (IsShapeshifted())
            {
                ResetShapeshifting();
            }
            
            if (effect.ranks[effectRank].shapeshiftingModel == null) return;
            GameObject shapeshiftModel = Instantiate(effect.ranks[effectRank].shapeshiftingModel, transform);
            shapeshiftModel.transform.localPosition = effect.ranks[effectRank].shapeshiftingmodelPosition;
            shapeshiftModel.transform.localScale = effect.ranks[effectRank].shapeshiftingmodelScale;
            shapeshiftModel.transform.localEulerAngles = Vector3.zero;

            ShapeshiftedEffect = effect;
            ShapeshiftedEffectRank = effectRank;
            Shapeshifted = true;
            ShapeshiftingGameobject = shapeshiftModel;
            ShapeshiftingNodeSockets = shapeshiftModel.GetComponent<NodeSockets>();

            appearance.cachedBodyParent.SetActive(false);
            appearance.cachedArmorsParent.SetActive(false);

            controllerEssentials.anim.runtimeAnimatorController = InCombat ? 
                effect.ranks[effectRank].shapeshiftingAnimatorControllerCombat : effect.ranks[effectRank].shapeshiftingAnimatorController;
            controllerEssentials.anim.avatar =
                effect.ranks[effectRank].shapeshiftingAnimatorAvatar;
            controllerEssentials.anim.applyRootMotion =
                effect.ranks[effectRank].shapeshiftingAnimatorUseRootMotion;
            controllerEssentials.anim.updateMode =
                effect.ranks[effectRank].shapeshiftingAnimatorUpdateMode;
            controllerEssentials.anim.cullingMode =
                effect.ranks[effectRank].shapeshiftingAnimatorCullingMode;

            
            Character.Instance.CharacterData.ShapeshiftingActionBarSlots.Clear();
            if (effect.ranks[effectRank].shapeshiftingOverrideMainActionBar)
            {
                int abilities = 0;
                foreach (var ability in effect.ranks[effectRank].shapeshiftingActiveAbilities)
                {
                    CharacterEntries.ActionBarSlotEntry acSlotEntry = new CharacterEntries.ActionBarSlotEntry
                    {
                        contentType = CharacterEntries.ActionBarSlotContentType.Ability,
                        ID = ability,
                    };

                    Character.Instance.CharacterData.ShapeshiftingActionBarSlots.Add(acSlotEntry);
                    abilities++;
                    if (abilities >= ActionBarManager.Instance.GetMainActionBarSlots()) break;
                }
                
                ActionBarManager.Instance.InitShapeshiftingActionBar();
            }
            
            CombatManager.Instance.HandleNestedEffects(this, effect.ranks[effectRank].nestedEffects);
            
            StatCalculator.CalculateShapeshiftingStats(this);
            ShapeshiftingSlotsDisplayManager.Instance.DisplaySlots();
            
            appearance.HideWeapon(1);
            appearance.HideWeapon(2);

            if (!effect.ranks[effectRank].canCameraAim)
            {
                controllerEssentials.ResetCameraAiming();
            }
        }

        public override void ResetShapeshifting()
        {
            Shapeshifted = false;
            Destroy(ShapeshiftingGameobject);

            appearance.cachedBodyParent.SetActive(true);
            appearance.cachedArmorsParent.SetActive(true);
            
            ShapeshiftedEffect = null;
            ShapeshiftedEffectRank = -1;

            controllerEssentials.anim.runtimeAnimatorController =
                CachedAnimatorController;
            controllerEssentials.anim.avatar =
                CachedAnimatorAvatar;
            controllerEssentials.anim.applyRootMotion =
                CachedAnimatorUseRootMotion;
            controllerEssentials.anim.updateMode =
                CachedAnimatorUpdateMode;
            controllerEssentials.anim.cullingMode =
                CachedAnimatorCullingMode;
            
            appearance.HandleAnimatorOverride();

            Character.Instance.CharacterData.ShapeshiftingActionBarSlots.Clear();
            StatCalculator.ResetShapeshiftingStats(this);
            
            Character.Instance.CharacterData.ShapeshiftingActionBarSlots.Clear();
            ActionBarManager.Instance.ResetTemporaryActionBar();

            List<int> effectsToRemove = new List<int>();
            var list = GetStates();
            for (var index = 0; index < list.Count; index++)
            {
                var state = list[index];
                if (state.stateEffect.effectType != RPGEffect.EFFECT_TYPE.Shapeshifting) continue;
                effectsToRemove.Add(index);
                CombatManager.Instance.ResetNestedEffects(this, state.stateEffect, state.effectRank);
                break;
            }

            foreach (var effectToRemove in effectsToRemove)
            {
                EndStateEffect(effectToRemove);
            }
            
            ShapeshiftingSlotsDisplayManager.Instance.DisplaySlots();
            appearance.ShowWeapon(GameState.playerEntity.equippedWeapons[0].item, 1, null, GameState.playerEntity.IsInCombat());
            appearance.ShowWeapon(GameState.playerEntity.equippedWeapons[1].item, 2, null, GameState.playerEntity.IsInCombat());
        }
        
        public override void InterruptCastActions()
        {
            Debug.LogError(CurrentAnimationCoroutines.Count);
            base.InterruptCastActions();
        }

        public override void InitStandTime(RPGAbility.RPGAbilityRankData rank)
        {
            controllerEssentials.InitStandTime(rank.standTimeDuration);
        }
        
        public override void InitCastSlow(RPGAbility.RPGAbilityRankData rank)
        {
            controllerEssentials.InitCastMoveSlow(rank.castSpeedSlowAmount, rank.castSpeedSlowTime, rank.castSpeedSlowRate);
        }
        
        public override void InitKnockback(CombatEntity attacker, float distance)
        {
            controllerEssentials.InitKnockback(distance, attacker.transform);
        }
        
        public override void InitMotion(RPGEffect.RPGEffectRankData rank)
        {
            if (controllerEssentials.motionActive) return;
            if(!rank.motionIgnoreUseCondition && !CombatManager.Instance.CombatNodeCanInitMotion(this)) return;
            controllerEssentials.InitMotion(rank.motionDistance, rank.motionDirection, rank.motionSpeed, rank.isImmuneDuringMotion);
        }
        
        public override void InitMount(RPGEffect effect, int rank)
        {
            base.InitMount(effect, rank);

            if (appearance.armatureParentGO != null)
            {
                if (effect.ranks[rank].ReParentCharacterArmature && !string.IsNullOrEmpty(effect.ranks[rank].RiderReParentName))
                {
                    appearance.armatureParentGO.transform.SetParent(RPGBuilderUtilities
                        .GetChildByName(mount, effect.ranks[rank].RiderReParentName).transform);
                }

                appearance.armatureParentGO.transform.localPosition = effect.ranks[rank].RiderPosition;
                appearance.armatureParentGO.transform.localEulerAngles = effect.ranks[rank].RiderRotation;
            }

            if (!effect.ranks[rank].mountCanAim)
            {
                controllerEssentials.ResetCameraAiming();
            }
        }

        public override void ResetMount()
        {
            base.ResetMount();

            if (appearance.armatureParentGO != null)
            {
                appearance.armatureParentGO.transform.SetParent(transform);
                appearance.armatureParentGO.transform.localPosition = appearance.armatureParentOffset;
                appearance.armatureParentGO.transform.localEulerAngles = Vector3.zero;
            }
        }

        public override void InitActiveBlocking(RPGEffect effect, RPGEffect.RPGEffectRankData rank)
        {
            ActiveBlockingState.effect = effect;
            ActiveBlockingState.effectRank = rank;
            ActiveBlocking = true;
            ActiveBlockingState.curBlockChargeTime = 0;
            ActiveBlockingState.curBlockHitCount = 0;

            if (rank.blockEndType == RPGEffect.BLOCK_END_TYPE.MaxDamageBlocked)
            {
                ActiveBlockingState.curBlockedDamageLeft = rank.blockMaxDamage;
                CombatEvents.Instance.OnPlayerActiveBlockedDamage();
            }

            if (rank.isBlockChargeTime)
            {
                ActiveBlockingState.targetBlockChargeTime = rank.blockChargeTime;
                ActiveBlockingState.targetBlockChargeTime *= 1 - (CombatUtilities.GetTotalOfStatType(this,
                    RPGStat.STAT_TYPE.BLOCK_ACTIVE_CHARGE_TIME_SPEED_MODIFIER) / 100f);
            }
            else
            {
                ActiveBlockingState.blockIsDoneCharging = true;
                ActiveBlockingState.targetBlockChargeTime = 0;
            }

            if (rank.isBlockLimitedDuration)
            {
                ActiveBlockingState.blockDurationLeft = rank.blockDuration;
                ActiveBlockingState.blockDurationLeft += ActiveBlockingState.blockDurationLeft *
                                                         (CombatUtilities.GetTotalOfStatType(this,
                                                             RPGStat.STAT_TYPE.BLOCK_ACTIVE_DURATION_MODIFIER) / 100f);
                ActiveBlockingState.cachedBlockMaxDuration = ActiveBlockingState.blockDurationLeft;
            }
            else
            {
                ActiveBlockingState.blockDurationLeft = 0;
            }

            ActiveBlockingState.curBlockPowerFlat = rank.blockPowerFlat +
                                                    (int) CombatUtilities.GetTotalOfStatType(this,
                                                        RPGStat.STAT_TYPE.BLOCK_ACTIVE_FLAT_AMOUNT);
            ActiveBlockingState.curBlockPowerModifier = rank.blockPowerModifier +
                                                        (int) CombatUtilities.GetTotalOfStatType(this,
                                                            RPGStat.STAT_TYPE.BLOCK_ACTIVE_PERCENT_AMOUNT);
            if (ActiveBlockingState.curBlockPowerModifier > 100)
            {
                ActiveBlockingState.curBlockPowerModifier = 100;
            }

            controllerEssentials.anim.SetBool("isActiveBlocking", true);
            CombatEvents.Instance.OnPlayerStartedActiveBlocking();
        }

        public override void InitGroundAbility(RPGAbility ability, RPGAbility.RPGAbilityRankData rank)
        {
            base.InitGroundAbility(ability, rank);
            indicatorManagerRef.ShowIndicator(rank.groundRadius * 1.25f, rank.groundRange);
            GroundCasting = true;
            
            if (RPGBCharacterControllerEssentials.IsUsingBuiltInControllers())
            {
                controllerEssentials.builtInController.StartCoroutine(
                    controllerEssentials.builtInController.UpdateCachedGroundCasting(true));
            }
        }
        
        public void InitActionAbilityCooldown(int id, float nextUse)
        {
            foreach (var actionAb in Character.Instance.CharacterData.ActionAbilities)
            {
                if (actionAb.ability.ID == id)
                {
                    actionAb.NextTimeUse = Time.time + nextUse;
                }
            }
        }
        
        public void InitAACooldown(float nextAA)
        {
            _nextAutoAttack = Time.time + nextAA;
        }
        
        private void InitCurrentClickPos(RPGAbility.RPGAbilityRankData rankRef)
        {
            if (rankRef.targetType != RPGAbility.TARGET_TYPES.PROJECTILE || !rankRef.projectileShootOnClickPosition) return;
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var plane = new Plane(Vector3.up, new Vector3(0, transform.position.y + rankRef.effectPositionOffset.y, 0));
            if (plane.Raycast(ray, out var distance))
            {
                _currentProjectileClickPos = ray.GetPoint(distance);
            }
        }
        
        public override void SetTarget(CombatEntity entity)
        {
            if (entity == null) return;
            if (Cursor.lockState == CursorLockMode.Locked && entity == this) return;
            CurrentTarget = entity;
            CombatEvents.Instance.OnPlayerTargetChanged(this, entity);
        }

        public override void ResetTarget()
        {
            CurrentTarget = null;
            isAutoAttacking = false;
            CombatEvents.Instance.OnPlayerTargetChanged(this, null);
        }
        
        public override bool CanStartCasting()
        {
            if (controllerEssentials.GETControllerType() ==
                RPGBuilderGeneralSettings.ControllerTypes.TopDownClickToMove)
            {
                return controllerEssentials.IsGrounded() && !controllerEssentials.IsMoving();
            }

            return controllerEssentials.IsGrounded();
        }
        
        protected override void  HandleLeaping()
        {
            if (!Leaping) return;
            LeapAnimation += Time.deltaTime;

            if (Physics.Raycast(transform.position, transform.forward, 0.3f, CurrentAbilityCastedCurRank.groundLeapBlockLayers))
            {
                ResetLeaping();
                return;
            }

            controllerEssentials.lastPosition = transform.position;
            Vector3 movement = MathParabola.Parabola(StartPos, LeapEndPos, LeapHeight, LeapAnimation / LeapSpeed) - transform.position;
            controllerEssentials.charController.Move(movement);

            if (controllerEssentials.IsInMotionWithoutProgress(0.05f))
            {
                ResetLeaping();
                return;
            }

            if (Vector3.Distance(transform.position, LeapEndPos) < 0.15f)
            {
                ResetLeaping();
            }
        }

        protected override void ResetLeaping()
        {
            base.ResetLeaping();
            controllerEssentials.EndGroundLeap();
        }
        
        public override IEnumerator RemoveComboEntry(CombatData.ActiveCombo combo, bool resetActionBarImage, bool resetComboActive)
        {
            yield return new WaitForFixedUpdate();
            if(resetComboActive) RPGBuilderUtilities.SetAbilityComboActive(combo.initialAbilityID, false);
            if(resetActionBarImage) CombatEvents.Instance.OnPlayerComboRemoved(combo.initialAbilityID);
            Destroy(combo.UISlotREF.gameObject);
            ActiveCombos.Remove(combo);
        }

        public override void ResetActiveBlocking()
        {
            base.ResetActiveBlocking();
            controllerEssentials.anim.SetBool("isActiveBlocking", false);
            CombatEvents.Instance.OnPlayerStoppedActiveBlocking();
        }

        public override void InitAnimation(AnimationEntry animationEntry, string parameterName)
        {
            StartCoroutine(PlayAnimation(animationEntry, parameterName));

            if (animationEntry.ShowWeapons && !InCombat)
                appearance.ShowWeaponsTemporarily(animationEntry.ShowWeaponsDuration);
        }

        #endregion

        #region UNITY EVENTS

        #endregion

        #region INTERACTION EVENTS
        public override void InitObjectInteraction(InteractableObject interactable, float duration)
        {
            base.InitObjectInteraction(interactable, duration);
            GeneralEvents.Instance.OnPlayerStartedInteracting(duration);
        }
        
        protected override void ResetObjectInteraction()
        {
            base.ResetObjectInteraction();
            GeneralEvents.Instance.OnPlayerStoppedInteracting();
        }
        
        public override void InterruptObjectInteraction()
        {
            base.InterruptObjectInteraction();
            GeneralEvents.Instance.OnPlayerStoppedInteracting();
        }
        #endregion

        #region STATES INFO

        public override bool IsImmune()
        {
            return States.Any(t => t.stateEffect.effectType == RPGEffect.EFFECT_TYPE.Immune) || MotionImmune;
        }
        
        public override bool IsInMotion()
        {
            return controllerEssentials.motionActive;
        }

        #endregion
        
        #region ON MOUSE EVENTS

        protected override void OnMouseDown()
        {
            if (UIEvents.Instance.CursorHoverUI) return;
            if (GameDatabase.Instance.GetCharacterSettings().CanTargetPlayerOnClick)
            {
                SetTarget(this);
            }
        }

        protected override void OnMouseOver()
        {

        }

        protected override void OnMouseExit()
        {
            
        }
        #endregion
    }
}
