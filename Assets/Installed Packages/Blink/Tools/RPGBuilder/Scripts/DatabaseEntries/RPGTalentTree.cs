using System;
using System.Collections.Generic;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.Templates;
using UnityEngine;

public class RPGTalentTree : RPGBuilderDatabaseEntry
{
    [HideInInspector] public string _name;
    [HideInInspector] public string displayName;
    [HideInInspector] public string _fileName;
    [HideInInspector] public Sprite icon;

    
    public int TiersAmount;
    [PointID] public int treePointAcceptedID = -1;

    public enum TalentTreeNodeType
    {
        ability,
        recipe,
        resourceNode,
        bonus
    }

    [Serializable]
    public class Node_DATA
    {
        public TalentTreeNodeType nodeType;
        public string nodeName;
        [AbilityID] public int abilityID = -1;
        [RecipeID] public int recipeID = -1;
        public int resourceNodeID = -1;
        public int bonusID = -1;
        public int Tier;
        public int Row;

        public RequirementsTemplate RequirementsTemplate;
    }

    [RPGDataList] public List<Node_DATA> nodeList = new List<Node_DATA>();


    public void UpdateEntryData(RPGTalentTree newEntryData)
    {
        ID = newEntryData.ID;
        entryName = newEntryData.entryName;
        entryFileName = newEntryData.entryFileName;
        entryDisplayName = newEntryData.entryDisplayName;
        entryIcon = newEntryData.entryIcon;
        entryDescription = newEntryData.entryDescription;

        nodeList = newEntryData.nodeList;
        treePointAcceptedID = newEntryData.treePointAcceptedID;
        TiersAmount = newEntryData.TiersAmount;
    }
}