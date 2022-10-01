using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.AI;
using BLINK.RPGBuilder.Data;
using BLINK.RPGBuilder.Logic;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.Templates;
using BLINK.RPGBuilder.World;
using UnityEngine;

namespace BLINK.RPGBuilder.Combat
{
    public class CombatEntity : MonoBehaviour, IPlayerInteractable
    {
        #region FIELDS

        #region COMBAT

        protected bool Dead;
        public bool IsDead() {return Dead;}
        protected bool InCombat;
        public bool IsInCombat() {return InCombat;}
        protected float inCombatTime;
        public float GetInCombatTime() {return inCombatTime;}
        protected float outOfCombatTime;
        public float GetOutOfCombatTime() {return outOfCombatTime;}
        protected bool Stealth;
        public bool IsStealth() {return Stealth;}
        protected bool Leaping;
        public bool IsLeaping() { return Leaping;}
        protected bool Knockedback;
        public bool IsKnockedBack() {return Knockedback;}
        protected bool ActiveBlocking;
        public bool IsActiveBlocking() {return ActiveBlocking;}
        protected bool Casting;
        public bool IsCasting() { return Casting;}
        protected bool Channeling;
        public bool IsChanneling() { return Channeling;}
        protected bool InteractiveNodeCasting;
        public bool IsInteractiveNodeCasting() { return InteractiveNodeCasting;}
        protected bool InteractingWithObject;
        public bool IsPersistentNPC() { return !IsPlayer() && PersistentNPC;}
        public void SetPersistentNPC(bool value) {PersistentNPC = value;}
        protected bool PersistentNPC;

        public bool IsInteractingWithObject()
        {
            return InteractingWithObject;
        }

        protected bool MotionImmune;
        public bool IsMotionImmune() { return MotionImmune;}
        public void SetMotionImmune(bool motionImmune) {MotionImmune = motionImmune;}

        protected int Level;
        public int GetLevel() { return Level;}
        public void SetLevel(int newLevel) { Level = newLevel;}

        protected CombatEntity CurrentTarget;
        
        protected CombatEntity ownerEntity;
        public CombatEntity GetOwnerEntity() { return ownerEntity;}
        public void SetOwnerEntity(CombatEntity owner) { ownerEntity = owner;}

        public bool IsPet() {return ownerEntity != this;}
        
        protected RPGFaction faction;
        public RPGFaction GetFaction() { return faction;}
        public void SetFaction(RPGFaction newFaction) { faction = newFaction;}
        
        protected NPCSpawner Spawner;
        
        protected RPGSpecies species;
        public RPGSpecies GetSpecies() { return species;}
        
        protected CombatEntity LatestAttacker;
        protected RPGAbility.RPGAbilityRankData LatestAbilityRankHit;
        protected Dictionary<int, CombatData.CombatEntityStat> Stats = new Dictionary<int, CombatData.CombatEntityStat>();
        public Dictionary<int, CombatData.CombatEntityStat> GetStats ()
        {
            return Stats;
        }
        
        [SerializeField] protected List<CombatData.CombatEntityStateEffect> States = new List<CombatData.CombatEntityStateEffect>();
        public List<CombatData.CombatEntityStateEffect> GetStates ()
        {
            return States;
        }

        public CombatData.ActiveBlockingState ActiveBlockingState = new CombatData.ActiveBlockingState();
        protected List<CombatData.ActiveCombo> ActiveCombos = new List<CombatData.ActiveCombo>();
        public List<CombatData.ActiveCombo> GetActiveCombos ()
        {
            return ActiveCombos;
        }
        protected List<CombatData.ActiveToggledAbilities> ActiveToggledAbilities = new List<CombatData.ActiveToggledAbilities>();
        public List<CombatData.ActiveToggledAbilities> GetActiveToggledAbilities ()
        {
            return ActiveToggledAbilities;
        }
        
        protected Dictionary<AnimationTemplate, int> AnimationTemplateSequenceIndex = new Dictionary<AnimationTemplate, int>();
        public Dictionary<AnimationTemplate, int> GetAnimationTemplateSequenceIndex ()
        {
            return AnimationTemplateSequenceIndex;
        }
        
        protected float LastCombatActionTimer;
        
        protected float CurrentCastProgress;
        protected float TargetCastTime;
        protected float CurrentChannelProgress;
        protected float TargetChannelTime;
        protected float CurrentInteractionProgress;
        protected float TargetInteractionTime;
        protected Vector3 CurrentProjectileClickPos;
        
        protected RPGAbility.RPGAbilityRankData CurrentAbilityCastedCurRank;
        public RPGAbility.RPGAbilityRankData GetCurrentAbilityCastedRank() {return CurrentAbilityCastedCurRank;}
        protected RPGAbility CurrentAbilityCasted;
        protected CombatEntity CurrentTargetCasted;
        public CombatEntity GetCurrentTargetCasted() {return CurrentTargetCasted;}
        public void SetCurrentTargetCasted(CombatEntity entity) {CurrentTargetCasted = entity;}
        protected InteractiveNode CurrentInteractiveNodeCasted;
        protected InteractableObject currentInteractableObject;

        protected bool isMounted;
        public bool IsMounted() {return isMounted;}
        
