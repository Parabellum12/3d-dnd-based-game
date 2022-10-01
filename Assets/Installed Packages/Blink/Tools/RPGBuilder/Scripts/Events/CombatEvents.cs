using System;
using BLINK.RPGBuilder.Combat;
using UnityEngine;

public class CombatEvents : MonoBehaviour
{
    
    public static event Action<CombatEntity> CombatEntered;
    public static event Action<CombatEntity> CombatExited;
    
    // PLAYER
    public static event Action<CombatEntity, CombatEntity> TargetChanged;
    public static event Action PlayerDied;
    public static event Action<CombatEntity> EntityDied;
    public static event Action PlayerRequestedRespawned;
    public static event Action PlayerRespawned;
    public static event Action<RPGNpc> PlayerKilledNPC;
    
    // STATS
    public static event Action<CombatEntity, RPGStat, float, float> StatValueChanged;
    public static event Action<CombatEntity, RPGStat> StatChanged;
    public static event Action<CombatEntity, float> MovementSpeedChanged;
    public static event Action<CombatEntity> StatsChanged;
    
    // STATES
    public static event Action<CombatEntity, int> StateStarted;
    public static event Action<CombatEntity, int> StateEnded;
    public static event Action<CombatEntity, int> StateRefreshed;
    
    // COMBAT ACTIONS
    public static event Action<CombatEntity, RPGAbility, RPGAbility.RPGAbilityRankData, float> StartedCastingAbility;
    public static event Action<CombatEntity> StoppedCastingAbility;
    
    public static event Action<CombatEntity, RPGAbility, RPGAbility.RPGAbilityRankData, float> StartedChannelingAbility;
    public static event Action<CombatEntity> StoppedChannelingAbility;
    
    public static event Action PlayerStartedActiveBlocking;
    public static event Action PlayerActiveBlockedDamage;
    public static event Action PlayerStoppedActiveBlocking;
    
    public static event Action<CombatCalculations.DamageResult> DamageDealt;
    public static event Action<CombatCalculations.DamageResult> DamageBlocked;
    
    public static event Action<CombatCalculations.HealResult> Healed;
    
    // PETS
    public static event Action PlayerSummonedPet;
    public static event Action PlayerUnsummonedPet;
    public static event Action PlayerPetUpdate;
    
    public static event Action PlayerPetsFollow;
    public static event Action PlayerPetsStay;
    public static event Action PlayerPetsAggro;
    public static event Action PlayerPetsDefend;
    public static event Action PlayerPetsReset;
    public static event Action PlayerPetsAttack;
    
    // COMBOS
    public static event Action<int, RPGCombo.ComboEntry, KeyCode> PlayerComboStarted;
    public static event Action<int, KeyCode, bool> PlayerComboUpdated;
    public static event Action<int> PlayerComboRemoved;
    
