using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class RPGBuilderEditorEnchantmentModule : RPGBuilderEditorModule
{
    private Dictionary<int, RPGEnchantment> entries = new Dictionary<int, RPGEnchantment>();
    private RPGEnchantment currentEntry;

    private readonly List<RPGBuilderDatabaseEntry> allItemTypes = new List<RPGBuilderDatabaseEntry>();
    private readonly List<RPGBuilderDatabaseEntry> allRarityTypes = new List<RPGBuilderDatabaseEntry>();
    private readonly List<RPGBuilderDatabaseEntry> allWeaponTypes = new List<RPGBuilderDatabaseEntry>();
    private readonly List<RPGBuilderDatabaseEntry> allWeaponSlots = new List<RPGBuilderDatabaseEntry>();
    private readonly List<RPGBuilderDatabaseEntry> allArmorTypes = new List<RPGBuilderDatabaseEntry>();
    private readonly List<RPGBuilderDatabaseEntry> allArmorSlots = new List<RPGBuilderDatabaseEntry>();

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

        RPGBuilderEditor.Instance.InitializeFilters(RPGBuilderEditor.Instance.EditorFilters.enchantmentFilters);
    }

    public override void InstantiateCurrentEntry(int index)
    {
        if (entries.Count == 0) return;
        currentEntry = Instantiate(entries[index]);
        RPGBuilderEditor.Instance.CurrentEntry = currentEntry;
    }

    public override void LoadEntries()
    {
        Dictionary<int, RPGEnchantment> dictionary = new Dictionary<int, RPGEnchantment>();
        databaseEntries.Clear();
        allItemTypes.Clear();
        allRarityTypes.Clear();
        allWeaponTypes.Clear();
        allWeaponSlots.Clear();
        allArmorTypes.Clear();
        allArmorSlots.Clear();
        var allEntries =
            Resources.LoadAll<RPGEnchantment>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
        for (var index = 0; index < allEntries.Length; index++)
        {
            var entry = allEntries[index];
            dictionary.Add(index, entry);
            databaseEntries.Add(entry);
        }

        entries = dictionary;

        foreach (var typeEntry in Resources.LoadAll<RPGBItemType>(RPGBuilderEditor.Instance.EditorSettings.DatabasePath)
        )
        {
            allItemTypes.Add(typeEntry);
        }

        foreach (var typeEntry in Resources.LoadAll<RPGBItemRarity>(RPGBuilderEditor.Instance.EditorSettings
            .DatabasePath))
        {
            allRarityTypes.Add(typeEntry);
        }

        foreach (var typeEntry in Resources.LoadAll<RPGBWeaponType>(RPGBuilderEditor.Instance.EditorSettings
            .DatabasePath))
        {
            allWeaponTypes.Add(typeEntry);
        }

        foreach (var typeEntry in Resources.LoadAll<RPGBWeaponHandSlot>(RPGBuilderEditor.Instance.EditorSettings
            .DatabasePath))
        {
            allWeaponSlots.Add(typeEntry);
        }

        foreach (var typeEntry in Resources.LoadAll<RPGBArmorType>(
            RPGBuilderEditor.Instance.EditorSettings.DatabasePath))
        {
            allArmorTypes.Add(typeEntry);
        }

        foreach (var typeEntry in Resources.LoadAll<RPGBArmorSlot>(
            RPGBuilderEditor.Instance.EditorSettings.DatabasePath))
        {
            allArmorSlots.Add(typeEntry);
        }
    }

    public override void CreateNewEntry()
    {
        if (EditorApplication.isCompiling)
        {
            Debug.LogError("You cannot interact with the RPG Builder while the editor is compiling");
            return;
        }

        currentEntry = CreateInstance<RPGEnchantment>();
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
        RPGEnchantment entryFile = (RPGEnchantment) updatedEntry;
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
            if (entries.Count > 0 &&
                entries[0] != null)
            {
                RPGBuilderEditor.Instance.SelectDatabaseEntry(0, true);
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
        RPGBuilderEditor.Instance.EditorFilters.enchantmentModuleSection.showBaseInfo =
            RPGBuilderEditorUtility.HandleModuleBanner("BASE INFO",
                RPGBuilderEditor.Instance.EditorFilters.enchantmentModuleSection.showBaseInfo);
        if (RPGBuilderEditor.Instance.EditorFilters.enchantmentModuleSection.showBaseInfo)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            RPGBuilderEditorFields.DrawID(currentEntry.ID);
            currentEntry.entryName =
                RPGBuilderEditorFields.DrawHorizontalTextField("Name", "", RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.entryName);
            currentEntry.entryDisplayName =
                RPGBuilderEditorFields.DrawHorizontalTextField("Display Name", "",
                    RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.entryDisplayName);
            currentEntry.entryFileName = RPGBuilderEditorFields.DrawFileNameField(
                "File Name", "", RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.entryName + AssetNameSuffix);
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.enchantmentModuleSection.showRequirements =
            RPGBuilderEditorUtility.HandleModuleBanner("REQUIREMENTS",
                RPGBuilderEditor.Instance.EditorFilters.enchantmentModuleSection.showRequirements);
        if (RPGBuilderEditor.Instance.EditorFilters.enchantmentModuleSection.showRequirements)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.CenteredMargin, false);
            if (GUILayout.Button("+ Add Requirement",
                RPGBuilderEditor.Instance.EditorSkin.GetStyle("HorizontalAddButton"),
                GUILayout.MinWidth(150),
                GUILayout.ExpandWidth(true)))
            {
                currentEntry.applyRequirements.Add(
                    new RPGEnchantment.ApplyRequirements());
            }

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.CenteredMargin, false);

            var ThisList2 = serialObj.FindProperty("applyRequirements");
            currentEntry.applyRequirements =
                RPGBuilderEditor.Instance
                    .GetTargetObjectOfProperty(ThisList2) as List<RPGEnchantment.ApplyRequirements>;

            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            for (var a = 0; a < currentEntry.applyRequirements.Count; a++)
            {
                GUILayout.Space(10);
                EditorGUILayout.BeginHorizontal();
                currentEntry.applyRequirements[a].type =
                    (RPGEnchantment.ApplyRequirementType) RPGBuilderEditorFields.DrawHorizontalEnum("Require", "",
                        (int)currentEntry.applyRequirements[a].type,
                        Enum.GetNames(typeof(RPGEnchantment.ApplyRequirementType)));
                if (RPGBuilderEditorFields.DrawSmallRemoveButton())
                {
                    currentEntry.applyRequirements.RemoveAt(a);
                    return;
                }
                EditorGUILayout.EndHorizontal();

                switch (currentEntry.applyRequirements[a].type)
                {
                    case RPGEnchantment.ApplyRequirementType.ItemType:
                        RPGBuilderEditorFields.DrawHorizontalLabel("Type", "");
                        int itemTypeIndex = EditorGUILayout.Popup(
                            RPGBuilderEditorUtility.GetTypeEntryIndex(allItemTypes, currentEntry.applyRequirements[a].ItemType),
                            RPGBuilderEditorUtility.GetTypeEntriesAsStringArray(allItemTypes.ToArray()));
                        currentEntry.applyRequirements[a].ItemType = (RPGBItemType) allItemTypes[itemTypeIndex];
                        EditorGUILayout.EndHorizontal();
                        break;
                    case RPGEnchantment.ApplyRequirementType.ItemRarity:
                        RPGBuilderEditorFields.DrawHorizontalLabel("Rarity", "");
                        int itemRarityIndex = EditorGUILayout.Popup(
                            RPGBuilderEditorUtility.GetTypeEntryIndex(allRarityTypes, currentEntry.applyRequirements[a].ItemRarity),
                            RPGBuilderEditorUtility.GetTypeEntriesAsStringArray(allRarityTypes.ToArray()));
                        currentEntry.applyRequirements[a].ItemRarity = (RPGBItemRarity) allRarityTypes[itemRarityIndex];
                        EditorGUILayout.EndHorizontal();
                        break;
                    case RPGEnchantment.ApplyRequirementType.ArmorType:
                        RPGBuilderEditorFields.DrawHorizontalLabel("Armor Type", "");
                        int armorTypeIndex = EditorGUILayout.Popup(
                            RPGBuilderEditorUtility.GetTypeEntryIndex(allArmorTypes, currentEntry.applyRequirements[a].ArmorType),
                            RPGBuilderEditorUtility.GetTypeEntriesAsStringArray(allArmorTypes.ToArray()));
                        currentEntry.applyRequirements[a].ArmorType = (RPGBArmorType) allArmorTypes[armorTypeIndex];
                        EditorGUILayout.EndHorizontal();
                        break;
                    case RPGEnchantment.ApplyRequirementType.ArmorSlot:
                        RPGBuilderEditorFields.DrawHorizontalLabel("Slot", "");
                        int armorSlotIndex = EditorGUILayout.Popup(
                            RPGBuilderEditorUtility.GetTypeEntryIndex(allArmorSlots, currentEntry.applyRequirements[a].ArmorSlot),
                            RPGBuilderEditorUtility.GetTypeEntriesAsStringArray(allArmorSlots.ToArray()));
                        currentEntry.applyRequirements[a].ArmorSlot = (RPGBArmorSlot) allArmorSlots[armorSlotIndex];
                        EditorGUILayout.EndHorizontal();
                        break;
                    case RPGEnchantment.ApplyRequirementType.WeaponType:
                        RPGBuilderEditorFields.DrawHorizontalLabel("Weapon Type", "");
                        int weaponTypeIndex = EditorGUILayout.Popup(
                            RPGBuilderEditorUtility.GetTypeEntryIndex(allWeaponTypes, currentEntry.applyRequirements[a].WeaponType),
                            RPGBuilderEditorUtility.GetTypeEntriesAsStringArray(allWeaponTypes.ToArray()));
                        currentEntry.applyRequirements[a].WeaponType = (RPGBWeaponType) allWeaponTypes[weaponTypeIndex];
                        EditorGUILayout.EndHorizontal();
                        break;
                    case RPGEnchantment.ApplyRequirementType.WeaponSlot:
                        RPGBuilderEditorFields.DrawHorizontalLabel("Slot", "");
                        int weaponSlotIndex = EditorGUILayout.Popup(
                            RPGBuilderEditorUtility.GetTypeEntryIndex(allWeaponSlots, currentEntry.applyRequirements[a].WeaponSlot),
                            RPGBuilderEditorUtility.GetTypeEntriesAsStringArray(allWeaponSlots.ToArray()));
                        currentEntry.applyRequirements[a].WeaponSlot = (RPGBWeaponHandSlot) allWeaponSlots[weaponSlotIndex];
                        EditorGUILayout.EndHorizontal();
                        break;
                }
            }

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.enchantmentModuleSection.showTiers =
            RPGBuilderEditorUtility.HandleModuleBanner("TIERS",
                RPGBuilderEditor.Instance.EditorFilters.enchantmentModuleSection.showTiers);
        if (RPGBuilderEditor.Instance.EditorFilters.enchantmentModuleSection.showTiers)
        {
            GUILayout.Space(10);
            if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Tier", true))
            {
                currentEntry.enchantmentTiers.Add(new RPGEnchantment.EnchantmentTier());
            }

            var ThisList3 = serialObj.FindProperty("enchantmentTiers");
            currentEntry.enchantmentTiers =
                RPGBuilderEditor.Instance.GetTargetObjectOfProperty(ThisList3) as List<RPGEnchantment.EnchantmentTier>;

            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            for (var a = 0; a < currentEntry.enchantmentTiers.Count; a++)
            {
                GUILayout.Space(10);
                var tierNumber = a + 1;
                GUILayout.BeginHorizontal();
                RPGBuilderEditorFields.DrawTitleLabelExpanded("Tier " + tierNumber + ": ", "");
                if (RPGBuilderEditorFields.DrawSmallRemoveButton())
                {
                    currentEntry.enchantmentTiers.RemoveAt(a);
                    return;
                }
                EditorGUILayout.EndHorizontal();
                
                currentEntry.enchantmentTiers[a].successRate =
                    RPGBuilderEditorFields.DrawHorizontalFloatFillBar("Success Rate", "",
                        currentEntry.enchantmentTiers[a].successRate);

                RPGBuilderEditorFields.DrawTitleLabelExpanded("Time:", "", true);
                currentEntry.enchantmentTiers[a].enchantTime =
                    RPGBuilderEditorFields.DrawHorizontalFloatField("Enchanting Time",
                        "",
                        RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.enchantmentTiers[a].enchantTime);

                RPGBuilderEditorFields.DrawTitleLabelExpanded("Rewards:", "", true);
                currentEntry.enchantmentTiers[a].skillXPAmount =
                    RPGBuilderEditorFields.DrawHorizontalIntField("Skill EXP", "",
                        RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.enchantmentTiers[a].skillXPAmount);

                currentEntry.enchantmentTiers[a].skillID =
                    RPGBuilderEditorFields.DrawDatabaseEntryField(
                        currentEntry.enchantmentTiers[a].skillID, "Skill",
                        "Skill", "");

                RPGBuilderEditorFields.DrawTitleLabelExpanded("Visual:", "", true);
                currentEntry.enchantmentTiers[a].enchantingParticle =
                    (GameObject) RPGBuilderEditorFields.DrawHorizontalObject<GameObject>("Particle", "",
                        currentEntry.enchantmentTiers[a].enchantingParticle);
                
                GUILayout.Space(10);
                if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Currency Cost", true))
                {
                    currentEntry.enchantmentTiers[a].currencyCosts
                        .Add(new RPGEnchantment.CurrencyCost());
                }
                
                for (var t = 0; t < currentEntry.enchantmentTiers[a].currencyCosts.Count; t++)
                {
                    GUILayout.Space(3);
                    EditorGUILayout.BeginHorizontal();
                    currentEntry.enchantmentTiers[a].currencyCosts[t].currencyID =
                        RPGBuilderEditorFields.DrawDatabaseEntryField(
                            currentEntry.enchantmentTiers[a].currencyCosts[t]
                                .currencyID, "Currency", "Currency", "");
                    if (RPGBuilderEditorFields.DrawSmallRemoveButton())
                    {
                        currentEntry.enchantmentTiers[a].currencyCosts.RemoveAt(t);
                        return;
                    }
                    EditorGUILayout.EndHorizontal();

                    currentEntry.enchantmentTiers[a].currencyCosts[t].amount =
                        RPGBuilderEditorFields.DrawHorizontalIntField(
                            "Amount", "",
                            RPGBuilderEditor.Instance.FieldHeight,
                            currentEntry.enchantmentTiers[a].currencyCosts[t]
                                .amount);
                    GUILayout.Space(5);
                }

                GUILayout.Space(10);
                if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Item Cost", true))
                {
                    currentEntry.enchantmentTiers[a].itemCosts
                        .Add(new RPGEnchantment.ItemCost());
                }
                
                for (var t = 0; t < currentEntry.enchantmentTiers[a].itemCosts.Count; t++)
                {
                    if (RPGBuilderEditorFields.DrawHorizontalEntryRemoveButton(
                        currentEntry.enchantmentTiers[a].itemCosts[t].itemID,
                        "Item"))
                    {
                        currentEntry.enchantmentTiers[a].itemCosts.RemoveAt(a);
                        return;
                    }

                    currentEntry.enchantmentTiers[a].itemCosts[t].itemID =
                        RPGBuilderEditorFields.DrawDatabaseEntryField(
                            currentEntry.enchantmentTiers[a].itemCosts[t].itemID,
                            "Item", "Item", "");

                    currentEntry.enchantmentTiers[a].itemCosts[t].itemCount =
                        RPGBuilderEditorFields.DrawHorizontalIntField(
                            "Count", "",
                            RPGBuilderEditor.Instance.FieldHeight,
                            currentEntry.enchantmentTiers[a].itemCosts[t]
                                .itemCount);

                    GUILayout.Space(5);
                }

                GUILayout.Space(10);
                if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Stat", true))
                {
                    currentEntry.enchantmentTiers[a].stats
                        .Add(new RPGEnchantment.TierStat());
                }

                RPGBuilderEditorFields.DrawTitleLabelExpanded("Stat Bonuses:", "");
                for (var t = 0; t < currentEntry.enchantmentTiers[a].stats.Count; t++)
                {
                    EditorGUILayout.BeginHorizontal();
                    currentEntry.enchantmentTiers[a].stats[t].statID =
                        RPGBuilderEditorFields.DrawDatabaseEntryField(
                            currentEntry.enchantmentTiers[a].stats[t].statID,
                            "Stat", "Stat", "");
                    if (RPGBuilderEditorFields.DrawSmallRemoveButton())
                    {
                        currentEntry.enchantmentTiers[a].stats.RemoveAt(t);
                        return;
                    }
                    EditorGUILayout.EndHorizontal();

                    currentEntry.enchantmentTiers[a].stats[t].amount =
                        RPGBuilderEditorFields.DrawHorizontalFloatField(
                            "Amount", "",
                            RPGBuilderEditor.Instance.FieldHeight,
                            currentEntry.enchantmentTiers[a].stats[t].amount);

                    RPGStat entryReference = (RPGStat) RPGBuilderEditorUtility.GetEntryByID(currentEntry.enchantmentTiers[a].stats[t].statID, "Stat");
                    if (entryReference != null)
                    {
                        if (!entryReference.isPercentStat)
                        {
                            currentEntry.enchantmentTiers[a].stats[t].isPercent =
                                RPGBuilderEditorFields.DrawHorizontalToggle(
                                    "Is Percent?", "",
                                    RPGBuilderEditor.Instance.FieldHeight,
                                    currentEntry.enchantmentTiers[a].stats[t]
                                        .isPercent);
                        }
                        else
                        {
                            currentEntry.enchantmentTiers[a].stats[t].isPercent =
                                false;
                        }
                    }

                    GUILayout.Space(5);
                }
            }

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        serialObj.ApplyModifiedProperties();

        GUILayout.Space(25);
        GUILayout.EndScrollView();
    }

    public override void ConvertDatabaseEntriesAfterUpdate()
    {
        var allEntries =
            Resources.LoadAll<RPGEnchantment>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
        foreach (var entry in allEntries)
        {
            EditorUtility.SetDirty(entry);
             entry.entryName = entry._name;
             AssetDatabase.RenameAsset(RPGBuilderEditor.Instance.EditorData.ResourcePath + 
             RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName + "/" + entry._fileName + ".asset", entry.entryName + AssetNameSuffix);
             entry.entryFileName = entry.entryName + AssetNameSuffix;
            entry.entryDisplayName = entry.displayName;
            EditorUtility.SetDirty(entry);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public override void ConvertStringsToTypeEntries()
    {
        var allEntries = Resources.LoadAll<RPGEnchantment>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);

        RPGBuilderEditorModule itemTypes = RPGBuilderEditorUtility.GetModuleByName("Item Types");
        RPGBuilderEditorModule itemRarities = RPGBuilderEditorUtility.GetModuleByName("Item Rarities");
        RPGBuilderEditorModule weaponTypes = RPGBuilderEditorUtility.GetModuleByName("Weapon Types");
        RPGBuilderEditorModule armorTypes = RPGBuilderEditorUtility.GetModuleByName("Armor Types");
        RPGBuilderEditorModule armorSlots = RPGBuilderEditorUtility.GetModuleByName("Armor Slots");
        RPGBuilderEditorModule weaponSlots = RPGBuilderEditorUtility.GetModuleByName("Weapon Hand Slots");
        
        foreach (var entry in allEntries)
        {
            EditorUtility.SetDirty(entry);
            
            foreach (var requirement in entry.applyRequirements)
            {
                {
                    RPGBuilderDatabaseEntry entryFile = itemTypes.GetEntryByName(requirement.itemType);
                    if (entryFile != null)
                    {
                        requirement.ItemType = (RPGBItemType) entryFile;
                    }
                }
                {
                    RPGBuilderDatabaseEntry entryFile = itemTypes.GetEntryByName(requirement.itemRarity);
                    if (entryFile != null)
                    {
                        requirement.ItemRarity = (RPGBItemRarity) entryFile;
                    }
                }
                {
                    RPGBuilderDatabaseEntry entryFile = weaponTypes.GetEntryByName(requirement.weaponType);
                    if (entryFile != null)
                    {
                        requirement.WeaponType = (RPGBWeaponType) entryFile;
                    }
                }
                {
                    RPGBuilderDatabaseEntry entryFile = armorTypes.GetEntryByName(requirement.armorType);
                    if (entryFile != null)
                    {
                        requirement.ArmorType = (RPGBArmorType) entryFile;
                    }
                }
                {
                    RPGBuilderDatabaseEntry entryFile = armorSlots.GetEntryByName(requirement.armorSlot);
                    if (entryFile != null)
                    {
                        requirement.ArmorSlot = (RPGBArmorSlot) entryFile;
                    }
                }
                {
                    RPGBuilderDatabaseEntry entryFile = weaponSlots.GetEntryByName(requirement.weaponSlot);
                    if (entryFile != null)
                    {
                        requirement.WeaponSlot = (RPGBWeaponHandSlot) entryFile;
                    }
                }
            }

            EditorUtility.SetDirty(entry);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
