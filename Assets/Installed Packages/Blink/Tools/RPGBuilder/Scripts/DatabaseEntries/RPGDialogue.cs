using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RPGDialogue : RPGBuilderDatabaseEntry
{
    [HideInInspector] public string _name;
    [HideInInspector] public string _fileName;
    [HideInInspector] public string displayName;
    [HideInInspector] public string description;

    public RPGDialogueGraph dialogueGraph;

    public bool hasExitNode;
    public string exitNodeText = "Goodbye";
    
    public void UpdateEntryData(RPGDialogue newEntryData)
    {
        ID = newEntryData.ID;
        entryName = newEntryData.entryName;
        entryFileName = newEntryData.entryFileName;
        entryDisplayName = newEntryData.entryDisplayName;
        entryIcon = newEntryData.entryIcon;
        entryDescription = newEntryData.entryDescription;
        
        dialogueGraph = newEntryData.dialogueGraph;
        hasExitNode = newEntryData.hasExitNode;
        exitNodeText = newEntryData.exitNodeText;
    }
}
