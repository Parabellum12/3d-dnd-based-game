using System.Collections.Generic;
using System.IO;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.WorldPersistence;
using UnityEngine;


public static class RPGBuilderJsonSaver
{
    public static void GenerateCharacterEquippedtemsData()
    {
        if (Character.Instance.CharacterData.ArmorPiecesEquipped.Count == 0)
            for (var i = 0; i < GameDatabase.Instance.GetArmorSlots().Count; i++)
                Character.Instance.CharacterData.ArmorPiecesEquipped.Add(new CharacterEntries.ArmorEquippedEntry());
        
        CharacterEntries.WeaponEquippedEntry newWeapon1 = new CharacterEntries.WeaponEquippedEntry();
        Character.Instance.CharacterData.WeaponsEquipped.Add(newWeapon1);
        CharacterEntries.WeaponEquippedEntry newWeapon2 = new CharacterEntries.WeaponEquippedEntry();
        Character.Instance.CharacterData.WeaponsEquipped.Add(newWeapon2);
    }
    
    private static void SaveEquippedItems()
    {

        if (Character.Instance.CharacterData.ArmorPiecesEquipped.Count <
            GameDatabase.Instance.GetArmorSlots().Count)
        {
            int diff = GameDatabase.Instance.GetArmorSlots().Count -
                       Character.Instance.CharacterData.ArmorPiecesEquipped.Count;

            for (int i = 0; i < diff; i++)
            {
                Character.Instance.CharacterData.ArmorPiecesEquipped.Add(new CharacterEntries.ArmorEquippedEntry());
            }
        }
        if (Character.Instance.CharacterData.ArmorPiecesEquipped.Count >
            GameDatabase.Instance.GetArmorSlots().Count)
        {
            int diff = Character.Instance.CharacterData.ArmorPiecesEquipped.Count - GameDatabase.Instance.GetArmorSlots().Count;

            for (int i = 0; i < diff; i++)
            {
                Character.Instance.CharacterData.ArmorPiecesEquipped.RemoveAt(Character.Instance.CharacterData.ArmorPiecesEquipped.Count-1);
            }
        }

        for (var i = 0; i < Character.Instance.CharacterData.ArmorPiecesEquipped.Count; i++)
        {
            var itemID = -1;
            var itemDataID = -1;
            if (GameState.playerEntity.equippedArmors[i].item != null)
            {
                itemID = GameState.playerEntity.equippedArmors[i].item.ID;
                itemDataID = GameState.playerEntity.equippedArmors[i].temporaryItemDataID;
            }

            Character.Instance.CharacterData.ArmorPiecesEquipped[i].itemID = itemID;
            Character.Instance.CharacterData.ArmorPiecesEquipped[i].itemDataID = itemDataID;
        }

        if (Character.Instance.CharacterData.WeaponsEquipped.Count <
            GameDatabase.Instance.GetEconomySettings().weaponSlotsList.Count)
        {
            int diff = GameDatabase.Instance.GetEconomySettings().weaponSlotsList.Count -
                       Character.Instance.CharacterData.WeaponsEquipped.Count;

            for (int i = 0; i < diff; i++)
            {
                Character.Instance.CharacterData.WeaponsEquipped.Add(new CharacterEntries.WeaponEquippedEntry());
            }
        }
        if (Character.Instance.CharacterData.WeaponsEquipped.Count >
            GameDatabase.Instance.GetEconomySettings().weaponSlotsList.Count)
        {
            int diff = Character.Instance.CharacterData.WeaponsEquipped.Count - GameDatabase.Instance.GetEconomySettings().weaponSlotsList.Count;

            for (int i = 0; i < diff; i++)
            {
                Character.Instance.CharacterData.WeaponsEquipped.RemoveAt(Character.Instance.CharacterData.WeaponsEquipped.Count-1);
            }
        }

        for (var i = 0; i < Character.Instance.CharacterData.WeaponsEquipped.Count; i++)
        {
            var itemID = -1;
            var itemDataID = -1;
            if (GameState.playerEntity.equippedWeapons[i].item != null)
            {
                itemID = GameState.playerEntity.equippedWeapons[i].item.ID;
                itemDataID = GameState.playerEntity.equippedWeapons[i].temporaryItemDataID;
            }

            Character.Instance.CharacterData.WeaponsEquipped[i].itemID = itemID;
            Character.Instance.CharacterData.WeaponsEquipped[i].itemDataID = itemDataID;
        }
    }