        protected GameObject mount;
        public GameObject GetMount() { return mount;}
        protected Animator mountAnimator;
        public Animator GetMountAnimator() { return mountAnimator;}
        protected RPGEffect mountEffect;
        public RPGEffect GetMountEffect() {return mountEffect;}
        protected int mountEffectRank;
        public RPGEffect.RPGEffectRankData GetMountEffectRank() {return mountEffect.ranks[mountEffectRank];}

        #region EVENTS
        private void OnEnable()
        {
            CombatEvents.DamageBlocked += OnBlock;
        }

        private void OnDisable()
        {
            CombatEvents.DamageBlocked -= OnBlock;
        }
        #endregion

        #region Leaping
        protected Vector3 LeapEndPos;
        protected Vector3 StartPos;
        protected float LeapHeight;
        protected float LeapSpeed;
        protected float LeapAnimation;
        #endregion

        #endregion

        #region OWNED_PETS
        
        protected CombatData.PetMovementActionTypes CurrentPetsMovementActionType = CombatData.PetMovementActionTypes.Follow;
        public CombatData.PetMovementActionTypes GetCurrentPetsMovementActionType() {return CurrentPetsMovementActionType;}

        public void SetCurrentPetsMovementActionType(CombatData.PetMovementActionTypes action)
        {
            CurrentPetsMovementActionType = action;
            if (action == CombatData.PetMovementActionTypes.Stay)
            {
                foreach (var pet in CurrentPets)
                {
                    pet.GetAIEntity().ResetMovement();
                }
            } else if (action == CombatData.PetMovementActionTypes.Follow)
            {
                foreach (var pet in CurrentPets)
                {
                    pet.GetAIEntity().StartMovement();
                }
            }
        }
        protected CombatData.PetCombatActionTypes CurrentPetsCombatActionType = CombatData.PetCombatActionTypes.Defend;
        public CombatData.PetCombatActionTypes GetCurrentPetsCombatActionType() {return CurrentPetsCombatActionType;}
        public void SetCurrentPetsCombatActionType(CombatData.PetCombatActionTypes actionType) {CurrentPetsCombatActionType = actionType;}
        protected List<CombatEntity> CurrentPets = new List<CombatEntity>();
        public List<CombatEntity> GetCurrentPets ()
        {
            return CurrentPets;
        }
        
        #endregion

        #region VISUAL

        protected Animator ThisAnimator;
        public Animator GetAnimator() {  return ThisAnimator;}
        public void SetAnimator(Animator animator) {  ThisAnimator = animator;}
        
        public NodeSockets NodeSockets;

        public Renderer mainRenderer;
        
        public List<ActiveAnimationCoroutine> CurrentAnimationCoroutines = new List<ActiveAnimationCoroutine>();
        
        protected List<GameObject> OwnedCombatVisuals = new List<GameObject>();
        public List<GameObject> GetOwnedCombatVisuals ()
        {
            return OwnedCombatVisuals;
        }
        protected List<GameObject> OwnedLogicCombatVisuals = new List<GameObject>();
        public List<GameObject> GetOwnedLogicCombatVisuals ()
        {
            return OwnedLogicCombatVisuals;
        }
        protected List<GameObject> DestroyedOnStealthCombatVisuals = new List<GameObject>();
        public List<GameObject> GetDestroyedOnStealthCombatVisuals ()
        {
            return DestroyedOnStealthCombatVisuals;
        }
        protected List<GameObject> DestroyedOnStealthEndCombatVisuals = new List<GameObject>();
        public List<GameObject> GetDestroyedOnStealthEndCombatVisuals ()
        {
            return DestroyedOnStealthEndCombatVisuals;
        }
        #endregion

        #region EQUIPPED ITEMS

        public List<EconomyData.EquippedArmor> equippedArmors = new List<EconomyData.EquippedArmor>();
        public List<EconomyData.EquippedWeapon> equippedWeapons = new List<EconomyData.EquippedWeapon>();

        #endregion

        #region SHAPESHIFTING
        protected bool Shapeshifted;
        public bool IsShapeshifted() {return Shapeshifted;}
        
        public GameObject ShapeshiftingGameobject;
        public NodeSockets ShapeshiftingNodeSockets;
        public RPGEffect ShapeshiftedEffect;
        public int ShapeshiftedEffectRank;
        public RuntimeAnimatorController CachedAnimatorController;
        public Avatar CachedAnimatorAvatar;
        public bool CachedAnimatorUseRootMotion;
        public AnimatorUpdateMode CachedAnimatorUpdateMode = AnimatorUpdateMode.Normal;
        public AnimatorCullingMode CachedAnimatorCullingMode = AnimatorCullingMode.AlwaysAnimate;

        #endregion
        
        #endregion

        #region INIT

        protected virtual void Start()
        {
            CachedAnimatorController = ThisAnimator.runtimeAnimatorController;
            CachedAnimatorAvatar = ThisAnimator.avatar;
            CachedAnimatorUseRootMotion = ThisAnimator.applyRootMotion;
            CachedAnimatorUpdateMode = ThisAnimator.updateMode;
            CachedAnimatorCullingMode = ThisAnimator.cullingMode;
        }

