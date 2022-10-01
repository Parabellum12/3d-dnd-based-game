using System.Linq;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.Data;
using BLINK.RPGBuilder.Managers;
using UnityEngine;

public static class CombatCalculations
{
    public static DamageResult GenerateDamageResult(CombatEntity casterEntity, CombatEntity targetEntity, string damageActionType, int value)
    {
        return new DamageResult {caster = casterEntity, target = targetEntity, DamageAmount = value, DamageActionType = damageActionType};
    }
    public static HealResult GenerateHealResult(CombatEntity casterEntity, CombatEntity targetEntity, string healActionType, int value)
    {
        return new HealResult {caster = casterEntity, target = targetEntity, HealAmount = value, HealActionType = healActionType};
    }
    
    private static float GetInitialDamageValue(RPGEffect.RPGEffectRankData effectRank, float damage, CombatEntity targetEntity)
    {
        RPGStat alteredStat = GameDatabase.Instance.GetStats()[effectRank.alteredStatID];
        
        switch (effectRank.hitValueType)
        {
            case RPGAbility.COST_TYPES.FLAT:
                damage = effectRank.Damage;
                break;
            case RPGAbility.COST_TYPES.PERCENT_OF_MAX:
                damage = CombatUtilities.GetMaxStatValue(targetEntity, alteredStat.ID) * ((float)effectRank.Damage/100f);
                break;
            case RPGAbility.COST_TYPES.PERCENT_OF_CURRENT:
                damage = CombatUtilities.GetCurrentStatValue(targetEntity, alteredStat.ID) * ((float)effectRank.Damage/100f);
                break;
        }
        return damage;
    }
    private static float AddWeaponBonus(CombatEntity casterEntity, RPGEffect.RPGEffectRankData effectRank, float value)
    {
        if (casterEntity == null) return value;
        int weaponBonus = 0;
        
        if (effectRank.useWeapon1Damage)
        {
            EconomyData.EquippedWeapon weapon1 = casterEntity.GetEquippedWeaponByIndex(0);
            if (weapon1 != null && weapon1.item != null)
            {
                weaponBonus += Random.Range(weapon1.item.minDamage, weapon1.item.maxDamage);
            }
        }
        
        if (effectRank.useWeapon2Damage)
        {
            EconomyData.EquippedWeapon weapon2 = casterEntity.GetEquippedWeaponByIndex(1);
            if (weapon2 != null && weapon2.item != null)
            {
                weaponBonus += Random.Range(weapon2.item.minDamage, weapon2.item.maxDamage);
            }
        }
        
        float weaponMod = effectRank.weaponDamageModifier / 100;
        value += weaponBonus * weaponMod;
        
        return value;
    }
    private static float AddRequiredEffectBonus(CombatEntity targetEntity, RPGEffect.RPGEffectRankData effectRank, float damage)
    {
        if (!CombatUtilities.IsEffectActiveOnTarget(GameDatabase.Instance.GetEffects()[effectRank.requiredEffectID], targetEntity)) return damage;
        damage += damage * (effectRank.requiredEffectDamageModifier / 100);
        return damage;
    }
    private static float AddStatBonusBonus(CombatEntity casterEntity, RPGEffect.RPGEffectRankData effectRank, float damage)
    {
        if (casterEntity == null) return damage;
        damage += CombatUtilities.GetCurrentStatValue(casterEntity, effectRank.damageStatID) * (effectRank.damageStatModifier / 100);
        return damage;
    }
    private static float AddBaseDamage(CombatEntity casterEntity, RPGEffect.RPGEffectRankData effectRank, float damage)
    {
        if (casterEntity == null) return damage;
        foreach (var t in casterEntity.GetStats())
        {
            foreach (var t1 in t.Value.stat.statBonuses.Where(t1 => t1.statType == RPGStat.STAT_TYPE.BASE_DAMAGE_TYPE))
            {
                if (effectRank.mainDamageType != t1.MainDamageType) continue;
                damage += t.Value.currentValue * t1.modifyValue;
            }
        }

        return damage;
    }
    private static float AddBaseHealing(CombatEntity casterEntity, RPGEffect.RPGEffectRankData effectRank, float heal)
    {
        if (casterEntity == null) return heal;
        foreach (var t in casterEntity.GetStats())
        {
            foreach (var t1 in t.Value.stat.statBonuses.Where(t1 => t1.statType == RPGStat.STAT_TYPE.HEALING || t1.statType == RPGStat.STAT_TYPE.GLOBAL_HEALING))
            {
                if (effectRank.mainDamageType != t1.MainDamageType) continue;
                heal += t.Value.currentValue * t1.modifyValue;
            }
        }

        return heal;
    }
    private static float GetResistances(CombatEntity targetEntity, RPGEffect.RPGEffectRankData effectRank, float resistance)
    {
        foreach (var stat in targetEntity.GetStats())
        {
            foreach (var statBonus in stat.Value.stat.statBonuses)
            {
                if (statBonus.statType == RPGStat.STAT_TYPE.BASE_DAMAGE_TYPE)
                {
                    if (effectRank.mainDamageType != statBonus.MainDamageType) continue;

                    if (statBonus.ResistanceStatID != -1)
                    {
                        resistance += CombatUtilities.GetCurrentStatValue(targetEntity, statBonus.ResistanceStatID);
                    }
                }
            }
        }

        return resistance;
    }
    
    
    private static float GetAbsorptions(CombatEntity targetEntity, RPGEffect.RPGEffectRankData effectRank, float absorption)
    {
        foreach (var stat in targetEntity.GetStats())
        {
            foreach (var statBonus in stat.Value.stat.statBonuses)
            {
                if (statBonus.statType == RPGStat.STAT_TYPE.HEALING)
                {
                    if (effectRank.customHealingType != statBonus.CustomHealingType) continue;

                    if (statBonus.ResistanceStatID != -1)
                    {
                        absorption += CombatUtilities.GetCurrentStatValue(targetEntity, statBonus.ResistanceStatID);
                    }
                }
            }
        }

        return absorption;
    }
    
    
    private static float GetPenetrations(CombatEntity casterEntity, RPGEffect.RPGEffectRankData effectRank, float penetration)
    {
        if (casterEntity == null) return penetration;
        foreach (var stat in casterEntity.GetStats())
        {
            foreach (var statBonus in stat.Value.stat.statBonuses)
            {
                switch (statBonus.statType)
                {
                    case RPGStat.STAT_TYPE.BASE_DAMAGE_TYPE when statBonus.MainDamageType == effectRank.mainDamageType:
                    {
                        if (statBonus.PenetrationStatID != -1)
                        {
                            penetration += CombatUtilities.GetCurrentStatValue(casterEntity, statBonus.PenetrationStatID);
                        }
                        break;
                    }
                    case RPGStat.STAT_TYPE.DAMAGE when statBonus.CustomDamageType == effectRank.customDamageType:
                    {
                        if (statBonus.PenetrationStatID != -1)
                        {
                            penetration += CombatUtilities.GetCurrentStatValue(casterEntity, statBonus.PenetrationStatID);
                        }
                        break;
                    }
                }
            }
        }

        return penetration;
    }

    
    private class SecondaryDamageResult
    {
        public float damage;
        public bool isActiveBlocked;
        public int damageBlocked;
    }
    private static SecondaryDamageResult GetSecondaryTypeBonus(CombatEntity casterEntity, CombatEntity targetEntity, RPGEffect.RPGEffectRankData effectRank)
    {
        SecondaryDamageResult result = new SecondaryDamageResult();
        if (casterEntity == null || effectRank.customDamageType == null || effectRank.customDamageType.entryName == "NONE") return result;

        float secondaryDamage = CombatUtilities.GetTotalOfStatFunction(casterEntity, RPGStat.STAT_TYPE.DAMAGE, effectRank.customDamageType);
        
        if (targetEntity.GetSpecies() != null)
        {
            foreach (var trait in targetEntity.GetSpecies().traits)
            {
                if (trait.damageType == effectRank.customDamageType) secondaryDamage *= trait.modifier / 100f;
            }
        }

        if (targetEntity.CanActiveBlockThis(casterEntity) && (targetEntity.ActiveBlockingState.effectRank.blockAnyDamage ||
                                                              targetEntity.ActiveBlockingState.effectRank.blockedCustomDamageTypes
                                                                  .Contains(effectRank.customDamageType)))
        {
            float blockedAmount = secondaryDamage * (targetEntity.ActiveBlockingState.curBlockPowerModifier / 100f);
            blockedAmount += (int) targetEntity.ActiveBlockingState.curBlockPowerFlat;
            switch (targetEntity.ActiveBlockingState.effectRank.blockEndType)
            {
                case RPGEffect.BLOCK_END_TYPE.MaxDamageBlocked
                    when blockedAmount > targetEntity.ActiveBlockingState.curBlockedDamageLeft:
                    blockedAmount = targetEntity.ActiveBlockingState.curBlockedDamageLeft;
                    break;
                case RPGEffect.BLOCK_END_TYPE.Stat:
                    int curStatAmount = (int) CombatUtilities.GetCurrentStatValue(targetEntity, targetEntity.ActiveBlockingState.effectRank.blockStatID);
                    if (blockedAmount > curStatAmount) blockedAmount = curStatAmount;
                    break;
            }

            if (blockedAmount < 0) blockedAmount = 0;
            if (blockedAmount > secondaryDamage)
            {
                blockedAmount = secondaryDamage;
                secondaryDamage = 0;
            }
            else
            {
                secondaryDamage -= blockedAmount;
                if (secondaryDamage < 0) secondaryDamage = 0;
            }

            switch (targetEntity.ActiveBlockingState.effectRank.blockEndType)
            {
                case RPGEffect.BLOCK_END_TYPE.MaxDamageBlocked:
                    targetEntity.ReduceActiveBlockingDamageLeft((int) blockedAmount);
                    break;
                case RPGEffect.BLOCK_END_TYPE.Stat:
                    CombatUtilities.UpdateCurrentStatValue(targetEntity, targetEntity.ActiveBlockingState.effectRank.blockStatID, (int) blockedAmount);
                    break;
            }

            result.damageBlocked += (int) blockedAmount;
            result.isActiveBlocked = true;
        }

        result.damage = secondaryDamage;
        return result;
    }
    private static float AddDamageOverTimeBonus(CombatEntity casterEntity, float damage)
    {
        if (casterEntity == null) return damage;
        float DamageOverTimeBonus = CombatUtilities.GetTotalOfStatType(casterEntity, RPGStat.STAT_TYPE.DOT_BONUS);
        if (DamageOverTimeBonus > 0) damage += damage * (DamageOverTimeBonus / 100);
        return damage;
    }
    private static float AddHealOverTimeBonus(CombatEntity casterEntity, float heal)
    {
        if (casterEntity == null) return heal;
        float healOverTimeBonus = CombatUtilities.GetTotalOfStatType(casterEntity, RPGStat.STAT_TYPE.HOT_BONUS);
        if (healOverTimeBonus > 0) heal += heal * (healOverTimeBonus / 100);
        return heal;
    }
    private static float AddMaxHealthBonus(CombatEntity casterEntity, RPGEffect.RPGEffectRankData effectRank, float extraDamage)
    {
        if (casterEntity == null) return extraDamage;
        extraDamage += (CombatUtilities.GetMaxStatValue(casterEntity, GameDatabase.Instance.GetCombatSettings().HealthStatID) / 100f) * effectRank.maxHealthModifier;
        return extraDamage;
    }
    private static float AddMissingHealthBonus(CombatEntity casterEntity, RPGEffect.RPGEffectRankData effectRank, float damage, float extraDamage)
    {
        if (casterEntity == null) return extraDamage;
        float missingHealthPercent = CombatUtilities.GetCurrentStatValue(casterEntity, GameDatabase.Instance.GetCombatSettings().HealthStatID) /
            CombatUtilities.GetMaxStatValue(casterEntity, GameDatabase.Instance.GetCombatSettings().HealthStatID);
        
        missingHealthPercent = 1 - missingHealthPercent;
        if (!(missingHealthPercent > 0)) return extraDamage;
        missingHealthPercent *= 100;
        missingHealthPercent *= effectRank.missingHealthModifier / 100;
        missingHealthPercent /= 100;
        extraDamage += damage * missingHealthPercent;
        return extraDamage;
    }
    private static float GetCriticalHitChance(CombatEntity casterEntity)
    {
        if (casterEntity == null) return 0;
        float value = CombatUtilities.GetTotalOfStatType(casterEntity, RPGStat.STAT_TYPE.CRIT_CHANCE);
        if (casterEntity.IsPet())
        {
            value += CombatUtilities.GetTotalOfStatType(casterEntity, RPGStat.STAT_TYPE.MINION_CRIT_CHANCE);
        }

        if (value > 100)
        {
            value = 100;
        }

        return value;
    }
    private static float GetStatValue(CombatEntity casterEntity, RPGStat.STAT_TYPE statType, RPGStat.STAT_TYPE petStatType)
    {
        if (casterEntity == null) return 0;
        float value = CombatUtilities.GetTotalOfStatType(casterEntity, statType);
        if (casterEntity.IsPet())
        {
            value += CombatUtilities.GetTotalOfStatType(casterEntity, petStatType);
        }

        return value;
    }
    private static bool IsDamageCrit(float criticalChance)
    {
        if (!(criticalChance > 0)) return false;
        float result = Random.Range(0, 100);
        return result <= criticalChance;
    }
    private static float HandleCriticalDamage(float damage, float criticalHitPower)
    {
        var finalCriticalHitPower = GameDatabase.Instance.GetCombatSettings().CriticalHitBonus + criticalHitPower;
        finalCriticalHitPower /= 100;
        damage *= finalCriticalHitPower;
        
        return damage;
    }
    private static float HandleDamageModifiers(float damage, float DamageDealtModifier, float DamageTakenModifier)
    {
        DamageDealtModifier /= 100;
        DamageTakenModifier /= 100;
        damage += damage * DamageDealtModifier;
        damage += damage * DamageTakenModifier;
        
        return damage;
    }
    private static float HandleResistances(CombatEntity casterEntity, CombatEntity targetEntity, RPGEffect.RPGEffectRankData effectRank, float damage)
    {
        float resistancePercent = GetResistances(targetEntity, effectRank, 0);
        float penetrationPercent = GetPenetrations(casterEntity, effectRank, 0);
        if (!(resistancePercent > 0)) return damage;
        var finalBaseTypeRES = resistancePercent - penetrationPercent;
        if (finalBaseTypeRES < 0) finalBaseTypeRES = 0;
        finalBaseTypeRES /= 100;
        finalBaseTypeRES = 1 - finalBaseTypeRES;
        damage *= finalBaseTypeRES;
        
        return damage;
    }
    
