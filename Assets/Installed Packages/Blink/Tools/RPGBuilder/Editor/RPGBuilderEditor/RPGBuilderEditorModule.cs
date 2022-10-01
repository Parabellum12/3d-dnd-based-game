using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using BindingFlags = System.Reflection.BindingFlags;
using FieldInfo = System.Reflection.FieldInfo;

public abstract class RPGBuilderEditorModule : ScriptableObject
{
    [Header("GENERAL INFO")]
    public string ModuleName;
    public string EntryType;
    public string TypeName;
    public string AssetFolderName;
    public string AssetNameSuffix;
    public string OldFolderName;
    public string IDFileName;
    public string OldIDFileName;
    public string DocumentationLink;
    public bool IsSettingModule;
    public bool IsTypeModule;

    [Header("DISPLAY SETTINGS")]
    public bool IsEnabled = true;
    public bool ShowIconInList;

    [Header("EXPORT/IMPORT")]
    public bool ExportEnabled = true;
    public bool ImportEnabled = true;
    public string EntryAttributeName;

    [Header("FUNCTIONALITIES")]
    public bool ShowEntrySearchBar = true;
    public bool ShowSaveButton = true;
    public bool ShowNewButton = true;
    public bool ShowDuplicateButton = true;
    public bool ShowDeleteButton = true;
    public bool ShowDocumentationButton = true;
    public bool ShowFiltersButton = true;

    [Header("DATA")]
    protected string searchText;
    public List<RPGBuilderDatabaseEntry> databaseEntries = new List<RPGBuilderDatabaseEntry>();

    public abstract void Initialize();
    public abstract void InstantiateCurrentEntry(int index);
    public abstract void LoadEntries();
    public abstract void CreateNewEntry();
    public abstract bool SaveConditionsMet();
    public abstract void UpdateEntryData(RPGBuilderDatabaseEntry updatedEntry);
    public abstract void ClearEntries();
    public abstract void DrawView();
    public virtual void OnSave(){}

    public virtual void OnExitModule() {}

    public virtual void ShowSaveErrorMessage()
    {
        //Debug.LogError("COULD NOT SAVE " + EntryType + " BECAUSE SOME CONDITIONS WERE NOT MET");
    }

    public bool ContainsInvalidCharacters(string entryName)
    {
        return entryName.Contains("#") || entryName.Contains("<") || entryName.Contains("$") ||
               entryName.Contains("+") || entryName.Contains("%") || entryName.Contains(">") ||
               entryName.Contains("!") ||
               entryName.Contains("`") || entryName.Contains("&") || entryName.Contains("*") ||
               entryName.Contains("'") || entryName.Contains("|") || entryName.Contains("{") ||
               entryName.Contains("?") ||
               entryName.Contains("=") || entryName.Contains("}") || entryName.Contains("/") ||
               entryName.Contains(":") || entryName.Contains("\"") || entryName.Contains("@") ||
               entryName.Contains("\\") ||
               entryName.Contains(".");
    }
    
    public virtual void DeleteAllEntries()
    {
        int deletedEntries = 0;
        int maxEntries = databaseEntries.Count;
        foreach (var entry in databaseEntries)
        {
            RPGBuilderEditor.Instance.DeleteDatabaseEntry(this, entry, false);
            deletedEntries++;
            EditorUtility.DisplayProgressBar("Deleting ALL " + ModuleName,
                "Completed: " + deletedEntries + " / " + maxEntries,
                (float) ((float) deletedEntries / (float) maxEntries));
        }

        ClearEntries();
        RPGBuilderEditor.Instance.CurrentEntry = null;

        EditorUtility.ClearProgressBar();
        DataSavingSystem.DeleteAssetIDFile(IDFileName);

        RPGBuilderEditor.Instance.RequestFilterCheck();
        RPGBuilderEditor.Instance.RequestEntryListRedraw();
    }
    public virtual RPGBuilderDatabaseEntry GetEntryByID(int ID)
    {
        return databaseEntries.FirstOrDefault(entry => entry.ID == ID);
    }
    public virtual RPGBuilderDatabaseEntry GetEntryByName(string entryName)
    {
        return databaseEntries.FirstOrDefault(entry => entry.entryName == entryName);
    }
    public virtual int GetEntryIndexByID(int ID)
    {
        for (int i = 0; i < databaseEntries.Count; i++)
        {
            if (databaseEntries[i].ID == ID) return i;
        }

        return -1;
    }
    public virtual int GetEntryIndexByName(string entryName)
    {
        for (int i = 0; i < databaseEntries.Count; i++)
        {
            if (databaseEntries[i].entryName == entryName) return i;
        }

        return -1;
    }
    public virtual void ConvertDatabaseEntriesAfterUpdate()
    {
        
    }
    public virtual void ConvertStringsToTypeEntries()
    {
        
    }
    public virtual void InjectEntries(List<string> injectedEntriesFiles)
    {
        foreach (var entryFile in injectedEntriesFiles)
        {
            var existingEntry = (RPGBuilderDatabaseEntry) AssetDatabase.LoadAssetAtPath(entryFile, typeof(RPGBuilderDatabaseEntry));
            if (existingEntry == null) continue;
            int previousID = existingEntry.ID;
            existingEntry.ID = -1;
            RPGBuilderEditor.Instance.SaveDatabaseEntry(this, existingEntry, true);

            foreach (var category in RPGBuilderEditor.Instance.EditorCategories.Where(category => category.category.IsEnabled))
            {
                foreach (var module in category.modules.Where(module => module.IsEnabled))
                {
                    module.UpdateEntriesAfterInjection(previousID, existingEntry.ID, module.EntryAttributeName, injectedEntriesFiles);
                }
            }
        }
    }
    protected virtual void UpdateEntriesAfterInjection(int previousID, int newID, string attribute, List<string> injectedEntriesFiles)
    {
        foreach (var existingEntry in injectedEntriesFiles.Select(entryFile => (RPGBuilderDatabaseEntry) AssetDatabase.LoadAssetAtPath(entryFile, typeof(RPGBuilderDatabaseEntry))).Where(existingEntry => existingEntry != null))
        {
            ScanObjectForAttributes(existingEntry, attribute, previousID, newID);
        }
    }
    private void ScanObjectForAttributes(object obj, string customAttribute, int previousID, int newID)
    {
        FieldInfo[] customAttributeFields = GetFields(obj, customAttribute);
        FieldInfo[] customAttributeListFields = GetFields(obj, "RPGDataListAttribute");

        if (customAttributeFields != null)
        {
            foreach (FieldInfo field in customAttributeFields)
            {
                if ((int) field.GetValue(obj) == previousID)
                {
                    field.SetValue(obj, newID);
                }
            }
        }

        if (customAttributeListFields != null)
        {
            foreach (FieldInfo field in customAttributeListFields)
            {
                IEnumerable<object> objectList = (IEnumerable<object>) field.GetValue(obj);
                foreach (object childObj in objectList)
                {
                    ScanObjectForAttributes(childObj, customAttribute, previousID, newID);
                }
            }
        }
    }
    private FieldInfo[] GetFields(object target, string attributeName)
    {
        Type convertedType = GetType(attributeName);
        if (convertedType == null) return null;
        return target.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .Where(f => f.IsDefined(convertedType)).ToArray();
    }
    private static Type GetType(string TypeName)
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
}
