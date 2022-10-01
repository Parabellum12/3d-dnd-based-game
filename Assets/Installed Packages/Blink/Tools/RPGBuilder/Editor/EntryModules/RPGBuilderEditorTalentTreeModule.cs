using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.Templates;
using UnityEditor;
using UnityEngine;

public class RPGBuilderEditorTalentTreeModule : RPGBuilderEditorModule
{
    private Dictionary<int, RPGTalentTree> entries = new Dictionary<int, RPGTalentTree>();
    private RPGTalentTree currentEntry;
    
    public override void Initialize()
    {
        LoadEntries();
        if (entries.Count != 0)
        {
            currentEntry = Instantiate(entries[RPGBuilderEditor.Instance.CurrentEntryIndex]);
            RPGBuilderEditor.Instance.CurrentEntry = currentEntry;
        }
        else
        {
            CreateNewEntry();
        }
        RPGBuilderEditor.Instance.InitializeFilters(RPGBuilderEditor.Instance.EditorFilters.talentTreeFilters);
    }

    public override void InstantiateCurrentEntry(int index)
    {
        if (entries.Count == 0) return;
        currentEntry = Instantiate(entries[index]);
        RPGBuilderEditor.Instance.CurrentEntry = currentEntry;
    }

    public override void LoadEntries()
    {
        Dictionary<int, RPGTalentTree> dictionary = new Dictionary<int, RPGTalentTree>();
        databaseEntries.Clear();
        var allEntries = Resources.LoadAll<RPGTalentTree>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
        for (var index = 0; index < allEntries.Length; index++)
        {
            var entry = allEntries[index];
            dictionary.Add(index, entry);
            databaseEntries.Add(entry);
        }

        entries = dictionary;
    }

    public override void CreateNewEntry()
    {
        if (EditorApplication.isCompiling)
        { 
            Debug.LogError("You cannot interact with the RPG Builder while the editor is compiling");
            return;
        }
        
        currentEntry = CreateInstance<RPGTalentTree>();
        RPGBuilderEditor.Instance.CurrentEntry = currentEntry;
        RPGBuilderEditor.Instance.CurrentEntryIndex = -1;
    }

   

    public override bool SaveConditionsMet()
    {
        if (string.IsNullOrEmpty(currentEntry.entryName))
        {
            RPGBuilderEditorUtility.DisplayDialogueWindow("Invalid Name", "Enter a valid name", "OK");
            return false;
        }
        if (ContainsInvalidCharacters(currentEntry.entryName))
        {
            RPGBuilderEditorUtility.DisplayDialogueWindow("Invalid Characters", "The Name contains invalid characters", "OK");
            return false;
        }
        
        return true;
    }
    
    public override void UpdateEntryData(RPGBuilderDatabaseEntry updatedEntry)
    {
        RPGTalentTree entryFile = (RPGTalentTree)updatedEntry;
        entryFile.UpdateEntryData(currentEntry);
    }
    
    public override void ClearEntries()
    {
        databaseEntries.Clear();
        entries.Clear();
        currentEntry = null;
    }
    
    private int getRealNodeIndex(int tempAbIndex)
    {
        for (var i = 0; i < currentEntry.nodeList.Count; i++)
            if (tempNodeLIst[tempAbIndex] == currentEntry.nodeList[i])
                return i;
        return -1;
    }
    
    private List<RPGTalentTree.Node_DATA> tempNodeLIst = new List<RPGTalentTree.Node_DATA>();


    private void HandleTalentTreesTempNodeList()
    {
        tempNodeLIst.Clear();
        if (!RPGBuilderEditor.Instance.EditorFilters.talentTreeModuleSection.showNodes)
        {
            return;
        }

        var tempSearchText2 = "";
        if (!string.IsNullOrEmpty(searchText))
            tempSearchText2 = searchText.ToLower();

        foreach (var t in currentEntry.nodeList)
        {
            var nodeIsNull = false;
            var nodeName2 = "";

            switch (t.nodeType)
            {
                case RPGTalentTree.TalentTreeNodeType.ability:
                    nodeIsNull = t.abilityID == -1;
                    nodeName2 = "";
                    var ability = RPGBuilderEditorUtility.GetEntryByID(t.abilityID, "Ability");
                    if (!nodeIsNull && ability != null)
                    {
                        nodeName2 = ability.entryName;
                        nodeName2 = nodeName2.ToLower();
                    }

                    break;
                case RPGTalentTree.TalentTreeNodeType.recipe:
                    nodeIsNull = t.recipeID == -1;
                    nodeName2 = "";
                    var recipe = RPGBuilderEditorUtility.GetEntryByID(t.recipeID, "Recipe");
                    if (!nodeIsNull && recipe != null)
                    {
                        nodeName2 = recipe.entryName;
                        nodeName2 = nodeName2.ToLower();
                    }

                    break;
                case RPGTalentTree.TalentTreeNodeType.resourceNode:
                    nodeIsNull = t.resourceNodeID == -1;
                    nodeName2 = "";
                    var resource = RPGBuilderEditorUtility.GetEntryByID(t.resourceNodeID, "Resource");
                    if (!nodeIsNull && resource != null)
                    {
                        nodeName2 = resource.entryName;
                        nodeName2 = nodeName2.ToLower();
                    }

                    break;
                case RPGTalentTree.TalentTreeNodeType.bonus:
                    nodeIsNull = t.bonusID == -1;
                    nodeName2 = "";
                    var bonusREF = RPGBuilderEditorUtility.GetEntryByID(t.bonusID, "Bonus");
                    if (!nodeIsNull && bonusREF != null)
                    {
                        nodeName2 = bonusREF.entryName;
                        nodeName2 = nodeName2.ToLower();
                    }

                    break;
            }

            if (string.IsNullOrEmpty(searchText) || nodeName2.Contains(tempSearchText2))
            {
                tempNodeLIst.Add(t);
            }
        }
    }
    