    private static float HandleAbsorption(CombatEntity targetEntity, RPGEffect.RPGEffectRankData effectRank, float heal)
    {
        float absorptionPercent = GetAbsorptions(targetEntity, effectRank, 0);
        if (!(absorptionPercent > 0)) return heal;
        absorptionPercent /= 100;
        absorptionPercent += 1;
        heal *= absorptionPercent;
        
        return heal;
    }

    private static void HandlePassiveStats(CombatEntity casterEntity, CombatEntity targetEntity, RPGEffect.RPGEffectRankData effectRank, DamageResult result)
    {
        if (casterEntity == null) return;
        switch (effectRank.mainDamageType)
        {
            case RPGEffect.MainDamageTypes.Physical:
            {
                if (effectRank.alteredStatID != GameDatabase.Instance.GetCombatSettings().HealthStatID) break;

                float passiveBlockChance = 0,
                    blockFlatAmount = 0,
                    blockPercentModifier = 0,
                    dodgeChance = 0,
                    glancingBlowChance = 0;
                foreach (var t in targetEntity.GetStats())
                {
                    foreach (var t1 in t.Value.stat.statBonuses)
                    {
                        switch (t1.statType)
                        {
                            case RPGStat.STAT_TYPE.BLOCK_CHANCE:
                                passiveBlockChance += t.Value.currentValue * t1.modifyValue;
                                break;
                            case RPGStat.STAT_TYPE.BLOCK_FLAT:
                                blockFlatAmount += t.Value.currentValue * t1.modifyValue;
                                break;
                            case RPGStat.STAT_TYPE.BLOCK_MODIFIER:
                                blockPercentModifier += t.Value.currentValue * t1.modifyValue;
                                break;
                            case RPGStat.STAT_TYPE.DODGE_CHANCE:
                                dodgeChance += t.Value.currentValue * t1.modifyValue;
                                break;
                            case RPGStat.STAT_TYPE.GLANCING_BLOW_CHANCE:
                                glancingBlowChance += t.Value.currentValue * t1.modifyValue;
                                break;
                        }
                    }
                }

                if (casterEntity.IsPet())
                {
                    dodgeChance += CombatUtilities.GetTotalOfStatType(casterEntity.GetOwnerEntity(),
                        RPGStat.STAT_TYPE.MINION_DODGE_CHANCE);

                    glancingBlowChance += CombatUtilities.GetTotalOfStatType(casterEntity.GetOwnerEntity(),
                        RPGStat.STAT_TYPE.MINION_GLANCING_BLOW_CHANCE);
                }

                else if (glancingBlowChance > 0)
                {
                    if (Random.Range(0f, 100f) <= glancingBlowChance)
                    {
                        result.DamageAmount /= 2;
                    }
                }
                
                if (passiveBlockChance > 0)
                {
                    if (Random.Range(0f, 100f) <= passiveBlockChance)
                    {
                        float blockedPassivelyAmount = result.DamageAmount * (blockPercentModifier / 100);
                        blockedPassivelyAmount += blockFlatAmount;
                        result.DamageAmount -= blockedPassivelyAmount;

                        result.DamageBlockedPassively = (int)blockedPassivelyAmount;

                        if (result.DamageAmount < 0)
                        {
                            result.DamageAmount = 0;
                        }

                        int HealthGainedOnBlock = (int) CombatUtilities.GetTotalOfStatType(targetEntity, RPGStat.STAT_TYPE.HEALTH_ON_BLOCK);

                        if (targetEntity.IsPet())
                        {
                            HealthGainedOnBlock += (int) CombatUtilities.GetTotalOfStatType(
                                casterEntity.GetOwnerEntity(),
                                RPGStat.STAT_TYPE.MINION_HEALTH_ON_BLOCK);
                        }

                        if (HealthGainedOnBlock > 0)
                        {
                            var heal = HealingCalculation(targetEntity, targetEntity, null, null, false);
                            targetEntity.Heal(heal, GameDatabase.Instance.GetCombatSettings().HealthStatID);
                            CombatEvents.Instance.OnHealed(heal);
                        }
                    }
                }
                
                if (dodgeChance > 0)
                {
                    if (Random.Range(0f, 100f) <= dodgeChance)
                    {
                        result.DamageAmount = 0;
                        result.DamageActionType += "DODGED";
                    }
                }

                break;
            }
            case RPGEffect.MainDamageTypes.Magical:
                break;
        }
    }