        public virtual void InitStats()
        {
            foreach (var t in GameDatabase.Instance.GetStats().Values)
            {
                var newAttributeToLoad = new CombatData.CombatEntityStat
                {
                    name = t.entryName,
                    stat = t,
                    currentMinValue = t.minValue,
                    currentValue = t.baseValue,
                    currentMaxValue = t.maxValue
                };


                Stats.Add(t.ID, newAttributeToLoad);
            }
        }
        
        public virtual void InitNPC(RPGNpc npcTemplate, CombatEntity owner)
        {
            
        }
        
        public virtual void InitNPCLevel()
        {

        }
        public virtual void InitCollisions(Collider localCollider)
        {

        }
        
        public virtual void InitializeVitalityStats(List<CombatData.VitalityStatEntry> vitalityStats)
        {
            foreach (var savedStat in vitalityStats)
            {
                foreach (var stat in Stats.Values)
                {
                    if(stat.stat.ID != savedStat.StatID) continue;
                    stat.currentValue = savedStat.value;

                    if (stat.stat.ID == GameDatabase.Instance.GetCombatSettings().HealthStatID)
                    {
                        bool hasDeathCondition = false;
                        float deathValue = 0;
                        foreach (var vitalityAction in stat.stat.vitalityActions)
                        {
                            if(vitalityAction.GameActionsTemplate == null) continue;
                            foreach (var gameAction in vitalityAction.GameActionsTemplate.GameActions)
                            {
                                if (gameAction.type != GameActionsData.GameActionType.Death) continue;
                                hasDeathCondition = true;
                                deathValue = vitalityAction.value;
                            }
                        }

                        if (hasDeathCondition && stat.currentValue <= deathValue)
                        {
                            stat.currentValue = deathValue+1;
                        }
                    }
                    StatCalculator.ClampStat(stat.stat, this);
                    CombatEvents.Instance.OnStatValueChanged(this, stat.stat, stat.currentValue, stat.currentMaxValue);
                }
            }
            CombatEvents.Instance.OnStatsChanged(this);
        }
        #endregion

        #region COMBAT LOOPS
        
        
        protected virtual void FixedUpdate()
        {
            HandleStates();
            HandleCombatTimes();
        }

        protected virtual void HandleCombatTimes()
        {
            if (InCombat) inCombatTime += Time.deltaTime;
            else outOfCombatTime += Time.deltaTime;
        }

        protected virtual void HandleCombatState()
        {
            
        }
        protected virtual void HandleToggledAbilities()
        {
            
        }
        public virtual void RemoveToggledAbility(RPGAbility ability)
        {
        }
        
        protected virtual void HandleStatShifting()
        {
            if (Dead) return;
            foreach (var t in Stats)
            {
                if (!t.Value.stat.isVitalityStat || !CanShift(t.Value)) continue;
                float baseShiftValue = 0;
                switch (InCombat)
                {
                    case true when Time.time >= t.Value.nextCombatShift && t.Value.stat.isShiftingInCombat:
                        t.Value.nextCombatShift = Time.time + t.Value.stat.shiftIntervalInCombat;
                        baseShiftValue = t.Value.stat.shiftAmountInCombat;
                        break;
                    case false when Time.time >= t.Value.nextRestShift && t.Value.stat.isShiftingOutsideCombat:
                        t.Value.nextRestShift = Time.time + t.Value.stat.shiftIntervalOutsideCombat;
                        baseShiftValue = t.Value.stat.shiftAmountOutsideCombat;
                        break;
                    default:
                        continue;
                }

                CombatUtilities.UpdateCurrentStatValue(this, t.Value.stat.ID,
                    baseShiftValue + CombatUtilities.GetTotalShiftAmount(this, t.Value.stat));
            }
        }

        protected virtual void HandleActiveCombos()
        {
            foreach (var combo in ActiveCombos)
            {
                if (combo.readyTime > 0)
                {
                    combo.curLoadTime += Time.deltaTime;
                    if (!(combo.curLoadTime >= combo.readyTime)) continue;
                    combo.curLoadTime = 0;
                    combo.readyTime = 0;
                }
                else
                {
                    combo.curTime -= Time.deltaTime;
                
                    if(combo.curTime > 0) continue;
                    StartCoroutine(RemoveComboEntry(combo, true, true));
                }
            }
        }

