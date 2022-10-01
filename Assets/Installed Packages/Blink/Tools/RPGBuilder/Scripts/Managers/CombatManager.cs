using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.AI;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.Templates;
using BLINK.RPGBuilder.UI;
using BLINK.RPGBuilder.UIElements;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BLINK.RPGBuilder.Managers
{
    public class CombatManager : MonoBehaviour
    {
        public float currentGCD;
        
        private void Awake()
        {
            if (Instance != null) return;
            Instance = this;
        }
        
        private void OnEnable()
        {
            CombatEvents.PlayerFactionStanceChanged += HandleFactionChangeAggro;
        }

        private void OnDisable()
        {
            CombatEvents.PlayerFactionStanceChanged -= HandleFactionChangeAggro;
        }

        public void HandleTurnOffCombat()
        {
            foreach (var t in GameState.combatEntities)
            {
                t.DisableCombat();
            }
        }

        public void RemoveCombatNodeFromThreatTables(CombatEntity entity)
        {
            foreach (var t in GameState.combatEntities)
                t.RemoveFromThreatTable(entity);
        }

        public void DestroyDeadNodeCombatEntities(CombatEntity entity)
        {
            foreach (var t in entity.GetOwnedCombatVisuals())
            {
                if (entity.GetOwnedLogicCombatVisuals().Contains(t)) entity.GetOwnedLogicCombatVisuals().Remove(t);
                if (entity.GetDestroyedOnStealthCombatVisuals().Contains(t)) entity.GetDestroyedOnStealthCombatVisuals().Remove(t);
                if (entity.GetDestroyedOnStealthEndCombatVisuals().Contains(t)) entity.GetDestroyedOnStealthEndCombatVisuals().Remove(t);
                Destroy(t.gameObject);
            }
        }

        private void DestroyStunnedNodeCombatEntities(CombatEntity entity)
        {
            foreach (var t in entity.GetOwnedLogicCombatVisuals())
            {
                if (entity.GetOwnedCombatVisuals().Contains(t)) entity.GetOwnedCombatVisuals().Remove(t);
                if (entity.GetDestroyedOnStealthCombatVisuals().Contains(t)) entity.GetDestroyedOnStealthCombatVisuals().Remove(t);
                if (entity.GetDestroyedOnStealthEndCombatVisuals().Contains(t)) entity.GetDestroyedOnStealthEndCombatVisuals().Remove(t);
                Destroy(t.gameObject);
            }
        }
        
        public void DestroyStealthNodeCombatEntities(CombatEntity entity)
        {
            foreach (var t in entity.GetDestroyedOnStealthCombatVisuals())
            {
                if (entity.GetOwnedCombatVisuals().Contains(t)) entity.GetOwnedCombatVisuals().Remove(t);
                if (entity.GetOwnedLogicCombatVisuals().Contains(t)) entity.GetOwnedLogicCombatVisuals().Remove(t);
                if (entity.GetDestroyedOnStealthEndCombatVisuals().Contains(t)) entity.GetDestroyedOnStealthEndCombatVisuals().Remove(t);
                Destroy(t.gameObject);
            }
        }
        
        public void DestroyStealthEndNodeCombatEntities(CombatEntity entity)
        {
            foreach (var t in entity.GetDestroyedOnStealthEndCombatVisuals())
            {
                if (entity.GetOwnedCombatVisuals().Contains(t)) entity.GetOwnedCombatVisuals().Remove(t);
                if (entity.GetOwnedLogicCombatVisuals().Contains(t)) entity.GetOwnedLogicCombatVisuals().Remove(t);
                if (entity.GetDestroyedOnStealthCombatVisuals().Contains(t)) entity.GetDestroyedOnStealthCombatVisuals().Remove(t);
                Destroy(t.gameObject);
            }
        }

        public static CombatManager Instance { get; private set; }

        public bool LayerContains(LayerMask mask, int layer)
        {
            return mask == (mask | (1 << layer));
        }

        public List<CombatEntity> getPotentialCombatNodes(Collider[] allColliders, CombatEntity casterCbtInfo,
            RPGAbility.RPGAbilityRankData rankREF)
        {
            List<CombatEntity> allCbtNodes = new List<CombatEntity>();
            foreach (Collider t in allColliders)
            {
                CombatEntity thisCbtNode = t.GetComponent<CombatEntity>();
                if (thisCbtNode == null) continue;
                FactionManager.CanHitResult hitResult = FactionManager.Instance.AttackerCanHitTarget(rankREF, casterCbtInfo, thisCbtNode);
                if (hitResult.canHit) allCbtNodes.Add(thisCbtNode);
            }

            return allCbtNodes;
        }

        private CombatData.CombatEntityStateEffect GenerateNewState(RPGEffect effect, int effectRank, CombatEntity casterInfo, Sprite icon)
        {
            return new CombatData.CombatEntityStateEffect
            {
                stateName = effect.entryName,
                casterEntity = casterInfo,
                stateMaxDuration = effect.duration,
                stateCurDuration = 0,
                curStack = 1,
                maxStack = effect.stackLimit,
                stateEffect = effect,
                stateEffectID = effect.ID,
                effectRank = effect.ranks[effectRank],
                effectRankIndex = effectRank,
                stateIcon = icon
            };
        }

        private bool isEffectCC(RPGEffect.EFFECT_TYPE effectType)
        {
            return effectType == RPGEffect.EFFECT_TYPE.Stun
                   || effectType == RPGEffect.EFFECT_TYPE.Root
                   || effectType == RPGEffect.EFFECT_TYPE.Silence
                   || effectType == RPGEffect.EFFECT_TYPE.Sleep
                   || effectType == RPGEffect.EFFECT_TYPE.Taunt;
        }
        private bool isEffectUnique(RPGEffect.EFFECT_TYPE effectType)
        {
            return effectType == RPGEffect.EFFECT_TYPE.Immune
                   || effectType == RPGEffect.EFFECT_TYPE.Stun
                   || effectType == RPGEffect.EFFECT_TYPE.Root
                   || effectType == RPGEffect.EFFECT_TYPE.Silence
                   || effectType == RPGEffect.EFFECT_TYPE.Sleep
                   || effectType == RPGEffect.EFFECT_TYPE.Taunt
                   || effectType == RPGEffect.EFFECT_TYPE.Shapeshifting
                   || effectType == RPGEffect.EFFECT_TYPE.Flying
                   || effectType == RPGEffect.EFFECT_TYPE.Stealth
                   || effectType == RPGEffect.EFFECT_TYPE.Mount;
        }
        private bool isEffectState(RPGEffect.EFFECT_TYPE effectType)
        {
            return effectType == RPGEffect.EFFECT_TYPE.DamageOverTime
                   || effectType == RPGEffect.EFFECT_TYPE.HealOverTime
                   || effectType == RPGEffect.EFFECT_TYPE.Stat
                   || effectType == RPGEffect.EFFECT_TYPE.Shapeshifting
                   || effectType == RPGEffect.EFFECT_TYPE.Flying
                   || effectType == RPGEffect.EFFECT_TYPE.Stealth
                   || effectType == RPGEffect.EFFECT_TYPE.Mount;
        }

        private void InitNewStateEffect(RPGEffect effect, int effectRank, CombatEntity casterInfo, Sprite icon,
            CombatEntity targetInfo)
        {
            var newState = GenerateNewState(effect, effectRank, casterInfo, icon);

            newState.curPulses = 0;
            newState.maxPulses = effect.pulses;
            newState.pulseInterval = effect.duration / effect.pulses;

            targetInfo.GetStates().Add(newState);

            CombatEvents.Instance.OnStateStarted(targetInfo, targetInfo.GetStates().Count - 1);
        }

        private float getTotalCCDuration(CombatEntity casterInfo, CombatEntity targetInfo, float duration)
        {
            float CC_POWER = CombatUtilities.GetTotalOfStatType(casterInfo, RPGStat.STAT_TYPE.CC_POWER);
            float CC_RES = CombatUtilities.GetTotalOfStatType(targetInfo, RPGStat.STAT_TYPE.CC_RESISTANCE);
                    
            duration += duration * (CC_POWER / 100f);
            float CCResMod = 1 - (CC_RES / 100f);
            duration *= CCResMod;
            return duration;
        }

        public IEnumerator InitEntityState(CombatEntity casterEntity, CombatEntity targetEntity, RPGEffect effect, int effectRank, Sprite icon, float delay)
        {
            if (targetEntity.IsDead()) yield break;

            yield return new WaitForSeconds(delay);

            if (targetEntity.IsDead()) yield break;

            if (effect.effectType == RPGEffect.EFFECT_TYPE.Mount && targetEntity.IsMounted())
            {
                targetEntity.ResetMount();
                yield break;
            }

            var hasSameUniqueEffect = false;
            var hasSameState = false;
            var curStateIndex = -1;
            var allNodeStates = targetEntity.GetStates();

            if (isEffectUnique(effect.effectType))
            {
                GameEvents.Instance.OnTriggerVisualEffectsList(targetEntity, effect.ranks[effectRank].VisualEffectEntries, ActivationType.Start);
                GameEvents.Instance.OnTriggerAnimationsList(targetEntity, effect.ranks[effectRank].AnimationEntries, ActivationType.Start);
                GameEvents.Instance.OnTriggerSoundsList(targetEntity, effect.ranks[effectRank].SoundEntries, ActivationType.Start, targetEntity.transform);
            }

            for (var i = 0; i < allNodeStates.Count; i++)
            {
                if (!isEffectUnique(effect.effectType) || effect.effectType != allNodeStates[i].stateEffect.effectType) continue;
                hasSameUniqueEffect = true;
                curStateIndex = i;
                break;
            }

            if (!hasSameUniqueEffect)
            {
                for (var i = 0; i < allNodeStates.Count; i++)
                {
                    if (!isEffectState(effect.effectType) || effect != allNodeStates[i].stateEffect) continue;
                    hasSameState = true;
                    curStateIndex = i;
                    break;
                }
            }

            if (hasSameUniqueEffect)
            {
                switch (effect.effectType)
                {
                    case RPGEffect.EFFECT_TYPE.Shapeshifting:
                        targetEntity.ResetShapeshifting();
                        break;
                    default:
                    {
                        CombatEvents.Instance.OnStateEnded(targetEntity, curStateIndex);

                        targetEntity.GetStates().RemoveAt(curStateIndex);
                        break;
                    }
                }

                var newState = GenerateNewState(effect, effectRank, casterEntity, icon);

                if (isEffectCC(effect.effectType))
                    newState.stateMaxDuration = getTotalCCDuration(casterEntity, targetEntity, newState.stateMaxDuration);

                targetEntity.GetStates().Add(newState);
                
                    CombatEvents.Instance.OnStateStarted(targetEntity, targetEntity.GetStates().Count - 1);

                switch (effect.effectType)
                {
                    case RPGEffect.EFFECT_TYPE.Shapeshifting:
                        targetEntity.InitShapeshifting(effect, effectRank);
                        break;
                    case RPGEffect.EFFECT_TYPE.Flying:
                        targetEntity.InitFlying();
                        break;
                    case RPGEffect.EFFECT_TYPE.Stealth:
                        targetEntity.InitStealth(effect.ranks[effectRank].showStealthActionBar,
                            effect.ranks[effectRank].nestedEffects);
                        break;
                    case RPGEffect.EFFECT_TYPE.Root:
                    case RPGEffect.EFFECT_TYPE.Sleep:
                    case RPGEffect.EFFECT_TYPE.Stun:
                        targetEntity.InitStun(casterEntity);
                        break;
                    case RPGEffect.EFFECT_TYPE.Mount:
                        targetEntity.InitMount(effect, effectRank);
                        break;
                }
            }
            else if (hasSameState)
            {
                if (targetEntity.GetStates()[curStateIndex].casterEntity == casterEntity || targetEntity.GetStates()[curStateIndex].stateEffect.allowMixedCaster
                ) // same effect: from same caster || mixed caster is allowed
                {
                    if (targetEntity.GetStates()[curStateIndex].curStack < targetEntity.GetStates()[curStateIndex].maxStack)
                        targetEntity.GetStates()[curStateIndex].curStack++;
                    else
                    {
                        if (effect.allowMultiple)
                        {
                            InitNewStateEffect(effect, effectRank, casterEntity, icon, targetEntity);
                            yield break;
                        }
                    }

                    // REFRESH THE EFFECT
                    targetEntity.GetStates()[curStateIndex].curPulses = 0;
                    targetEntity.GetStates()[curStateIndex].nextPulse = 0;
                    targetEntity.GetStates()[curStateIndex].stateCurDuration = 0;

                        CombatEvents.Instance.OnStateRefreshed(targetEntity, curStateIndex);
                }
                else if (targetEntity.GetStates()[curStateIndex].stateEffect.allowMultiple)
                {
                    // caster is: not same || mixed caster is not allowed
                    // we add it as a new effect
                    InitNewStateEffect(effect, effectRank, casterEntity, icon, targetEntity);
                }

                if (!targetEntity.IsPlayer()) HandleMobAggro(casterEntity, targetEntity);
            }
            else
            {
                var newState = GenerateNewState(effect, effectRank, casterEntity, icon);

                if (isEffectCC(effect.effectType))
                {
                    newState.stateMaxDuration = getTotalCCDuration(casterEntity, targetEntity, newState.stateMaxDuration);
                    if (newState.stateMaxDuration == 0)
                        yield break;
                }

                if (effect.effectType == RPGEffect.EFFECT_TYPE.DamageOverTime ||
                    effect.effectType == RPGEffect.EFFECT_TYPE.HealOverTime ||
                    effect.effectType == RPGEffect.EFFECT_TYPE.Stat)
                {
                    newState.curPulses = 0;
                    newState.maxPulses = effect.pulses;
                    newState.pulseInterval = effect.duration / effect.pulses;
                }

                targetEntity.GetStates().Add(newState);
                    CombatEvents.Instance.OnStateStarted(targetEntity, targetEntity.GetStates().Count - 1);


                switch (effect.effectType)
                {
                    case RPGEffect.EFFECT_TYPE.Stun:
                    case RPGEffect.EFFECT_TYPE.Sleep:
                        if(targetEntity.IsCasting() || targetEntity.IsChanneling()) targetEntity.InterruptCastActions();
                        if(targetEntity.IsInteractingWithObject()) targetEntity.InterruptObjectInteraction();
                        DestroyStunnedNodeCombatEntities(targetEntity);
                        targetEntity.InitStun(casterEntity);
                        break;
                    case RPGEffect.EFFECT_TYPE.Shapeshifting:
                        targetEntity.InitShapeshifting(effect, effectRank);
                        break;
                    case RPGEffect.EFFECT_TYPE.Flying:
                        targetEntity.InitFlying();
                        break;
                    case RPGEffect.EFFECT_TYPE.Stealth:
                        targetEntity.InitStealth(effect.ranks[effectRank].showStealthActionBar,
                            effect.ranks[effectRank].nestedEffects);
                        break;
                    case RPGEffect.EFFECT_TYPE.Root:
                        targetEntity.InitStun(casterEntity);
                        break;
                    case RPGEffect.EFFECT_TYPE.Mount:
                        targetEntity.InitMount(effect,effectRank);
                        break;
                }
            }
        }

        public void InitializePersistentState(CharacterEntries.StateEntry state, CombatEntity targetEntity)
        {
            RPGEffect effect = GameDatabase.Instance.GetEffects()[state.EffectID];

            if (isEffectUnique(effect.effectType))
            {
                GameEvents.Instance.OnTriggerVisualEffectsList(targetEntity,
                    effect.ranks[state.EffectRank].VisualEffectEntries, ActivationType.Start);
                GameEvents.Instance.OnTriggerAnimationsList(targetEntity,
                    effect.ranks[state.EffectRank].AnimationEntries, ActivationType.Start);
                GameEvents.Instance.OnTriggerSoundsList(targetEntity, effect.ranks[state.EffectRank].SoundEntries,
                    ActivationType.Start, targetEntity.transform);
            }

            var newState = GenerateNewState(effect, state.EffectRank, null, effect.entryIcon);
            newState.curStack = state.curStack;
            newState.stateCurDuration = state.stateCurDuration;

            if (isEffectCC(effect.effectType)) newState.stateMaxDuration = state.stateMaxDuration;

            if (effect.effectType == RPGEffect.EFFECT_TYPE.DamageOverTime ||
                effect.effectType == RPGEffect.EFFECT_TYPE.HealOverTime ||
                effect.effectType == RPGEffect.EFFECT_TYPE.Stat)
            {
                newState.curPulses = state.curPulses;
                newState.maxPulses = state.maxPulses;
                newState.pulseInterval = state.pulseInterval;
            }

            targetEntity.GetStates().Add(newState);

                CombatEvents.Instance.OnStateStarted(targetEntity, targetEntity.GetStates().Count - 1);

            switch (effect.effectType)
            {
                case RPGEffect.EFFECT_TYPE.Stun:
                case RPGEffect.EFFECT_TYPE.Sleep:
                    if(targetEntity.IsCasting() || targetEntity.IsChanneling()) targetEntity.InterruptCastActions();
                    if(targetEntity.IsInteractingWithObject()) targetEntity.InterruptObjectInteraction();
                    DestroyStunnedNodeCombatEntities(targetEntity);
                    break;
                case RPGEffect.EFFECT_TYPE.Shapeshifting:
                    targetEntity.InitShapeshifting(effect, state.EffectRank);
                    break;
                case RPGEffect.EFFECT_TYPE.Flying:
                    targetEntity.InitFlying();
                    break;
                case RPGEffect.EFFECT_TYPE.Stealth:
                    targetEntity.InitStealth(effect.ranks[state.EffectRank].showStealthActionBar,
                        effect.ranks[state.EffectRank].nestedEffects);
                    break;
                case RPGEffect.EFFECT_TYPE.Mount:
                    targetEntity.InitMount(effect, state.EffectRank);
                    break;
            }
        }

        public void HandleNestedEffects(CombatEntity targetInfo, List<RPGAbility.AbilityEffectsApplied> nestedEffects)
        {
            foreach (var effectApplied in from effectApplied in nestedEffects
                let chance = Random.Range(0, 100f)
                where effectApplied.chance == 0 || chance <= effectApplied.chance
                select effectApplied)
            {
                GameActionsManager.Instance.ApplyEffect(RPGCombatDATA.TARGET_TYPE.Caster, targetInfo,
                    effectApplied.effectID);
            }
        }

        public void ResetNestedEffects(CombatEntity targetInfo, RPGEffect parentEffect,
            RPGEffect.RPGEffectRankData parentEffectRank)
        {
            List<int> effectsToRemove = new List<int>();

            foreach (var nestedEffect in parentEffectRank.nestedEffects)
            {
                if (!GameDatabase.Instance.GetEffects()[nestedEffect.effectID].endless) continue;
                effectsToRemove.Add(nestedEffect.effectID);
            }

            var stateList = targetInfo.GetStates();
            for (var index = 0; index < stateList.Count; index++)
            {
                var state = stateList[index];
                if (effectsToRemove.Contains(state.stateEffectID)) targetInfo.EndStateEffect(index);
            }
        }

        public float CalculateCastTime(CombatEntity cbtNode, float baseCastTime)
        {
            float curCastMod = CombatUtilities.GetTotalOfStatType(cbtNode, RPGStat.STAT_TYPE.CAST_SPEED);

            if (curCastMod == 0) return baseCastTime;
            curCastMod /= 100;
            if (curCastMod > 0)
            {
                curCastMod = 1 - curCastMod;
                if (curCastMod < 0) curCastMod = 0;
                baseCastTime *= curCastMod;
                return baseCastTime;
            }

            curCastMod = Mathf.Abs(curCastMod);
            baseCastTime += baseCastTime * curCastMod;
            return baseCastTime;

        }

        public void InitExtraAbility(CombatEntity casterEntity, RPGAbility ability)
        {
            var abilityRank = casterEntity.GetCurrentAbilityRank(ability, false);

            if (abilityRank.targetType != RPGAbility.TARGET_TYPES.GROUND &&
                abilityRank.targetType != RPGAbility.TARGET_TYPES.GROUND_LEAP)
            {
                GameEvents.Instance.OnTriggerVisualEffectsList(casterEntity, abilityRank.VisualEffectEntries, ActivationType.Start);
                GameEvents.Instance.OnTriggerSoundsList(casterEntity, abilityRank.SoundEntries, ActivationType.Start, casterEntity.transform);
            }

            switch (abilityRank.targetType)
            {
                case RPGAbility.TARGET_TYPES.AOE:
                    EXECUTE_AOE_ABILITY(casterEntity, abilityRank);
                    break;

                case RPGAbility.TARGET_TYPES.CONE:
                    EXECUTE_CONE_ABILITY(casterEntity, abilityRank);
                    break;

                case RPGAbility.TARGET_TYPES.PROJECTILE:
                    StartCoroutine(EXECUTE_PROJECTILE_ABILITY(casterEntity, null, ability, abilityRank));
                    break;

                case RPGAbility.TARGET_TYPES.LINEAR:
                    EXECUTE_LINEAR_ABILITY(casterEntity, ability, abilityRank);
                    break;
            }
        }

        private bool checkTarget(CombatEntity casterInfo, CombatEntity targetInfo, RPGAbility.RPGAbilityRankData rankREF)
        {
            if (rankREF.targetType != RPGAbility.TARGET_TYPES.TARGET_INSTANT && rankREF.targetType != RPGAbility.TARGET_TYPES.TARGET_PROJECTILE) return true;
            if (targetInfo == null)
            {
                if (casterInfo == GameState.playerEntity)
                    UIEvents.Instance.OnShowAlertMessage("This ability requires a target", 3);
                return false;
            }

            FactionManager.CanHitResult hitResult =
                FactionManager.Instance.AttackerCanHitTarget(rankREF, casterInfo, targetInfo);
            if (!hitResult.canHit)
            {
                return false;
            }

            if (targetInfo != null)
            {
                var dist = Vector3.Distance(casterInfo.transform.position, targetInfo.transform.position);

                if (rankREF.mustLookAtTarget)
                {
                    var pointDirection = targetInfo.transform.position - casterInfo.transform.position;
                    var angle = Vector3.Angle(casterInfo.transform.forward, pointDirection);
                    if (!(angle < 70))
                    {
                        if (casterInfo == GameState.playerEntity)
                            UIEvents.Instance.OnShowAlertMessage("The target is not in line of sight", 3);
                        return false;
                    }
                }

                float totalMinRange = rankREF.minRange + (rankREF.minRange *
                                                          (CombatUtilities.GetTotalOfStatType(casterInfo,
                                                              RPGStat.STAT_TYPE.ABILITY_TARGET_MIN_RANGE) / 100));
                if (dist < totalMinRange)
                {
                    if (casterInfo == GameState.playerEntity)
                        UIEvents.Instance.OnShowAlertMessage("The target is too close", 3);
                    return false;
                }

                float totalMaxRange = rankREF.maxRange + (rankREF.maxRange *
                                                          (CombatUtilities.GetTotalOfStatType(casterInfo,
                                                              RPGStat.STAT_TYPE.ABILITY_TARGET_MAX_RANGE) / 100));

                if (dist > totalMaxRange)
                {
                    if (casterInfo == GameState.playerEntity)
                        UIEvents.Instance.OnShowAlertMessage("The target is too far", 3);
                    return false;
                }

                bool processHit = hitResult.canHit;

                if (processHit || !casterInfo.IsPlayer()) return processHit;
                UIEvents.Instance.OnShowAlertMessage(hitResult.errorMessage, 3);

                return false;
            }

            UIEvents.Instance.OnShowAlertMessage("This ability requires a target", 3);
            return false;
        }

        public bool UseRequirementsMet(CombatEntity casterInfo, CombatEntity targetInfo, RPGAbility ability, RPGAbility.RPGAbilityRankData rankREF, bool abMustBeKnown)
        {
            if (casterInfo.IsMounted() && !rankREF.canUseWhileMounted)
            {
                UIEvents.Instance.OnShowAlertMessage("This ability cannot be used while mounted", 3);
                return false;
            }
            
            if (casterInfo.IsPlayer() && !rankREF.CanUseDuringGCD && currentGCD > 0)
            {
                UIEvents.Instance.OnShowAlertMessage("Not ready to use abilities yet", 3);
                return false;
            }
            
            if (abMustBeKnown)
            {
                if (!casterInfo.IsAbilityReady(ability))
                {
                    if(casterInfo == GameState.playerEntity)UIEvents.Instance.OnShowAlertMessage("This ability is not ready yet", 3);
                    return false;
                }
            }

            if (rankREF.activationType == RPGAbility.AbilityActivationType.Casted && !rankREF.castInRun && !casterInfo.CanStartCasting())
            {
               return false;
            }
            
            bool noReq = GameModifierManager.Instance.GetGameModifierBool(
                RPGGameModifier.CategoryType.Combat + "+" +
                RPGGameModifier.CombatModuleType.Ability + "+" +
                RPGGameModifier.AbilityModifierType.No_Use_Requirement, ability.ID);

            if (noReq) return checkTarget(casterInfo, targetInfo, rankREF);

            return RequirementsManager.Instance.RequirementsMet(
                casterInfo, (rankREF.UseRequirementsTemplate && rankREF.RequirementsTemplate != null) ? rankREF.RequirementsTemplate.Requirements : rankREF.Requirements).Result
                   && checkTarget(casterInfo, targetInfo, rankREF);
        }

        public void HandleAbilityCost(CombatEntity nodeCombatInfo, RPGAbility.RPGAbilityRankData rankREF)
        {
            foreach (var group in rankREF.Requirements)
            {
                foreach (var requirement in group.Requirements)
                {
                    if (requirement.type == RequirementsData.RequirementType.StatCost)
                    {
                        int CostValue = requirement.Amount1;
                        var statREF = GameDatabase.Instance.GetStats()[requirement.StatID];
                        float useMod = 0;
                        switch (requirement.AmountType)
                        {
                            case RequirementsData.AmountType.PercentOfCurrent:
                                useMod = (float) requirement.Amount1 / 100f;
                                CostValue = (int) (CombatUtilities.GetCurrentStatValue(nodeCombatInfo, statREF.ID) *
                                                   useMod);
                                break;
                            case RequirementsData.AmountType.PercentOfMax:
                                useMod = (float) requirement.Amount1 / 100f;
                                CostValue = (int) (CombatUtilities.GetMaxStatValue(nodeCombatInfo, statREF.ID) *
                                                   useMod);
                                break;
                        }

                        if (CostValue < 1) CostValue = 1;

                        CombatUtilities.UpdateCurrentStatValue(nodeCombatInfo, requirement.StatID, -CostValue);
                    }
                }
            }
        }

        public void AbilityUsed(CombatEntity cbtNode, int abilityID)
        {
            List<RPGCombo> newCombos = new List<RPGCombo>();
            bool cancelCombos = false;
            foreach (var combo in GameDatabase.Instance.GetCombos().Values)
            {
                if (combo.initialAbilityID != abilityID) continue;
                if (combo.combos.Count == 0) continue;
                int isComboActive = RPGBuilderUtilities.IsComboActive(cbtNode, combo.ID, 0);
                if (isComboActive != -1)
                {
                    KeyCode nextKey = KeyCode.None;
                    switch (combo.combos[0].keyType)
                    {
                        case RPGCombo.KeyType.StartAbilityKey:
                            nextKey = RPGBuilderUtilities.GetAbilityKey(abilityID);
                            break;
                        case RPGCombo.KeyType.OverrideKey:
                            nextKey = combo.combos[0].overrideKey;
                            break;
                        case RPGCombo.KeyType.ActionKey:
                            nextKey = RPGBuilderUtilities.GetCurrentKeyByActionKeyName(combo.combos[0].actionKeyName);
                            break;
                    }

                    CombatEvents.Instance.OnPlayerComboUpdated(isComboActive, nextKey,
                        combo.combos[0].keyType == RPGCombo.KeyType.StartAbilityKey);
                }
                else
                {
                    if (combo.StartCancelOtherCombos) cancelCombos = true;

                    CombatData.ActiveCombo newCombo = new CombatData.ActiveCombo
                    {
                        combo = combo,
                        initialAbilityID = combo.initialAbilityID,
                        readyTime = combo.combos[0].readyTime,
                        curLoadTime = 0,
                        expireTime = combo.combos[0].expireTime
                    };
                    newCombo.curTime = newCombo.expireTime;

                    newCombos.Add(combo);

                    cbtNode.GetActiveCombos().Add(newCombo);
                    if (cbtNode != GameState.playerEntity) continue;

                    KeyCode nextKey = KeyCode.None;
                    switch (combo.combos[0].keyType)
                    {
                        case RPGCombo.KeyType.StartAbilityKey:
                            nextKey = RPGBuilderUtilities.GetAbilityKey(abilityID);
                            break;
                        case RPGCombo.KeyType.OverrideKey:
                            nextKey = combo.combos[0].overrideKey;
                            break;
                        case RPGCombo.KeyType.ActionKey:
                            nextKey = RPGBuilderUtilities.GetCurrentKeyByActionKeyName(combo.combos[0].actionKeyName);
                            break;
                    }

                    CombatEvents.Instance.OnPlayerComboStarted(cbtNode.GetActiveCombos().Count - 1, combo.combos[0],
                        nextKey);
                }
            }

            if (cancelCombos)
            {
                foreach (var previousCombo in cbtNode.GetActiveCombos())
                {
                    if (newCombos.Contains(previousCombo.combo)) continue;
                    cbtNode.StartCoroutine(cbtNode.RemoveComboEntry(previousCombo, true, true));
                }
            }

            for (var index = 0; index < cbtNode.GetActiveCombos().Count; index++)
            {
                var activeCombo = cbtNode.GetActiveCombos()[index];
                foreach (var comboEntry in activeCombo.combo.combos.Where(comboEntry => comboEntry.abilityID == abilityID))
                {
                    if ((activeCombo.combo.combos.Count - 1) <= activeCombo.comboIndex)
                    {
                        cbtNode.StartCoroutine(cbtNode.RemoveComboEntry(activeCombo, true, true));
                    }
                    else
                    {
                        activeCombo.comboIndex++;

                        KeyCode nextKey = KeyCode.None;
                        switch (activeCombo.combo.combos[activeCombo.comboIndex].keyType)
                        {
                            case RPGCombo.KeyType.StartAbilityKey:
                                nextKey = RPGBuilderUtilities.GetAbilityKey(activeCombo.initialAbilityID);
                                break;
                            case RPGCombo.KeyType.OverrideKey:
                                nextKey = activeCombo.combo.combos[activeCombo.comboIndex].overrideKey;
                                break;
                            case RPGCombo.KeyType.ActionKey:
                                nextKey = RPGBuilderUtilities.GetCurrentKeyByActionKeyName(activeCombo.combo
                                    .combos[activeCombo.comboIndex].actionKeyName);
                                break;
                        }

                        CombatEvents.Instance.OnPlayerComboUpdated(index, nextKey,
                            activeCombo.combo.combos[activeCombo.comboIndex].keyType ==
                            RPGCombo.KeyType.StartAbilityKey);
                    }
                }
            }
        }

        public void CancelOtherComboOptions(CombatEntity cbtNode, RPGCombo combo)
        {
            foreach (var activeCombos in GameState.playerEntity.GetActiveCombos().Where(activeCombos => activeCombos.combo != combo))
            {
                cbtNode.StartCoroutine(cbtNode.RemoveComboEntry(activeCombos, false, false));
            }
        }

        public void HandleAbilityTypeActions(CombatEntity casterEntity, CombatEntity targetInfo, RPGAbility ability, RPGAbility.RPGAbilityRankData abilityRank, bool OnStart)
        {
            GameEvents.Instance.OnTriggerAnimationsList(casterEntity, abilityRank.AnimationEntries, OnStart ? ActivationType.Start : ActivationType.Completed);
            GameEvents.Instance.OnTriggerVisualEffectsList(casterEntity, abilityRank.VisualEffectEntries, OnStart ? ActivationType.Start : ActivationType.Completed);
            GameEvents.Instance.OnTriggerSoundsList(casterEntity, abilityRank.SoundEntries, OnStart ? ActivationType.Start : ActivationType.Completed, casterEntity.transform);
            
            switch (abilityRank.targetType)
            {
                case RPGAbility.TARGET_TYPES.SELF:
                    EXECUTE_SELF_ABILITY(casterEntity, ability, abilityRank);
                    break;

                case RPGAbility.TARGET_TYPES.AOE:
                    EXECUTE_AOE_ABILITY(casterEntity, abilityRank);
                    break;

                case RPGAbility.TARGET_TYPES.CONE:
                    EXECUTE_CONE_ABILITY(casterEntity, abilityRank);
                    break;

                case RPGAbility.TARGET_TYPES.PROJECTILE:
                case RPGAbility.TARGET_TYPES.TARGET_PROJECTILE:
                    StartCoroutine(EXECUTE_PROJECTILE_ABILITY(casterEntity, targetInfo, ability, abilityRank));
                    break;

                case RPGAbility.TARGET_TYPES.LINEAR:
                    EXECUTE_LINEAR_ABILITY(casterEntity, ability, abilityRank);
                    break;

                case RPGAbility.TARGET_TYPES.GROUND:
                    INIT_GROUND_ABILITY(ability, abilityRank);
                    break;

                case RPGAbility.TARGET_TYPES.GROUND_LEAP:
                    if (casterEntity != GameState.playerEntity) return;
                    INIT_GROUND_ABILITY(ability, abilityRank);
                    break;

                case RPGAbility.TARGET_TYPES.TARGET_INSTANT:
                    if (targetInfo != null && !targetInfo.IsDead())
                    {
                        EXECUTE_TARGET_INSTANT_ABILITY(casterEntity, targetInfo, abilityRank);
                    }

                    break;
            }
        }

        private float getAutoAttackCD()
        {
            float cd = GameState.playerEntity.autoAttackData.WeaponItem != null ?
                GameState.playerEntity.autoAttackData.WeaponItem.AttackSpeed : GameDatabase.Instance.GetAbilities()[GameState.playerEntity.autoAttackData.CurrentAutoAttackAbilityID].ranks[0].cooldown;

            float curAttackSpeed = 0;
            foreach (var t in GameState.playerEntity.GetStats())
            {
                foreach (var t1 in t.Value.stat.statBonuses)
                {
                    switch (t1.statType)
                    {
                        case RPGStat.STAT_TYPE.ATTACK_SPEED:
                            curAttackSpeed += t.Value.currentValue;
                            break;
                    }
                }
            }

            if (curAttackSpeed != 0)
            {
                cd = cd - cd * (curAttackSpeed / 100);
            }
            return cd;
        }

        private bool CombatNodeCanInitAbility(CombatEntity entity, RPGAbility thisAbility, bool abMustBeKnown)
        {
            if (!GameState.combatEnabled) return false;
            if (entity.IsDead()) return false;
            if (entity.IsCasting()) return false;
            if (entity.IsChanneling()) return false;
            if (entity.IsLeaping()) return false;
            if (entity.IsStunned()) return false;
            if (entity.IsInMotion()) return false;
            if (entity.IsSleeping()) return false;
            if (entity.IsSilenced()) return false;
            if (!entity.IsPlayer()) return true;
            if(entity.IsPlayer() && GameState.playerEntity.GroundCasting) return false;
            if((abMustBeKnown && !GameState.playerEntity.IsShapeshifted()) && !CombatUtilities.IsAbilityKnown(thisAbility.ID)) return false;

            return true;
        }
        public bool CombatNodeCanInitMotion(CombatEntity cbtNode)
        {
            return !cbtNode.IsDead() && !cbtNode.IsCasting() && !cbtNode.IsChanneling() && !cbtNode.IsLeaping();
        }

        public void AbilityKeyUp(RPGAbility thisAbility, bool abMustBeKnown)
        {
            int curRank = RPGBuilderUtilities.GetCharacterAbilityRank(thisAbility.ID);
            if (!abMustBeKnown) curRank = 0;
            var rankREF = thisAbility.ranks[curRank];

            foreach (var effectApplied in rankREF.effectsApplied)
            {
                RPGEffect effect = GameDatabase.Instance.GetEffects()[effectApplied.effectID];
                RPGEffect.RPGEffectRankData effectRankREF = GameDatabase.Instance.GetEffects()[effectApplied.effectID]
                    .ranks[effectApplied.effectRank];
                switch (effect.effectType)
                {
                   case RPGEffect.EFFECT_TYPE.Blocking:
                       if (GameState.playerEntity.IsActiveBlocking() && GameState.playerEntity.ActiveBlockingState.effectRank == effectRankREF && effectRankREF.blockDurationType == RPGEffect.BLOCK_DURATION_TYPE.HoldKey)
                       {
                           GameState.playerEntity.ResetActiveBlocking();
                       }
                       break;
                }
            }
        }

        private bool HandleInitAbilityStealthActions(CombatEntity casterNode, RPGAbility.RPGAbilityRankData rankREF)
        {
            int tagEffectID = RPGBuilderUtilities.getStealthTagEffectID(rankREF);
            int currentActiveStealthEffectID = RPGBuilderUtilities.getActiveStealthEffectID(casterNode);
            if (tagEffectID == currentActiveStealthEffectID)
            {
                casterNode.ResetStealth();
                return true;
            }

            if (!rankREF.cancelStealth) return false;
            casterNode.ResetStealth();
            return false;

        }

        private bool HandleInitAbilityShapeshiftingActions(CombatEntity casterNode, RPGAbility.RPGAbilityRankData rankREF)
        {
            int tagEffectID = RPGBuilderUtilities.getShapeshiftingTagEffectID(rankREF);
            int currentActiveShapeshiftingEffectID =
                RPGBuilderUtilities.GETActiveShapeshiftingEffectID(casterNode);
            if (tagEffectID != currentActiveShapeshiftingEffectID) return false;
            casterNode.ResetShapeshifting();
            return true;
        }

        private void HandleInitAbilityControllerActions(RPGAbility.RPGAbilityRankData rankREF)
        {
            GameState.playerEntity.controllerEssentials.AbilityInitActions(rankREF);
        }

        private void HandleInitAbilityActions(CombatEntity casterEntity, RPGAbility.RPGAbilityRankData abilityRank)
        {
            if (abilityRank.targetType == RPGAbility.TARGET_TYPES.GROUND || abilityRank.targetType == RPGAbility.TARGET_TYPES.GROUND_LEAP) return;

            ExecuteCasterEffects(casterEntity, abilityRank);

            if (abilityRank.standTimeDuration > 0)
            {
                casterEntity.InitStandTime(abilityRank);
            }

            if (abilityRank.castSpeedSlowAmount > 0)
            {
                casterEntity.InitCastSlow(abilityRank);
            }
            
        }

        private void HandleInitAbilityToggleActions(CombatEntity casterNode, RPGAbility.RPGAbilityRankData rankREF, RPGAbility ability)
        {
            if (RPGBuilderUtilities.isAbilityToggled(casterNode, ability))
            {
                casterNode.RemoveToggledAbility(ability);
            }
            else
            {
                CombatData.ActiveToggledAbilities newToggledAbility = new CombatData.ActiveToggledAbilities {ability = ability, rank = rankREF, nextTrigger = 0};
                GameState.playerEntity.GetActiveToggledAbilities().Add(newToggledAbility);

                if (!rankREF.isToggleCostOnTrigger)
                {
                    HandleAbilityCost(casterNode, rankREF);
                }

                ActionBarManager.Instance.UpdateToggledAbilities();
            }
        }

        public void HandleInitCooldown(CombatEntity casterNode, RPGAbility.RPGAbilityRankData rank, RPGAbility ability)
        {
            if (rank.targetType == RPGAbility.TARGET_TYPES.GROUND || rank.targetType == RPGAbility.TARGET_TYPES.GROUND_LEAP) return;
            switch (ability.abilityType)
            {
                case RPGAbility.AbilityType.Normal:
                    casterNode.StartAbilityCooldown(rank, ability.ID);
                    break;
                case RPGAbility.AbilityType.PlayerAutoAttack:
                    GameState.playerEntity.InitAACooldown(getAutoAttackCD());
                    break;
                case RPGAbility.AbilityType.PlayerActionAbility:
                    GameState.playerEntity.InitActionAbilityCooldown(ability.ID,
                        rank.cooldown);
                    break;
            }

            if (rank.isGCD)
            {
                StartGCD();
            }
        }

        public void InitAbility(CombatEntity casterEntity, RPGAbility thisAbility, RPGAbility.RPGAbilityRankData rank, bool abilityKnown)
        {
            if (!CombatNodeCanInitAbility(casterEntity, thisAbility, abilityKnown))
            {
                casterEntity.EndAbility(0);
                return;
            }
            
            CombatEntity target = casterEntity.IsPlayer() ? casterEntity.GetTarget() :
                casterEntity.GetAIEntity().AlliedEntityTarget != null ? casterEntity.GetAIEntity().AlliedEntityTarget :
                casterEntity.GetTarget();
            if (!UseRequirementsMet(casterEntity, target, thisAbility, rank, abilityKnown))
            {
                casterEntity.EndAbility(0);
                return;
            }
            
            if(casterEntity.IsStealth() && HandleInitAbilityStealthActions(casterEntity, rank)) return;
            if (casterEntity.IsShapeshifted() && HandleInitAbilityShapeshiftingActions(casterEntity, rank)) return;
            if (casterEntity.IsPlayer())  HandleInitAbilityControllerActions(rank);
            
            HandleInitAbilityActions(casterEntity, rank);
            casterEntity.InitAbility(thisAbility, rank);

            
            switch (rank.activationType)
            {
                case RPGAbility.AbilityActivationType.Instant:
                case RPGAbility.AbilityActivationType.Channeled:
                    if (rank.isToggle)
                    {
                        HandleInitAbilityToggleActions(casterEntity, rank, thisAbility);
                    }
                    else
                    {
                        HandleAbilityTypeActions(casterEntity, target, thisAbility, rank, true);
                        if(!casterEntity.IsPlayer()) casterEntity.GetAIEntity().AlliedEntityTarget = null;
                        
                        HandleInitCooldown(casterEntity, rank, thisAbility);

                        if (rank.activationType == RPGAbility.AbilityActivationType.Channeled)
                        {
                            casterEntity.InitChanneling(thisAbility, rank);
                        }

                        HandleAbilityCost(casterEntity, rank);
                        casterEntity.EndAbility(rank.AIAttackTime);
                    }

                    AbilityUsed(casterEntity, thisAbility.ID);
                    break;
                case RPGAbility.AbilityActivationType.Casted:
                    if (rank.targetType == RPGAbility.TARGET_TYPES.TARGET_INSTANT || rank.targetType == RPGAbility.TARGET_TYPES.TARGET_PROJECTILE)
                    {
                        if (target.IsDead())
                        {
                            if(casterEntity.IsPlayer()) UIEvents.Instance.OnShowAlertMessage("The target is dead", 3); 
                            casterEntity.GetAIEntity().AlliedEntityTarget = null;
                            return;
                        }
                        casterEntity.SetCurrentTargetCasted(target);
                        if(!casterEntity.IsPlayer()) casterEntity.GetAIEntity().AlliedEntityTarget = null;
                    }

                    casterEntity.InitCasting(thisAbility, rank);
                    GameEvents.Instance.OnTriggerAnimationsList(casterEntity, rank.AnimationEntries, ActivationType.Start);
                    GameEvents.Instance.OnTriggerVisualEffectsList(casterEntity, rank.VisualEffectEntries, ActivationType.Start);
                    GameEvents.Instance.OnTriggerSoundsList(casterEntity, rank.SoundEntries, ActivationType.Start, casterEntity.transform);

                    if (!rank.comboStarsAfterCastComplete) AbilityUsed(casterEntity, thisAbility.ID);
                    break;
                case RPGAbility.AbilityActivationType.Charged:
                    break;
            }
        }

        public bool actionAbIsReady(RPGAbility ab)
        {
            foreach (var t in Character.Instance.CharacterData.ActionAbilities)
            {
                if (t.ability == ab) return Time.time >= t.NextTimeUse;
            }
            return false;
        }

        private void FixedUpdate()
        {
            if (currentGCD > 0)
            {
                currentGCD -= Time.deltaTime;
            }
            else
            {
                currentGCD = 0;
            }
        }

        public void StartGCD()
        {
            var finalCD = GameDatabase.Instance.GetCombatSettings().GlobalCooldownDuration;
            float cdrecoveryspeed = CombatUtilities.GetTotalOfStatType(GameState.playerEntity, RPGStat.STAT_TYPE.GCD_DURATION);

            if (cdrecoveryspeed != 0)
            {
                cdrecoveryspeed /= 100;
                finalCD -= finalCD * cdrecoveryspeed;
            }
            currentGCD = finalCD;
        }
        

        private void EXECUTE_LINEAR_ABILITY(CombatEntity nodeCombatInfo, RPGAbility ability, RPGAbility.RPGAbilityRankData rankREF)
        {
            if (rankREF.ConeHitCount == 1)
            {
                List<CombatEntity> allCbtNodes = getPotentialCombatNodes(Physics.OverlapSphere(nodeCombatInfo.transform.position, rankREF.linearLength), nodeCombatInfo, rankREF);
                List<CombatEntity> cbtNodeInArea = new List<CombatEntity>();

                foreach (var t in allCbtNodes)
                {
                    double Fi = nodeCombatInfo.transform.rotation.eulerAngles.y * Mathf.Deg2Rad;
                    double cs = Mathf.Cos((float) Fi);
                    double sn = Mathf.Sin((float) Fi);

                    var tx = t.gameObject.transform.position.x - nodeCombatInfo.transform.position.x;
                    var tz = t.gameObject.transform.position.z - nodeCombatInfo.transform.position.z;

                    var ptx = (float) (cs * tx - sn * tz);
                    var ptz = (float) (sn * tx + cs * tz);

                    if (!(-(rankREF.linearWidth / 2) <= ptx) || !(ptx <= rankREF.linearWidth / 2)) continue;
                    if (ptz >= 0 && ptz <= rankREF.linearLength) cbtNodeInArea.Add(t);
                }

                int totalUnitHit = rankREF.MaxUnitHit +
                                   (int)CombatUtilities.GetTotalOfStatType(nodeCombatInfo, RPGStat.STAT_TYPE.ABILITY_MAX_HIT);
                var closestUnits = getClosestUnits(nodeCombatInfo, cbtNodeInArea, totalUnitHit);
                foreach (var t in closestUnits)
                    ExecuteAbilityEffects(nodeCombatInfo, t, rankREF);
            }
            else
            {
                StartCoroutine(EXECUTE_LINEAR_ABILITY_PULSE(nodeCombatInfo, ability));
            }
        }

        private IEnumerator EXECUTE_LINEAR_ABILITY_PULSE(CombatEntity nodeCombatInfo, RPGAbility ability)
        {
            var rank = nodeCombatInfo.GetCurrentAbilityRank(ability, true);

            for (var x = 0; x < rank.ConeHitCount; x++)
            {
                if(nodeCombatInfo.IsDead()) yield break;
                List<CombatEntity> allCbtNodes = getPotentialCombatNodes(Physics.OverlapSphere(nodeCombatInfo.transform.position, rank.linearLength), nodeCombatInfo, rank);
                List<CombatEntity> cbtNodeInArea = new List<CombatEntity>();

                double Fi, cs, sn;

                foreach (var t in allCbtNodes)
                {
                    Fi = nodeCombatInfo.transform.rotation.eulerAngles.y * Mathf.Deg2Rad;
                    cs = Mathf.Cos((float) Fi);
                    sn = Mathf.Sin((float) Fi);

                    var tx = t.gameObject.transform.position.x - nodeCombatInfo.transform.position.x;
                    var tz = t.gameObject.transform.position.z - nodeCombatInfo.transform.position.z;

                    var ptx = (float) (cs * tx - sn * tz);
                    var ptz = (float) (sn * tx + cs * tz);

                    if (!(-(rank.linearWidth / 2) <= ptx) || !(ptx <= rank.linearWidth / 2)) continue;
                    if (ptz >= 0 && ptz <= rank.linearLength) cbtNodeInArea.Add(t);
                }

                int totalUnitHit = rank.MaxUnitHit +
                                   (int)CombatUtilities.GetTotalOfStatType(nodeCombatInfo, RPGStat.STAT_TYPE.ABILITY_MAX_HIT);
                var closestUnits = getClosestUnits(nodeCombatInfo, cbtNodeInArea, totalUnitHit);
                foreach (var t in closestUnits)
                    ExecuteAbilityEffects(nodeCombatInfo, t, rank);

                yield return new WaitForSeconds(rank.ConeHitInterval);
            }
        }

        private List<CombatEntity> RemoveAlreadyHitTargetsFromArray(ProjectileHitDetection projREF, List<CombatEntity> allCbtNodes)
        {
            foreach (var t in projREF.alreadyHitNodes)
            {
                if (allCbtNodes.Contains(t)) allCbtNodes.Remove(t);
            }

            return allCbtNodes;
        }

        public void FIND_NEARBY_UNITS(CombatEntity nodeCombatInfo, GameObject projectileGO, RPGAbility ability,
            ProjectileHitDetection projREF)
        {
            var rank = nodeCombatInfo.GetCurrentAbilityRank(ability, true);

            List<CombatEntity> allCbtNodes = getPotentialCombatNodes(Physics.OverlapSphere(projectileGO.transform.position, rank.projectileNearbyUnitDistanceMax), nodeCombatInfo, rank);

            var hitNodeList = RemoveAlreadyHitTargetsFromArray(projREF, allCbtNodes);
            if (hitNodeList.Count == 0)
            {
                Destroy(projectileGO);
            }
            else
            {
                int totalUnitHit = rank.MaxUnitHit + (int) CombatUtilities.GetTotalOfStatType(nodeCombatInfo, RPGStat.STAT_TYPE.ABILITY_MAX_HIT);
                var closestUnits = getClosestNearbyUnits(projREF.gameObject, hitNodeList, totalUnitHit);
                if (closestUnits.Count > 0)
                    projREF.curNearbyTargetGO = closestUnits[0].gameObject;
                else
                    Destroy(projectileGO);
            }
        }

        private void EXECUTE_SELF_ABILITY(CombatEntity casterInfo, RPGAbility ability, RPGAbility.RPGAbilityRankData rankREF)
        {
            ExecuteAbilityEffects(casterInfo, casterInfo, rankREF);
        }

        private void EXECUTE_AOE_ABILITY(CombatEntity nodeCombatInfo, RPGAbility.RPGAbilityRankData rank)
        {
            if (rank.AOEHitCount == 1)
            {
                float totalRadius = rank.AOERadius +
                                    (rank.AOERadius * (CombatUtilities.GetTotalOfStatType(nodeCombatInfo, RPGStat.STAT_TYPE.AOE_RADIUS)/100));
                
                List<CombatEntity> allCbtNodes = getPotentialCombatNodes(Physics.OverlapSphere(nodeCombatInfo.transform.position, totalRadius), nodeCombatInfo, rank);
                int totalUnitHit = rank.MaxUnitHit +
                                   (int)CombatUtilities.GetTotalOfStatType(nodeCombatInfo, RPGStat.STAT_TYPE.ABILITY_MAX_HIT);
                var closestUnits = getClosestUnits(nodeCombatInfo, allCbtNodes, totalUnitHit);
                foreach (var t in closestUnits)
                    ExecuteAbilityEffects(nodeCombatInfo, t, rank);
            }
            else
            {
                StartCoroutine(EXECUTE_AOE_ABILITY_PULSE(nodeCombatInfo, rank));
            }
        }

        private IEnumerator EXECUTE_AOE_ABILITY_PULSE(CombatEntity nodeCombatInfo, RPGAbility.RPGAbilityRankData rankREF)
        {
            if (nodeCombatInfo == null) yield break;

            for (var x = 0; x < rankREF.AOEHitCount; x++)
            {
                if(nodeCombatInfo.IsDead()) yield break;
                if (nodeCombatInfo == null) yield break;
                if (!rankREF.canBeUsedStunned && nodeCombatInfo.IsStunned()) yield break;
                
                float totalRadius = rankREF.AOERadius +
                                    (rankREF.AOERadius * (CombatUtilities.GetTotalOfStatType(nodeCombatInfo, RPGStat.STAT_TYPE.AOE_RADIUS)/100));
                List<CombatEntity> allCbtNodes = getPotentialCombatNodes(Physics.OverlapSphere(nodeCombatInfo.transform.position, totalRadius), nodeCombatInfo, rankREF);
                
                int totalUnitHit = rankREF.MaxUnitHit +
                                   (int)CombatUtilities.GetTotalOfStatType(nodeCombatInfo, RPGStat.STAT_TYPE.ABILITY_MAX_HIT);
                var closestUnits = getClosestUnits(nodeCombatInfo, allCbtNodes, totalUnitHit);
                foreach (var t in closestUnits)
                    ExecuteAbilityEffects(nodeCombatInfo, t, rankREF);

                yield return new WaitForSeconds(rankREF.AOEHitInterval);
            }
        }

        private void EXECUTE_CONE_ABILITY(CombatEntity nodeCombatInfo, RPGAbility.RPGAbilityRankData rankREF)
        {
            if (rankREF.ConeHitCount == 1)
            {
                List<CombatEntity> allCbtNodes =
                    getPotentialCombatNodes(Physics.OverlapSphere(nodeCombatInfo.transform.position, rankREF.minRange), nodeCombatInfo, rankREF);

                var unitInCone = new List<CombatEntity>();
                foreach (var t in allCbtNodes)
                {
                    var pointDirection = t.transform.position - nodeCombatInfo.transform.position;
                    var angle = Vector3.Angle(nodeCombatInfo.transform.forward, pointDirection);
                    if (angle < rankREF.coneDegree)
                        unitInCone.Add(t);
                }

                int totalUnitHit = rankREF.MaxUnitHit +
                                   (int) CombatUtilities.GetTotalOfStatType(nodeCombatInfo, RPGStat.STAT_TYPE.ABILITY_MAX_HIT);
                var closestUnits = getClosestUnits(nodeCombatInfo, unitInCone, totalUnitHit);
                foreach (var t in closestUnits)
                    ExecuteAbilityEffects(nodeCombatInfo, t, rankREF);
            }
            else
            {
                StartCoroutine(EXECUTE_CONE_ABILITY_PULSE(nodeCombatInfo, rankREF));
            }
        }

        private IEnumerator EXECUTE_CONE_ABILITY_PULSE(CombatEntity nodeCombatInfo, RPGAbility.RPGAbilityRankData rankREF)
        {
            for (var x = 0; x < rankREF.ConeHitCount; x++)
            {
                if(nodeCombatInfo.IsDead()) yield break;
                List<CombatEntity> allCbtNodes = getPotentialCombatNodes(Physics.OverlapSphere(nodeCombatInfo.transform.position, rankREF.minRange), nodeCombatInfo, rankREF);
                var unitInCone = new List<CombatEntity>();
                foreach (var t in allCbtNodes)
                {
                    var pointDirection = t.transform.position - nodeCombatInfo.transform.position;
                    var angle = Vector3.Angle(nodeCombatInfo.transform.forward, pointDirection);
                    if (angle < rankREF.coneDegree)
                        unitInCone.Add(t);
                }

                int totalUnitHit = rankREF.MaxUnitHit +
                                   (int)CombatUtilities.GetTotalOfStatType(nodeCombatInfo, RPGStat.STAT_TYPE.ABILITY_MAX_HIT);
                var closestUnits = getClosestUnits(nodeCombatInfo, unitInCone, totalUnitHit);
                foreach (var t in closestUnits)
                    ExecuteAbilityEffects(nodeCombatInfo, t, rankREF);

                yield return new WaitForSeconds(rankREF.ConeHitInterval);
            }
        }

        private void EXECUTE_TARGET_INSTANT_ABILITY(CombatEntity casterInfo, CombatEntity targetInfo, RPGAbility.RPGAbilityRankData rankREF)
        {
            if (rankREF.hitEffect != null)
            {
                SpawnHitEffect(targetInfo, rankREF);
            }
            ExecuteAbilityEffects(casterInfo, targetInfo, rankREF);
        }

        private void INIT_GROUND_ABILITY(RPGAbility ability, RPGAbility.RPGAbilityRankData rankREF)
        {
            GameState.playerEntity.InitGroundAbility(ability, rankREF);
        }

        public void EXECUTE_GROUND_ABILITY(CombatEntity casterEntity, RPGAbility ability, RPGCombatDATA.CombatVisualActivationType activationType, RPGAbility.RPGAbilityRankData abilityRank)
        {

            if (abilityRank.targetType == RPGAbility.TARGET_TYPES.GROUND_LEAP)
                EXECUTE_GROUND_LEAP_MOVEMENT(casterEntity, ability, GameState.playerEntity.indicatorManagerRef.GetIndicatorPosition());

            if (abilityRank.groundVisualEffect != null)
            {
                Vector3 spawnPos = GameState.playerEntity.indicatorManagerRef.GetIndicatorPosition() + abilityRank.effectPositionOffset;
                
                var cbtVisualGO = Instantiate(abilityRank.groundVisualEffect, spawnPos, abilityRank.groundVisualEffect.transform.rotation);
                var groundHitRef = cbtVisualGO.AddComponent<GroundHitDetection>();
                groundHitRef.InitGroundAbility(casterEntity, abilityRank.groundVisualEffectDuration, ability);
            }
            GameState.playerEntity.indicatorManagerRef.HideIndicator();
            
            GameEvents.Instance.OnTriggerVisualEffectsList(casterEntity, abilityRank.VisualEffectEntries, ActivationType.Start);
            GameEvents.Instance.OnTriggerAnimationsList(casterEntity, abilityRank.AnimationEntries, ActivationType.Start);
            GameEvents.Instance.OnTriggerSoundsList(casterEntity, abilityRank.SoundEntries, ActivationType.Start, casterEntity.transform);
            
            casterEntity.StartAbilityCooldown(abilityRank, ability.ID);

            if (abilityRank.isGCD && casterEntity == GameState.playerEntity)
            {
                StartGCD();
            }
            
            if (abilityRank.standTimeDuration > 0)
                GameState.playerEntity.controllerEssentials.InitStandTime(abilityRank.standTimeDuration);

            if (abilityRank.castSpeedSlowAmount > 0)
                GameState.playerEntity.controllerEssentials.InitCastMoveSlow(abilityRank.castSpeedSlowAmount,
                    abilityRank.castSpeedSlowTime, abilityRank.castSpeedSlowRate);
        }

        private void EXECUTE_GROUND_LEAP_MOVEMENT(CombatEntity nodeCombatInfo, RPGAbility ability, Vector3 LeapEndPOS)
        {
            var rank = nodeCombatInfo.GetCurrentAbilityRank(ability, true);

            GameState.playerEntity.controllerEssentials.InitGroundLeap();
            nodeCombatInfo.InitLeaping(nodeCombatInfo.transform.position, LeapEndPOS, rank.groundLeapHeight,
                rank.groundLeapSpeed);
        }

        public void EXECUTE_GROUND_ABILITY_HIT(CombatEntity casterInfo, CombatEntity targetInfo, RPGAbility ability, RPGAbility.RPGAbilityRankData rankREF)
        {
            ExecuteAbilityEffects(casterInfo, targetInfo, rankREF);
        }

        private ProjectileHitDetection InitProjectileComponents(GameObject projGO, RPGAbility.RPGAbilityRankData rankREF)
        {
            // INIT PROJECTILE
            var projHitRef = projGO.AddComponent<ProjectileHitDetection>();

            Rigidbody rb = projGO.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = projGO.AddComponent<Rigidbody>();
            }
            
            projHitRef.RB = rb;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            switch (rankREF.projectileColliderType)
            {
                case RPGNpc.NPCColliderType.Capsule:
                    CapsuleCollider capsule = projGO.AddComponent<CapsuleCollider>();
                    capsule.center = rankREF.colliderCenter;
                    capsule.radius = rankREF.colliderRadius;
                    capsule.height = rankREF.colliderHeight;
                    capsule.isTrigger = true;
                    break;
                case RPGNpc.NPCColliderType.Sphere:
                    SphereCollider sphere = projGO.AddComponent<SphereCollider>();
                    sphere.center = rankREF.colliderCenter;
                    sphere.radius = rankREF.colliderRadius;
                    sphere.isTrigger = true;
                    break;
                case RPGNpc.NPCColliderType.Box:
                    BoxCollider box = projGO.AddComponent<BoxCollider>();
                    box.center = rankREF.colliderCenter;
                    box.size = rankREF.colliderSize;
                    box.isTrigger = true;
                    break;
            }

            return projHitRef;
        }

        public void SpawnHitEffect(CombatEntity cbtNode, RPGAbility.RPGAbilityRankData rankREF)
        {
            Vector3 spawnPos = GetEffectSpawnPosition(cbtNode, rankREF.hitEffectUseSocket, rankREF.hitEffectPositionOffset, rankREF.hitEffectNodeSocket);
            var hitEffectGO = Instantiate(rankREF.hitEffect, spawnPos, Quaternion.identity);
            if (rankREF.hitAttachedToNode)
            {
                hitEffectGO.transform.SetParent(cbtNode.transform);
            }
            Destroy(hitEffectGO, rankREF.hitEffectDuration);
        }

        public Vector3 GetEffectSpawnPosition(CombatEntity casterNode, bool useSocket, Vector3 PositionOffset, RPGBNodeSocket socketName)
        {
            if (!useSocket) return casterNode.transform.position + PositionOffset;
            if (casterNode.IsShapeshifted())
            {
                if (casterNode.ShapeshiftingNodeSockets != null)
                {
                    Transform shapeshiftSocketREF =
                        casterNode.ShapeshiftingNodeSockets.GetSocketTransform(socketName);
                    return shapeshiftSocketREF != null
                        ? shapeshiftSocketREF.position
                        : casterNode.transform.position + PositionOffset;
                }

                return casterNode.transform.position + PositionOffset;
            }

            if (casterNode.NodeSockets == null)
                return casterNode.transform.position + PositionOffset;
            Transform socketREF =
                casterNode.NodeSockets.GetSocketTransform(socketName);
            return socketREF != null
                ? socketREF.position
                : casterNode.transform.position + PositionOffset;
        }

        private Transform GetEffectTransform(CombatEntity entity, bool useSocket, RPGBNodeSocket socketName)
        {
            if (!useSocket) return entity.transform;
            
            if (entity == GameState.playerEntity && entity.IsShapeshifted())
            {
                if (entity.ShapeshiftingNodeSockets != null)
                {
                    Transform shapeshiftSocketREF =
                        entity.ShapeshiftingNodeSockets.GetSocketTransform(socketName);
                    return shapeshiftSocketREF != null
                        ? shapeshiftSocketREF
                        : entity.transform;
                }

                return entity.transform;
            }

            if (entity.NodeSockets == null)
            {
                return entity.transform;
            }
            Transform socketREF = entity.NodeSockets.GetSocketTransform(socketName);
            return socketREF != null
                ? socketREF
                : entity.transform;
        }

        private IEnumerator EXECUTE_PROJECTILE_ABILITY(CombatEntity casterNode, CombatEntity targetInfo, RPGAbility ab, RPGAbility.RPGAbilityRankData rank)
        {
            if (rank.projectileEffect == null) yield break;
            float totalCount = rank.projectileCount + CombatUtilities.GetTotalOfStatType(casterNode, RPGStat.STAT_TYPE.PROJECTILE_COUNT);

            float yOffset = 0f;
            var intervalOffset = 0f;
            if (rank.projectileAngleSpread > 0)
            {
                float totalSpread = rank.projectileAngleSpread + (rank.projectileAngleSpread *
                                                                  (CombatUtilities.GetTotalOfStatType(casterNode,
                                                                      RPGStat.STAT_TYPE
                                                                          .PROJECTILE_ANGLE_SPREAD) / 100));

                yOffset = -(totalSpread / 2);
                var projCountOnEachSide = (totalCount - 1) / 2;
                intervalOffset = totalSpread / 2 / projCountOnEachSide;

            }

            for (var i = 0; i < totalCount; i++)
            {
                if(i == 0) yield return new WaitForSeconds(rank.firstProjectileDelay);
                Vector3 projectileSpawnPosition = GetEffectSpawnPosition(casterNode, rank.projectileUseNodeSocket,
                    rank.effectPositionOffset, rank.projectileNodeSocket);
                var projectileGameObject =
                    Instantiate(rank.projectileEffect, projectileSpawnPosition, Quaternion.identity);
                ProjectileHitDetection projectileLogic = InitProjectileComponents(projectileGameObject, rank);
                projectileLogic.InitProjectile(casterNode, ab, rank);

                if (rank.projectileParentedToCaster)
                {
                    projectileGameObject.transform.SetParent(casterNode.transform);
                    projectileGameObject.transform.localPosition = Vector3.zero;
                }

                casterNode.SetProjectileRotation(projectileGameObject, rank, yOffset);

                switch (rank.targetType)
                {
                    case RPGAbility.TARGET_TYPES.TARGET_PROJECTILE:
                        projectileLogic.RB.useGravity = rank.projectileAffectedByGravity;
                        if (!rank.projectileAffectedByGravity)
                        {
                            casterNode.SetProjectileGravity(projectileLogic.RB);
                        }
                        projectileLogic.targettedProjectileTarget = targetInfo;
                        projectileLogic.targettedProjectileTargetTransform = GetEffectTransform(targetInfo,
                            rank.projectileTargetUseNodeSocket, rank.projectileTargetNodeSocket);
                        break;
                    case RPGAbility.TARGET_TYPES.PROJECTILE:
                    {
                        if (!rank.useCustomCollision)
                        {
                            projectileLogic.RB.useGravity = rank.projectileAffectedByGravity;
                            if (!rank.projectileAffectedByGravity)
                            {
                                casterNode.SetProjectileGravity(projectileLogic.RB);
                            }

                            float totalProjectileSpeed = rank.projectileSpeed + (rank.projectileSpeed *
                                (CombatUtilities.GetTotalOfStatType(casterNode, RPGStat.STAT_TYPE.PROJECTILE_SPEED) /
                                 100));
                            projectileLogic.RB.AddRelativeForce(projectileLogic.transform.forward * (totalProjectileSpeed * 50));
                        }

                        break;
                    }
                }

                if (rank.projectileSoundTemplate != null)
                {
                    GameEvents.Instance.OnTriggerSound(casterNode, rank.projectileSoundTemplate, projectileGameObject.transform);
                }

                if (rank.projectileAngleSpread > 0) yOffset += intervalOffset;
                yield return new WaitForSeconds(rank.projectileDelay);
            }
        }

        public void EXECUTE_PROJECTILE_ABILITY_HIT(CombatEntity casterInfo, CombatEntity targetInfo,
            RPGAbility.RPGAbilityRankData rankREF)
        {
            ExecuteAbilityEffects(casterInfo, targetInfo, rankREF);
        }

        public void ExecuteDOTPulse(CombatEntity casterInfo, CombatEntity targetEntity, RPGEffect effect,
            RPGEffect.RPGEffectRankData effectRank, int curStack, RPGAbility.RPGAbilityRankData abilityRank)
        {
            if (targetEntity.IsDead() || targetEntity.IsImmune()) return;

            CombatCalculations.DamageResult result = CombatCalculations.DamageCalculation(casterInfo, targetEntity, abilityRank, effect, effectRank, true);
            result.DamageAmount /= effect.pulses;
            result.DamageAmount *= curStack;

            targetEntity.TakeDamage(result, abilityRank, effectRank.alteredStatID);
            CombatEvents.Instance.OnDamageDealt(result);

            if (targetEntity.IsStealth() && effectRank.removeStealth) targetEntity.ResetStealth();

            if (effectRank.alteredStatID == GameDatabase.Instance.GetCombatSettings().HealthStatID)
            {
                HandleLifesteal(casterInfo, targetEntity, effect, effectRank, (int) result.DamageAmount);
                if (result.DamageAmount > 0) HandleThorn(casterInfo, targetEntity, result);
            }

            HandlePetDefendOwner(targetEntity, casterInfo);

            if (!targetEntity.IsPlayer()) HandleMobAggro(casterInfo, targetEntity);

            GameEvents.Instance.OnTriggerVisualEffectsList(targetEntity, effectRank.VisualEffectEntries, ActivationType.Start);
            GameEvents.Instance.OnTriggerSoundsList(targetEntity, effectRank.SoundEntries, ActivationType.Start, targetEntity.transform);
        }

        private void HandlePetDefendOwner(CombatEntity owner, CombatEntity attacker)
        {
            if (owner.GetCurrentPets().Count <= 0) return;
            foreach (var node in owner.GetCurrentPets().Where(node => node.GetTarget() == null)) node.SetTarget(attacker);
        }

        public void ExecuteHOTPulse(CombatEntity casterInfo, CombatEntity targetEntity, RPGEffect effect, RPGEffect.RPGEffectRankData effectRank, int curStack)
        {
            if (targetEntity.IsDead()) return;
            CombatCalculations.HealResult result = CombatCalculations.HealingCalculation(casterInfo, targetEntity, effect, effectRank, true);
            result.HealAmount /= effect.pulses;
            result.HealAmount *= curStack;
            
            targetEntity.Heal(result, effectRank.alteredStatID);
            CombatEvents.Instance.OnHealed(result);
            
            GameEvents.Instance.OnTriggerVisualEffectsList(targetEntity, effectRank.VisualEffectEntries, ActivationType.Start);
            GameEvents.Instance.OnTriggerSoundsList(targetEntity, effectRank.SoundEntries, ActivationType.Start, targetEntity.transform);
        }

        private void HandleLifesteal(CombatEntity casterInfo, CombatEntity targetInfo, RPGEffect effect, RPGEffect.RPGEffectRankData effectRank, int dmg)
        {
            var lifesteal = getLifesteal(casterInfo, effect, effectRank, dmg);
            if (lifesteal <= 0) return;
            CombatCalculations.HealResult result = CombatCalculations.GenerateHealResult(casterInfo, casterInfo,"HEAL",  lifesteal);
            casterInfo.Heal(result, GameDatabase.Instance.GetCombatSettings().HealthStatID);
            CombatEvents.Instance.OnHealed(result);
        }

        private void HandleThorn(CombatEntity casterInfo, CombatEntity targetInfo, CombatCalculations.DamageResult result)
        {
            if (GameState.playerEntity == null || casterInfo == null || targetInfo == null) return;
            float thorn = CombatUtilities.GetTotalOfStatType(targetInfo, RPGStat.STAT_TYPE.THORN);
            if (targetInfo.IsPet())
            {
                thorn += CombatUtilities.GetTotalOfStatType(targetInfo.GetOwnerEntity(),
                    RPGStat.STAT_TYPE.MINION_THORN);
            }

            if (!(thorn > 0)) return;
            
            thorn /= 100;
            var thornDamage = (int) (result.DamageAmount * thorn);
            if (thornDamage == 0) return;

            CombatCalculations.DamageResult ThornResult =
                CombatCalculations.GenerateDamageResult(casterInfo, targetInfo, "THORN", thornDamage);
            casterInfo.TakeDamage(ThornResult, null, GameDatabase.Instance.GetCombatSettings().HealthStatID);
            CombatEvents.Instance.OnDamageDealt(ThornResult);
            
            if (targetInfo.IsStealth()) targetInfo.ResetStealth();
            HandlePetDefendOwner(casterInfo, targetInfo);
        }

        public void HandleMobAggro(CombatEntity casterInfo, CombatEntity targetInfo)
        {
            if (casterInfo.IsDead()) return;
            HandleNPCAggroLinks(casterInfo, targetInfo);
            if (targetInfo.GetTarget() != null) return;
            targetInfo.SetTarget(casterInfo);
        }

        private void HandleNPCAggroLinks(CombatEntity casterInfo, CombatEntity targetInfo)
        {
            if(targetInfo.IsPlayer()) return;

            foreach (var entity in GameState.combatEntities)
            {
                foreach (var aggroLink in targetInfo.GetNPCData().aggroLinks)
                {
                    if(Vector3.Distance(targetInfo.transform.position, entity.transform.position) > aggroLink.maxDistance) continue;
                    if (entity == null || entity.IsPlayer() || entity == targetInfo) continue;
                    if (!casterInfo.IsPlayer() || !targetInfo.IsNPC()) continue;
                    if ((aggroLink.type != AIData.AggroLinkType.NPC || entity.GetNPCData().ID != aggroLink.npcID) && (aggroLink.type != AIData.AggroLinkType.Family || entity.GetNPCData().npcFamily != aggroLink.npcFamily)) continue;
                    if (entity.GetTarget() != null) continue;
                    if (FactionManager.Instance.GetCombatNodeAlignment(entity, targetInfo) != CombatData.EntityAlignment.Ally) continue;
                    if (!casterInfo.IsDead()) entity.GetAIEntity().AlterThreatTable(casterInfo, 1);
                }
            }
        }

        public void HandleFactionChangeAggro(RPGFaction faction)
        {
            foreach (var cbtNode in from cbtNode in GameState.combatEntities where !cbtNode.IsPlayer() where cbtNode.GetTarget() == GameState.playerEntity where FactionManager.Instance.GetCombatNodeAlignment(cbtNode, GameState.playerEntity) == CombatData.EntityAlignment.Ally select cbtNode)
            {
                cbtNode.ResetTarget();
            }
        }

        private bool VitalityActionConditionMet(CombatEntity cbtNode, RPGStat.VitalityActions vitalityAction,
            int statID)
        {
            switch (vitalityAction.valueType)
            {
                case RPGStat.VitalityActionsValueType.Equal:
                    if (vitalityAction.isPercent)
                    {
                        return (cbtNode.GetStats()[statID].currentValue / cbtNode.GetStats()[statID].currentMaxValue) *
                            100 == vitalityAction.value;
                    }
                    else
                    {
                        return cbtNode.GetStats()[statID].currentValue == vitalityAction.value;
                    }
                case RPGStat.VitalityActionsValueType.Above:
                    if (vitalityAction.isPercent)
                    {
                        return (cbtNode.GetStats()[statID].currentValue / cbtNode.GetStats()[statID].currentMaxValue) *
                            100 > vitalityAction.value;
                    }
                    else
                    {
                        return cbtNode.GetStats()[statID].currentValue > vitalityAction.value;
                    }
                case RPGStat.VitalityActionsValueType.Below:
                    if (vitalityAction.isPercent)
                    {
                        return (cbtNode.GetStats()[statID].currentValue / cbtNode.GetStats()[statID].currentMaxValue) *
                            100 < vitalityAction.value;
                    }
                    else
                    {
                        return cbtNode.GetStats()[statID].currentValue < vitalityAction.value;
                    }
                case RPGStat.VitalityActionsValueType.EqualOrAbove:
                    if (vitalityAction.isPercent)
                    {
                        return (cbtNode.GetStats()[statID].currentValue / cbtNode.GetStats()[statID].currentMaxValue) *
                            100 >= vitalityAction.value;
                    }
                    else
                    {
                        return cbtNode.GetStats()[statID].currentValue >= vitalityAction.value;
                    }
                case RPGStat.VitalityActionsValueType.EqualOrBelow:
                    if (vitalityAction.isPercent)
                    {
                        return (cbtNode.GetStats()[statID].currentValue / cbtNode.GetStats()[statID].currentMaxValue) *
                            100 <= vitalityAction.value;
                    }
                    else
                    {
                        return cbtNode.GetStats()[statID].currentValue <= vitalityAction.value;
                    }
            }

            return false;
        }

        public void HandleVitalityStatActions(CombatEntity cbtNode, RPGStat stat)
        {
            List<RPGStat.VitalityActions> allVitActions = new List<RPGStat.VitalityActions>(stat.vitalityActions);
            allVitActions = cbtNode.GetAllVitalityActions(allVitActions, stat, stat.ID);
            
            foreach (var vitalityAction in allVitActions)
            {
                if (!VitalityActionConditionMet(cbtNode, vitalityAction, stat.ID)) continue;
                if(vitalityAction.GameActionsTemplate == null) continue;
                GameActionsManager.Instance.TriggerGameActions(cbtNode, vitalityAction.GameActionsTemplate.GameActions);
            }
        }

        public void CheckIfPetsShouldAttack(CombatEntity attacker, CombatEntity attacked)
        {
            foreach (var pet in attacker.GetCurrentPets())
            {
                if(pet.GetTarget() == null && pet.GetAIEntity().CanAggroTarget(attacked)) pet.SetTarget(attacked);
            }
        }

        private IEnumerator EFFECTS_LOGIC(RPGEffect effect, RPGEffect.RPGEffectRankData effectRank, CombatEntity casterInfo,
            CombatEntity targetEntity, RPGAbility.RPGAbilityRankData abilityRank, float delay)
        {
            if (targetEntity.IsDead()) yield break;

            yield return new WaitForSeconds(delay);

            if (targetEntity.IsDead()) yield break;

            switch (effect.effectType)
            {
                case RPGEffect.EFFECT_TYPE.InstantDamage:
                    CombatCalculations.DamageResult result = CombatCalculations.DamageCalculation(casterInfo, targetEntity, abilityRank, effect, effectRank, false);
                    if (!targetEntity.IsImmune())
                    {
                        targetEntity.TakeDamage(result, abilityRank, effectRank.alteredStatID);
                        CombatEvents.Instance.OnDamageDealt(result);
                        HandlePetDefendOwner(targetEntity, casterInfo);
                        if (targetEntity.IsStealth() && effectRank.removeStealth) targetEntity.ResetStealth();
                        if (effectRank.alteredStatID ==
                            GameDatabase.Instance.GetCombatSettings().HealthStatID)
                            HandleLifesteal(casterInfo, targetEntity, effect, effectRank, (int)result.DamageAmount);
                    }

                    if(!targetEntity.IsDead() && !targetEntity.IsPlayer()) HandleMobAggro(casterInfo, targetEntity);
                    if ((int)result.DamageAmount > 0 && effectRank.alteredStatID ==
                        GameDatabase.Instance.GetCombatSettings().HealthStatID)
                        HandleThorn(casterInfo, targetEntity, result);
                    break;
                case RPGEffect.EFFECT_TYPE.InstantHeal:
                    var heal = CombatCalculations.HealingCalculation(casterInfo, targetEntity, effect, effectRank, false);
                    targetEntity.Heal(heal, effectRank.alteredStatID);
                    CombatEvents.Instance.OnHealed(heal);
                    break;
                case RPGEffect.EFFECT_TYPE.Teleport:
                    switch (effectRank.teleportType)
                    {
                        case RPGEffect.TELEPORT_TYPE.gameScene:
                            if (targetEntity == GameState.playerEntity)
                                RPGBuilderEssentials.Instance.TeleportToGameScene(effectRank.gameSceneID, effectRank.teleportPOS);
                            break;
                        case RPGEffect.TELEPORT_TYPE.position:
                            if (targetEntity == GameState.playerEntity)
                                GameState.playerEntity.controllerEssentials.TeleportToTarget(effectRank
                                    .teleportPOS);
                            break;
                        case RPGEffect.TELEPORT_TYPE.target:
                            if (GameState.playerEntity.GetTarget() != null && casterInfo == GameState.playerEntity)
                                GameState.playerEntity.controllerEssentials.TeleportToTarget(targetEntity);
                            break;
                        case RPGEffect.TELEPORT_TYPE.directional:
                            if (targetEntity == GameState.playerEntity)
                            {
                                RaycastHit obstacle;
                                Vector3 targetPOS = GameState.playerEntity.transform.position;
                                switch (effectRank.teleportDirectionalType)
                                {
                                    case RPGEffect.TELEPORT_DIRECTIONAL_TYPE.forward:
                                        targetPOS += GameState.playerEntity.transform.forward *
                                                     effectRank.teleportDirectionalDistance;
                                        break;
                                    case RPGEffect.TELEPORT_DIRECTIONAL_TYPE.left:
                                        targetPOS += -GameState.playerEntity.transform.right *
                                                     effectRank.teleportDirectionalDistance;
                                        break;
                                    case RPGEffect.TELEPORT_DIRECTIONAL_TYPE.right:
                                        targetPOS += GameState.playerEntity.transform.right *
                                                     effectRank.teleportDirectionalDistance;
                                        break;
                                    case RPGEffect.TELEPORT_DIRECTIONAL_TYPE.backward:
                                        targetPOS += -GameState.playerEntity.transform.forward *
                                                     effectRank.teleportDirectionalDistance;
                                        break;
                                    case RPGEffect.TELEPORT_DIRECTIONAL_TYPE.diagonalTopLeft:
                                        targetPOS += GameState.playerEntity.transform.forward *
                                                     effectRank.teleportDirectionalDistance;
                                        targetPOS += -GameState.playerEntity.transform.right *
                                                     effectRank.teleportDirectionalDistance;
                                        break;
                                    case RPGEffect.TELEPORT_DIRECTIONAL_TYPE.diagonalTopRight:
                                        targetPOS += GameState.playerEntity.transform.forward *
                                                     effectRank.teleportDirectionalDistance;
                                        targetPOS += GameState.playerEntity.transform.right *
                                                     effectRank.teleportDirectionalDistance;
                                        break;
                                    case RPGEffect.TELEPORT_DIRECTIONAL_TYPE.diagonalBackwardLeft:
                                        targetPOS += -GameState.playerEntity.transform.forward *
                                                     effectRank.teleportDirectionalDistance;
                                        targetPOS += -GameState.playerEntity.transform.right *
                                                     effectRank.teleportDirectionalDistance;
                                        break;
                                    case RPGEffect.TELEPORT_DIRECTIONAL_TYPE.diagonalBackwardRight:
                                        targetPOS += -GameState.playerEntity.transform.forward *
                                                     effectRank.teleportDirectionalDistance;
                                        targetPOS += GameState.playerEntity.transform.right *
                                                     effectRank.teleportDirectionalDistance;
                                        break;
                                }

                                if (Physics.Linecast(new Vector3(GameState.playerEntity.transform.position.x,
                                        GameState.playerEntity.transform.position.y + 1f,
                                        GameState.playerEntity.transform.position.z), targetPOS, out obstacle,
                                    effectRank.teleportDirectionalBlockLayers))
                                {
                                    targetPOS = GameState.playerEntity.transform.position;
                                    switch (effectRank.teleportDirectionalType)
                                    {
                                        case RPGEffect.TELEPORT_DIRECTIONAL_TYPE.forward:
                                            targetPOS += GameState.playerEntity.transform.forward * (obstacle.distance - 1f);
                                            break;
                                        case RPGEffect.TELEPORT_DIRECTIONAL_TYPE.left:
                                            targetPOS += -GameState.playerEntity.transform.right * (obstacle.distance - 1f);
                                            break;
                                        case RPGEffect.TELEPORT_DIRECTIONAL_TYPE.right:
                                            targetPOS += GameState.playerEntity.transform.right * (obstacle.distance - 1f);
                                            break;
                                        case RPGEffect.TELEPORT_DIRECTIONAL_TYPE.backward:
                                            targetPOS += -GameState.playerEntity.transform.forward * (obstacle.distance - 1f);
                                            break;
                                        case RPGEffect.TELEPORT_DIRECTIONAL_TYPE.diagonalTopLeft:
                                            targetPOS += GameState.playerEntity.transform.forward * (obstacle.distance - 1f);
                                            targetPOS += -GameState.playerEntity.transform.right * (obstacle.distance - 1f);
                                            break;
                                        case RPGEffect.TELEPORT_DIRECTIONAL_TYPE.diagonalTopRight:
                                            targetPOS += GameState.playerEntity.transform.forward * (obstacle.distance - 1f);
                                            targetPOS += GameState.playerEntity.transform.right * (obstacle.distance - 1f);
                                            break;
                                        case RPGEffect.TELEPORT_DIRECTIONAL_TYPE.diagonalBackwardLeft:
                                            targetPOS += -GameState.playerEntity.transform.forward * (obstacle.distance - 1f);
                                            targetPOS += -GameState.playerEntity.transform.right * (obstacle.distance - 1f);
                                            break;
                                        case RPGEffect.TELEPORT_DIRECTIONAL_TYPE.diagonalBackwardRight:
                                            targetPOS += -GameState.playerEntity.transform.forward * (obstacle.distance - 1f);
                                            targetPOS += GameState.playerEntity.transform.right * (obstacle.distance - 1f);
                                            break;
                                    }

                                    if (obstacle.point.y > GameState.playerEntity.transform.position.y)
                                    {
                                        targetPOS.y += (obstacle.point.y - GameState.playerEntity.transform.position.y) *
                                                       1.1f;
                                    }
                                }

                                GameState.playerEntity.controllerEssentials.TeleportToTarget(targetPOS);
                            }

                            break;
                    }

                    break;
                case RPGEffect.EFFECT_TYPE.Pet:
                    var maxSummonCount = getCurrentSummonCount(casterInfo);
                    for (var x = 0; x < effectRank.petSPawnCount; x++)
                    {
                        if (casterInfo.GetCurrentPets().Count < maxSummonCount)
                        {
                            CombatEntity petEntity =
                                GenerateNPCEntity(GameDatabase.Instance.GetNPCs()[effectRank.petNPCDataID],
                                    true, false, casterInfo,
                                    GetPetSpawnPosition(casterInfo), casterInfo.transform.rotation, null);

                            float totalDuration = effectRank.petDuration +
                                                  (effectRank.petDuration *
                                                      CombatUtilities.GetTotalOfStatType(casterInfo,
                                                          RPGStat.STAT_TYPE.MINION_DURATION) / 100);

                            if (effectRank.petScaleWithCharacter)
                            {
                                petEntity.SetLevel(Character.Instance.CharacterData.Level);
                            }
                            
                            StartCoroutine(destroyPet(totalDuration, petEntity));
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (casterInfo == GameState.playerEntity) CombatEvents.Instance.OnPlayerPetUpdate();

                    break;
                case RPGEffect.EFFECT_TYPE.RollLootTable:
                    var lootTableREF = GameDatabase.Instance.GetLootTables()[effectRank.lootTableID];
                    if(lootTableREF.RequirementsTemplate != null && RequirementsManager.Instance.RequirementsMet(GameState.playerEntity, lootTableREF.RequirementsTemplate.Requirements).Result) yield break;
                    var lootData = new List<LootBag.Loot_Data>();
                    float LOOTCHANCEMOD = CombatUtilities.GetTotalOfStatType(GameState.playerEntity,
                        RPGStat.STAT_TYPE.LOOT_CHANCE_MODIFIER);
                    foreach (var t1 in lootTableREF.lootItems)
                    {
                        var itemDropAmount = Random.Range(0f, 100f);
                        if (LOOTCHANCEMOD > 0) itemDropAmount += itemDropAmount * (LOOTCHANCEMOD / 100);
                        if (!(itemDropAmount <= t1.dropRate)) continue;
                        var stack = t1.min == t1.max ? t1.min : Random.Range(t1.min, t1.max + 1);

                        RPGItem itemREF = GameDatabase.Instance.GetItems()[t1.itemID];
                        var newLoot = new LootBag.Loot_Data();
                        newLoot.item = itemREF;
                        newLoot.count = stack;
                        newLoot.itemDataID =
                            RPGBuilderUtilities.HandleNewItemDATA(itemREF.ID, CharacterEntries.ItemEntryState.InWorld);

                        lootData.Add(newLoot);
                    }

                    foreach (var t in lootData)
                    {
                        if (t.looted) continue;
                        int itemsLeftOver = RPGBuilderUtilities.HandleItemLooting(t.item.ID,  -1,t.count, false, true);
                        if (itemsLeftOver == 0)
                        {
                            RPGBuilderUtilities.SetNewItemDataState(t.itemDataID, CharacterEntries.ItemEntryState.InBag);
                            t.looted = true;
                        }
                        else
                        {
                            t.count = itemsLeftOver;
                        }
                    }

                    ItemTooltip.Instance.Hide();

                    break;
                case RPGEffect.EFFECT_TYPE.Knockback:
                    if (targetEntity.CanActiveBlockThis(casterInfo) && targetEntity.ActiveBlockingState.effectRank.isBlockKnockback) yield break;
                    targetEntity.InitKnockback(casterInfo, effectRank.knockbackDistance);
                    if(!targetEntity.IsPlayer()) HandleMobAggro(casterInfo, targetEntity);
                    break;
                case RPGEffect.EFFECT_TYPE.Motion:
                    targetEntity.InitMotion(effectRank);
                    break;
                case RPGEffect.EFFECT_TYPE.Blocking:
                    targetEntity.InitActiveBlocking(effect, effectRank);
                    break;

                case RPGEffect.EFFECT_TYPE.Dispel:
                    List<int> effectsDispelledIndexes = new List<int>();
                    switch (effectRank.dispelType)
                    {
                        case RPGEffect.DISPEL_TYPE.EffectType:
                            for (var index = 0; index < targetEntity.GetStates().Count; index++)
                            {
                                var state = targetEntity.GetStates()[index];
                                if (state.stateEffect.effectType != effectRank.dispelEffectType) continue;
                                effectsDispelledIndexes.Add(index);
                            }

                            break;
                        case RPGEffect.DISPEL_TYPE.EffectTag:
                            for (var index = 0; index < targetEntity.GetStates().Count; index++)
                            {
                                var state = targetEntity.GetStates()[index];
                                if (state.stateEffect.EffectTag != effectRank.DispelEffectTag) continue;
                                effectsDispelledIndexes.Add(index);
                            }

                            break;
                        case RPGEffect.DISPEL_TYPE.Effect:
                            for (var index = 0; index < targetEntity.GetStates().Count; index++)
                            {
                                var state = targetEntity.GetStates()[index];
                                if (state.stateEffect.ID != effectRank.dispelEffectID) continue;
                                effectsDispelledIndexes.Add(index);
                            }

                            break;
                    }

                    DispelEffects(targetEntity, effectsDispelledIndexes);
                    break;
                
                case RPGEffect.EFFECT_TYPE.GameAction:
                    GameActionsManager.Instance.TriggerGameActions(casterInfo, (effectRank.UseGameActionsTemplate && effectRank.GameActionsTemplate != null)
                        ? effectRank.GameActionsTemplate.GameActions : effectRank.GameActions);
                    break;
            }

            GameEvents.Instance.OnTriggerVisualEffectsList(targetEntity, effectRank.VisualEffectEntries, ActivationType.Start);
            GameEvents.Instance.OnTriggerAnimationsList(targetEntity, effectRank.AnimationEntries, ActivationType.Start);
            GameEvents.Instance.OnTriggerSoundsList(targetEntity, effectRank.SoundEntries, ActivationType.Start, targetEntity.transform);
        }

        private Vector3 GetPetSpawnPosition(CombatEntity owner)
        {
            Vector3 sourcePos = new Vector3(owner.transform.position.x, owner.transform.position.y + 0.25f,
                owner.transform.position.z);
            if (!Physics.Raycast(sourcePos, -owner.transform.right, 2f,
                GameDatabase.Instance.GetCombatSettings().ProjectileDestroyLayers))
            {
                return owner.transform.position + (-owner.transform.right * 1.5f);
            }
            
            if (!Physics.Raycast(sourcePos, owner.transform.right, 2f,
                GameDatabase.Instance.GetCombatSettings().ProjectileDestroyLayers))
            {
                return owner.transform.position + (owner.transform.right * 1.5f);
            }
            
            if (!Physics.Raycast(sourcePos, owner.transform.forward, 2f,
                GameDatabase.Instance.GetCombatSettings().ProjectileDestroyLayers))
            {
                return owner.transform.position + (owner.transform.forward * 1.5f);
            }
            
            if (!Physics.Raycast(sourcePos, -owner.transform.forward, 2f,
                GameDatabase.Instance.GetCombatSettings().ProjectileDestroyLayers))
            {
                return owner.transform.position + (-owner.transform.forward * 1.5f);
            }

            return owner.transform.position;
        }

        private void DispelEffects(CombatEntity targetInfo, List<int> indexes)
        {
            foreach (var index in indexes)
            {
                targetInfo.EndStateEffect(index);
            }
        }

        private IEnumerator destroyPet(float duration, CombatEntity nodeREF)
        {
            yield return  new WaitForSeconds(duration);
            nodeREF.EntityDeath();
        }

        private IEnumerator HandleSpreadEffect(CombatEntity casterInfo, CombatEntity targetInfo, RPGEffect effect, int effectRank,
            int maxUnits, float maxDistance, RPGAbility.RPGAbilityRankData rankREF, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            float totalRadius = maxDistance +
                                (maxDistance * (CombatUtilities.GetTotalOfStatType(casterInfo, RPGStat.STAT_TYPE.AOE_RADIUS) / 100));

            List<CombatEntity> allCbtNodes =
                getPotentialCombatNodes(Physics.OverlapSphere(targetInfo.transform.position, totalRadius), casterInfo,
                    rankREF);
            if (allCbtNodes.Contains(targetInfo)) allCbtNodes.Remove(targetInfo);
            int totalUnitHit = maxUnits + (int) CombatUtilities.GetTotalOfStatType(casterInfo, RPGStat.STAT_TYPE.ABILITY_MAX_HIT);
            var closestUnits = getClosestUnits(targetInfo, allCbtNodes, totalUnitHit);
            foreach (var t in closestUnits)
            {
                if (effect.isState)
                    StartCoroutine(InitEntityState(casterInfo, t, effect, effectRank, effect.entryIcon, 0));
                else
                    StartCoroutine(EFFECTS_LOGIC(effect, effect.ranks[effectRank], casterInfo, t, rankREF, 0));
            }
        }

        bool isEffectActiveOnTarget(RPGEffect effectRequired, CombatEntity target)
        {
            foreach (var effect in target.GetStates())
            {
                if (effect.stateEffect == effectRequired) return true;
            }

            return false;
        }

        private void ExecuteAbilityEffects(CombatEntity casterInfo, CombatEntity targetInfo, RPGAbility.RPGAbilityRankData rankREF)
        {
            foreach (var action in rankREF.Actions)
            {
                if(action.GameActionsTemplate == null) continue;
                if (action.RequirementsTemplate == null || RequirementsManager.Instance.RequirementsMet(
                    action.RequirementsTarget == RPGCombatDATA.TARGET_TYPE.Caster ? casterInfo : targetInfo,
                    action.RequirementsTemplate.Requirements).Result)
                {
                    GameActionsManager.Instance.TriggerGameActions(
                        action.ActionsTarget == RPGCombatDATA.TARGET_TYPE.Caster ? casterInfo : targetInfo,
                        action.GameActionsTemplate.GameActions);
                }
            }
            
            foreach (var effectApplied in rankREF.effectsApplied)
            {
                var thisEffect = GameDatabase.Instance.GetEffects()[effectApplied.effectID];
                var rdmEffectChance = Random.Range(0f, 100f);
                if (!(rdmEffectChance <= effectApplied.chance)) continue;
                if (thisEffect.isState)
                    StartCoroutine(InitEntityState(casterInfo, effectApplied.target == RPGCombatDATA.TARGET_TYPE.Target ? targetInfo : casterInfo,
                        thisEffect, effectApplied.effectRank, thisEffect.entryIcon, effectApplied.delay));
                else
                    StartCoroutine(EFFECTS_LOGIC(thisEffect, GameDatabase.Instance.GetEffects()[effectApplied.effectID].ranks[effectApplied.effectRank], casterInfo,
                        effectApplied.target == RPGCombatDATA.TARGET_TYPE.Target ? targetInfo : casterInfo, rankREF, effectApplied.delay));

                if (effectApplied.isSpread)
                {
                    StartCoroutine(HandleSpreadEffect(casterInfo, targetInfo, thisEffect, effectApplied.effectRank, effectApplied.spreadUnitMax,
                        effectApplied.spreadDistanceMax, rankREF, effectApplied.delay));
                }
            }
        }

        private void ExecuteCasterEffects(CombatEntity casterInfo, RPGAbility.RPGAbilityRankData rankREF)
        {
            foreach (var t in rankREF.casterEffectsApplied)
            {
                var thisEffect = GameDatabase.Instance.GetEffects()[t.effectID];
                var rdmEffectChance = Random.Range(0f, 100f);
                if (!(rdmEffectChance <= t.chance)) continue;
                if (thisEffect.isState)
                    StartCoroutine(InitEntityState(casterInfo, casterInfo,
                        thisEffect, t.effectRank, thisEffect.entryIcon, t.delay));
                else
                    StartCoroutine(EFFECTS_LOGIC(thisEffect, GameDatabase.Instance.GetEffects()[t.effectID].ranks[t.effectRank], casterInfo,
                        casterInfo, rankREF, t.delay));
            }
        }

        public void ExecuteEffect(CombatEntity casterInfo, CombatEntity targetInfo, RPGEffect effect, int effectRank, RPGAbility.RPGAbilityRankData abilityRank, float delay)
        {
            if (effect.isState)
                StartCoroutine(InitEntityState(casterInfo, targetInfo, effect, effectRank, effect.entryIcon, delay));
            else
                StartCoroutine(EFFECTS_LOGIC(effect, effect.ranks[effectRank], casterInfo, targetInfo, abilityRank, delay));
        }

        public bool CanCombatNodeBlockThis(CombatEntity targetInfo, CombatEntity attacker)
        {
            var pointDirection = attacker.transform.position - targetInfo.transform.position;
            var angle = Vector3.Angle(targetInfo.transform.forward, pointDirection);
            return angle < (targetInfo.ActiveBlockingState.effectRank.blockAngle + (targetInfo.ActiveBlockingState.effectRank.blockAngle *
                                                                    CombatUtilities.GetTotalOfStatType(targetInfo,
                                                                        RPGStat.STAT_TYPE.BLOCK_ACTIVE_ANGLE)/100f));
        }

        public int getCurrentSummonCount(CombatEntity nodeRef)
        {
            return (int)CombatUtilities.GetTotalOfStatType(nodeRef, RPGStat.STAT_TYPE.SUMMON_COUNT);
        }

        public void HandleOnKillActions(CombatEntity attacker, CombatEntity deadUnit, RPGAbility.RPGAbilityRankData rankREF)
        {
            if (rankREF == null) return;
            if (!AbilityHasTag(rankREF, RPGAbility.ABILITY_TAGS.onKill)) return;
            int HEALTHONKILL = (int) CombatUtilities.GetTotalOfStatType(attacker, RPGStat.STAT_TYPE.HEALTH_ON_KILL);
            if (attacker.IsPet())
            {
                HEALTHONKILL += (int) CombatUtilities.GetTotalOfStatType(attacker.GetOwnerEntity(),
                    RPGStat.STAT_TYPE.MINION_HEALTH_ON_KILL);
            }

            if (HEALTHONKILL > 0)
            {
                var heal = CombatCalculations.HealingCalculation(attacker, attacker, null, null, false);
                heal.HealAmount += HEALTHONKILL;
                attacker.Heal(heal, GameDatabase.Instance.GetCombatSettings().HealthStatID);
                CombatEvents.Instance.OnHealed(heal);
            }

            foreach (var t in attacker.GetStats())
            {
                foreach (var t1 in t.Value.stat.statBonuses)
                {
                    switch (t1.statType)
                    {
                        case RPGStat.STAT_TYPE.EFFECT_TRIGGER:
                            if (!(Random.Range(0f, 100f) <= t.Value.currentValue)) continue;
                            foreach (var t2 in t.Value.stat.onHitEffectsData)
                            {
                                if (t2.tagType != RPGAbility.ABILITY_TAGS.onKill) continue;
                                if (Random.Range(0f, 100f) <= t2.chance)
                                {
                                    ExecuteEffect(attacker, attacker, GameDatabase.Instance.GetEffects()[t2.effectID], 
                                        t2.effectRank,null, 0);
                                }
                            }

                            break;
                    }
                }
            }
        }

        public bool AbilityHasTag(RPGAbility.RPGAbilityRankData rankREF, RPGAbility.ABILITY_TAGS tag)
        {
            foreach (var t in rankREF.tagsData)
            {
                if (t.tag == tag) return true;
            }

            return false;
        }

        private bool isDamageCrit(float critRate)
        {
            if (!(critRate > 0)) return false;
            float critChance = Random.Range(0, 100);
            return critChance <= critRate;
        }


        private int getLifesteal(CombatEntity casterInfo, RPGEffect effect, RPGEffect.RPGEffectRankData effectRank, int dmg)
        {
            float lifestealStat = 0;

            foreach (var t in casterInfo.GetStats())
            {
                foreach (var t1 in t.Value.stat.statBonuses)
                {
                    switch (t1.statType)
                    {
                        case RPGStat.STAT_TYPE.LIFESTEAL:
                            lifestealStat += t.Value.currentValue;
                            break;
                    }
                }
            }

            if (casterInfo.IsPet())
            {
                lifestealStat += (int)CombatUtilities.GetTotalOfStatType(casterInfo.GetOwnerEntity(),
                    RPGStat.STAT_TYPE.MINION_LIFESTEAL);
            }

            var curLifesteal = (effectRank.lifesteal / 100) + (lifestealStat / 100);
            return (int) (dmg * curLifesteal);
        }


        private List<CombatEntity> getClosestUnits(CombatEntity playerCombatInfo, List<CombatEntity> allCbtNodes, int maxUnitHit)
        {
            var closestUnits = new List<CombatEntity>();
            var allDistances = new List<float>();

            foreach (var t in allCbtNodes)
                if (allDistances.Count >= maxUnitHit)
                {
                    var dist = Vector3.Distance(playerCombatInfo.gameObject.transform.position, t.transform.position);
                    var CurBiggestDistanceInArray = Mathf.Max(allDistances.ToArray());
                    var IndexOfBiggest = allDistances.IndexOf(CurBiggestDistanceInArray);
                    if (!(dist < CurBiggestDistanceInArray)) continue;
                    allDistances[IndexOfBiggest] = dist;
                    closestUnits[IndexOfBiggest] = t;
                }
                else
                {
                    allDistances.Add(Vector3.Distance(playerCombatInfo.gameObject.transform.position,
                        t.transform.position));
                    closestUnits.Add(t);
                }

            return closestUnits;
        }

        private List<CombatEntity> getClosestUnits(CombatEntity playerCombatInfo, List<Collider> hitColliders, int maxUnitHit)
        {
            var closestUnits = new List<CombatEntity>();
            var allDistances = new List<float>();

            foreach (var t in hitColliders)
                if (allDistances.Count >= maxUnitHit)
                {
                    var dist = Vector3.Distance(playerCombatInfo.gameObject.transform.position, t.transform.position);
                    var CurBiggestDistanceInArray = Mathf.Max(allDistances.ToArray());
                    var IndexOfBiggest = allDistances.IndexOf(CurBiggestDistanceInArray);
                    if (!(dist < CurBiggestDistanceInArray)) continue;
                    allDistances[IndexOfBiggest] = dist;
                    closestUnits[IndexOfBiggest] = t.GetComponent<CombatEntity>();
                }
                else
                {
                    allDistances.Add(Vector3.Distance(playerCombatInfo.gameObject.transform.position,
                        t.transform.position));
                    closestUnits.Add(t.GetComponent<CombatEntity>());
                }

            return closestUnits;
        }

        private List<CombatEntity> getClosestNearbyUnits(GameObject projGO, List<CombatEntity> hitNodes, int maxUnitHit)
        {
            var closestUnits = new List<CombatEntity>();
            var allDistances = new List<float>();

            foreach (var t in hitNodes)
                if (allDistances.Count >= maxUnitHit)
                {
                    var dist = Vector3.Distance(projGO.transform.position, t.transform.position);
                    var CurBiggestDistanceInArray = Mathf.Max(allDistances.ToArray());
                    var IndexOfBiggest = allDistances.IndexOf(CurBiggestDistanceInArray);
                    if (!(dist < CurBiggestDistanceInArray)) continue;
                    allDistances[IndexOfBiggest] = dist;
                    closestUnits[IndexOfBiggest] = t;
                }
                else
                {
                    allDistances.Add(Vector3.Distance(projGO.transform.position, t.transform.position));
                    closestUnits.Add(t);
                }

            return closestUnits;
        }

        public CombatEntity GenerateNPCEntity(RPGNpc npcData, bool isPet, bool isPersistent, CombatEntity petOwner, Vector3 spawnPos, Quaternion rotation, NPCSpawner spawner)
        {
            GameObject logicTemplate = GetAILogicTemplate(npcData);
            if (logicTemplate == null) return null;
            GameObject newNPC = Instantiate(logicTemplate, spawnPos, rotation);
            newNPC.name = npcData.entryName;
            
            CombatEntity entity = newNPC.GetComponent<CombatEntity>();
            if (entity == null)
            {
                Debug.LogError("Could not find a COMBAT ENTITY on the NPC Template");
                Destroy(newNPC);
            }
            
            if (isPet)
            {
                petOwner.GetCurrentPets().Add(entity);
                entity.SetOwnerEntity(petOwner);
            }
            entity.SetPersistentNPC(isPersistent);
            GameState.Instance.RegisterEntity(entity);
            if(spawner != null) entity.SetSpawner(spawner);
            entity.InitNPC(npcData, petOwner);
            return entity;
        }

        private GameObject GetAILogicTemplate(RPGNpc npcData)
        {
            if (npcData.AILogicTemplate != null)
            {
                return npcData.AILogicTemplate;
            }

            if (GameDatabase.Instance.GetCombatSettings().DefaultAILogicTemplate != null)
            {
                return GameDatabase.Instance.GetCombatSettings().DefaultAILogicTemplate;
            }

            Debug.LogError("Could not find an AI Logic Template for " + npcData.entryName);
            return null;
        }
    }
}