    private static void HandleStatEffects(CombatEntity casterEntity, CombatEntity targetEntity, RPGAbility.RPGAbilityRankData abilityRank, RPGEffect.RPGEffectRankData effectRank, DamageResult result)
    {
        if (casterEntity == null) return;
        if (abilityRank == null || effectRank.alteredStatID != GameDatabase.Instance.GetCombatSettings().HealthStatID) return;
        if (!CombatUtilities.AbilityHasTag(abilityRank, RPGAbility.ABILITY_TAGS.onHit)) return;
        
        int HealthGainedOnHit =
            (int) CombatUtilities.GetTotalOfStatType(casterEntity, RPGStat.STAT_TYPE.HEALTH_ON_HIT);
        if (casterEntity.IsPet())
        {
            HealthGainedOnHit += (int) CombatUtilities.GetTotalOfStatType(casterEntity.GetOwnerEntity(),
                RPGStat.STAT_TYPE.MINION_HEALTH_ON_HIT);
        }

        if (HealthGainedOnHit > 0)
        {
            var heal = HealingCalculation(casterEntity, casterEntity, null, null, false);
            targetEntity.Heal(heal, GameDatabase.Instance.GetCombatSettings().HealthStatID);
            CombatEvents.Instance.OnHealed(heal);
        }

        foreach (var t in casterEntity.GetStats())
        {
            foreach (var t1 in t.Value.stat.statBonuses)
            {
                switch (t1.statType)
                {
                    case RPGStat.STAT_TYPE.EFFECT_TRIGGER:
                        if (!(Random.Range(0f, 100f) <= t.Value.currentValue)) continue;
                        foreach (var t2 in t.Value.stat.onHitEffectsData.Where(t2 => t2.tagType == RPGAbility.ABILITY_TAGS.onHit).Where(t2 => Random.Range(0f, 100f) <= t2.chance))
                        {
                            CombatManager.Instance.ExecuteEffect(casterEntity,
                                t2.targetType == RPGCombatDATA.TARGET_TYPE.Caster
                                    ? casterEntity
                                    : targetEntity,
                                GameDatabase.Instance.GetEffects()[t2.effectID],
                                t2.effectRank,
                                null, 0);
                        }

                        break;
                }
            }
        }
    }
    