        protected virtual void HandleActiveBlocking()
        {
            UpdateActiveBlockingUI();
            if (ActiveBlockingState.effectRank.isBlockChargeTime && !ActiveBlockingState.blockIsDoneCharging)
            {
                ActiveBlockingState.curBlockChargeTime += Time.deltaTime;

                if (ActiveBlockingState.curBlockChargeTime >= ActiveBlockingState.targetBlockChargeTime)
                {
                    ActiveBlockingState.blockIsDoneCharging = true;
                }
            }

            if (!ActiveBlockingState.blockIsDoneCharging) return;

            if (ActiveBlockingState.effectRank.blockStatDecay && ActiveBlockingState.effectRank.blockStatID != -1)
            {
                if (Time.time >= ActiveBlockingState.nextBlockStatDrain)
                {
                    ActiveBlockingState.nextBlockStatDrain =
                        Time.time + ActiveBlockingState.effectRank.blockStatDecayInterval;
                    CombatUtilities.UpdateCurrentStatValue(this, ActiveBlockingState.effectRank.blockStatID,
                        -ActiveBlockingState.effectRank.blockStatDecayAmount);
                }
            }

            if (ActiveBlockingState.effectRank.blockDurationType == RPGEffect.BLOCK_DURATION_TYPE.Time &&
                ActiveBlockingState.effectRank.isBlockLimitedDuration && ActiveBlockingState.blockDurationLeft > 0)
            {
                ActiveBlockingState.blockDurationLeft -= Time.deltaTime;

                if (ActiveBlockingState.blockDurationLeft <= 0)
                {
                    ResetActiveBlocking();
                }
            }

            if (ActiveBlockingState.effectRank.isBlockPowerDecay)
            {
                float decayAmount = Time.deltaTime;
                decayAmount *= (ActiveBlockingState.effectRank.blockPowerDecay +
                                (ActiveBlockingState.effectRank.blockPowerDecay *
                                    CombatUtilities.GetTotalOfStatType(this,
                                        RPGStat.STAT_TYPE.BLOCK_ACTIVE_DECAY_MODIFIER) / 100f));
                ActiveBlockingState.curBlockPowerFlat -= decayAmount;
                if (ActiveBlockingState.curBlockPowerFlat < 0) ActiveBlockingState.curBlockPowerFlat = 0;
                ActiveBlockingState.curBlockPowerModifier -= decayAmount;
                if (ActiveBlockingState.curBlockPowerModifier < 0) ActiveBlockingState.curBlockPowerModifier = 0;
            }
        }

        protected virtual void UpdateActiveBlockingUI()
        {
            
        }
        
        
        protected virtual bool CanShift(CombatData.CombatEntityStat stat)
        {
            return true;
        }

        protected virtual void HandleStates()
        {
            for (var i = 0; i < States.Count; i++)
            {
                if(!States[i].stateEffect.endless) States[i].stateCurDuration += Time.deltaTime;
                if (States[i].curPulses > 0)
                {
                    States[i].nextPulse -= Time.deltaTime;
                }
                
                if (States[i].nextPulse <= 0 && States[i].curPulses < States[i].maxPulses)
                {
                    States[i].nextPulse = States[i].pulseInterval;
                    States[i].curPulses++;
                    
                    switch (States[i].stateEffect.effectType)
                    {
                        case RPGEffect.EFFECT_TYPE.DamageOverTime:
                            CombatManager.Instance.ExecuteDOTPulse(States[i].casterEntity, this, States[i].stateEffect, States[i].effectRank, States[i].curStack, States[i].abilityRank);
                            break;
                        case RPGEffect.EFFECT_TYPE.HealOverTime:
                            CombatManager.Instance.ExecuteHOTPulse(States[i].casterEntity, this, States[i].stateEffect, States[i].effectRank, States[i].curStack);
                            break;
                        case RPGEffect.EFFECT_TYPE.Stat:
                            StatCalculator.CalculateEffectsStats(this);
                            GameEvents.Instance.OnTriggerVisualEffectsList(this, States[i].effectRank.VisualEffectEntries, ActivationType.Start);
                            GameEvents.Instance.OnTriggerSoundsList(this, States[i].effectRank.SoundEntries, ActivationType.Start, transform);
                            break;
                    }

                    if (i + 1 > States.Count) return;
                }
                
                if (States[i].stateEffect.endless || !(States[i].stateCurDuration >= States[i].stateMaxDuration)) continue;
                EndStateEffect(i);
                return;
            }
        }

        #endregion

        #region COMBAT EVENTS

        public virtual void InitAbility(RPGAbility ab, RPGAbility.RPGAbilityRankData rank)
        {
            
        }
        public virtual void EndAbility(float attackTime)
        {
            
        }
        
        public virtual void TakeDamage(CombatCalculations.DamageResult result, RPGAbility.RPGAbilityRankData abilityRank,
            int alteredStatID)
        {
            if (Dead) return;
            
            if(result.caster.GetCurrentPets().Count > 0) CombatManager.Instance.CheckIfPetsShouldAttack(result.caster, this);

            RPGStat alteredStatRef = GameDatabase.Instance.GetStats()[alteredStatID];
            if (alteredStatRef == null) return;

            if (CombatUtilities.StatHasDeathCondition(alteredStatRef))
            {
                LatestAttacker = result.caster;
                LatestAbilityRankHit = abilityRank;
            }
        }

        public virtual void Heal(CombatCalculations.HealResult healResult, int alteredStatID)
        {
            RPGStat alteredStatRef = GameDatabase.Instance.GetStats()[alteredStatID];
            if (alteredStatRef == null) return;
            CombatUtilities.UpdateCurrentStatValue(this, alteredStatID, healResult.HealAmount);
        }

        public virtual void EntityDeath ()
        {
            if (Dead) return;
            Dead = true;
            
            CombatManager.Instance.RemoveCombatNodeFromThreatTables(this);

            foreach (var entity in GameState.combatEntities)
            {
                if(entity == this || entity.Dead) continue;
                if (entity.GetTarget() == this)
                {
                    entity.ResetTarget();
                }
            }
            
            if(Casting || Channeling) InterruptCastActions();
            if(InteractingWithObject) InterruptObjectInteraction();
            CombatManager.Instance.DestroyDeadNodeCombatEntities(this);
            
            EndAllStateEffects();
            
            if(this == GameState.playerEntity.GetTarget()) CombatEvents.Instance.OnPlayerTargetChanged(this, null);
            
            CombatEvents.Instance.OnEntityDied(this);
            
        }