    public override void DrawView()
    {
        if (currentEntry == null)
        {
            if (entries.Count > 0 && entries[0] != null)
            {
                RPGBuilderEditor.Instance.SelectDatabaseEntry(0, true);
            }
            else
            {
                CreateNewEntry();
            }
        }
        
        RPGBuilderEditorUtility.UpdateViewAndFieldData();

        float topSpace = RPGBuilderEditor.Instance.ButtonHeight + 5;
        GUILayout.Space(topSpace);
        
        RPGBuilderEditor.Instance.ViewScroll = EditorGUILayout.BeginScrollView(RPGBuilderEditor.Instance.ViewScroll,
            false, false,
            GUILayout.Width(RPGBuilderEditor.Instance.ViewWidth),
            GUILayout.MaxWidth(RPGBuilderEditor.Instance.ViewWidth),
            
            GUILayout.ExpandHeight(true));
        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.talentTreeModuleSection.showBaseInfo =
            RPGBuilderEditorUtility.HandleModuleBanner("BASE INFO", RPGBuilderEditor.Instance.EditorFilters.talentTreeModuleSection.showBaseInfo);
        if (RPGBuilderEditor.Instance.EditorFilters.talentTreeModuleSection.showBaseInfo)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, false);
            currentEntry.entryIcon = RPGBuilderEditorFields.DrawIcon(currentEntry.entryIcon, 100, 100);
            GUILayout.BeginVertical();
            RPGBuilderEditorFields.DrawID( currentEntry.ID);
            currentEntry.entryName =
                RPGBuilderEditorFields.DrawHorizontalTextField("Name", "", RPGBuilderEditor.Instance.FieldHeight, currentEntry.entryName);
            currentEntry.entryDisplayName = RPGBuilderEditorFields.DrawHorizontalTextField("Display Name", "", RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.entryDisplayName);
            currentEntry.entryFileName = RPGBuilderEditorFields.DrawFileNameField("File Name", "", RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.entryName + AssetNameSuffix);
            
