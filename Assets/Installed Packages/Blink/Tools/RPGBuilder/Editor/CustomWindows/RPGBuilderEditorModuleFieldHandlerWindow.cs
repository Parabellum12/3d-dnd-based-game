using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class RPGBuilderEditorModuleFieldHandlerWindow : EditorWindow
{
    
    private RPGBuilderEditorDATA editorDATA;
    private RPGBuilderEditorFilters editorFilters;
    private GUISkin skin;

    private bool alreadyOpened;
    
    //[MenuItem("BLINK/Module Handler")]
    private static void Init()
    {
        // Get existing open window or if none, make a new one:
        var window = (RPGBuilderEditorModuleFieldHandlerWindow) GetWindow(typeof(RPGBuilderEditorModuleFieldHandlerWindow));
        window.Show();
    }
    
    private void OnEnable()
    {
        if (!alreadyOpened)
        {
            editorDATA = Resources.Load<RPGBuilderEditorDATA>("EditorData/RPGBuilderEditorData");
            skin = Resources.Load<GUISkin>(editorDATA.RPGBEditorDataPath + "RPGBuilderSkin");
            editorFilters = Resources.Load<RPGBuilderEditorFilters>(editorDATA.RPGBEditorDataPath + "RPGBuilderEditorFilters");
        }

        alreadyOpened = true;
    }

    private Vector2 filtersScrollPosition;
    private void OnGUI()
    {
        Rect panelRect = new Rect(0, 0, Screen.width, Screen.height);
        GUILayout.BeginArea(panelRect);
        
        filtersScrollPosition = GUILayout.BeginScrollView(filtersScrollPosition, false, false,GUILayout.Width(Screen.width), GUILayout.Height(position.height));

        GUILayout.BeginHorizontal();
        
        EditorGUILayout.LabelField("Asset Type", GUILayout.Width(editorDATA.filterLabelFieldWidth));
        
        GUILayout.EndHorizontal();
        
        EditorGUILayout.BeginVertical();
        DrawAssetModule();
        EditorGUILayout.EndVertical();

        GUILayout.Space(30);
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private void DrawAssetModule()
    {
        RPGBuilderEditorFilters.ModuleContentData moduleContentData = getModuleContentData();

        if (moduleContentData != null)
        {
            if (GUILayout.Button("Update Fields", skin.GetStyle("HorizontalAddButton"), GUILayout.MinWidth(150), GUILayout.ExpandWidth(true), GUILayout.Height(20)))
            {
                if (EditorUtility.DisplayDialog("Confirm UPDATE",
                    "This will update the data to the current class fields", "YES", "Cancel"))
                {
                    UpdateModuleContentData(moduleContentData);
                }
            }
            GUILayout.Space(8);
            
            if (GUILayout.Button("SAVE DATA", skin.GetStyle("HorizontalAddButton"), GUILayout.MinWidth(150), GUILayout.ExpandWidth(true), GUILayout.Height(20)))
            {
                if (EditorUtility.DisplayDialog("SAVE",
                    "This will save the data", "YES", "Cancel"))
                {
                    SaveModuleData();
                }
            }
            GUILayout.Space(8);

            foreach (var category in moduleContentData.categories)
            {
                category.categoryDisplayName = EditorGUILayout.TextField(category.categoryName, category.categoryDisplayName);

                foreach (var field in category.fields)
                {
                    if (!field.display)
                    {
                        EditorGUI.BeginDisabledGroup(true);
                    }

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(field.fieldBaseName);

                    for (var index = 0; index < field.fieldNames.Count; index++)
                    {
                        field.fieldNames[index] = EditorGUILayout.TextField(field.fieldNames[index], GUILayout.MinWidth(200));
                    }
                    if (!field.display)
                    {
                        EditorGUI.EndDisabledGroup();
                    }
                    
                    field.display = EditorGUILayout.Toggle(field.display);
                    
                    GUILayout.EndHorizontal();
                    
                    
                }
            }
        }
        else
        {
            if (!GUILayout.Button("Generate Fields", skin.GetStyle("HorizontalAddButton"), GUILayout.MinWidth(150),
                GUILayout.ExpandWidth(true), GUILayout.Height(20))) return;
            if (EditorUtility.DisplayDialog("Confirm GENERATE", "This will clear the modules data, do not do it if you do not know what this is",
                "YES", "Cancel"))
            {
                GenerateModuleContentData();
            }
        }
        
    }

    private void SaveModuleData()
    {
        EditorUtility.SetDirty(editorFilters);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    
    RPGBuilderEditorFilters.ModuleContentData temporaryModuleContentData = new RPGBuilderEditorFilters.ModuleContentData();
    private void UpdateModuleContentData(RPGBuilderEditorFilters.ModuleContentData moduleContentData)
    {
        temporaryModuleContentData = new RPGBuilderEditorFilters.ModuleContentData();
        temporaryModuleContentData.categories = moduleContentData.categories;
        temporaryModuleContentData.moduleName = moduleContentData.moduleName;
        temporaryModuleContentData.thisType = getAssetType();
        temporaryModuleContentData.moduleDisplayName = moduleContentData.moduleDisplayName;
        
        editorFilters.modules.Remove(moduleContentData);
        
        RPGBuilderEditorFilters.ModuleContentData newModuleContentData = new RPGBuilderEditorFilters.ModuleContentData();
        newModuleContentData.moduleName = temporaryModuleContentData.moduleName;
        newModuleContentData.thisType =  getAssetType();
        newModuleContentData.moduleDisplayName = temporaryModuleContentData.moduleDisplayName;
        GenerateCategories(newModuleContentData.moduleName, newModuleContentData.thisType.ToString(), newModuleContentData, getAssetType().ToString());
        
        editorFilters.modules.Add(newModuleContentData);

        foreach (var module in editorFilters.modules)
        {
            if(module.moduleName != newModuleContentData.moduleName) continue;

            foreach (var category in module.categories)
            {
                RPGBuilderEditorFilters.ModuleCategory existingCategory = getModuleCategory(category.categoryName);
                if (existingCategory != null)
                {
                    category.showInEditor = existingCategory.showInEditor;
                    category.categoryDisplayName = existingCategory.categoryDisplayName;

                    foreach (var field in category.fields)
                    {
                        
                        RPGBuilderEditorFilters.CategoryFieldData existingField = getCategoryField(category.categoryName, field.fieldBaseName);
                        if (existingField != null)
                        {
                            field.fieldNames = existingField.fieldNames;
                        }
                    }
                }
            }
        }

        temporaryModuleContentData = null;
        
        EditorUtility.SetDirty(editorFilters);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private RPGBuilderEditorFilters.ModuleCategory getModuleCategory(string categoryName)
    {
        foreach (var category in temporaryModuleContentData.categories)
        {
            if(category.categoryName != categoryName) continue;
            return category;
        }

        return null;
    }
    private RPGBuilderEditorFilters.CategoryFieldData getCategoryField(string categoryName, string fieldName)
    {
        foreach (var category in temporaryModuleContentData.categories)
        {
            if(category.categoryName != categoryName) continue;
            foreach (var field in category.fields)
            {
                if(field.fieldBaseName != fieldName) continue;
                return field;
            }
        }

        return null;
    }
    
    

   private void GenerateModuleContentData()
    {
        RPGBuilderEditorFilters.ModuleContentData newModuleContentData = new RPGBuilderEditorFilters.ModuleContentData();
        newModuleContentData.moduleName = getAssetType().ToString();
        newModuleContentData.thisType = getAssetType();
        GenerateCategories(newModuleContentData.moduleName, newModuleContentData.thisType.ToString(), newModuleContentData, getAssetType().ToString());
        
        editorFilters.modules.Add(newModuleContentData);
        
        EditorUtility.SetDirty(editorFilters);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    
    private void GenerateCategories(string categoryName, string typeName, RPGBuilderEditorFilters.ModuleContentData module, string parentType)
    {
        RPGBuilderEditorFilters.ModuleCategory newCategory = new RPGBuilderEditorFilters.ModuleCategory();
        newCategory.categoryName = categoryName;
        newCategory.fields = GETFields(GetType(typeName));
        newCategory.thisType = GetType(typeName);
        newCategory.parentType = parentType;
        module.categories.Add(newCategory);

        foreach (var field in newCategory.fields)
        {
            if (field.thisType.ToString().Contains("+") && field.thisType.ToString().Contains("System.Collections.Generic.List"))
            {
                if (field.thisType.ToString().Contains(newCategory.parentType))
                {
                    // THIS LOCAL FIELD LIST HAD A LIST IN IT
                    string fieldTypeToString = field.thisType.ToString();
                    int indexOfBeginning = fieldTypeToString.IndexOf("[");
                    string finalName = fieldTypeToString.Remove(0, indexOfBeginning + 1);
                    finalName = finalName.Remove(finalName.Length - 1);

                    GenerateCategories(finalName, finalName, module, newCategory.parentType);
                    field.display = false;
                }
                else
                {
                    field.display = false;
                }
            } else if (field.fieldBaseName.Contains("REF"))
            {
                field.display = false;
            }
        }
    }

    private List<RPGBuilderEditorFilters.CategoryFieldData> GETFields(Type type)
    {
        List<RPGBuilderEditorFilters.CategoryFieldData> fieldList = new List<RPGBuilderEditorFilters.CategoryFieldData>();
        var fieldValues = type.GetFields ();

        foreach (var field in fieldValues)
        {
            RPGBuilderEditorFilters.CategoryFieldData newVar = new RPGBuilderEditorFilters.CategoryFieldData();
            newVar.thisType = field.FieldType;
            newVar.fieldBaseName = field.Name;
            newVar.fieldNames.Add(newVar.fieldBaseName);
            
            fieldList.Add(newVar);
        }
        
        return fieldList;
    }
    public static Type GetType(string TypeName)
    {
        var type = Type.GetType(TypeName);

        if (type != null)
            return type;

        if (TypeName.Contains("."))
        {
            var assemblyName = TypeName.Substring(0, TypeName.IndexOf('.'));
            var assembly = Assembly.Load(assemblyName);
            if (assembly == null)
                return null;

            type = assembly.GetType(TypeName);
            if (type != null)
                return type;

        }

        var currentAssembly = Assembly.GetExecutingAssembly();
        var referencedAssemblies = currentAssembly.GetReferencedAssemblies();
        foreach (var assemblyName in referencedAssemblies)
        {

            var assembly = Assembly.Load(assemblyName);
            if (assembly == null) continue;
            type = assembly.GetType(TypeName);
            if (type != null)
                return type;
        }

        return null;
    }

    private Type getAssetType()
    {
        /*
        switch (currentAssetType)
        {
            case RPGBuilderEditor.AssetType.Ability:
                return typeof(RPGAbility);
            case RPGBuilderEditor.AssetType.Effect:
                return typeof(RPGEffect);
            case RPGBuilderEditor.AssetType.Item:
                return typeof(RPGItem);
            case RPGBuilderEditor.AssetType.NPC:
                return typeof(RPGNpc);
            case RPGBuilderEditor.AssetType.Stat:
                return typeof(RPGStat);
            case RPGBuilderEditor.AssetType.Skill:
                return typeof(RPGSkill);
            case RPGBuilderEditor.AssetType.LevelTemplate:
                return typeof(RPGLevelsTemplate);
            case RPGBuilderEditor.AssetType.Race:
                return typeof(RPGRace);
            case RPGBuilderEditor.AssetType.Class:
                return typeof(RPGClass);
            case RPGBuilderEditor.AssetType.TalentTree:
                return typeof(RPGTalentTree);
            case RPGBuilderEditor.AssetType.TreePoint:
                return typeof(RPGTreePoint);
            case RPGBuilderEditor.AssetType.LootTable:
                return typeof(RPGLootTable);
            case RPGBuilderEditor.AssetType.WorldPosition:
                return typeof(RPGWorldPosition);
            case RPGBuilderEditor.AssetType.MerchantTable:
                return typeof(RPGMerchantTable);
            case RPGBuilderEditor.AssetType.Currency:
                return typeof(RPGCurrency);
            case RPGBuilderEditor.AssetType.Task:
                return typeof(RPGTask);
            case RPGBuilderEditor.AssetType.Quest:
                return typeof(RPGQuest);
            case RPGBuilderEditor.AssetType.CraftingRecipe:
                return typeof(RPGCraftingRecipe);
            case RPGBuilderEditor.AssetType.CraftingStation:
                return typeof(RPGCraftingStation);
            case RPGBuilderEditor.AssetType.ResourceNode:
                return typeof(RPGResourceNode);
            case RPGBuilderEditor.AssetType.Bonus:
                return typeof(RPGBonus);
            case RPGBuilderEditor.AssetType.GameScene:
                return typeof(RPGGameScene);
            case RPGBuilderEditor.AssetType.GearSet:
                return typeof(RPGGearSet);
            case RPGBuilderEditor.AssetType.Enchantment:
                return typeof(RPGEnchantment);
            case RPGBuilderEditor.AssetType.Spellbook:
                return typeof(RPGSpellbook);
            case RPGBuilderEditor.AssetType.Faction:
                return typeof(RPGFaction);
            case RPGBuilderEditor.AssetType.WeaponTemplate:
                return typeof(RPGWeaponTemplate);
            case RPGBuilderEditor.AssetType.Dialogue:
                return typeof(RPGDialogue);
            case RPGBuilderEditor.AssetType.GameModifier:
                return typeof(RPGGameModifier);
            case RPGBuilderEditor.AssetType.Species:
                return typeof(RPGSpecies);
            case RPGBuilderEditor.AssetType.Combo:
                return typeof(RPGCombo);
            default:
                return null;
        }
        */
        return null;
    }

    private RPGBuilderEditorFilters.ModuleContentData getModuleContentData()
    {
        foreach (var module in editorFilters.modules)
        {
            if(module.moduleName != getAssetType().ToString()) continue;
            return module;
        }

        return null;
    }
}