        public virtual void InstantDeath()
        {
            CombatUtilities.SetCurrentStatValue(this, GameDatabase.Instance.GetHealthStat().ID, 0);
            CombatEvents.Instance.OnStatValueChanged(this, GameDatabase.Instance.GetHealthStat(),
                0, CombatUtilities.GetMaxStatValue(this, GameDatabase.Instance.GetHealthStat().ID));
            CombatEvents.Instance.OnStatsChanged(this);
            EntityDeath();
        }

        public virtual void InitRespawn(Vector3 respawnPosition)
        {
        }

        public virtual void EnterCombat()
        {
            InCombat = true;
            LastCombatActionTimer = 0;
            inCombatTime = 0;
            outOfCombatTime = 0;
            CombatEvents.Instance.OnCombatEntered(this);
        }
        public virtual void ResetCombat()
        {
            InCombat = false;
            LastCombatActionTimer = 0;
            inCombatTime = 0;
            outOfCombatTime = 0;
            CombatEvents.Instance.OnCombatExited(this);
        }

        protected virtual void EndAllStateEffects()
        {
            for (int i = 0; i < States.Count; i++)
            {
                EndStateEffect(i);
            }
        }
        
        public virtual void EndStateEffect(int stateIndex)
        {
            
            switch (States[stateIndex].stateEffect.effectType)
            {
                case RPGEffect.EFFECT_TYPE.Stun:
                case RPGEffect.EFFECT_TYPE.Sleep:
                case RPGEffect.EFFECT_TYPE.Root:
                    ThisAnimator.SetBool("Stunned", false);
                    ResetStun();
                    break;
                case RPGEffect.EFFECT_TYPE.Shapeshifting:
                    break;

                case RPGEffect.EFFECT_TYPE.Flying:
                    break;

                case RPGEffect.EFFECT_TYPE.Stealth:
                    ResetStealth();
                    return;
                case RPGEffect.EFFECT_TYPE.Mount:
                    ResetMount();
                    break;
            }

            bool statEffect = States[stateIndex].stateEffect.effectType == RPGEffect.EFFECT_TYPE.Stat;
            
            CombatEvents.Instance.OnStateEnded(this, stateIndex);
            States.RemoveAt(stateIndex);
            if (statEffect) StatCalculator.CalculateEffectsStats(this);
        }
        
        public virtual void RemoveEffectByID(int effectID)
        {
            for (var index = 0; index < States.Count; index++)
            {
                var effect = States[index];
                if (effect.stateEffect.ID != effectID) continue;

                EndStateEffect(index);
                break;
            }
        }
        
        public virtual void InitStandTime(RPGAbility.RPGAbilityRankData rank)
        {
            
        }
        
        public virtual void InitCastSlow(RPGAbility.RPGAbilityRankData rank)
        {
            
        }
        
        public virtual void InitEffectType(RPGEffect effect, int rank)
        {
            
        }
        
        public virtual void InitShapeshifting(RPGEffect effect, int rank)
        {
            
        }
        
        public virtual void ResetShapeshifting()
        {
            
        }
        public virtual void InitStealth(bool showActionBar, List<RPGAbility.AbilityEffectsApplied> nestedEffects)
        {
            
        }
        public virtual void ResetStealth()
        {
            Stealth = false;
            CombatManager.Instance.DestroyStealthEndNodeCombatEntities(this);
        }
        
        public virtual void ResetStun()
        {
            
        }
        
        public virtual void InitFlying()
        {
            
        }
        public virtual void InitStun(CombatEntity attacker)
        {
            if(Casting || Channeling) InterruptCastActions();
            if(InteractingWithObject) InterruptObjectInteraction();
            if(!Dead) ThisAnimator.SetBool("Stunned", true);
        }
        
        public virtual void InitKnockback(CombatEntity attacker, float distance)
        {
            
        }
        public virtual void ResetKnockback()
        {
            
        }
        
        public virtual void HandleKnockback()
        {
        }
        
        public virtual void InitMotion(RPGEffect.RPGEffectRankData rank)
        {
            
        }
        
        public virtual void InitMount(RPGEffect effect, int rankIndex)
        {
            if (effect.ranks[rankIndex].MountPrefab == null) return;
            isMounted = true;
            GameObject newMount = Instantiate(effect.ranks[rankIndex].MountPrefab, transform);
            mount = newMount;
            mountEffect = effect;
            mountEffectRank = rankIndex;
            newMount.transform.localPosition = effect.ranks[rankIndex].MountPosition;
            newMount.transform.localScale = effect.ranks[rankIndex].MountScale;
            newMount.transform.localEulerAngles = Vector3.zero;

            Animator anim = newMount.AddComponent<Animator>();
            anim.runtimeAnimatorController = effect.ranks[rankIndex].MountAnimatorController;
            anim.avatar = effect.ranks[rankIndex].MountAvatar;
            mountAnimator = anim;
            
            StatCalculator.CalculateMountStats(this);
            CombatManager.Instance.HandleNestedEffects(this, effect.ranks[rankIndex].nestedEffects);
            
            if(!string.IsNullOrEmpty(effect.ranks[rankIndex].MountAnimationParameter)) ThisAnimator.SetBool(effect.ranks[rankIndex].MountAnimationParameter, true);
        }