    public class DamageResult
    {
        public CombatEntity caster;
        public CombatEntity target;
        
        public float DamageAmount;
        public int DamageBlockedActively;
        public int DamageBlockedPassively;
        
        public string DamageActionType;
        
        public bool IsActivelyBlocked;
        public bool IsDamageOverTime;
    }

    public static DamageResult DamageCalculation(CombatEntity casterEntity, CombatEntity targetEntity, RPGAbility.RPGAbilityRankData abilityRank, RPGEffect effect, RPGEffect.RPGEffectRankData effectRank, bool isDamageOverTime)
    {
        DamageResult result = new DamageResult {caster = casterEntity, target =  targetEntity, DamageAmount = GetInitialDamageValue(effectRank, 0, targetEntity), IsDamageOverTime = isDamageOverTime, DamageActionType = "NO_DAMAGE_TYPE"};

        if (!effectRank.FlatCalculation)
        {
            result.DamageAmount = AddBaseDamage(casterEntity, effectRank, result.DamageAmount);
            result.DamageActionType = effectRank.mainDamageType.ToString();
        }
        if (effectRank.weaponDamageModifier > 0) result.DamageAmount = AddWeaponBonus(casterEntity, effectRank, result.DamageAmount);
        if (effectRank.damageStatID != -1 && effectRank.damageStatModifier > 0) result.DamageAmount = AddStatBonusBonus(casterEntity, effectRank, result.DamageAmount);
        if (effectRank.requiredEffectID != -1) result.DamageAmount = AddRequiredEffectBonus(targetEntity, effectRank, result.DamageAmount);
        if (effectRank.skillModifierID != -1 && effectRank.skillModifier > 0) result.DamageAmount += RPGBuilderUtilities.getSkillLevel(effectRank.skillModifierID) * effectRank.skillModifier;
        SecondaryDamageResult secondaryDamageResult = GetSecondaryTypeBonus(casterEntity, targetEntity, effectRank);
        if (secondaryDamageResult != null)
        {
            result.IsActivelyBlocked = secondaryDamageResult.isActiveBlocked;
            result.DamageBlockedActively += secondaryDamageResult.damageBlocked;
            result.DamageAmount += secondaryDamageResult.damage;
        }

        if (effect.effectType == RPGEffect.EFFECT_TYPE.DamageOverTime) result.DamageAmount = AddDamageOverTimeBonus(casterEntity, result.DamageAmount);

        float extraDamage = 0;
        if (effectRank.maxHealthModifier > 0) extraDamage = AddMaxHealthBonus(casterEntity, effectRank, extraDamage);
        if (effectRank.missingHealthModifier > 0) extraDamage = AddMissingHealthBonus(casterEntity, effectRank, result.DamageAmount, extraDamage);
        
        result.DamageAmount += extraDamage;

        float CriticalHitChance = effectRank.FlatCalculation ? 0 : GetCriticalHitChance(casterEntity);
        float DamageDealtModifier = effectRank.FlatCalculation ? 0 : GetStatValue(casterEntity, RPGStat.STAT_TYPE.DMG_DEALT, RPGStat.STAT_TYPE.MINION_DAMAGE);
        float DamageTakenModifier = effectRank.FlatCalculation ? 0 : CombatUtilities.GetTotalOfStatType(targetEntity, RPGStat.STAT_TYPE.DMG_TAKEN);
        float CriticalHitPower = effectRank.FlatCalculation ? 0 : GetStatValue(casterEntity, RPGStat.STAT_TYPE.CRIT_POWER, RPGStat.STAT_TYPE.MINION_CRIT_POWER);
        if (!effectRank.CannotCrit && IsDamageCrit(CriticalHitChance))
        {
            result.DamageAmount = HandleCriticalDamage(result.DamageAmount, CriticalHitPower);
            result.DamageActionType += "_CRITICAL";
        }
        if (DamageDealtModifier != 0 || DamageTakenModifier != 0) result.DamageAmount = HandleDamageModifiers(result.DamageAmount, DamageDealtModifier, DamageTakenModifier);
        if (result.DamageAmount > 0) result.DamageAmount = HandleResistances(casterEntity, targetEntity, effectRank, result.DamageAmount);

        HandlePassiveStats(casterEntity, targetEntity, effectRank, result);

        if(result.DamageAmount > 0) HandleStatEffects(casterEntity, targetEntity, abilityRank, effectRank, result);

        if (result.IsActivelyBlocked)
        {
            result.DamageActionType = "BLOCKED";
            CombatEvents.Instance.OnDamageBlocked(result);
        }

        result.DamageAmount = Mathf.RoundToInt(result.DamageAmount);
        return result;
    }
    
