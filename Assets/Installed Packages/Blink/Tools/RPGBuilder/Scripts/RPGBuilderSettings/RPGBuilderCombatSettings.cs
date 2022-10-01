using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.AI;
using BLINK.RPGBuilder.Templates;
using UnityEngine;

public class RPGBuilderCombatSettings : RPGBuilderDatabaseEntry
{
    public List<string> FactionStancesList = new List<string>();
    public List<string> AbilityCooldownTagList = new List<string>();
    public List<string> EffectTagList = new List<string>();
    
    public float CriticalHitBonus = 2;
    public float GlobalCooldownDuration = 0.75f;
    
    public float ResetCombatDuration = 15;

    public bool AutomaticCombatStates = true;
    
    [StatID] public int HealthStatID = -1;

    public AIBehaviorTemplate DefaultAIBehaviorTemplate;
    public GameObject DefaultAILogicTemplate;

    public LayerMask ProjectileRaycastLayers;
    public LayerMask ProjectileDestroyLayers;
    public LayerMask InterruptLeapLayers;

    public float NPCSpawnerDistanceCheckInterval = 5;
    
    public void UpdateEntryData(RPGBuilderCombatSettings newEntryData)
    {
        CriticalHitBonus = newEntryData.CriticalHitBonus;
        HealthStatID = newEntryData.HealthStatID;
        ResetCombatDuration = newEntryData.ResetCombatDuration;
        AutomaticCombatStates = newEntryData.AutomaticCombatStates;
        FactionStancesList = newEntryData.FactionStancesList;
        GlobalCooldownDuration = newEntryData.GlobalCooldownDuration;
        AbilityCooldownTagList = newEntryData.AbilityCooldownTagList;
        EffectTagList = newEntryData.EffectTagList;
        DefaultAIBehaviorTemplate = newEntryData.DefaultAIBehaviorTemplate;
        DefaultAILogicTemplate = newEntryData.DefaultAILogicTemplate;
        ProjectileRaycastLayers = newEntryData.ProjectileRaycastLayers;
        ProjectileDestroyLayers = newEntryData.ProjectileDestroyLayers;
        InterruptLeapLayers = newEntryData.InterruptLeapLayers;
        NPCSpawnerDistanceCheckInterval = newEntryData.NPCSpawnerDistanceCheckInterval;
    }
}
