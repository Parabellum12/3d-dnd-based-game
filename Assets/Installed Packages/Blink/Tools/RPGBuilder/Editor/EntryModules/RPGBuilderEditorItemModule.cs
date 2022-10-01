using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Data;
using BLINK.RPGBuilder.Templates;
using UnityEditor;
using UnityEngine;

public class RPGBuilderEditorItemModule : RPGBuilderEditorModule
{
    private Dictionary<int, RPGItem> entries = new Dictionary<int, RPGItem>();
    private RPGItem currentEntry;

    private readonly List<RPGBuilderDatabaseEntry> allItemTypes = new List<RPGBuilderDatabaseEntry>();
    private readonly List<RPGBuilderDatabaseEntry> allRarityTypes = new List<RPGBuilderDatabaseEntry>();
    private readonly List<RPGBuilderDatabaseEntry> allWeaponTypes = new List<RPGBuilderDatabaseEntry>();
    private readonly List<RPGBuilderDatabaseEntry> allWeaponSlots = new List<RPGBuilderDatabaseEntry>();
    private readonly List<RPGBuilderDatabaseEntry> allArmorTypes = new List<RPGBuilderDatabaseEntry>();
    private readonly List<RPGBuilderDatabaseEntry> allArmorSlots = new List<RPGBuilderDatabaseEntry>();
    private readonly List<RPGBuilderDatabaseEntry> allSocketTypes = new List<RPGBuilderDatabaseEntry>();
    private readonly List<RPGBuilderDatabaseEntry> allBodyParts = new List<RPGBuilderDatabaseEntry>();
    private readonly List<RPGBuilderDatabaseEntry> allGenders = new List<RPGBuilderDatabaseEntry>();

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

