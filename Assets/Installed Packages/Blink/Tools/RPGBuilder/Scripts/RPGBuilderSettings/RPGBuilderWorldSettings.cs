using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RPGBuilderWorldSettings : RPGBuilderDatabaseEntry
{
    public List<string> dialogueKeywordsList = new List<string>();
    
    public bool useGameModifiers;
    public int negativePointsRequired;
    public bool checkMinNegativeModifier, checkMaxPositiveModifier;
    public int minimumRequiredNegativeGameModifiers;
    public int maximumRequiredPositiveGameModifiers;
    public int baseGameModifierPointsInMenu;
    public int baseGameModifierPointsInWorld;
    
    public LayerMask worldInteractableLayer;

    public bool UseYears;
    public bool UseMonths;
    public bool UseWeeks;
    public bool UseDays;
    public bool UseHours;
    public bool UseMinutes;
    public bool UseSeconds;
    public bool UseSystemTime;
    public int StartingYear = 1000;
    public int StartingMonth = 1;
    public int MonthsPerYear = 12;
    public int StartingWeek = 1;
    public int WeeksPerMonth = 4;
    public int StartingDay = 1;
    public int DaysPerWeek = 7;
    public int StartingHour = 12;
    public int HoursPerDay = 24;
    public int StartingMinute;
    public int MinutesPerHour = 60;
    public int StartingSecond;
    public int SecondsPerMinutes = 60;

    public float GlobalTimeSpeed = 1;
    public float SecondDuration = 60;
    public float HourDuration = 60;
    
    public void UpdateEntryData(RPGBuilderWorldSettings newEntryData)
    {
        dialogueKeywordsList = newEntryData.dialogueKeywordsList;
        useGameModifiers = newEntryData.useGameModifiers;
        negativePointsRequired = newEntryData.negativePointsRequired;
        minimumRequiredNegativeGameModifiers = newEntryData.minimumRequiredNegativeGameModifiers;
        maximumRequiredPositiveGameModifiers = newEntryData.maximumRequiredPositiveGameModifiers;
        baseGameModifierPointsInMenu = newEntryData.baseGameModifierPointsInMenu;
        baseGameModifierPointsInWorld = newEntryData.baseGameModifierPointsInWorld;
        checkMinNegativeModifier = newEntryData.checkMinNegativeModifier;
        checkMaxPositiveModifier = newEntryData.checkMaxPositiveModifier;
        worldInteractableLayer = newEntryData.worldInteractableLayer;
        
        UseYears = newEntryData.UseYears;
        UseMonths = newEntryData.UseMonths;
        UseWeeks = newEntryData.UseWeeks;
        UseDays = newEntryData.UseDays;
        UseHours = newEntryData.UseHours;
        UseMinutes = newEntryData.UseMinutes;
        UseSeconds = newEntryData.UseSeconds;
        UseSystemTime = newEntryData.UseSystemTime;
        StartingYear = newEntryData.StartingYear;
        StartingMonth = newEntryData.StartingMonth;
        StartingWeek = newEntryData.StartingWeek;
        StartingDay = newEntryData.StartingDay;
        StartingHour = newEntryData.StartingHour;
        StartingMinute = newEntryData.StartingMinute;
        StartingSecond = newEntryData.StartingSecond;
        GlobalTimeSpeed = newEntryData.GlobalTimeSpeed;
        SecondDuration = newEntryData.SecondDuration;
        HourDuration = newEntryData.HourDuration;
        
        MonthsPerYear = newEntryData.MonthsPerYear;
        WeeksPerMonth = newEntryData.WeeksPerMonth;
        DaysPerWeek = newEntryData.DaysPerWeek;
        HoursPerDay = newEntryData.HoursPerDay;
        MinutesPerHour = newEntryData.MinutesPerHour;
        SecondsPerMinutes = newEntryData.SecondsPerMinutes;
    }
}
