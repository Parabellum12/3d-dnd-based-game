using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RPGBuilderEditorEconomySettingsModule : RPGBuilderEditorModule
{
    private RPGBuilderEconomySettings currentEntry;
    
    private readonly List<RPGBuilderDatabaseEntry> allWeaponTypes = new List<RPGBuilderDatabaseEntry>();

    public override void Initialize()
    {
        LoadEntries();
    }

    public override void InstantiateCurrentEntry(int index)
    {
        LoadEntries();
    }

    public override void LoadEntries()
    {
        currentEntry = Resources.Load<RPGBuilderEconomySettings>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath +
                                                                 AssetFolderName + "/" + EntryType);
        if (currentEntry != null)
        {
            currentEntry = Instantiate(currentEntry);
        }
        else
        {
            AssetDatabase.CreateAsset(CreateInstance<RPGBuilderEconomySettings>(),
                RPGBuilderEditor.Instance.EditorData.ResourcePath +
                RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName + "/" + EntryType + ".asset");
            currentEntry = Resources.Load<RPGBuilderEconomySettings>(
                RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath +
                AssetFolderName + "/" + EntryType);
        }

        databaseEntries.Clear();
        databaseEntries.Add(currentEntry);

        RPGBuilderEditor.Instance.CurrentEntry = currentEntry;
        
        allWeaponTypes.Clear();
        foreach (var typeEntry in Resources.LoadAll<RPGBWeaponType>(RPGBuilderEditor.Instance.EditorSettings.DatabasePath))
        {
            allWeaponTypes.Add(typeEntry);
        }
    }

    public override void CreateNewEntry()
    {
    }

    public override bool SaveConditionsMet()
    {
        return true;
    }

    public override void UpdateEntryData(RPGBuilderDatabaseEntry updatedEntry)
    {
        RPGBuilderEconomySettings entryFile = (RPGBuilderEconomySettings) updatedEntry;
        entryFile.UpdateEntryData(currentEntry);
    }

    public override void ClearEntries()
    {
        databaseEntries.Clear();
        currentEntry = null;
    }

    public override void DrawView()
    {
        RPGBuilderEditorUtility.UpdateViewAndFieldData();

        float topSpace = RPGBuilderEditor.Instance.ButtonHeight + 5;
        GUILayout.Space(topSpace);

        float panelWidth = RPGBuilderEditorUtility.GetScreenWidth();
        panelWidth -= panelWidth * RPGBuilderEditor.Instance.EditorData.CategoryMenuWidthPercent;
        float panelHeight = RPGBuilderEditorUtility.GetScreenHeight();
        Rect panelRect = new Rect(
            RPGBuilderEditorUtility.GetScreenWidth() * RPGBuilderEditor.Instance.EditorData.CategoryMenuWidthPercent, 0,
            panelWidth, panelHeight);

        RPGBuilderEditor.Instance.ViewScroll = EditorGUILayout.BeginScrollView(RPGBuilderEditor.Instance.ViewScroll,
            false, false,
            GUILayout.Width(panelRect.width),
            GUILayout.MaxWidth(panelRect.width),
            GUILayout.ExpandHeight(true));

        RPGBuilderEditor.Instance.EditorFilters.itemSettingsModuleSection.showInventorySettings =
            RPGBuilderEditorUtility.HandleModuleBanner("INVENTORY",
                RPGBuilderEditor.Instance.EditorFilters.itemSettingsModuleSection.showInventorySettings);
        if (RPGBuilderEditor.Instance.EditorFilters.itemSettingsModuleSection.showInventorySettings)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            currentEntry.InventorySlots = RPGBuilderEditorFields.DrawHorizontalIntField("Base Slots", "",
                RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.InventorySlots);
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.itemSettingsModuleSection.showWeaponSettings =
            RPGBuilderEditorUtility.HandleModuleBanner("WEAPON",
                RPGBuilderEditor.Instance.EditorFilters.itemSettingsModuleSection.showWeaponSettings);
        if (RPGBuilderEditor.Instance.EditorFilters.itemSettingsModuleSection.showWeaponSettings)
        {
            GUILayout.Space(10);
            RPGBuilderEditor.Instance.EditorFilters.itemSettingsModuleSection.showWeaponAnimatorOverride =
                RPGBuilderEditorUtility.HandleModuleBanner("WEAPON ANIMATOR OVERRIDE",
                    RPGBuilderEditor.Instance.EditorFilters.itemSettingsModuleSection.showWeaponAnimatorOverride);
            if (RPGBuilderEditor.Instance.EditorFilters.itemSettingsModuleSection.showWeaponAnimatorOverride)
            {
                GUILayout.Space(10);
                if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Animator Override", true))
                {
                    currentEntry.weaponAnimatorOverrides.Add(new RPGItemDATA.WeaponAnimatorOverride());
                }

                for (var a = 0; a < currentEntry.weaponAnimatorOverrides.Count; a++)
                {
                    GUILayout.Space(10);
                    RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
                    GUILayout.BeginHorizontal();
                    string elementName = currentEntry.weaponAnimatorOverrides[a].WeaponType1 != null ?
                        currentEntry.weaponAnimatorOverrides[a].WeaponType1.entryName : "";
                    if (currentEntry.weaponAnimatorOverrides[a].requireWeapon2)
                    {
                        elementName += " + " + (currentEntry.weaponAnimatorOverrides[a].WeaponType2 != null ?
                            currentEntry.weaponAnimatorOverrides[a].WeaponType2.entryName : "");
                    }
                    RPGBuilderEditorFields.DrawTitleLabelExpanded(elementName, "");
                    
                    if (RPGBuilderEditorFields.DrawSmallRemoveButton())
                    {
                        currentEntry.weaponAnimatorOverrides.RemoveAt(a);
                        return;
                    }
                    GUILayout.EndHorizontal();

                    RPGBuilderEditorFields.DrawHorizontalLabel("Weapon Type 1", "");
                    int weaponTypeIndex = EditorGUILayout.Popup(RPGBuilderEditorUtility.GetTypeEntryIndex(allWeaponTypes, currentEntry.weaponAnimatorOverrides[a].WeaponType1),
                        RPGBuilderEditorUtility.GetTypeEntriesAsStringArray(allWeaponTypes.ToArray()));
                    currentEntry.weaponAnimatorOverrides[a].WeaponType1 = (RPGBWeaponType) allWeaponTypes[weaponTypeIndex];
                    EditorGUILayout.EndHorizontal();

                    currentEntry.weaponAnimatorOverrides[a].requireWeapon2 =
                        RPGBuilderEditorFields.DrawHorizontalToggle("Require Second Weapon?",
                            "", RPGBuilderEditor.Instance.FieldHeight, currentEntry.weaponAnimatorOverrides[a].requireWeapon2);
                    if (currentEntry.weaponAnimatorOverrides[a].requireWeapon2)
                    {
                        RPGBuilderEditorFields.DrawHorizontalLabel("Weapon Type 2", "");
                        int weaponTypeIndex2 = EditorGUILayout.Popup(RPGBuilderEditorUtility.GetTypeEntryIndex(allWeaponTypes, currentEntry.weaponAnimatorOverrides[a].WeaponType2),
                            RPGBuilderEditorUtility.GetTypeEntriesAsStringArray(allWeaponTypes.ToArray()));
                        currentEntry.weaponAnimatorOverrides[a].WeaponType2 = (RPGBWeaponType) allWeaponTypes[weaponTypeIndex2];
                        EditorGUILayout.EndHorizontal();
                    }

                    currentEntry.weaponAnimatorOverrides[a].restAnimatorOverride = (RuntimeAnimatorController)
                        RPGBuilderEditorFields.DrawHorizontalObject<RuntimeAnimatorController>("Rest Animator Override", "",
                            currentEntry.weaponAnimatorOverrides[a].restAnimatorOverride);
                    currentEntry.weaponAnimatorOverrides[a].combatAnimatorOverride = (RuntimeAnimatorController)
                        RPGBuilderEditorFields.DrawHorizontalObject<RuntimeAnimatorController>("Combat Animator Override", "",
                            currentEntry.weaponAnimatorOverrides[a].combatAnimatorOverride);

                    RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
                }
            }

        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.itemSettingsModuleSection.showLoot =
            RPGBuilderEditorUtility.HandleModuleBanner("LOOT", RPGBuilderEditor.Instance.EditorFilters.itemSettingsModuleSection.showLoot);
        if (RPGBuilderEditor.Instance.EditorFilters.itemSettingsModuleSection.showLoot)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            currentEntry.LootBagPrefab = (GameObject) RPGBuilderEditorFields.DrawHorizontalObject<GameObject>("Loot Bag prefab", "",
                currentEntry.LootBagPrefab);
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(30);
        GUILayout.EndScrollView();
    }
    
    public override void ConvertDatabaseEntriesAfterUpdate ()
    {
        var entry = Resources.Load<RPGBuilderEconomySettings>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName + "/" + EntryType);
        var oldSettings = Resources.Load<RPGItemDATA>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName + "/" + "ItemSettings");

        if (entry == null)
        {
            Debug.LogError("Could not find " + EntryType);
            return;
        }
        if (oldSettings == null)
        {
            Debug.LogError("Could not find " + "ItemSettings");
            return;
        }
        
        EditorUtility.SetDirty(entry);

        entry.itemRarityList = oldSettings.itemRarityList;
        entry.itemRarityImagesList = oldSettings.itemRarityImagesList;
        entry.itemRarityColorsList = oldSettings.itemRarityColorsList;
        entry.itemTypeList = oldSettings.itemTypeList;
        entry.weaponTypeList = oldSettings.weaponTypeList;
        entry.armorTypeList = oldSettings.armorTypeList;
        entry.armorSlotsList = oldSettings.armorSlotsList;
        entry.weaponSlotsList = oldSettings.weaponSlotsList;
        entry.slotTypeList = oldSettings.slotTypeList;
        entry.socketTypeList = oldSettings.socketTypeList;
        entry.weaponAnimatorOverrides = oldSettings.weaponAnimatorOverrides;
        entry.InventorySlots = oldSettings.InventorySlots;
        entry.LootBagPrefab = oldSettings.LootBagPrefab;
        
        EditorUtility.SetDirty(entry);
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
