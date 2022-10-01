using System;
using System.Collections.Generic;
using UnityEngine;

public class RPGMerchantTable : RPGBuilderDatabaseEntry
{
    [HideInInspector] public string _name;
    [HideInInspector] public string _fileName;
    [HideInInspector] public string displayName;

    [Serializable]
    public class ON_SALE_ITEMS_DATA
    {
        [ItemID] public int itemID = -1;
        [CurrencyID] public int currencyID = -1;
        public int cost;
    }

    [RPGDataList] public List<ON_SALE_ITEMS_DATA> onSaleItems = new List<ON_SALE_ITEMS_DATA>();

    public void UpdateEntryData(RPGMerchantTable newEntryData)
    {
        ID = newEntryData.ID;
        entryName = newEntryData.entryName;
        entryFileName = newEntryData.entryFileName;
        entryDisplayName = newEntryData.entryDisplayName;
        entryIcon = newEntryData.entryIcon;
        entryDescription = newEntryData.entryDescription;
        
        onSaleItems = newEntryData.onSaleItems;
    }
}