            currentEntry.treePointAcceptedID = RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.treePointAcceptedID, "Point", "Point Type", "");
            
            currentEntry.TiersAmount =
                RPGBuilderEditorFields.DrawHorizontalIntField("Tier Amount", "",
                    RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.TiersAmount);
            GUILayout.EndVertical();
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, false);
        }

        HandleTalentTreesTempNodeList();

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.talentTreeModuleSection.showNodes =
            RPGBuilderEditorUtility.HandleModuleBanner("NODES", RPGBuilderEditor.Instance.EditorFilters.talentTreeModuleSection.showNodes);
        if (RPGBuilderEditor.Instance.EditorFilters.talentTreeModuleSection.showNodes)
        {
            ScriptableObject scriptableObj = currentEntry;
            var serialObj = new SerializedObject(scriptableObj);
            var ThisList = serialObj.FindProperty("nodeList");

            GUILayout.Space(10);
            if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Node", true))
            {
                currentEntry.nodeList.Add(new RPGTalentTree.Node_DATA());
            }
            GUILayout.Space(10);

            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.CenteredMargin - 50, false);
            searchText = GUILayout.TextArea(searchText, RPGBuilderEditor.Instance.EditorSkin.GetStyle("ModuleSearchText"), 
                GUILayout.MinWidth(150), GUILayout.ExpandWidth(true), GUILayout.Height(30));
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.CenteredMargin - 50, false);
            GUILayout.Space(10);

            currentEntry.nodeList = RPGBuilderEditor.Instance.GetTargetObjectOfProperty(ThisList) as List<RPGTalentTree.Node_DATA>;

            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            for (var i = 0; i < tempNodeLIst.Count; i++)
            {
                var realAbTreeIndex = getRealNodeIndex(i);
                switch (currentEntry.nodeList[realAbTreeIndex].nodeType)
                {
                    case RPGTalentTree.TalentTreeNodeType.ability:
                    {
                        if (RPGBuilderEditorFields.DrawHorizontalEntryRemoveButton(
                            currentEntry.nodeList[realAbTreeIndex].abilityID,
                            "Ability"))
                        {
                            currentEntry.nodeList.RemoveAt(i);
                            return;
                        }
                        break;
                    }
                    case RPGTalentTree.TalentTreeNodeType.bonus:
                    {
                        if (RPGBuilderEditorFields.DrawHorizontalEntryRemoveButton(
                            currentEntry.nodeList[realAbTreeIndex].bonusID,
                            "Bonus"))
                        {
                            currentEntry.nodeList.RemoveAt(i);
                            return;
                        }
                        break;
                    }
                    case RPGTalentTree.TalentTreeNodeType.recipe:
                    {
                        if (RPGBuilderEditorFields.DrawHorizontalEntryRemoveButton(
                            currentEntry.nodeList[realAbTreeIndex].recipeID,
                            "Recipe"))
                        {
                            currentEntry.nodeList.RemoveAt(i);
                            return;
                        }
                        break;
                    }
                    case RPGTalentTree.TalentTreeNodeType.resourceNode:
                    {
                        if (RPGBuilderEditorFields.DrawHorizontalEntryRemoveButton(
                            currentEntry.nodeList[realAbTreeIndex].resourceNodeID,
                            "Resource"))
                        {
                            currentEntry.nodeList.RemoveAt(i);
                            return;
                        }
                        break;
                    }
                }
                
                switch (currentEntry.nodeList[realAbTreeIndex].nodeType)
                {
                    case RPGTalentTree.TalentTreeNodeType.ability:
                        currentEntry.nodeList[realAbTreeIndex].abilityID = 
                            RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.nodeList[realAbTreeIndex].abilityID, "Ability", "Ability", "");
                        break;
                    case RPGTalentTree.TalentTreeNodeType.bonus:
                    {
                        currentEntry.nodeList[realAbTreeIndex].bonusID = 
                            RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.nodeList[realAbTreeIndex].bonusID, "Bonus", "Bonus", "");
                        break;
                    }
                    case RPGTalentTree.TalentTreeNodeType.recipe:
                    {
                        currentEntry.nodeList[realAbTreeIndex].recipeID = 
                            RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.nodeList[realAbTreeIndex].recipeID, "Recipe", "Recipe", "");
                        break;
                    }
                    case RPGTalentTree.TalentTreeNodeType.resourceNode:
                    {
                        currentEntry.nodeList[realAbTreeIndex].resourceNodeID = 
                            RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.nodeList[realAbTreeIndex].resourceNodeID, "Resource", "Resource", "");
                        break;
                    }
                }
                
                currentEntry.nodeList[realAbTreeIndex].nodeType =
                    (RPGTalentTree.TalentTreeNodeType) RPGBuilderEditorFields.DrawHorizontalEnum("Type", "What type of node is it?",
                        (int)currentEntry.nodeList[realAbTreeIndex].nodeType,
                        Enum.GetNames(typeof(RPGTalentTree.TalentTreeNodeType)));
                
                RPGBuilderEditorFields.DrawHorizontalLabel("Tier", "What tier is this ability part of?");
                currentEntry.nodeList[realAbTreeIndex].Tier =
                    RPGBuilderEditorFields.DrawHorizontalIntField("", "", 0, currentEntry.nodeList[realAbTreeIndex].Tier);
                EditorGUILayout.EndHorizontal();
                RPGBuilderEditorFields.DrawHorizontalLabel("Slot", "The slot of the ability in its tier. FROM 1 to 7");
                currentEntry.nodeList[realAbTreeIndex].Row =
                    RPGBuilderEditorFields.DrawHorizontalIntField("", "", 0, currentEntry.nodeList[realAbTreeIndex].Row);
                EditorGUILayout.EndHorizontal();
                currentEntry.nodeList[realAbTreeIndex].RequirementsTemplate = (RequirementsTemplate)
                    RPGBuilderEditorFields.DrawHorizontalObject<RequirementsTemplate>("Requirements", "", currentEntry.nodeList[realAbTreeIndex].RequirementsTemplate);

                GUILayout.Space(15);
            }
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);

            if (tempNodeLIst.Count > 3)
            {
                GUILayout.Space(10);
                if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Node", true))
                {
                    currentEntry.nodeList.Add(new RPGTalentTree.Node_DATA());
                }
            }

            serialObj.ApplyModifiedProperties();
        }

        GUILayout.Space(25);
        GUILayout.EndScrollView();
    
    }
    
    public override void ConvertDatabaseEntriesAfterUpdate ()
    {
        var allEntries = Resources.LoadAll<RPGTalentTree>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
        foreach (var entry in allEntries)
        {
            EditorUtility.SetDirty(entry);
             entry.entryName = entry._name;
             AssetDatabase.RenameAsset(RPGBuilderEditor.Instance.EditorData.ResourcePath + 
             RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName + "/" + entry._fileName + ".asset", entry.entryName + AssetNameSuffix);
             entry.entryFileName = entry.entryName + AssetNameSuffix;
            entry.entryDisplayName = entry.displayName;
            entry.entryIcon = entry.icon;
            EditorUtility.SetDirty(entry);
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
