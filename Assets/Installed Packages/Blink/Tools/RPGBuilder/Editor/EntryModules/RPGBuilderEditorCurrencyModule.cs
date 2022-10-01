using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class RPGBuilderEditorCurrencyModule : RPGBuilderEditorModule
{
    private Dictionary<int, RPGCurrency> entries = new Dictionary<int, RPGCurrency>();
    private RPGCurrency currentEntry;

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

        RPGBuilderEditor.Instance.InitializeFilters(RPGBuilderEditor.Instance.EditorFilters.currencyFilters);
    }

    public override void InstantiateCurrentEntry(int index)
    {
        if (entries.Count == 0) return;
        currentEntry = Instantiate(entries[index]);
        RPGBuilderEditor.Instance.CurrentEntry = currentEntry;
    }

    public override void LoadEntries()
    {
        Dictionary<int, RPGCurrency> dictionary = new Dictionary<int, RPGCurrency>();
        databaseEntries.Clear();
        var allEntries =
            Resources.LoadAll<RPGCurrency>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
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

        currentEntry = CreateInstance<RPGCurrency>();
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
        RPGCurrency entryFile = (RPGCurrency) updatedEntry;
        entryFile.UpdateEntryData(currentEntry);
    }

    public override void ClearEntries()
    {
        databaseEntries.Clear();
        entries.Clear();
        currentEntry = null;
    }

    public override void DrawView()
    {
        if (currentEntry == null)
        {
            if (entries.Count > 0 && entries[0] != null)
            {
                RPGBuilderEditor.Instance.SelectDatabaseEntry(0, true);
                currentEntry = Instantiate(entries[RPGBuilderEditor.Instance.CurrentEntryIndex]);
            }
            else
            {
                CreateNewEntry();
            }
        }

        RPGBuilderEditorUtility.UpdateViewAndFieldData();

        ScriptableObject scriptableObj = currentEntry;
        var serialObj = new SerializedObject(scriptableObj);

        float topSpace = RPGBuilderEditor.Instance.ButtonHeight + 5;
        GUILayout.Space(topSpace);
        
        RPGBuilderEditor.Instance.ViewScroll = EditorGUILayout.BeginScrollView(RPGBuilderEditor.Instance.ViewScroll,
            false, false,
            GUILayout.Width(RPGBuilderEditor.Instance.ViewWidth),
            GUILayout.MaxWidth(RPGBuilderEditor.Instance.ViewWidth),
            GUILayout.ExpandHeight(true));

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.currencyModuleSection.showBaseInfo =
            RPGBuilderEditorUtility.HandleModuleBanner("BASE INFO",
                RPGBuilderEditor.Instance.EditorFilters.currencyModuleSection.showBaseInfo);
        if (RPGBuilderEditor.Instance.EditorFilters.currencyModuleSection.showBaseInfo)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, false);
            currentEntry.entryIcon =
                RPGBuilderEditorFields.DrawIcon(currentEntry.entryIcon, 100, 100);
            GUILayout.BeginVertical();
            RPGBuilderEditorFields.DrawID(currentEntry.ID);
            currentEntry.entryName =
                RPGBuilderEditorFields.DrawHorizontalTextField("Name", "", RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.entryName);
            currentEntry.entryDisplayName = RPGBuilderEditorFields.DrawHorizontalTextField(
                "Display Name", "",
                RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.entryDisplayName);
            currentEntry.entryFileName = RPGBuilderEditorFields.DrawFileNameField(
                "File Name", "",
                RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.entryName + AssetNameSuffix);
            currentEntry.entryDescription =
                RPGBuilderEditorFields.DrawHorizontalDescriptionField("Description",
                    "", RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.entryDescription);
            GUILayout.EndVertical();
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, false);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.currencyModuleSection.showSetupSettings =
            RPGBuilderEditorUtility.HandleModuleBanner("SETUP SETTINGS",
                RPGBuilderEditor.Instance.EditorFilters.currencyModuleSection.showSetupSettings);
        if (RPGBuilderEditor.Instance.EditorFilters.currencyModuleSection.showSetupSettings)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            currentEntry.minValue = RPGBuilderEditorFields.DrawHorizontalIntField(
                "Min.", "",
                RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.minValue);
            currentEntry.maxValue = RPGBuilderEditorFields.DrawHorizontalIntField(
                "Max.", "",
                RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.maxValue);

            if (currentEntry.maxValue <
                currentEntry.minValue)
                currentEntry.maxValue =
                    currentEntry.minValue;
            if (currentEntry.minValue >
                currentEntry.maxValue)
                currentEntry.maxValue =
                    currentEntry.minValue;

            currentEntry.baseValue = RPGBuilderEditorFields.DrawHorizontalIntField(
                "Starts At", "",
                RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.baseValue);
            if (currentEntry.baseValue >=
                currentEntry.maxValue &&
                currentEntry.maxValue > 0)
                currentEntry.baseValue =
                    currentEntry.maxValue - 1;

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            GUILayout.Space(10);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.currencyModuleSection.showConversion =
            RPGBuilderEditorUtility.HandleModuleBanner("CONVERSION SETTINGS",
                RPGBuilderEditor.Instance.EditorFilters.currencyModuleSection.showConversion);
        if (RPGBuilderEditor.Instance.EditorFilters.currencyModuleSection.showConversion)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            currentEntry.AmountToConvert =
                RPGBuilderEditorFields.DrawHorizontalIntField(
                    "Amt. For Conversion", "",
                    RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.AmountToConvert);
            if (currentEntry.AmountToConvert >=
                currentEntry.maxValue)
                currentEntry.AmountToConvert =
                    currentEntry.maxValue;

            currentEntry.convertToCurrencyID =
                RPGBuilderEditorFields.DrawDatabaseEntryField(
                    currentEntry.convertToCurrencyID, "Currency",
                    "Convert to", "");
            currentEntry.lowestCurrencyID =
                RPGBuilderEditorFields.DrawDatabaseEntryField(
                    currentEntry.lowestCurrencyID, "Currency",
                    "Lowest Currency", "");

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            GUILayout.Space(10);
        }

        if (currentEntry.lowestCurrencyID != -1 &&
            currentEntry.lowestCurrencyID ==
            currentEntry.ID)
        {
            GUILayout.Space(10);
            RPGBuilderEditor.Instance.EditorFilters.currencyModuleSection.showSuperiorCurrencies =
                RPGBuilderEditorUtility.HandleModuleBanner("SUPERIOR CURRENCIES",
                    RPGBuilderEditor.Instance.EditorFilters.currencyModuleSection.showSuperiorCurrencies);
            if (RPGBuilderEditor.Instance.EditorFilters.currencyModuleSection.showSuperiorCurrencies)
            {
                GUILayout.Space(10);
                if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Currency", true))
                {
                    currentEntry.aboveCurrencies.Add(
                        new RPGCurrency.AboveCurrencyDATA());
                }

                var ThisList2 = serialObj.FindProperty("aboveCurrencies");
                currentEntry.aboveCurrencies =
                    RPGBuilderEditor.Instance.GetTargetObjectOfProperty(ThisList2) as
                        List<RPGCurrency.AboveCurrencyDATA>;

                RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
                for (var a = 0; a < currentEntry.aboveCurrencies.Count; a++)
                {
                    GUILayout.Space(10);
                    if (RPGBuilderEditorFields.DrawHorizontalEntryRemoveButton(
                        currentEntry.aboveCurrencies[a].currencyID,
                        "Currency"))
                    {
                        currentEntry.aboveCurrencies.RemoveAt(a);
                        return;
                    }
                    
                    currentEntry.aboveCurrencies[a].currencyID =
                        RPGBuilderEditorFields.DrawDatabaseEntryField(
                            currentEntry.aboveCurrencies[a].currencyID, "Currency",
                            "Currency", "");
                }

                RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            }
        }

        serialObj.ApplyModifiedProperties();

        GUILayout.Space(25);
        GUILayout.EndScrollView();
    }

    public override void ConvertDatabaseEntriesAfterUpdate()
    {
        var allEntries =
            Resources.LoadAll<RPGCurrency>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
        foreach (var entry in allEntries)
        {
            EditorUtility.SetDirty(entry);
            entry.entryName = entry._name;
            AssetDatabase.RenameAsset(RPGBuilderEditor.Instance.EditorData.ResourcePath +
                                      RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName + "/" +
                                      entry._fileName + ".asset", entry.entryName + AssetNameSuffix);
            entry.entryFileName = entry.entryName + AssetNameSuffix;
            entry.entryDisplayName = entry.displayName;
            entry.entryIcon = entry.icon;
            entry.entryDescription = entry.description;
            EditorUtility.SetDirty(entry);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
