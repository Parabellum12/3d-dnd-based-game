using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RPGBuilderCharacterSettings : RPGBuilderDatabaseEntry
{
    public List<string> StatFunctionsList = new List<string>();
    public List<string> nodeSocketNames = new List<string>();

    public bool NoClasses = true;
    public bool CanTargetPlayerOnClick = true;
    
    [PointID] public int StatAllocationPointID = -1;
    public bool MustSpendAllStatPointsToCreateCharacter;
    public bool CanRefundStatPointInGame;

    [StatID] public int SprintStatDrainID = -1;
    public int SprintStatDrainAmount;
    public float SprintStatDrainInterval;
    
    public void UpdateEntryData(RPGBuilderCharacterSettings newEntryData)
    {
        NoClasses = newEntryData.NoClasses;
        StatAllocationPointID = newEntryData.StatAllocationPointID;
        MustSpendAllStatPointsToCreateCharacter = newEntryData.MustSpendAllStatPointsToCreateCharacter;
        CanRefundStatPointInGame = newEntryData.CanRefundStatPointInGame;
        CanTargetPlayerOnClick = newEntryData.CanTargetPlayerOnClick;
        SprintStatDrainID = newEntryData.SprintStatDrainID;
        SprintStatDrainInterval = newEntryData.SprintStatDrainInterval;
        SprintStatDrainAmount = newEntryData.SprintStatDrainAmount;
    }
}
