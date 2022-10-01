using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Managers;
using UnityEngine;

public class RPGBuilderUISettings : RPGBuilderDatabaseEntry
{
    public List<string> UIStatsCategoriesList = new List<string>();
    
    public GameObject RPGBuilderEssentialsPrefab;
    public GameObject LoadingScreenManagerPrefab;

    public Color PhysicalDamageColor = Color.white,
        MagicalDamageColor = Color.white,
        NeutralDamageColor = Color.white,
        HealingColor = Color.white,
        RequirementMetColor = Color.white,
        RequirementNotMetColor = Color.white,
        PhysicalCriticalDamageColor = Color.white,
        MagicalCriticalDamageColor = Color.white,
        NeutralCriticalDamageColor = Color.white,
        HealingCriticalColor = Color.white,
        SelfDamageColor = Color.white,
        SelfHealColor = Color.white,
        SelfDamageCriticalColor = Color.white,
        SelfHealCriticalColor = Color.white,
        ThornDamageColor = Color.white,
        EXPColor = Color.white,
        LevelUpColor = Color.white,
        FactionColor = Color.white,
        ImmuneColor = Color.white;
    
    public void UpdateEntryData(RPGBuilderUISettings newEntryData)
    {
        UIStatsCategoriesList = newEntryData.UIStatsCategoriesList;
        RPGBuilderEssentialsPrefab = newEntryData.RPGBuilderEssentialsPrefab;
        LoadingScreenManagerPrefab = newEntryData.LoadingScreenManagerPrefab;

        PhysicalDamageColor = newEntryData.PhysicalDamageColor;
        MagicalDamageColor = newEntryData.MagicalDamageColor;
        NeutralDamageColor = newEntryData.NeutralDamageColor;
        HealingColor = newEntryData.HealingColor;
        RequirementMetColor = newEntryData.RequirementMetColor;
        RequirementNotMetColor = newEntryData.RequirementNotMetColor;
        PhysicalCriticalDamageColor = newEntryData.PhysicalCriticalDamageColor;
        MagicalCriticalDamageColor = newEntryData.MagicalCriticalDamageColor;
        NeutralCriticalDamageColor = newEntryData.NeutralCriticalDamageColor;
        HealingCriticalColor = newEntryData.HealingCriticalColor;
        SelfDamageColor = newEntryData.SelfDamageColor;
        SelfHealColor = newEntryData.SelfHealColor;
        SelfDamageCriticalColor = newEntryData.SelfDamageCriticalColor;
        SelfHealCriticalColor = newEntryData.SelfHealCriticalColor;
        ThornDamageColor = newEntryData.ThornDamageColor;
        EXPColor = newEntryData.EXPColor;
        LevelUpColor = newEntryData.LevelUpColor;
        FactionColor = newEntryData.FactionColor;
        ImmuneColor = newEntryData.ImmuneColor;
    }
}