        public virtual void ResetMount()
        {
            Destroy(mount);
            isMounted = false;
            mount = null;
            mountAnimator = null;
            
            StatCalculator.ResetMountStats(this);
            CombatManager.Instance.ResetNestedEffects(this, mountEffect, mountEffect.ranks[mountEffectRank]);

            for (var index = 0; index < States.Count; index++)
            {
                var state = States[index];
                if(state.stateEffect != mountEffect) continue;
                EndStateEffect(index);
            }

            if(!string.IsNullOrEmpty(mountEffect.ranks[mountEffectRank].MountAnimationParameter)) ThisAnimator.SetBool(mountEffect.ranks[mountEffectRank].MountAnimationParameter, false);
            mountEffect = null;
            mountEffectRank = -1;
        }
        
        public virtual void InitActiveBlocking(RPGEffect effect, RPGEffect.RPGEffectRankData rank)
        {
            
        }
        public virtual void ReduceActiveBlockingDamageLeft(int amount)
        {
            ActiveBlockingState.curBlockedDamageLeft -= amount;
            
            if(ActiveBlockingState.curBlockedDamageLeft <= 0) ResetActiveBlocking();
        }
        public virtual IEnumerator IncreaseBlockHitCount()
        {
            ActiveBlockingState.curBlockHitCount++;
            yield return new WaitForEndOfFrame();
            if (ActiveBlockingState.curBlockHitCount >= (ActiveBlockingState.effectRank.blockHitCount + CombatUtilities.GetTotalOfStatType(this,
                RPGStat.STAT_TYPE.BLOCK_ACTIVE_COUNT)))
            {
                ResetActiveBlocking();
            }
        }
        
        public virtual void InitGroundAbility(RPGAbility ability, RPGAbility.RPGAbilityRankData rank)
        {
            CurrentAbilityCasted = ability;
            CurrentAbilityCastedCurRank = rank;
        }

        public virtual void InterruptCastActions()
        {
            GameEvents.Instance.OnTriggerVisualEffectsList(this, CurrentAbilityCastedCurRank.VisualEffectEntries,
                ActivationType.Interrupted);
            GameEvents.Instance.OnTriggerAnimationsList(this, CurrentAbilityCastedCurRank.AnimationEntries,
                ActivationType.Interrupted);
            GameEvents.Instance.OnTriggerSoundsList(this, CurrentAbilityCastedCurRank.SoundEntries,
                ActivationType.Interrupted, transform);

            if (Casting) ResetCasting();
            if (Channeling) ResetChanneling();
        }

        public virtual void InterruptObjectInteraction()
        {
            currentInteractableObject.TriggerVisualEffects(ActivationType.Interrupted);
            currentInteractableObject.TriggerAnimations(ActivationType.Interrupted);
            currentInteractableObject.TriggerSounds(ActivationType.Interrupted);
            
            InteractingWithObject = false;
            CurrentInteractionProgress = 0;
            TargetInteractionTime = 0;
            currentInteractableObject = null;
        }

        public virtual void DisableCombat()
        {
            
        }

        public virtual void RemoveFromEntityList()
        {
            GameState.Instance.UnregisterEntity(this);
        }
        
        public virtual void RemoveFromThreatTable(CombatEntity entity)
        {
            
        }

        public virtual void SetTarget(CombatEntity entity)
        {
            
        }
        public virtual void ResetTarget()
        {
            
        }
        
        public virtual CombatEntity GetTarget()
        {
            return CurrentTarget;
        }
        
        public virtual bool CanStartCasting()
        {
            return false;
        }
        
        public virtual IEnumerator RemoveComboEntry(CombatData.ActiveCombo combo, bool resetActionBarImage, bool resetComboActive)
        {
            yield break;
        }
        
        public virtual void ResetActiveBlocking()
        {
            ActiveBlocking = false;
            ActiveBlockingState.curBlockChargeTime = 0;
            ActiveBlockingState.targetBlockChargeTime = 0;
            ActiveBlockingState.blockDurationLeft = 0;
            ActiveBlockingState.curBlockPowerFlat = 0;
            ActiveBlockingState.curBlockHitCount = 0;
            ActiveBlockingState.curBlockPowerModifier = 0;
            ActiveBlockingState.curBlockedDamageLeft = 0;
            ActiveBlockingState.blockIsDoneCharging = false;
        }

        public virtual void InitAnimation(AnimationEntry animationEntry, string parameterName)
        {
            
        }
        
        public virtual void InitAnimationTemplate(AnimationTemplate animationTemplate, string parameterName)
        {
            
        }

