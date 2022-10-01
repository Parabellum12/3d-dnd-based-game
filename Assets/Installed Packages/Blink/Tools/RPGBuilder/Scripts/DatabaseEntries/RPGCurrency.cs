using System;
using System.Collections.Generic;
using UnityEngine;

public class RPGCurrency : RPGBuilderDatabaseEntry
{
    [HideInInspector] public Sprite icon;
    [HideInInspector] public string _name;
    [HideInInspector] public string _fileName;
    [HideInInspector] public string displayName;
    [HideInInspector] public string description;
    
    
    public int minValue;
    public int maxValue;
    public int baseValue;

    public int AmountToConvert;

    [CurrencyID] public int convertToCurrencyID = -1;

    [CurrencyID] public int lowestCurrencyID = -1;

    [Serializable]
    public class AboveCurrencyDATA
    {
        [CurrencyID] public int currencyID = -1;
    }

    [RPGDataList] public List<AboveCurrencyDATA> aboveCurrencies = new List<AboveCurrencyDATA>();

    public void UpdateEntryData(RPGCurrency newEntryData)
    {
        ID = newEntryData.ID;
        entryName = newEntryData.entryName;
        entryFileName = newEntryData.entryFileName;
        entryDisplayName = newEntryData.entryDisplayName;
        entryIcon = newEntryData.entryIcon;
        entryDescription = newEntryData.entryDescription;
        
        minValue = newEntryData.minValue;
        maxValue = newEntryData.maxValue;
        baseValue = newEntryData.baseValue;
        AmountToConvert = newEntryData.AmountToConvert;
        convertToCurrencyID = newEntryData.convertToCurrencyID;
        lowestCurrencyID = newEntryData.lowestCurrencyID;
        aboveCurrencies = newEntryData.aboveCurrencies;
    }
}