        RPGBuilderEditor.Instance.InitializeFilters(RPGBuilderEditor.Instance.EditorFilters.itemFilters);
    }

    public override void InstantiateCurrentEntry(int index)
    {
        if (entries.Count == 0) return;
        currentEntry = Instantiate(entries[index]);
        RPGBuilderEditor.Instance.CurrentEntry = currentEntry;
    }

    public override void LoadEntries()
    {
        Dictionary<int, RPGItem> dictionary = new Dictionary<int, RPGItem>();
        databaseEntries.Clear();
        allItemTypes.Clear();
        allRarityTypes.Clear();
        allWeaponTypes.Clear();
        allWeaponSlots.Clear();
        allArmorTypes.Clear();
        allArmorSlots.Clear();
        allSocketTypes.Clear();
        allBodyParts.Clear();
        allGenders.Clear();
        var allEntries =
            Resources.LoadAll<RPGItem>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
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

        foreach (var typeEntry in Resources.LoadAll<RPGBGemSocketType>(RPGBuilderEditor.Instance.EditorSettings
            .DatabasePath))
        {
            allSocketTypes.Add(typeEntry);
        }

        foreach (var typeEntry in Resources.LoadAll<RPGBBodyPart>(RPGBuilderEditor.Instance.EditorSettings
            .DatabasePath))
        {
            allBodyParts.Add(typeEntry);
        }

        foreach (var typeEntry in Resources.LoadAll<RPGBGender>(RPGBuilderEditor.Instance.EditorSettings
            .DatabasePath))
        {
            allGenders.Add(typeEntry);
        }
    }

    public override void CreateNewEntry()
    {
        if (EditorApplication.isCompiling)
        {
            Debug.LogError("You cannot interact with the RPG Builder while the editor is compiling");
            return;
        }

        currentEntry = CreateInstance<RPGItem>();
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
        RPGItem entryFile = (RPGItem) updatedEntry;
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
                RPGBuilderEditor.Instance.SelectDatabaseEntry(0, true);
            else
                CreateNewEntry();
        }

        if (allItemTypes.Count == 0)
        {
            Debug.LogError("No Item Types found, create at least one");
            return;
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
        RPGBuilderEditor.Instance.EditorFilters.itemModuleSection.showBaseInfo =
            RPGBuilderEditorUtility.HandleModuleBanner("BASE INFO",
                RPGBuilderEditor.Instance.EditorFilters.itemModuleSection.showBaseInfo);
        if (RPGBuilderEditor.Instance.EditorFilters.itemModuleSection.showBaseInfo)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, false);
            currentEntry.entryIcon = RPGBuilderEditorFields.DrawIcon(currentEntry.entryIcon, 100, 100);
            GUILayout.BeginVertical();
            RPGBuilderEditorFields.DrawID(currentEntry.ID);
            currentEntry.entryName =
                RPGBuilderEditorFields.DrawHorizontalTextField("Name", "", RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.entryName);
            currentEntry.entryDisplayName = RPGBuilderEditorFields.DrawHorizontalTextField("Display Name", "",
                RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.entryDisplayName);
            currentEntry.entryFileName = RPGBuilderEditorFields.DrawFileNameField("File Name", "",
                RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.entryName + AssetNameSuffix);
            currentEntry.entryDescription = RPGBuilderEditorFields.DrawHorizontalDescriptionField("Description", "",
                RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.entryDescription);
            GUILayout.EndVertical();
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, false);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.itemModuleSection.showTypes =
            RPGBuilderEditorUtility.HandleModuleBanner("TYPES",
                RPGBuilderEditor.Instance.EditorFilters.itemModuleSection.showTypes);
        if (RPGBuilderEditor.Instance.EditorFilters.itemModuleSection.showTypes)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            
            RPGBuilderEditorFields.DrawHorizontalLabel("Type", "");
            int itemTypeIndex = EditorGUILayout.Popup(
                RPGBuilderEditorUtility.GetTypeEntryIndex(allItemTypes, currentEntry.ItemType),
                RPGBuilderEditorUtility.GetTypeEntriesAsStringArray(allItemTypes.ToArray()));
            currentEntry.ItemType = (RPGBItemType) allItemTypes[itemTypeIndex];
            EditorGUILayout.EndHorizontal();
            if (currentEntry.ItemType != null)
            {
                if (currentEntry.ItemType.CanBeEquipped)
                {
                    if (currentEntry.ItemType.EquipType == EconomyData.EquipFunction.Armor)
                    {
                        RPGBuilderEditorFields.DrawHorizontalLabel("Armor Type", "");
                        int armorTypeIndex = EditorGUILayout.Popup(
                            RPGBuilderEditorUtility.GetTypeEntryIndex(allArmorTypes, currentEntry.ArmorType),
                            RPGBuilderEditorUtility.GetTypeEntriesAsStringArray(allArmorTypes.ToArray()));
                        currentEntry.ArmorType = (RPGBArmorType) allArmorTypes[armorTypeIndex];
                        EditorGUILayout.EndHorizontal();

                        RPGBuilderEditorFields.DrawHorizontalLabel("Slot", "");
                        int armorSlotIndex = EditorGUILayout.Popup(
                            RPGBuilderEditorUtility.GetTypeEntryIndex(allArmorSlots, currentEntry.ArmorSlot),
                            RPGBuilderEditorUtility.GetTypeEntriesAsStringArray(allArmorSlots.ToArray()));
                        currentEntry.ArmorSlot = (RPGBArmorSlot) allArmorSlots[armorSlotIndex];
                        EditorGUILayout.EndHorizontal();
                    } else if (currentEntry.ItemType.EquipType == EconomyData.EquipFunction.Weapon)
                    {
                        RPGBuilderEditorFields.DrawHorizontalLabel("Weapon Type", "");
                        int weaponTypeIndex = EditorGUILayout.Popup(
                            RPGBuilderEditorUtility.GetTypeEntryIndex(allWeaponTypes, currentEntry.WeaponType),
                            RPGBuilderEditorUtility.GetTypeEntriesAsStringArray(allWeaponTypes.ToArray()));
                        currentEntry.WeaponType = (RPGBWeaponType) allWeaponTypes[weaponTypeIndex];
                        EditorGUILayout.EndHorizontal();

                        RPGBuilderEditorFields.DrawHorizontalLabel("Slot", "");
                        int weaponSlotIndex = EditorGUILayout.Popup(
                            RPGBuilderEditorUtility.GetTypeEntryIndex(allWeaponSlots, currentEntry.WeaponSlot),
                            RPGBuilderEditorUtility.GetTypeEntriesAsStringArray(allWeaponSlots.ToArray()));
                        currentEntry.WeaponSlot = (RPGBWeaponHandSlot) allWeaponSlots[weaponSlotIndex];
                        EditorGUILayout.EndHorizontal();
                    }
                }
                else
                {
                    if (currentEntry.ItemType.ItemTypeFunction == EconomyData.ItemTypeFunction.Currency)
                    {
                        currentEntry.convertToCurrency = RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.convertToCurrency,
                                "Currency", "Currency", "");
                    }
                }
            }

            RPGBuilderEditorFields.DrawHorizontalLabel("Rarity", "");
            int itemRarityIndex = EditorGUILayout.Popup(
                RPGBuilderEditorUtility.GetTypeEntryIndex(allRarityTypes, currentEntry.ItemRarity),
                RPGBuilderEditorUtility.GetTypeEntriesAsStringArray(allRarityTypes.ToArray()));
            currentEntry.ItemRarity = (RPGBItemRarity) allRarityTypes[itemRarityIndex];
            EditorGUILayout.EndHorizontal();

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        if (currentEntry.ItemType != null)
        {
            switch (currentEntry.ItemType.ItemTypeFunction)
            {
                case EconomyData.ItemTypeFunction.Enchantment:
                {
                    GUILayout.Space(10);
                    RPGBuilderEditor.Instance.EditorFilters.itemModuleSection.showEnchantment =
                        RPGBuilderEditorUtility.HandleModuleBanner("ENCHANTMENTS",
                            RPGBuilderEditor.Instance.EditorFilters.itemModuleSection.showEnchantment);
                    if (RPGBuilderEditor.Instance.EditorFilters.itemModuleSection.showEnchantment)
                    {
                        GUILayout.Space(10);
                        RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin,
                            true);

                        currentEntry.enchantmentID =
                            RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.enchantmentID, "Enchantment",
                                "Enchantment", "");

                        currentEntry.isEnchantmentConsumed =
                            RPGBuilderEditorFields.DrawHorizontalToggle("Consume item?",
                                "",
                                RPGBuilderEditor.Instance.FieldHeight,
                                currentEntry.isEnchantmentConsumed);
                        RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin,
                            true);
                    }

                    break;
                }
                case EconomyData.ItemTypeFunction.Gem:
                {
                    GUILayout.Space(10);
                    RPGBuilderEditor.Instance.EditorFilters.itemModuleSection.showGem =
                        RPGBuilderEditorUtility.HandleModuleBanner("GEM BONUSES",
                            RPGBuilderEditor.Instance.EditorFilters.itemModuleSection.showGem);
                    if (RPGBuilderEditor.Instance.EditorFilters.itemModuleSection.showGem)
                    {
                        GUILayout.Space(10);
                        RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin,
                            true);

                        RPGBuilderEditorFields.DrawHorizontalLabel("Socket Type", "");
                        int socketTypeIndex = EditorGUILayout.Popup(
                            RPGBuilderEditorUtility.GetTypeEntryIndex(allSocketTypes,
                                currentEntry.gemData.GemSocketType),
                            RPGBuilderEditorUtility.GetTypeEntriesAsStringArray(allSocketTypes.ToArray()));
                        currentEntry.gemData.GemSocketType = (RPGBGemSocketType) allSocketTypes[socketTypeIndex];
                        EditorGUILayout.EndHorizontal();

                        GUILayout.Space(10);
                        if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Stat", true))
                        {
                            currentEntry.gemData.gemStats.Add(new RPGItem.GEM_DATA.GEM_STATS());
                        }

                        var ThisList21 = serialObj.FindProperty("gemData");
                        currentEntry.gemData =
                            RPGBuilderEditor.Instance.GetTargetObjectOfProperty(ThisList21) as RPGItem.GEM_DATA;

                        for (var a = 0; a < currentEntry.gemData.gemStats.Count; a++)
                        {
                            GUILayout.Space(10);
                            EditorGUILayout.BeginHorizontal();
                            currentEntry.gemData.gemStats[a].statID =
                                RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.gemData.gemStats[a].statID,
                                    "Stat", "Stat", "");
                            if (RPGBuilderEditorFields.DrawSmallRemoveButton())
                            {
                                currentEntry.gemData.gemStats.RemoveAt(a);
                                return;
                            }

                            EditorGUILayout.EndHorizontal();

                            currentEntry.gemData.gemStats[a].amount =
                                RPGBuilderEditorFields.DrawHorizontalFloatField("Amount",
                                    "",
                                    RPGBuilderEditor.Instance.FieldHeight,
                                    currentEntry.gemData.gemStats[a].amount);

                            RPGStat entryReference = (RPGStat)
                                RPGBuilderEditorUtility.GetEntryByID(currentEntry.gemData.gemStats[a].statID, "Stat");

                            if (entryReference != null)
                            {
                                if (!entryReference.isPercentStat)
                                {
                                    currentEntry.gemData.gemStats[a].isPercent =
                                        RPGBuilderEditorFields.DrawHorizontalToggle("Is Percent?",
                                            "",
                                            RPGBuilderEditor.Instance.FieldHeight,
                                            currentEntry.gemData.gemStats[a].isPercent);
                                }
                                else
                                {
                                    currentEntry.gemData.gemStats[a].isPercent = false;
                                }
                            }
                        }

                        RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin,
                            true);
                        GUILayout.Space(10);
                    }

                    break;
                }
            }
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.itemModuleSection.showLootSettings =
            RPGBuilderEditorUtility.HandleModuleBanner("LOOT SETTINGS",
                RPGBuilderEditor.Instance.EditorFilters.itemModuleSection.showLootSettings);
        if (RPGBuilderEditor.Instance.EditorFilters.itemModuleSection.showLootSettings)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);

            currentEntry.dropInWorld =
                RPGBuilderEditorFields.DrawHorizontalToggle("Drop in World?",
                    "",
                    RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.dropInWorld);

            if (currentEntry.dropInWorld)
            {
                currentEntry.itemWorldModel = (GameObject) RPGBuilderEditorFields.DrawHorizontalObject<GameObject>("In World Model", "",
                    currentEntry.itemWorldModel);
                currentEntry.durationInWorld =
                    RPGBuilderEditorFields.DrawHorizontalFloatField("Duration",
                        "",
                        RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.durationInWorld);
                RPGBuilderEditorFields.DrawHorizontalLabel("World Interactable Layers", "");
                currentEntry.worldInteractableLayer = EditorGUILayout.LayerField(currentEntry.worldInteractableLayer, RPGBuilderEditorFields.GetTextFieldStyle());
                EditorGUILayout.EndHorizontal();
            }

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        if (currentEntry.ItemType != null)
        {
            if (currentEntry.ItemType.CanBeEquipped)
            {
                if (currentEntry.ItemType.EquipType == EconomyData.EquipFunction.Weapon)
                {
                    GUILayout.Space(10);
                    RPGBuilderEditor.Instance.EditorFilters.itemModuleSection.showCombat =
                        RPGBuilderEditorUtility.HandleModuleBanner("COMBAT",
                            RPGBuilderEditor.Instance.EditorFilters.itemModuleSection.showCombat);
                    if (RPGBuilderEditor.Instance.EditorFilters.itemModuleSection.showCombat)
                    {
                        GUILayout.Space(10);
                        RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin,
                            true);
                        currentEntry.AttackSpeed = RPGBuilderEditorFields.DrawHorizontalFloatField("Attack Speed",
                            "",
                            RPGBuilderEditor.Instance.FieldHeight,
                            currentEntry.AttackSpeed);
                        currentEntry.minDamage = RPGBuilderEditorFields.DrawHorizontalIntField("Min DMG",
                            "",
                            RPGBuilderEditor.Instance.FieldHeight,
                            currentEntry.minDamage);
                        currentEntry.maxDamage = RPGBuilderEditorFields.DrawHorizontalIntField("Max DMG",
                            "",
                            RPGBuilderEditor.Instance.FieldHeight,
                            currentEntry.maxDamage);

                        currentEntry.autoAttackAbilityID =
                            RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.autoAttackAbilityID, "Ability",
                                "Auto Attack", "");

                        RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin,
                            true);
                        GUILayout.Space(10);
                    }
                }

                GUILayout.Space(10);
                RPGBuilderEditor.Instance.EditorFilters.itemModuleSection.showStats =
                    RPGBuilderEditorUtility.HandleModuleBanner("STATS",
                        RPGBuilderEditor.Instance.EditorFilters.itemModuleSection.showStats);
                if (RPGBuilderEditor.Instance.EditorFilters.itemModuleSection.showStats)
                {
                    GUILayout.Space(10);
                    if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Stat", true))
                    {
                        currentEntry.stats.Add(new RPGItem.ITEM_STATS());
                    }

                    var ThisList2 = serialObj.FindProperty("stats");
                    currentEntry.stats =
                        RPGBuilderEditor.Instance.GetTargetObjectOfProperty(ThisList2) as List<RPGItem.ITEM_STATS>;

                    RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
                    for (var a = 0; a < currentEntry.stats.Count; a++)
                    {
                        GUILayout.Space(10);
                        EditorGUILayout.BeginHorizontal();
                        currentEntry.stats[a].statID =
                            RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.stats[a].statID, "Stat", "Stat",
                                "");
                        if (RPGBuilderEditorFields.DrawSmallRemoveButton())
                        {
                            currentEntry.stats.RemoveAt(a);
                            return;
                        }

                        EditorGUILayout.EndHorizontal();

                        currentEntry.stats[a].amount = RPGBuilderEditorFields.DrawHorizontalFloatField("Amount",
                            "",
                            RPGBuilderEditor.Instance.FieldHeight,
                            currentEntry.stats[a].amount);

                        RPGStat entryReference = (RPGStat)
                            RPGBuilderEditorUtility.GetEntryByID(currentEntry.stats[a].statID, "Stat");
                        if (entryReference != null)
                        {
                            if (!entryReference.isPercentStat)
                            {
                                currentEntry.stats[a].isPercent = RPGBuilderEditorFields.DrawHorizontalToggle(
                                    "Is Percent?",
                                    "",
                                    RPGBuilderEditor.Instance.FieldHeight,
                                    currentEntry.stats[a].isPercent);
                            }
                            else
                            {
                                currentEntry.stats[a].isPercent = false;
                            }
                        }
                    }

                    RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
                }

                GUILayout.Space(10);
                RPGBuilderEditor.Instance.EditorFilters.itemModuleSection.showRandomStats =
                    RPGBuilderEditorUtility.HandleModuleBanner("RANDOM STATS",
                        RPGBuilderEditor.Instance.EditorFilters.itemModuleSection.showRandomStats);
                if (RPGBuilderEditor.Instance.EditorFilters.itemModuleSection.showRandomStats)
                {
                    GUILayout.Space(10);
                    RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);

                    currentEntry.randomStatsMax = RPGBuilderEditorFields.DrawHorizontalIntField("Max Stats",
                        "",
                        RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.randomStatsMax);
                    RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);

                    GUILayout.Space(10);
                    if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Random Stat", true))
                    {
                        currentEntry.randomStats.Add(new RPGItemDATA.RandomizedStatData());
                    }

                    var ThisList10 = serialObj.FindProperty("randomStats");
                    currentEntry.randomStats =
                        RPGBuilderEditor.Instance.GetTargetObjectOfProperty(ThisList10) as
                            List<RPGItemDATA.RandomizedStatData>;

                    RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
                    for (var a = 0; a < currentEntry.randomStats.Count; a++)
                    {
                        GUILayout.Space(10);
                        var requirementNumber = a + 1;
                        EditorGUILayout.BeginHorizontal();
                        if (RPGBuilderEditorFields.DrawSmallRemoveButton())
                        {
                            currentEntry.randomStats.RemoveAt(a);
                            return;
                        }

                        RPGStat entryReference = (RPGStat)
                            RPGBuilderEditorUtility.GetEntryByID(currentEntry.randomStats[a].statID, "Stat");
                        var effectName = "";
                        if (entryReference != null) effectName = entryReference.entryName;

                        EditorGUILayout.LabelField("" + requirementNumber + ": " + effectName);
                        EditorGUILayout.EndHorizontal();
                        currentEntry.randomStats[a].statID =
                            RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.randomStats[a].statID, "Stat",
                                "Stat", "");

                        currentEntry.randomStats[a].minValue = RPGBuilderEditorFields.DrawHorizontalFloatField("Min",
                            "",
                            RPGBuilderEditor.Instance.FieldHeight,
                            currentEntry.randomStats[a].minValue);
                        currentEntry.randomStats[a].maxValue = RPGBuilderEditorFields.DrawHorizontalFloatField("Max",
                            "",
                            RPGBuilderEditor.Instance.FieldHeight,
                            currentEntry.randomStats[a].maxValue);

                        currentEntry.randomStats[a].isInt = RPGBuilderEditorFields.DrawHorizontalToggle("Is Int?",
                            "",
                            RPGBuilderEditor.Instance.FieldHeight,
                            currentEntry.randomStats[a].isInt);

                        currentEntry.randomStats[a].chance =
                            RPGBuilderEditorFields.DrawHorizontalFloatFillBar("Chance", "",
                                currentEntry.randomStats[a].chance);

                        if (entryReference != null)
                        {
                            if (!entryReference.isPercentStat)
                            {
                                currentEntry.randomStats[a].isPercent = RPGBuilderEditorFields.DrawHorizontalToggle(
                                    "Is Percent?",
                                    "",
                                    RPGBuilderEditor.Instance.FieldHeight,
                                    currentEntry.randomStats[a].isPercent);
                            }
                            else
                            {
                                currentEntry.randomStats[a].isPercent = false;
                            }
                        }
                    }

                    RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
                }

                GUILayout.Space(10);
                RPGBuilderEditor.Instance.EditorFilters.itemModuleSection.showActionAbilities =
                    RPGBuilderEditorUtility.HandleModuleBanner("ACTION ABILITIES",
                        RPGBuilderEditor.Instance.EditorFilters.itemModuleSection.showActionAbilities);
                if (RPGBuilderEditor.Instance.EditorFilters.itemModuleSection.showActionAbilities)
                {
                    RPGBuilderEditorFields.DrawActionAbilityList(currentEntry.actionAbilities);
                }

                GUILayout.Space(10);
                RPGBuilderEditor.Instance.EditorFilters.itemModuleSection.showSocket =
                    RPGBuilderEditorUtility.HandleModuleBanner("SOCKETS",
                        RPGBuilderEditor.Instance.EditorFilters.itemModuleSection.showSocket);
                if (RPGBuilderEditor.Instance.EditorFilters.itemModuleSection.showSocket)
                {
                    GUILayout.Space(10);
                    if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Socket", true))
                    {
                        currentEntry.sockets.Add(new RPGItem.SOCKETS_DATA());
                    }

                    var ThisList20 = serialObj.FindProperty("sockets");
                    currentEntry.sockets =
                        RPGBuilderEditor.Instance.GetTargetObjectOfProperty(ThisList20) as List<RPGItem.SOCKETS_DATA>;

                    RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
                    for (var a = 0; a < currentEntry.sockets.Count; a++)
                    {
                        GUILayout.Space(10);

                        EditorGUILayout.BeginHorizontal();
                        int socketTypeIndex = EditorGUILayout.Popup(
                            RPGBuilderEditorUtility.GetTypeEntryIndex(allSocketTypes,
                                currentEntry.sockets[a].GemSocketType),
                            RPGBuilderEditorUtility.GetTypeEntriesAsStringArray(allSocketTypes.ToArray()));
                        currentEntry.sockets[a].GemSocketType = (RPGBGemSocketType) allSocketTypes[socketTypeIndex];
                        if (RPGBuilderEditorFields.DrawSmallRemoveButton())
                        {
                            currentEntry.sockets.RemoveAt(a);
                            return;
                        }

                        EditorGUILayout.EndHorizontal();
                    }

                    RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
                }

                GUILayout.Space(10);
                RPGBuilderEditor.Instance.EditorFilters.itemModuleSection.showVisuals =
                    RPGBuilderEditorUtility.HandleModuleBanner("VISUAL",
                        RPGBuilderEditor.Instance.EditorFilters.itemModuleSection.showVisuals);
                if (RPGBuilderEditor.Instance.EditorFilters.itemModuleSection.showVisuals)
                {
                    GUILayout.Space(10);
                    if (currentEntry.ItemType.EquipType == EconomyData.EquipFunction.Armor)
                    {
                        RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin,
                            true);

                                currentEntry.itemModelName = RPGBuilderEditorFields.DrawHorizontalTextField(
                                    "Model Name",
                                    "",
                                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.itemModelName);

                        currentEntry.modelMaterial =
                            (Material) RPGBuilderEditorFields.DrawHorizontalObject<Material>("(Optional) Material", "",
                                currentEntry.modelMaterial);

                        currentEntry.BodyCullingTemplate =
                            (BodyCullingTemplate) RPGBuilderEditorFields.DrawHorizontalObject<BodyCullingTemplate>(
                                "Body Culling", "", currentEntry.BodyCullingTemplate);
                        

                        RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
                    }
                    else if (currentEntry.ItemType.EquipType == EconomyData.EquipFunction.Weapon)
                    {
                        RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin,
                            true);
                        currentEntry.weaponModel =
                            (GameObject) RPGBuilderEditorFields.DrawHorizontalObject<GameObject>("Weapon Model", "",
                                currentEntry.weaponModel);

                        currentEntry.modelMaterial =
                            (Material) RPGBuilderEditorFields.DrawHorizontalObject<Material>("(Optional) Material", "",
                                currentEntry.modelMaterial);
                        RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin,
                            true);


                        GUILayout.Space(10);
                        RPGBuilderEditor.Instance.EditorFilters.itemModuleSection.showWeaponPositions =
                            RPGBuilderEditorUtility.HandleModuleBanner("WEAPON TRANSFORM VALUES",
                                RPGBuilderEditor.Instance.EditorFilters.itemModuleSection.showWeaponPositions);
                        if (RPGBuilderEditor.Instance.EditorFilters.itemModuleSection.showWeaponPositions)
                        {
                            GUILayout.Space(10);
                            RPGBuilderEditorUtility.StartHorizontalMargin(
                                RPGBuilderEditor.Instance.LongHorizontalMargin, true);
                            currentEntry.UseWeaponTransformTemplate =
                                RPGBuilderEditorFields.DrawHorizontalToggle("Use Template?", "",
                                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.UseWeaponTransformTemplate);

                            if (currentEntry.UseWeaponTransformTemplate)
                            {
                                currentEntry.WeaponTransformTemplate =
                                    (WeaponTransformTemplate) RPGBuilderEditorFields
                                        .DrawHorizontalObject<WeaponTransformTemplate>(
                                            "Template", "", currentEntry.WeaponTransformTemplate);
                            }
                            else
                            {
                                GUILayout.Space(10);
                                RPGBuilderEditorUtility.StartHorizontalMargin(
                                    RPGBuilderEditor.Instance.LongHorizontalMargin, false);
                                if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Race", false))
                                {
                                    currentEntry.WeaponTransforms.Add(new WeaponTransform());
                                }

                                RPGBuilderEditorUtility.EndHorizontalMargin(
                                    RPGBuilderEditor.Instance.LongHorizontalMargin, false);

                                currentEntry.WeaponTransforms = RPGBuilderEditorFields.DrawWeaponTransformList(
                                    currentEntry.WeaponTransforms,
                                    currentEntry.WeaponSlot.entryName, false);
                            }

                            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin,
                                true);
                            GUILayout.Space(10);
                        }
                    }
                }
            }
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.itemModuleSection.showRequirements =
            RPGBuilderEditorUtility.HandleModuleBanner("REQUIREMENTS",
                RPGBuilderEditor.Instance.EditorFilters.itemModuleSection.showRequirements);
        if (RPGBuilderEditor.Instance.EditorFilters.itemModuleSection.showRequirements)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.CenteredMargin, true);
            currentEntry.UseRequirementsTemplate = 
                RPGBuilderEditorFields.DrawHorizontalToggle("Use Template?", "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.UseRequirementsTemplate);

            if (currentEntry.UseRequirementsTemplate)
            {
                currentEntry.RequirementsTemplate = (RequirementsTemplate) RPGBuilderEditorFields.DrawHorizontalObject<RequirementsTemplate>(
                    "Template", "", currentEntry.RequirementsTemplate);
            }
            else
            {
                GUILayout.Space(10);
                if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Requirement Group", false))
                {
                    currentEntry.Requirements.Add(new RequirementsData.RequirementGroup());
                }

                currentEntry.Requirements = RPGBuilderEditorFields.DrawRequirementGroupsList(currentEntry.Requirements,false);
            }
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.CenteredMargin, true);
                        
            GUILayout.Space(10);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.itemModuleSection.showOnUseActions =
            RPGBuilderEditorUtility.HandleModuleBanner("ON USE ACTIONS",
                RPGBuilderEditor.Instance.EditorFilters.itemModuleSection.showOnUseActions);
        if (RPGBuilderEditor.Instance.EditorFilters.itemModuleSection.showOnUseActions)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            currentEntry.UseGameActionsTemplate = 
                RPGBuilderEditorFields.DrawHorizontalToggle("Use Template?", "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.UseGameActionsTemplate);

            if (currentEntry.UseGameActionsTemplate)
            {
                currentEntry.GameActionsTemplate = (GameActionsTemplate) RPGBuilderEditorFields.DrawHorizontalObject<GameActionsTemplate>(
                    "Template", "", currentEntry.GameActionsTemplate);
            }
            else
            {
                GUILayout.Space(10);
                if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Game Action", false))
                {
                    currentEntry.GameActions.Add(new GameActionsData.GameAction());
                }

                currentEntry.GameActions = RPGBuilderEditorFields.DrawGameActionsList(currentEntry.GameActions, false);
            }
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
                        
            GUILayout.Space(10);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.itemModuleSection.showGeneral =
            RPGBuilderEditorUtility.HandleModuleBanner("GENERAL",
                RPGBuilderEditor.Instance.EditorFilters.itemModuleSection.showGeneral);
        if (RPGBuilderEditor.Instance.EditorFilters.itemModuleSection.showGeneral)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            RPGBuilderEditorFields.DrawTitleLabelExpanded("Resell Settings:", "");
            currentEntry.sellCurrencyID =
                RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.sellCurrencyID, "Currency", "Currency", "");

            currentEntry.sellPrice =
                RPGBuilderEditorFields.DrawHorizontalIntField("Amount", "",
                    RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.sellPrice);

            RPGBuilderEditorFields.DrawTitleLabelExpanded("Stacking:", "", true);
            currentEntry.stackLimit =
                RPGBuilderEditorFields.DrawHorizontalIntField("Stack Limit", "",
                    RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.stackLimit);

            if (currentEntry.ItemType.CanBeEquipped)
            {
                currentEntry.stackLimit = 1;
            }

            if (currentEntry.stackLimit == 0)
            {
                currentEntry.stackLimit = 1;
            }

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            GUILayout.Space(10);
        }

        serialObj.ApplyModifiedProperties();

        GUILayout.Space(25);
        GUILayout.EndScrollView();

    }

    public override void ConvertDatabaseEntriesAfterUpdate()
    {
        var allEntries =
            Resources.LoadAll<RPGItem>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
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

    public override void ConvertStringsToTypeEntries()
    {
        var allEntries = Resources.LoadAll<RPGItem>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
        RPGBuilderEditorModule itemTypes = RPGBuilderEditorUtility.GetModuleByName("Item Types");
        RPGBuilderEditorModule itemRarities = RPGBuilderEditorUtility.GetModuleByName("Item Rarities");
        RPGBuilderEditorModule weaponTypes = RPGBuilderEditorUtility.GetModuleByName("Weapon Types");
        RPGBuilderEditorModule armorTypes = RPGBuilderEditorUtility.GetModuleByName("Armor Types");
        RPGBuilderEditorModule armorSlots = RPGBuilderEditorUtility.GetModuleByName("Armor Slots");
        RPGBuilderEditorModule weaponSlots = RPGBuilderEditorUtility.GetModuleByName("Weapon Hand Slots");
        RPGBuilderEditorModule socketTypes = RPGBuilderEditorUtility.GetModuleByName("Gem Socket Types");
        
        foreach (var entry in allEntries)
        {
            EditorUtility.SetDirty(entry);
            {
                RPGBuilderDatabaseEntry entryFile = itemTypes.GetEntryByName(entry.itemType);
                if (entryFile != null)
                {
                    entry.ItemType = (RPGBItemType) entryFile;
                }
            }
            {
                RPGBuilderDatabaseEntry entryFile = itemRarities.GetEntryByName(entry.rarity);
                if (entryFile != null)
                {
                    entry.ItemRarity = (RPGBItemRarity) entryFile;
                }
            }
            {
                RPGBuilderDatabaseEntry entryFile = weaponTypes.GetEntryByName(entry.weaponType);
                if (entryFile != null)
                {
                    entry.WeaponType = (RPGBWeaponType) entryFile;
                }
            }
            {
                RPGBuilderDatabaseEntry entryFile = armorTypes.GetEntryByName(entry.armorType);
                if (entryFile != null)
                {
                    entry.ArmorType = (RPGBArmorType) entryFile;
                }
            }
            {
                RPGBuilderDatabaseEntry entryFile = armorSlots.GetEntryByName(entry.equipmentSlot);
                if (entryFile != null)
                {
                    entry.ArmorSlot = (RPGBArmorSlot) entryFile;
                }
            }
            {
                RPGBuilderDatabaseEntry entryFile = weaponSlots.GetEntryByName(entry.slotType);
                if (entryFile != null)
                {
                    entry.WeaponSlot = (RPGBWeaponHandSlot) entryFile;
                }
            }
            {
                RPGBuilderDatabaseEntry entryFile = socketTypes.GetEntryByName(entry.gemData.socketType);
                if (entryFile != null)
                {
                    entry.gemData.GemSocketType = (RPGBGemSocketType) entryFile;
                }
            }
            {
                foreach (var socket in entry.sockets)
                {
                    RPGBuilderDatabaseEntry entryFile = socketTypes.GetEntryByName(socket.socketType);
                    if (entryFile != null)
                    {
                        socket.GemSocketType = (RPGBGemSocketType) entryFile;
                    }
                }
            }

            EditorUtility.SetDirty(entry);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
