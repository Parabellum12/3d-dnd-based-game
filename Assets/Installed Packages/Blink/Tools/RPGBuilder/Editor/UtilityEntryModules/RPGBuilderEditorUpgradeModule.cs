using System;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.AI;
using BLINK.RPGBuilder.Data;
using BLINK.RPGBuilder.Templates;
using UnityEditor;
using UnityEngine;

public class RPGBuilderEditorUpgradeModule : RPGBuilderEditorModule
{
    private RPGBuilderEditorUpgradeUtility currentEntry;

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
        currentEntry = Resources.Load<RPGBuilderEditorUpgradeUtility>(
            RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath +
            AssetFolderName + "/" + EntryType);
        if (currentEntry != null)
        {
            currentEntry = Instantiate(currentEntry);
        }
        else
        {
            AssetDatabase.CreateAsset(CreateInstance<RPGBuilderEditorUpgradeUtility>(),
                RPGBuilderEditor.Instance.EditorData.ResourcePath +
                RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName + "/" + EntryType + ".asset");
            currentEntry = Resources.Load<RPGBuilderEditorUpgradeUtility>(
                RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath +
                AssetFolderName + "/" + EntryType);
        }

        databaseEntries.Clear();
        databaseEntries.Add(currentEntry);

        RPGBuilderEditor.Instance.CurrentEntry = currentEntry;
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
        RPGBuilderEditorUpgradeUtility entryFile = (RPGBuilderEditorUpgradeUtility) updatedEntry;
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

        RPGBuilderEditor.Instance.ViewScroll = EditorGUILayout.BeginScrollView(
            RPGBuilderEditor.Instance.ViewScroll, false, false, GUIStyle.none, GUIStyle.none, GUIStyle.none,
            GUILayout.Width(panelRect.width), GUILayout.MaxWidth(panelRect.width),
            GUILayout.Height(RPGBuilderEditor.Instance.ViewRect.height - 40));

        RPGBuilderEditor.Instance.EditorFilters.upgradeUtilityModuleSection.showDatabaseUpgrade =
            RPGBuilderEditorUtility.HandleModuleBanner("UPGRADE DATABASE",
                RPGBuilderEditor.Instance.EditorFilters.upgradeUtilityModuleSection.showDatabaseUpgrade);