    private static void SaveStates()
    {
        Character.Instance.CharacterData.States.Clear();
        foreach (var state in GameState.playerEntity.GetStates())
        {
            if(!state.stateEffect.IsPersistent) continue;

            CharacterEntries.StateEntry savedState = new CharacterEntries.StateEntry
            {
                EffectName = state.stateEffect.entryName,
                EffectID = state.stateEffectID,
                EffectRank = state.effectRankIndex,
                curPulses = state.curPulses,
                maxPulses = state.maxPulses,
                nextPulse = state.nextPulse,
                pulseInterval = state.pulseInterval,
                stateCurDuration = state.stateCurDuration,
                stateMaxDuration = state.stateMaxDuration,
                curStack = state.curStack,
                maxStack = state.maxStack,
            };
            
            Character.Instance.CharacterData.States.Add(savedState);
        }
    }
    
    private static void SaveVitalityStats()
    {
        Character.Instance.CharacterData.VitalityStats.Clear();
        foreach (var stat in GameState.playerEntity.GetStats().Values)
        {
            if(!stat.stat.IsPersistent) continue;

            CombatData.VitalityStatEntry savedState = new CombatData.VitalityStatEntry
            {
                StatName = stat.stat.entryName,
                StatID = stat.stat.ID,
                value = stat.currentValue,
            };
            
            Character.Instance.CharacterData.VitalityStats.Add(savedState);
        }
    }

    private static void SaveNPCs()
    {
        List<ObjectSaverTemplate> npcSpawnerTemplateList = new List<ObjectSaverTemplate>();
        foreach (var saved in Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex].SavedNPCSpawners)
        {
            npcSpawnerTemplateList.Add(saved);
        }
        
        foreach (var saver in PersistenceManager.Instance.AllNPCSpawners)
        {
            Character.Instance.UpdateNPCSpawnerState(saver, npcSpawnerTemplateList);
        }
    }

    private static void SavePosition()
    {
        if (!LoadingScreenManager.Instance.isSceneLoading && !GameState.playerEntity.IsDead())
        {
            Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex].LastPosition =
                GameState.playerEntity.transform.position;
            Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex].LastRotation =
                new Vector3(0, GameState.playerEntity.transform.eulerAngles.y, 0);
        }
    }

    public static void SaveCharacterData()
    {
        if (GameState.playerEntity == null) return;
        SavePosition();

        SaveStates();
        SaveVitalityStats();
        SaveNPCs();
        
        SaveEquippedItems();
        PersistenceManager.Instance.SaveWorld();

        var json = JsonUtility.ToJson(Character.Instance.CharacterData);
        WriteToFile(Character.Instance.CharacterData.CharacterName + "_RPGBCharacter.txt", json);
    }

    public static void InitializeCharacterData(string charName)
    {
        JsonUtility.FromJsonOverwrite(ReadFromFile(charName + "_RPGBCharacter.txt"), Character.Instance.CharacterData);
    }
    public static CharacterData GetCharacterDataByName(string charName)
    {
        CharacterData curCharCombatData = new CharacterData();
        var json = ReadFromFile(charName + "_RPGBCharacter.txt");
        JsonUtility.FromJsonOverwrite(json, curCharCombatData);
        return curCharCombatData;
    }

    private static void WriteToFile(string fileName, string json)
    {
        var path = GetFilePath(fileName);
        var fileStream = new FileStream(path, FileMode.Create);

        using (var writer = new StreamWriter(fileStream))
        {
            writer.Write(json);
        }
    }

    public static void DeleteCharacter(string characterName)
    {

        var filePath = Application.persistentDataPath + "/" + characterName + "_RPGBCharacter.txt";

        // check if file exists
        if (!File.Exists(filePath))
            Debug.LogError("This character save file does not exist");
        else
            File.Delete(filePath);
    }

    private static string ReadFromFile(string fileName)
    {
        var path = GetFilePath(fileName);
        if (File.Exists(path))
        {
            using (var reader = new StreamReader(path))
            {
                var json = reader.ReadToEnd();
                return json;
            }
        }
        else
        {
            return "";
        }
    }

    private static string GetFilePath(string fileName)
    {
        return Application.persistentDataPath + "/" + fileName;
    }
}