        protected virtual IEnumerator PlayAnimation(AnimationEntry animationEntry, string parameterName)
        {
            yield return new WaitForSeconds(animationEntry.Delay);
            if (Dead) yield break;

            if (animationEntry.ModifySpeed)
                ThisAnimator.SetFloat(animationEntry.SpeedParameterName, animationEntry.ModifierSpeed);
            else 
                ThisAnimator.SetFloat(animationEntry.SpeedParameterName, 1);

            switch (animationEntry.Template.ParameterType)
            {
                case AnimationParameterType.Bool:
                    ThisAnimator.SetBool(parameterName, animationEntry.Template.BoolValue);
                    if (animationEntry.Template.ResetAfterDuration)
                    {
                        yield return new WaitForSeconds(animationEntry.Template.Duration);
                        ThisAnimator.SetBool(parameterName, !animationEntry.Template.BoolValue);
                        ResetQueuedAnimation(animationEntry);
                    }
                    break;
                case AnimationParameterType.Int:
                    ThisAnimator.SetInteger(parameterName, animationEntry.Template.IntValue);
                    break;
                case AnimationParameterType.Float:
                    ThisAnimator.SetFloat(parameterName, animationEntry.Template.FloatValue);
                    break;
                case AnimationParameterType.Trigger:
                    ThisAnimator.SetTrigger(parameterName);
                    break;
            }
        }

        public virtual void ResetQueuedAnimation(AnimationEntry animationEntry)
        {
            List<ActiveAnimationCoroutine> animationCoroutinesToRemove = new List<ActiveAnimationCoroutine>();
            foreach (var animationCoroutine in CurrentAnimationCoroutines)
            {
                if (ThisAnimator != animationCoroutine.Anim || animationEntry != animationCoroutine.AnimationEntry) continue;
                
                StopCoroutine(animationCoroutine.Coroutine);
                animationCoroutinesToRemove.Add(animationCoroutine);
            }
            
            foreach (var animationCoroutine in animationCoroutinesToRemove)
            {
                CurrentAnimationCoroutines.Remove(animationCoroutine);
            }
        }

        public virtual void OnBlock(CombatCalculations.DamageResult result)
        {
            if (ActiveBlockingState.effectRank.GameActionsTemplate != null)
            {
                GameActionsManager.Instance.TriggerGameActions(this, ActiveBlockingState.effectRank.GameActionsTemplate.GameActions);
            }

            switch (ActiveBlockingState.effectRank.blockEndType)
            {
                case RPGEffect.BLOCK_END_TYPE.MaxDamageBlocked:
                    CombatEvents.Instance.OnPlayerActiveBlockedDamage();
                    break;
                case RPGEffect.BLOCK_END_TYPE.HitCount:
                    StartCoroutine(IncreaseBlockHitCount());
                    break;
            }
        }
        #endregion

        #region UNITY EVENTS
        private void OnDestroy()
        {
            if (RPGBuilderEssentials.Instance.getCurrentScene().name == GameDatabase.Instance.GetGeneralSettings().mainMenuSceneName) return;
            RemoveFromEntityList();
        }

        #endregion
        
        #region COMBAT INFO

        public virtual RPGAbility.RPGAbilityRankData GetCurrentAbilityRank(RPGAbility ability, bool abMustBeKnown)
        {
            return null;
        }

        public virtual bool IsAbilityReady(RPGAbility ability)
        {
            return false;
        }
        
        public virtual void StartAbilityCooldown(RPGAbility.RPGAbilityRankData rank, int abilityID)
        {
        }
        
        public virtual void SetProjectileRotation(GameObject projectile, RPGAbility.RPGAbilityRankData rank, float yOffset)
        {
            
        }
        public virtual void SetProjectileGravity(Rigidbody projectile)
        {
            
        }
        public virtual List<RPGStat.VitalityActions> GetAllVitalityActions(List<RPGStat.VitalityActions> actions, RPGStat stat, int statIndex)
        {
            return actions;
        }
        
        public virtual bool CanActiveBlockThis(CombatEntity attacker)
        {
            return (ActiveBlocking && ActiveBlockingState.blockIsDoneCharging) && CombatManager.Instance.CanCombatNodeBlockThis(this, attacker);
        }

        public virtual bool IsPlayer()
        {
            return false;
        }
        
        public virtual bool IsNPC()
        {
            return false;
        }
        
        public virtual RPGNpc GetNPCData()
        {
            return null;
        }
        
        public virtual AIEntity GetAIEntity()
        {
            return null;
        }
        
        public virtual NPCSpawner GetSpawner()
        {
            return null;
        }
        public virtual void SetSpawner(NPCSpawner spawner)
        {
            Spawner = spawner;
        }
        #endregion

        #region EQUIPPED ITEMS

        public virtual EconomyData.EquippedWeapon GetEquippedWeaponByIndex(int weaponIndex)
        {
            return equippedWeapons.Count - 1 < weaponIndex ? null : equippedWeapons[weaponIndex];
        }

        #endregion

        #region LOCKED STATES

        #region Casting Logic
        public virtual void InitCasting(RPGAbility thisAbility, RPGAbility.RPGAbilityRankData rankREF)
        {
            EffectTriggered = false;
            Casting = true;
            CurrentAbilityCasted = thisAbility;
            CurrentAbilityCastedCurRank = rankREF;
            CurrentCastProgress = 0;
            TargetCastTime = CombatManager.Instance.CalculateCastTime(this, rankREF.castTime);
            
            CombatEvents.Instance.OnStartedCastingAbility(this, thisAbility, rankREF, TargetCastTime);
        }