        if (RPGBuilderEditor.Instance.EditorFilters.upgradeUtilityModuleSection.showDatabaseUpgrade)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);

            if (GUILayout.Button("Convert Database to 2.0",
                RPGBuilderEditor.Instance.EditorSkin.GetStyle("HorizontalAddButton"),
                GUILayout.MinWidth(150), GUILayout.ExpandWidth(true)))
            {
                
                int moduleConverted = 0;
                int moduleCount = RPGBuilderEditor.Instance.EditorCategories.Sum(category =>
                    category.modules.Count(module => !module.IsSettingModule && !module.IsTypeModule));

                EditorUtility.DisplayProgressBar("Converting Database to 2.0 Database Structure",
                    "Modules Converted: " + moduleConverted + " / " + moduleCount,
                    (float) ((float) moduleConverted / (float) moduleCount));

                foreach (var module in RPGBuilderEditor.Instance.EditorCategories.SelectMany(category =>
                    category.modules.Where(module => !module.IsSettingModule && !module.IsTypeModule)))
                {
                    module.ConvertDatabaseEntriesAfterUpdate();
                    module.LoadEntries();
                    moduleConverted++;
                    EditorUtility.DisplayProgressBar("Converting Database to 2.0 Database Structure",
                        "Modules Converted: " + moduleConverted + " / " + moduleCount,
                        (float) ((float) moduleConverted / (float) moduleCount));
                }

                EditorUtility.ClearProgressBar();
                
                int modulesConverted2 = 0;
                int moduleCount2 = RPGBuilderEditor.Instance.EditorCategories.Sum(
                    category => category.modules.Count(module => !module.IsSettingModule && !module.IsTypeModule));

                EditorUtility.DisplayProgressBar("Converting ID Files to 2.0",
                    "ID Files Converted: " + modulesConverted2 + " / " + moduleCount2,
                    (float) ((float) modulesConverted2 / (float) moduleCount2));

                foreach (var module in RPGBuilderEditor.Instance.EditorCategories.SelectMany(category =>
                    category.modules.Where(module => !module.IsSettingModule && !module.IsTypeModule)))
                {

                    var currentIDFile = DataSavingSystem.LoadOldAssetID(module.OldIDFileName);
                    if (currentIDFile == null)
                    {
                        Debug.LogError(module.IDFileName + "s could not be found");
                    }

                    if (currentIDFile != null)
                    {
                        int currentID = currentIDFile.id;
                        var newFile = new AssetIDHandler(module.IDFileName, 0) {id = currentID};
                        DataSavingSystem.SaveAssetID(newFile);
                        DataSavingSystem.DeleteOldAssetIDFile(module.OldIDFileName);
                    }

                    modulesConverted2++;
                    EditorUtility.DisplayProgressBar("Converting ID Files to 2.0",
                        "ID Files Converted: " + modulesConverted2 + " / " + moduleCount2,
                        (float) ((float) modulesConverted2 / (float) moduleCount2));
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                EditorUtility.ClearProgressBar();
                
                
                int moduleConverted3 = 0;
                int moduleCount3 = 8;

                EditorUtility.DisplayProgressBar("Converting Settings Files to 2.0",
                    "Settings Files Converted: " + moduleConverted3 + " / " + moduleCount3,
                    (float) ((float) moduleConverted3 / (float) moduleCount3));

                foreach (RPGBuilderEditor.EditorCategoryData category in RPGBuilderEditor.Instance.EditorCategories)
                {
                    foreach (RPGBuilderEditorModule module in category.modules.Where(module => module.IsSettingModule)
                        .Where(module => module.AssetFolderName == "Settings"))
                    {
                        module.ConvertDatabaseEntriesAfterUpdate();
                        module.LoadEntries();

                        moduleConverted3++;
                        EditorUtility.DisplayProgressBar("Converting Settings Files to 2.0",
                            "ID Files Converted: " + moduleConverted3 + " / " + moduleCount3,
                            (float) ((float) moduleConverted3 / (float) moduleCount3));
                    }
                }


                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                EditorUtility.ClearProgressBar();
                
                
                int moduleConverted4 = 0;
                int moduleCount4 = 6;

                EditorUtility.DisplayProgressBar("Converting Settings Lists to 2.0",
                    "Setting Lists Converted: " + moduleConverted4 + " / " + moduleCount4,
                    (float) ((float) moduleConverted4 / (float) moduleCount4));

                RPGBuilderEditorModule damageTypesModule = RPGBuilderEditorUtility.GetModuleByName("Damage Types");
                RPGBuilderEditorModule nodeSocketsModule = RPGBuilderEditorUtility.GetModuleByName("Node Sockets");
                RPGBuilderEditorModule factionStancesModule =
                    RPGBuilderEditorUtility.GetModuleByName("Faction Stances");
                RPGBuilderEditorModule abilityCooldownTagsModule =
                    RPGBuilderEditorUtility.GetModuleByName("Ability Cooldown Tags");
                RPGBuilderEditorModule effectTagsModule = RPGBuilderEditorUtility.GetModuleByName("Effect Tags");
                RPGBuilderEditorModule itemRaritiesModule = RPGBuilderEditorUtility.GetModuleByName("Item Rarities");
                RPGBuilderEditorModule itemTypesModule = RPGBuilderEditorUtility.GetModuleByName("Item Types");
                RPGBuilderEditorModule weaponTypesModule = RPGBuilderEditorUtility.GetModuleByName("Weapon Types");
                RPGBuilderEditorModule armorTypesModule = RPGBuilderEditorUtility.GetModuleByName("Armor Types");
                RPGBuilderEditorModule armorSlotsModule = RPGBuilderEditorUtility.GetModuleByName("Armor Slots");
                RPGBuilderEditorModule weaponSlotsModule =
                    RPGBuilderEditorUtility.GetModuleByName("Weapon Slots");
                RPGBuilderEditorModule weaponHandSlotsModule =
                    RPGBuilderEditorUtility.GetModuleByName("Weapon Hand Slots");
                RPGBuilderEditorModule gemSocketTypesModule =
                    RPGBuilderEditorUtility.GetModuleByName("Gem Socket Types");
                RPGBuilderEditorModule actionKeyCategoriesModule =
                    RPGBuilderEditorUtility.GetModuleByName("Action Key Categories");
                RPGBuilderEditorModule statCategoriesModule =
                    RPGBuilderEditorUtility.GetModuleByName("Stat Categories");
                RPGBuilderEditorModule textKeywordsModule = RPGBuilderEditorUtility.GetModuleByName("Text Keywords");

                RPGBuilderCharacterSettings characterSettings =
                    Resources.Load<RPGBuilderCharacterSettings>(RPGBuilderEditor.Instance.EditorSettings.DatabasePath +
                                                                "Settings/" + "Character_Settings");
                if (characterSettings != null)
                {
                    foreach (var listEntry in characterSettings.StatFunctionsList)
                    {
                        RPGBDamageType newTypeEntry = CreateInstance<RPGBDamageType>();
                        newTypeEntry.entryName = listEntry;
                        newTypeEntry.entryFileName = listEntry + damageTypesModule.AssetNameSuffix;
                        newTypeEntry.entryDisplayName = listEntry;

                        RPGBuilderEditor.Instance.GenerateTypeEntry(damageTypesModule, newTypeEntry);
                    }

                    damageTypesModule.LoadEntries();

                    foreach (var listEntry in characterSettings.nodeSocketNames)
                    {
                        RPGBNodeSocket newTypeEntry = CreateInstance<RPGBNodeSocket>();
                        newTypeEntry.entryName = listEntry;
                        newTypeEntry.entryFileName = listEntry + nodeSocketsModule.AssetNameSuffix;

                        RPGBuilderEditor.Instance.GenerateTypeEntry(nodeSocketsModule, newTypeEntry);
                    }

                    nodeSocketsModule.LoadEntries();

                    moduleConverted4++;
                    EditorUtility.DisplayProgressBar("Converting Settings Lists to 2.0",
                        "Setting Lists Converted: " + moduleConverted4 + " / " + moduleCount4,
                        (float) ((float) moduleConverted4 / (float) moduleCount4));
                }

                RPGBuilderCombatSettings combatSettings =
                    Resources.Load<RPGBuilderCombatSettings>(RPGBuilderEditor.Instance.EditorSettings.DatabasePath +
                                                             "Settings/" + "Combat_Settings");
                if (combatSettings != null)
                {
                    foreach (var listEntry in combatSettings.FactionStancesList)
                    {
                        RPGBFactionStance newTypeEntry = CreateInstance<RPGBFactionStance>();
                        newTypeEntry.entryName = listEntry;
                        newTypeEntry.entryFileName = listEntry + factionStancesModule.AssetNameSuffix;
                        newTypeEntry.entryDisplayName = listEntry;

                        RPGBuilderEditor.Instance.GenerateTypeEntry(factionStancesModule, newTypeEntry);
                    }

                    factionStancesModule.LoadEntries();

                    foreach (var listEntry in combatSettings.AbilityCooldownTagList)
                    {
                        RPGBAbilityCooldownTag newTypeEntry = CreateInstance<RPGBAbilityCooldownTag>();
                        newTypeEntry.entryName = listEntry;
                        newTypeEntry.entryFileName = listEntry + abilityCooldownTagsModule.AssetNameSuffix;
                        newTypeEntry.entryDisplayName = listEntry;

                        RPGBuilderEditor.Instance.GenerateTypeEntry(abilityCooldownTagsModule, newTypeEntry);
                    }

                    abilityCooldownTagsModule.LoadEntries();

                    foreach (var listEntry in combatSettings.EffectTagList)
                    {
                        RPGBEffectTag newTypeEntry = CreateInstance<RPGBEffectTag>();
                        newTypeEntry.entryName = listEntry;
                        newTypeEntry.entryFileName = listEntry + effectTagsModule.AssetNameSuffix;
                        newTypeEntry.entryDisplayName = listEntry;

                        RPGBuilderEditor.Instance.GenerateTypeEntry(effectTagsModule, newTypeEntry);
                    }

                    effectTagsModule.LoadEntries();

                    moduleConverted4++;
                    EditorUtility.DisplayProgressBar("Converting Settings Lists to 2.0",
                        "Setting Lists Converted: " + moduleConverted4 + " / " + moduleCount4,
                        (float) ((float) moduleConverted4 / (float) moduleCount4));
                }

                RPGBuilderEconomySettings economySettings =
                    Resources.Load<RPGBuilderEconomySettings>(RPGBuilderEditor.Instance.EditorSettings.DatabasePath +
                                                              "Settings/" + "Economy_Settings");
                if (economySettings != null)
                {
                    for (var index = 0; index < economySettings.itemRarityList.Count; index++)
                    {
                        var listEntry = economySettings.itemRarityList[index];
                        RPGBItemRarity newTypeEntry = CreateInstance<RPGBItemRarity>();
                        newTypeEntry.entryName = listEntry;
                        newTypeEntry.entryFileName = listEntry + itemRaritiesModule.AssetNameSuffix;
                        newTypeEntry.entryDisplayName = listEntry;
                        newTypeEntry.color = economySettings.itemRarityColorsList[index];
                        newTypeEntry.background = economySettings.itemRarityImagesList[index];

                        RPGBuilderEditor.Instance.GenerateTypeEntry(itemRaritiesModule, newTypeEntry);
                    }

                    itemRaritiesModule.LoadEntries();

                    foreach (var listEntry in economySettings.itemTypeList)
                    {
                        RPGBItemType newTypeEntry = CreateInstance<RPGBItemType>();
                        newTypeEntry.entryName = listEntry;
                        newTypeEntry.entryFileName = listEntry + itemTypesModule.AssetNameSuffix;
                        newTypeEntry.entryDisplayName = listEntry;
                        
                        if (newTypeEntry.entryName == "ARMOR")
                        {
                            newTypeEntry.CanBeEquipped = true;
                            newTypeEntry.EquipType = EconomyData.EquipFunction.Armor;
                        }
                        
                        if (newTypeEntry.entryName == "WEAPON")
                        {
                            newTypeEntry.CanBeEquipped = true;
                            newTypeEntry.EquipType = EconomyData.EquipFunction.Weapon;
                        }
                        
                        if (newTypeEntry.entryName == "CURRENCY")
                        {
                            newTypeEntry.ItemTypeFunction = EconomyData.ItemTypeFunction.Currency;
                        }
                        
                        if (newTypeEntry.entryName == "GEM")
                        {
                            newTypeEntry.ItemTypeFunction = EconomyData.ItemTypeFunction.Gem;
                        }
                        
                        if (newTypeEntry.entryName == "ENCHANTMENT")
                        {
                            newTypeEntry.ItemTypeFunction = EconomyData.ItemTypeFunction.Enchantment;
                        }

                        RPGBuilderEditor.Instance.GenerateTypeEntry(itemTypesModule, newTypeEntry);
                    }

                    itemTypesModule.LoadEntries();

                    foreach (var listEntry in economySettings.weaponTypeList)
                    {
                        RPGBWeaponType newTypeEntry = CreateInstance<RPGBWeaponType>();
                        newTypeEntry.entryName = listEntry;
                        newTypeEntry.entryFileName = listEntry + weaponTypesModule.AssetNameSuffix;
                        newTypeEntry.entryDisplayName = listEntry;

                        RPGBuilderEditor.Instance.GenerateTypeEntry(weaponTypesModule, newTypeEntry);
                    }

                    weaponTypesModule.LoadEntries();

                    foreach (var listEntry in economySettings.armorTypeList)
                    {
                        RPGBArmorType newTypeEntry = CreateInstance<RPGBArmorType>();
                        newTypeEntry.entryName = listEntry;
                        newTypeEntry.entryFileName = listEntry + armorTypesModule.AssetNameSuffix;
                        newTypeEntry.entryDisplayName = listEntry;

                        RPGBuilderEditor.Instance.GenerateTypeEntry(armorTypesModule, newTypeEntry);
                    }

                    armorTypesModule.LoadEntries();

                    foreach (var listEntry in economySettings.armorSlotsList)
                    {
                        RPGBArmorSlot newTypeEntry = CreateInstance<RPGBArmorSlot>();
                        newTypeEntry.entryName = listEntry;
                        newTypeEntry.entryFileName = listEntry + armorSlotsModule.AssetNameSuffix;
                        newTypeEntry.entryDisplayName = listEntry;

                        RPGBuilderEditor.Instance.GenerateTypeEntry(armorSlotsModule, newTypeEntry);
                    }

                    armorSlotsModule.LoadEntries();

                    foreach (var listEntry in economySettings.slotTypeList)
                    {
                        RPGBWeaponHandSlot newTypeEntry = CreateInstance<RPGBWeaponHandSlot>();
                        newTypeEntry.entryName = listEntry;
                        newTypeEntry.entryFileName = listEntry + weaponHandSlotsModule.AssetNameSuffix;
                        newTypeEntry.entryDisplayName = listEntry;

                        RPGBuilderEditor.Instance.GenerateTypeEntry(weaponHandSlotsModule, newTypeEntry);
                    }

                    weaponHandSlotsModule.LoadEntries();

                    foreach (var listEntry in economySettings.socketTypeList)
                    {
                        RPGBGemSocketType newTypeEntry = CreateInstance<RPGBGemSocketType>();
                        newTypeEntry.entryName = listEntry;
                        newTypeEntry.entryFileName = listEntry + gemSocketTypesModule.AssetNameSuffix;
                        newTypeEntry.entryDisplayName = listEntry;

                        RPGBuilderEditor.Instance.GenerateTypeEntry(gemSocketTypesModule, newTypeEntry);
                    }

                    gemSocketTypesModule.LoadEntries();

                    moduleConverted4++;
                    EditorUtility.DisplayProgressBar("Converting Settings Lists to 2.0",
                        "Setting Lists Converted: " + moduleConverted4 + " / " + moduleCount4,
                        (float) ((float) moduleConverted4 / (float) moduleCount4));
                }

                RPGBuilderGeneralSettings generalSettings =
                    Resources.Load<RPGBuilderGeneralSettings>(RPGBuilderEditor.Instance.EditorSettings.DatabasePath +
                                                              "Settings/" + "General_Settings");
                if (generalSettings != null)
                {
                    foreach (var listEntry in generalSettings.ActionKeyCategoryList)
                    {
                        RPGBActionKeyCategory newTypeEntry = CreateInstance<RPGBActionKeyCategory>();
                        newTypeEntry.entryName = listEntry;
                        newTypeEntry.entryFileName = listEntry + actionKeyCategoriesModule.AssetNameSuffix;
                        newTypeEntry.entryDisplayName = listEntry;

                        RPGBuilderEditor.Instance.GenerateTypeEntry(actionKeyCategoriesModule, newTypeEntry);
                    }

                    actionKeyCategoriesModule.LoadEntries();

                    moduleConverted4++;
                    EditorUtility.DisplayProgressBar("Converting Settings Lists to 2.0",
                        "Setting Lists Converted: " + moduleConverted4 + " / " + moduleCount4,
                        (float) ((float) moduleConverted4 / (float) moduleCount4));
                }

                RPGBuilderUISettings uiSettings =
                    Resources.Load<RPGBuilderUISettings>(RPGBuilderEditor.Instance.EditorSettings.DatabasePath +
                                                         "Settings/" + "UI_Settings");
                if (uiSettings != null)
                {
                    foreach (var listEntry in uiSettings.UIStatsCategoriesList)
                    {
                        RPGBStatCategory newTypeEntry = CreateInstance<RPGBStatCategory>();
                        newTypeEntry.entryName = listEntry;
                        newTypeEntry.entryFileName = listEntry + statCategoriesModule.AssetNameSuffix;
                        newTypeEntry.entryDisplayName = listEntry;

                        RPGBuilderEditor.Instance.GenerateTypeEntry(statCategoriesModule, newTypeEntry);
                    }

                    statCategoriesModule.LoadEntries();

                    moduleConverted4++;
                    EditorUtility.DisplayProgressBar("Converting Settings Lists to 2.0",
                        "Setting Lists Converted: " + moduleConverted4 + " / " + moduleCount4,
                        (float) ((float) moduleConverted4 / (float) moduleCount4));
                }

                RPGBuilderWorldSettings worldSettings =
                    Resources.Load<RPGBuilderWorldSettings>(RPGBuilderEditor.Instance.EditorSettings.DatabasePath +
                                                            "Settings/" + "World_Settings");
                if (worldSettings != null)
                {
                    foreach (var listEntry in worldSettings.dialogueKeywordsList)
                    {
                        RPGBTextKeyword newTypeEntry = CreateInstance<RPGBTextKeyword>();
                        newTypeEntry.entryName = listEntry;
                        newTypeEntry.entryFileName = listEntry + textKeywordsModule.AssetNameSuffix;
                        newTypeEntry.entryDisplayName = listEntry;

                        RPGBuilderEditor.Instance.GenerateTypeEntry(textKeywordsModule, newTypeEntry);
                    }

                    textKeywordsModule.LoadEntries();

                    moduleConverted4++;
                    EditorUtility.DisplayProgressBar("Converting Settings Lists to 2.0",
                        "Setting Lists Converted: " + moduleConverted4 + " / " + moduleCount4,
                        (float) ((float) moduleConverted4 / (float) moduleCount4));
                }
                
                RPGBuilderEditorModule genderModule = RPGBuilderEditorUtility.GetModuleByName("Genders");
                RPGBGender maleGender = CreateInstance<RPGBGender>();
                maleGender.entryName = "Male";
                maleGender.entryFileName = "Male" + genderModule.AssetNameSuffix;
                maleGender.entryDisplayName = "Male";
                RPGBuilderEditor.Instance.GenerateTypeEntry(genderModule, maleGender);
                
                RPGBGender femaleGender = CreateInstance<RPGBGender>();
                femaleGender.entryName = "Female";
                femaleGender.entryFileName = "Female" + genderModule.AssetNameSuffix;
                femaleGender.entryDisplayName = "Female";
                RPGBuilderEditor.Instance.GenerateTypeEntry(genderModule, femaleGender);
                genderModule.LoadEntries();

                RPGBuilderEditorModule itemModule = RPGBuilderEditorUtility.GetModuleByName("Items");
                RPGBuilderEditorModule weaponTransformsModule = RPGBuilderEditorUtility.GetModuleByName("Weapon Transforms");
                foreach (var weapon in itemModule.databaseEntries)
                {
                    RPGItem item = (RPGItem) weapon;
                    if (item.WeaponType != null)
                    {
                        WeaponTransformTemplate newWeaponTransform = CreateInstance<WeaponTransformTemplate>();
                        newWeaponTransform.entryName = item.name;
                        newWeaponTransform.entryFileName = item.name + weaponTransformsModule.AssetNameSuffix;
                        newWeaponTransform.entryDisplayName = item.name;

                        WeaponTransform newWpTransform = new WeaponTransform();

                        foreach (var weaponPosData in item.weaponPositionDatas)
                        {
                            newWpTransform.raceID = weaponPosData.raceID;

                            for (var index = 0; index < weaponPosData.genderPositionDatas.Count; index++)
                            {
                                var posData = weaponPosData.genderPositionDatas[index];
                                WeaponTransform.TransformValues transformValues = new WeaponTransform.TransformValues
                                {
                                    gender = index == 0
                                        ? (RPGBGender) genderModule.databaseEntries[1]
                                        : (RPGBGender) genderModule.databaseEntries[0],
                                    CombatPosition = posData.CombatPositionInSlot,
                                    CombatRotation = posData.CombatRotationInSlot,
                                    CombatScale = posData.CombatScaleInSlot,
                                    RestPosition = posData.RestPositionInSlot,
                                    RestRotation = posData.RestRotationInSlot,
                                    RestScale = posData.RestScaleInSlot,
                                    CombatPosition2 = posData.CombatPositionInSlot2,
                                    CombatRotation2 = posData.CombatRotationInSlot2,
                                    CombatScale2 = posData.CombatScaleInSlot2,
                                    RestPosition2 = posData.RestPositionInSlot2,
                                    RestRotation2 = posData.RestRotationInSlot2,
                                    RestScale2 = posData.RestScaleInSlot2
                                };

                                newWpTransform.transformValues.Add(transformValues);
                            }

                            newWeaponTransform.WeaponTransforms.Add(newWpTransform);
                        }
                        RPGBuilderEditor.Instance.GenerateTypeEntry(weaponTransformsModule, newWeaponTransform);
                    }
                }
                weaponTransformsModule.LoadEntries();

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                EditorUtility.ClearProgressBar();
                
                int moduleConverted5 = 0;
                int moduleCount5 = 50;

                EditorUtility.DisplayProgressBar("Convert Strings Type Entries",
                    "Modules Converted: " + moduleConverted5 + " / " + moduleCount5,
                    (float) ((float) moduleConverted5 / (float) moduleCount5));

                foreach (var module in RPGBuilderEditor.Instance.EditorCategories.SelectMany(category =>
                    category.modules.Where(module => !module.IsSettingModule && !module.IsTypeModule)))
                {
                    module.ConvertStringsToTypeEntries();
                    module.LoadEntries();
                    moduleConverted5++;
                    EditorUtility.DisplayProgressBar("Convert Strings Type Entries",
                        "Modules Converted: " + moduleConverted5 + " / " + moduleCount5,
                        (float) ((float) moduleConverted5 / (float) moduleCount5));
                }

                EditorUtility.ClearProgressBar();
                
                int moduleConverted6 = 0;
                int moduleCount6 = 0;
                List<RPGAbility> abilityList = Resources
                    .LoadAll<RPGAbility>(RPGBuilderEditor.Instance.EditorSettings.DatabasePath).ToList();
                moduleCount6 += abilityList.Count;
                List<RPGEffect> effectList = Resources
                    .LoadAll<RPGEffect>(RPGBuilderEditor.Instance.EditorSettings.DatabasePath).ToList();
                moduleCount6 += effectList.Count;

                EditorUtility.DisplayProgressBar("Converting Visual Effects to Templates",
                    "Created Templates: " + moduleConverted6 + " / " + moduleCount6,
                    (float) ((float) moduleConverted6 / (float) moduleCount6));

                RPGBuilderEditorModule visualEffectsModule = RPGBuilderEditorUtility.GetModuleByName("Visual Effects");
                foreach (var ability in abilityList)
                {
                    foreach (var rank in ability.ranks)
                    {
                        rank.VisualEffectEntries.Clear();
                        foreach (var visualEffect in rank.visualEffects)
                        {
                            if(visualEffect.EffectGO == null) continue;
                            EditorUtility.SetDirty(ability);
                            VisualEffectTemplate newTemplate = CreateInstance<VisualEffectTemplate>();
                            newTemplate.entryName = visualEffect.EffectGO.name;
                            newTemplate.entryFileName = newTemplate.entryName + visualEffectsModule.AssetNameSuffix;
                            newTemplate.Prefabs.Add(visualEffect.EffectGO);
                            newTemplate.ParentSoundToPrefab = visualEffect.SoundParentedToEffect;
                            newTemplate.IsDestroyedOnDeath = visualEffect.isDestroyedOnDeath;
                            newTemplate.IsDestroyedOnStun = visualEffect.isDestroyedOnDeath;
                            newTemplate.IsDestroyedOnStealth = visualEffect.isDestroyedOnStealth;
                            newTemplate.IsDestroyedOnStealthEnd = visualEffect.isDestroyedOnStealthEnd;

                            VisualEffectTemplate newTemplateFile = (VisualEffectTemplate)
                                RPGBuilderEditor.Instance.GenerateAndGetTypeEntry(visualEffectsModule, newTemplate);
                            EditorUtility.SetDirty(newTemplateFile);
                            AssetDatabase.SaveAssets();
                            
                            VisualEffectEntry newVisualEffectEntry = new VisualEffectEntry
                            {
                                Template = newTemplateFile
                            };

                            switch (visualEffect.activationType)
                            {
                                case RPGCombatDATA.CombatVisualActivationType.Activate:
                                    newVisualEffectEntry.ActivationType = ActivationType.Start;
                                    break;
                                case RPGCombatDATA.CombatVisualActivationType.CastCompleted:
                                    newVisualEffectEntry.ActivationType = ActivationType.Completed;
                                    break;
                                case RPGCombatDATA.CombatVisualActivationType.Completed:
                                    newVisualEffectEntry.ActivationType = ActivationType.Completed;
                                    break;
                                case RPGCombatDATA.CombatVisualActivationType.Interrupted:
                                    newVisualEffectEntry.ActivationType = ActivationType.Interrupted;
                                    break;
                            }
                            
                            newVisualEffectEntry.UseNodeSocket = visualEffect.UseNodeSocket;
                            newVisualEffectEntry.NodeSocket = visualEffect.NodeSocket;
                            newVisualEffectEntry.PositionOffset = visualEffect.positionOffset;
                            newVisualEffectEntry.ParentedToCaster = visualEffect.ParentedToCaster;
                            newVisualEffectEntry.Scale = visualEffect.effectScale;
                            newVisualEffectEntry.Duration = visualEffect.duration;
                            newVisualEffectEntry.Delay = visualEffect.delay;
                            
                            rank.VisualEffectEntries.Add(newVisualEffectEntry);
                            EditorUtility.SetDirty(ability);
                        }
                    }
                    
                    moduleConverted6++;
                    EditorUtility.DisplayProgressBar("Converting Visual Effects to Templates",
                        "Created Templates: " + moduleConverted6 + " / " + moduleCount6,
                        (float) ((float) moduleConverted6 / (float) moduleCount6));
                }
                
                foreach (var effect in effectList)
                {
                    foreach (var rank in effect.ranks)
                    {
                        rank.VisualEffectEntries.Clear();
                        foreach (var visualEffect in rank.visualEffects)
                        {
                            if(visualEffect.EffectGO == null) continue;
                            EditorUtility.SetDirty(effect);
                            VisualEffectTemplate newTemplate = CreateInstance<VisualEffectTemplate>();
                            newTemplate.entryName = visualEffect.EffectGO.name;
                            newTemplate.entryFileName = newTemplate.entryName + visualEffectsModule.AssetNameSuffix;
                            newTemplate.Prefabs.Add(visualEffect.EffectGO);
                            newTemplate.ParentSoundToPrefab = visualEffect.SoundParentedToEffect;
                            newTemplate.IsDestroyedOnDeath = visualEffect.isDestroyedOnDeath;
                            newTemplate.IsDestroyedOnStun = visualEffect.isDestroyedOnDeath;
                            newTemplate.IsDestroyedOnStealth = visualEffect.isDestroyedOnStealth;
                            newTemplate.IsDestroyedOnStealthEnd = visualEffect.isDestroyedOnStealthEnd;

                            VisualEffectTemplate newTemplateFile = (VisualEffectTemplate)
                                RPGBuilderEditor.Instance.GenerateAndGetTypeEntry(visualEffectsModule, newTemplate);
                            EditorUtility.SetDirty(newTemplateFile);
                            AssetDatabase.SaveAssets();
                            
                            VisualEffectEntry newVisualEffectEntry = new VisualEffectEntry
                            {
                                Template = newTemplateFile
                            };

                            switch (visualEffect.activationType)
                            {
                                case RPGCombatDATA.CombatVisualActivationType.Activate:
                                    newVisualEffectEntry.ActivationType = ActivationType.Start;
                                    break;
                                case RPGCombatDATA.CombatVisualActivationType.CastCompleted:
                                    newVisualEffectEntry.ActivationType = ActivationType.Completed;
                                    break;
                                case RPGCombatDATA.CombatVisualActivationType.Completed:
                                    newVisualEffectEntry.ActivationType = ActivationType.Completed;
                                    break;
                                case RPGCombatDATA.CombatVisualActivationType.Interrupted:
                                    newVisualEffectEntry.ActivationType = ActivationType.Interrupted;
                                    break;
                            }
                            
                            newVisualEffectEntry.UseNodeSocket = visualEffect.UseNodeSocket;
                            newVisualEffectEntry.NodeSocket = visualEffect.NodeSocket;
                            newVisualEffectEntry.PositionOffset = visualEffect.positionOffset;
                            newVisualEffectEntry.ParentedToCaster = visualEffect.ParentedToCaster;
                            newVisualEffectEntry.Scale = visualEffect.effectScale;
                            newVisualEffectEntry.Duration = visualEffect.duration;
                            newVisualEffectEntry.Delay = visualEffect.delay;
                            
                            rank.VisualEffectEntries.Add(newVisualEffectEntry);
                            EditorUtility.SetDirty(effect);
                        }
                    }
                    
                    moduleConverted6++;
                    EditorUtility.DisplayProgressBar("Converting Visual Effects to Templates",
                        "Created Templates: " + moduleConverted6 + " / " + moduleCount6,
                        (float) ((float) moduleConverted6 / (float) moduleCount6));
                }
                
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                visualEffectsModule.LoadEntries();
                EditorUtility.ClearProgressBar();
                
                
                int moduleConverted7 = 0;
                int moduleCount7 = 0;
                List<RPGNpc> NPCList = Resources
                    .LoadAll<RPGNpc>(RPGBuilderEditor.Instance.EditorSettings.DatabasePath).ToList();
                moduleCount7 += NPCList.Count;

                EditorUtility.DisplayProgressBar("Converting NPCs to Presets",
                    "Created Presets: " + moduleConverted7 + " / " + moduleCount7,
                    (float) ((float) moduleConverted7 / (float) moduleCount7));

                RPGBuilderEditorModule npcPresetsModule = RPGBuilderEditorUtility.GetModuleByName("Presets");
                foreach (var NPC in NPCList)
                {
                    EditorUtility.SetDirty(NPC);
                    NPCPresetTemplate newTemplate = CreateInstance<NPCPresetTemplate>();
                    newTemplate.entryName = NPC.name;
                    newTemplate.entryFileName = newTemplate.entryName + npcPresetsModule.AssetNameSuffix;
                    newTemplate.entryIcon = NPC.icon;

                    newTemplate.Position = NPC.modelPosition;
                    newTemplate.Scale = NPC.modelScale;
                    newTemplate.ColliderCenter = NPC.colliderCenter;
                    newTemplate.ColliderSize = NPC.colliderSize;
                    newTemplate.Prefab = NPC.NPCVisual;
                    newTemplate.AnimatorAvatar = NPC.animatorAvatar;
                    newTemplate.AnimatorController = NPC.animatorController;
                    newTemplate.ColliderHeight = NPC.colliderHeight;
                    newTemplate.ColliderRadius = NPC.colliderRadius;
                    switch (NPC.colliderType)
                    {
                        case RPGNpc.NPCColliderType.Capsule:
                            newTemplate.ColliderType = AIData.NPCColliderType.Capsule;
                            break;
                        case RPGNpc.NPCColliderType.Sphere:
                            newTemplate.ColliderType = AIData.NPCColliderType.Sphere;
                            break;
                        case RPGNpc.NPCColliderType.Box:
                            newTemplate.ColliderType = AIData.NPCColliderType.Box;
                            break;
                    }
                    newTemplate.AnimatorCullingMode = NPC.AnimatorCullingMode;
                    newTemplate.AnimatorUpdateMode = NPC.animatorUpdateMode;
                    newTemplate.NameplateYOffset = NPC.nameplateYOffset;
                    newTemplate.NavmeshAgentHeight = NPC.navmeshAgentHeight;
                    newTemplate.NavmeshAgentRadius = NPC.navmeshAgentRadius;
                    newTemplate.NavmeshObstacleAvoidance = NPC.navmeshObstacleAvoidance;
                    newTemplate.AnimatorUseRootMotion = NPC.animatorUseRootMotion;
                    newTemplate.NavmeshAgentAngularSpeed = NPC.navmeshAgentAngularSpeed;

                    NPCPresetTemplate newTemplateFile = (NPCPresetTemplate)
                        RPGBuilderEditor.Instance.GenerateAndGetTypeEntry(npcPresetsModule, newTemplate);
                    EditorUtility.SetDirty(newTemplateFile);
                    AssetDatabase.SaveAssets();

                    switch (NPC.npcType)
                    {
                        case RPGNpc.NPC_TYPE.MERCHANT:
                            NPC.isMerchant = true;
                            NPC.MerchantTables.Add(new AIData.NPCMerchantTable{MerchantTableID = NPC.merchantTableID});
                            break;
                        case RPGNpc.NPC_TYPE.QUEST_GIVER:
                            NPC.isQuestGiver = true;
                            break;
                        case RPGNpc.NPC_TYPE.DIALOGUE:
                            NPC.isDialogue = true;
                            break;
                    }

                    NPC.AILogicTemplate = RPGBuilderEditor.Instance.EditorData.AILogicTemplate;
                    
                    EditorUtility.SetDirty(NPC);

                    moduleConverted7++;
                    EditorUtility.DisplayProgressBar("Converting NPCs to Presets",
                        "Created Presets: " + moduleConverted7 + " / " + moduleCount7,
                        (float) ((float) moduleConverted7 / (float) moduleCount7));
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                npcPresetsModule.LoadEntries();
                EditorUtility.ClearProgressBar();
            }

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(30);
        GUILayout.EndScrollView();
    }
}