    public class HealResult
    {
        public CombatEntity caster;
        public CombatEntity target;
        
        public float HealAmount;
        public string HealActionType;
        
        public bool IsHealOverTime;
    }

    public static HealResult HealingCalculation(CombatEntity casterEntity, CombatEntity targetEntity, RPGEffect effect, RPGEffect.RPGEffectRankData effectRank, bool isHealOverTime)
    {
        HealResult result = new HealResult
        {
            caster = casterEntity, target = targetEntity,
            HealAmount = GetInitialDamageValue(effectRank, 0, targetEntity), IsHealOverTime = isHealOverTime,
            HealActionType = "HEAL",
        };

        if (!effectRank.FlatCalculation)
        {
            result.HealAmount = AddBaseHealing(casterEntity, effectRank, result.HealAmount);
        }
        if (effectRank.weaponDamageModifier > 0) result.HealAmount = AddWeaponBonus(casterEntity, effectRank, result.HealAmount);
        if (effectRank.damageStatID != -1 && effectRank.damageStatModifier > 0) result.HealAmount = AddStatBonusBonus(casterEntity, effectRank, result.HealAmount);
        if (effectRank.requiredEffectID != -1) result.HealAmount = AddRequiredEffectBonus(targetEntity, effectRank, result.HealAmount);
        if (effectRank.skillModifierID != -1 && effectRank.skillModifier > 0) result.HealAmount += RPGBuilderUtilities.getSkillLevel(effectRank.skillModifierID) * effectRank.skillModifier;
        SecondaryDamageResult secondaryDamageResult = GetSecondaryTypeBonus(casterEntity, targetEntity, effectRank);
        if (secondaryDamageResult != null) result.HealAmount += secondaryDamageResult.damage;

        if (effect.effectType == RPGEffect.EFFECT_TYPE.HealOverTime) result.HealAmount = AddHealOverTimeBonus(casterEntity, result.HealAmount);

        float extraHeal = 0;
        if (effectRank.maxHealthModifier > 0) extraHeal = AddMaxHealthBonus(casterEntity, effectRank, extraHeal);
        if (effectRank.missingHealthModifier > 0) extraHeal = AddMissingHealthBonus(casterEntity, effectRank, result.HealAmount, extraHeal);
        
        result.HealAmount += extraHeal;

        float CriticalHitChance = effectRank.FlatCalculation ? 0 : GetCriticalHitChance(casterEntity);
        float HealingDoneModifier = effectRank.FlatCalculation ? 0 : GetStatValue(casterEntity, RPGStat.STAT_TYPE.HEAL_DONE, RPGStat.STAT_TYPE.NONE);
        float HealingReceivedModifier = effectRank.FlatCalculation ? 0 : CombatUtilities.GetTotalOfStatType(targetEntity, RPGStat.STAT_TYPE.HEAL_RECEIVED);
        float CriticalHitPower = effectRank.FlatCalculation ? 0 : GetStatValue(casterEntity, RPGStat.STAT_TYPE.CRIT_POWER, RPGStat.STAT_TYPE.MINION_CRIT_POWER);
        if (!effectRank.CannotCrit && IsDamageCrit(CriticalHitChance))
        {
            result.HealAmount = HandleCriticalDamage(result.HealAmount, CriticalHitPower);
            result.HealActionType += "_CRITICAL";
        }
        if (HealingDoneModifier != 0 || HealingReceivedModifier != 0) result.HealAmount = HandleDamageModifiers(result.HealAmount, HealingDoneModifier, HealingReceivedModifier);

        if (result.HealAmount > 0) result.HealAmount = HandleAbsorption(targetEntity, effectRank, result.HealAmount);

        result.HealAmount = Mathf.RoundToInt(result.HealAmount);
        return result;
    }
}