        protected virtual void  HandleCasting()
        {

        }
        
        public virtual void  ResetCasting()
        {
            Casting = false;
            CurrentAbilityCasted = null;
            CurrentTargetCasted = null;
            CurrentCastProgress = 0;
            TargetCastTime = 0;
            CurrentAbilityCastedCurRank = null;
        }
        
        protected virtual void  CastingCompleted()
        {

        }
        
        protected bool EffectTriggered;
        protected virtual void TriggerEffect() {
            if (Casting) {
                EffectTriggered = true;
            }
        }
        #endregion
        
        #region Channeling Logic
        public virtual void InitChanneling(RPGAbility thisAbility, RPGAbility.RPGAbilityRankData rankREF)
        {
            Channeling = true;
            CurrentAbilityCasted = thisAbility;
            CurrentAbilityCastedCurRank = rankREF;
            CurrentChannelProgress = rankREF.channelTime;
            TargetChannelTime = 0;
        }

        protected virtual void  HandleChanneling()
        {

        }
        
        protected virtual void  ResetChanneling()
        {
            Channeling = false;
            CurrentAbilityCasted = null;
            CurrentChannelProgress = 0;
            TargetChannelTime = 0;
            CurrentAbilityCastedCurRank = null;
        }
        #endregion
        

        #region Object Interaction Logic
        public virtual void InitObjectInteraction(InteractableObject node, float duration)
        {
            InteractingWithObject = true;
            currentInteractableObject = node;
            CurrentInteractionProgress = 0;
            TargetInteractionTime = duration;
        }

        protected virtual void  HandleObjectInteraction()
        {

        }
        
        protected virtual void  NodeObjectInteractionCompleted()
        {

        }
        
        protected virtual void  ResetObjectInteraction()
        {
            currentInteractableObject.TriggerVisualEffects(ActivationType.Cancelled);
            currentInteractableObject.TriggerAnimations(ActivationType.Cancelled);
            currentInteractableObject.TriggerSounds(ActivationType.Cancelled);
            
            InteractingWithObject = false;
            CurrentInteractionProgress = 0;
            TargetInteractionTime = 0;
            currentInteractableObject = null;
        }
        #endregion
        
        #region Leaping Logic
        
        public virtual void InitLeaping(Vector3 startpos, Vector3 endpos, float height, float speed)
        {
            Leaping = true;
            StartPos = startpos;
            LeapEndPos = new Vector3(endpos.x, endpos.y - 0.025f, endpos.z);
            LeapHeight = height;
            LeapSpeed = speed;
            transform.LookAt(endpos);
        }

        protected virtual void  HandleLeaping()
        {
            
        }
        
        protected virtual void  ResetLeaping()
        {
            Leaping = false;
            LeapAnimation = 0;
            LeapHeight = 0;
            LeapSpeed = 0;

            if (CurrentAbilityCastedCurRank.targetType == RPGAbility.TARGET_TYPES.GROUND_LEAP && CurrentAbilityCastedCurRank.extraAbilityExecutedID != -1)
            {
                CombatManager.Instance.InitExtraAbility(this, GameDatabase.Instance.GetAbilities()[CurrentAbilityCastedCurRank.extraAbilityExecutedID]);
            }
        }
        #endregion
        #endregion

        #region STATES INFO

        public virtual bool IsStunned()
        {
            return States.Any(t => t.stateEffect.effectType == RPGEffect.EFFECT_TYPE.Stun);
        }

        public virtual bool IsSilenced()
        {
            return States.Any(t => t.stateEffect.effectType == RPGEffect.EFFECT_TYPE.Silence);
        }

        public virtual bool IsSleeping()
        {
            return States.Any(t => t.stateEffect.effectType == RPGEffect.EFFECT_TYPE.Sleep);
        }

        public virtual bool IsRooted()
        {
            return States.Any(t => t.stateEffect.effectType == RPGEffect.EFFECT_TYPE.Root);
        }

        public virtual bool IsTaunted()
        {
            return States.Any(t => t.stateEffect.effectType == RPGEffect.EFFECT_TYPE.Taunt);
        }

        public virtual bool IsImmune()
        {
            return States.Any(t => t.stateEffect.effectType == RPGEffect.EFFECT_TYPE.Immune);
        }
        
        public virtual bool IsInMotion()
        {
            return false;
        }

        #endregion

        #region ON MOUSE EVENTS

        protected virtual void OnMouseDown()
        {

        }

        protected virtual void OnMouseOver()
        {

        }

        protected virtual void OnMouseExit()
        {

        }


        #endregion

        #region IPLAYERINTERACTABLE

        public virtual void Interact()
        {
            
        }

        public virtual  void ShowInteractableUI()
        {
            
        }

        public virtual  string getInteractableName()
        {
            return "";
        }

        public virtual  bool isReadyToInteract()
        {
            return false;
        }

        public virtual  RPGCombatDATA.INTERACTABLE_TYPE getInteractableType()
        {
            return RPGCombatDATA.INTERACTABLE_TYPE.None;
        }

        protected virtual bool HasInteractions()
        {
            return false;
        }

        #endregion
        
    }
}