    // FACTIONS
    public static event Action<RPGFaction, int> PlayerFactionPointChanged;
    public static event Action<RPGFaction> PlayerFactionChanged;
    public static event Action<RPGFaction> PlayerFactionStanceChanged;
    
    
    public static CombatEvents Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null) return;
        Instance = this;
    }
    
    
    public virtual void OnCombatEntered(CombatEntity entity)
    {
        if(entity != null) CombatEntered?.Invoke(entity);
    }
    public virtual void OnCombatExited(CombatEntity entity)
    {
        if(entity != null) CombatExited?.Invoke(entity);
    }
    
    #region PLAYER
    public virtual void OnPlayerTargetChanged(CombatEntity entity, CombatEntity target)
    {
        TargetChanged?.Invoke(entity, target);
    }
    public virtual void OnPlayerDied()
    {
        PlayerDied?.Invoke();
    }
    public virtual void OnEntityDied(CombatEntity entity)
    {
        if(entity != null) EntityDied?.Invoke(entity);
    }
    public virtual void OnPlayerRequestedRespawned()
    {
        PlayerRequestedRespawned?.Invoke();
    }
    public virtual void OnPlayerRespawned()
    {
        PlayerRespawned?.Invoke();
    }
    public virtual void OnPlayerKilledNPC(RPGNpc npc)
    {
        PlayerKilledNPC?.Invoke(npc);
    }
    #endregion

    #region COMBAT ACTIONS
    public virtual void OnDamageDealt(CombatCalculations.DamageResult result)
    {
        if(result != null) DamageDealt?.Invoke(result);
    }
    public virtual void OnDamageBlocked(CombatCalculations.DamageResult result)
    {
        if(result != null) DamageBlocked?.Invoke(result);
    }
    public virtual void OnHealed(CombatCalculations.HealResult result)
    {
        if(result != null) Healed?.Invoke(result);
    }

    #endregion

    #region STATS
    public virtual void OnStatValueChanged(CombatEntity entity, RPGStat stat, float currentValue, float maxValue)
    {
        if (entity != null && stat != null) StatValueChanged?.Invoke(entity, stat, currentValue, maxValue);
    }
    public virtual void OnStatsChanged(CombatEntity entity)
    {
        if (entity != null) StatsChanged?.Invoke(entity);
    }
    public virtual void OnStatChanged(CombatEntity entity, RPGStat stat)
    {
        if (entity != null && stat != null) StatChanged?.Invoke(entity, stat);
    }
        
    public virtual void OnMovementSpeedChanged(CombatEntity entity, float newValue)
    {
        if(entity != null) MovementSpeedChanged?.Invoke(entity, newValue);
    }
    #endregion
    
    #region STATES
    public virtual void OnStateStarted(CombatEntity entity, int stateIndex)
    {
        if(entity != null && stateIndex != -1) StateStarted?.Invoke(entity, stateIndex);
    }
    public virtual void OnStateEnded(CombatEntity entity, int stateIndex)
    {
        if (entity != null && stateIndex != -1) StateEnded?.Invoke(entity, stateIndex);
    }
    public virtual void OnStateRefreshed(CombatEntity entity, int stateIndex)
    {
        if(entity != null && stateIndex != -1) StateRefreshed?.Invoke(entity, stateIndex);
    }
    #endregion
        
    #region CASTING
    public virtual void OnStartedCastingAbility(CombatEntity entity, RPGAbility ability, RPGAbility.RPGAbilityRankData abilityRank, float castDuration)
    {
        if(entity != null && ability != null && abilityRank != null) StartedCastingAbility?.Invoke(entity, ability, abilityRank, castDuration);
    }
    public virtual void OnStoppedCastingAbility(CombatEntity entity)
    {
        if(entity != null) StoppedCastingAbility?.Invoke(entity);
    }
    #endregion

    #region CHANNELING
    public virtual void OnStartedChannelingAbility(CombatEntity entity, RPGAbility ability, RPGAbility.RPGAbilityRankData abilityRank, float castDuration)
    {
        if(entity != null && ability != null && abilityRank != null) StartedChannelingAbility?.Invoke(entity, ability, abilityRank, castDuration);
    }
    public virtual void OnStoppedChannelingAbility(CombatEntity entity)
    {
        if(entity != null) StoppedChannelingAbility?.Invoke(entity);
    }
    #endregion
    
    #region PETS
    public virtual void OnPlayerPetUpdate()
    {
        PlayerPetUpdate?.Invoke();
    }
    public virtual void OnPlayerPetsFollow()
    {
        PlayerPetsFollow?.Invoke();
    }
    public virtual void OnPlayerPetsStay()
    {
        PlayerPetsStay?.Invoke();
    }
    public virtual void OnPlayerPetsAggro()
    {
        PlayerPetsAggro?.Invoke();
    }
    public virtual void OnPlayerPetsDefend()
    {
        PlayerPetsDefend?.Invoke();
    }
    public virtual void OnPlayerPetsReset()
    {
        PlayerPetsReset?.Invoke();
    }
    public virtual void OnPlayerPetsAttack()
    {
        PlayerPetsAttack?.Invoke();
    }
    #endregion

    #region COMBOS
    public virtual void OnPlayerComboStarted(int activeComboIndex, RPGCombo.ComboEntry comboEntry, KeyCode key)
    {
        PlayerComboStarted?.Invoke(activeComboIndex, comboEntry, key);
    }
    public virtual void OnPlayerComboUpdated(int activeComboIndex, KeyCode key, bool useInitialKey)
    {
        PlayerComboUpdated?.Invoke(activeComboIndex, key, useInitialKey);
    }
    public virtual void OnPlayerComboRemoved(int abilityID)
    {
        PlayerComboRemoved?.Invoke(abilityID);
    }
    #endregion

    #region ACTIVE BLOCKING
    public virtual void OnPlayerStartedActiveBlocking()
    {
        PlayerStartedActiveBlocking?.Invoke();
    }
    public virtual void OnPlayerActiveBlockedDamage()
    {
        PlayerActiveBlockedDamage?.Invoke();
    }
    public virtual void OnPlayerStoppedActiveBlocking()
    {
        PlayerStoppedActiveBlocking?.Invoke();
    }
    #endregion
    
    #region FACTIONS
    public virtual void OnPlayerFactionPointChanged(RPGFaction faction, int amount)
    {
        PlayerFactionPointChanged?.Invoke(faction, amount);
    }
    public virtual void OnPlayerFactionStanceChanged(RPGFaction faction)
    {
        PlayerFactionStanceChanged?.Invoke(faction);
    }
    #endregion
}
