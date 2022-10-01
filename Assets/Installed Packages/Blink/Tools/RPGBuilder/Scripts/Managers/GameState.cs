using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.AI;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.Data;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.World;
using BLINK.RPGBuilder.WorldPersistence;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameState : MonoBehaviour
{
    public static GameState Instance { get; private set; }

    private static bool inGame;
    public static bool IsInGame()
    {
        return inGame;
    }
    
    public static PlayerCombatEntity playerEntity;
    public static RPGGameScene CurrentGameScene;
    public static List<CharacterGraveyard> allGraveyards = new List<CharacterGraveyard>();
    public static List<NPCSpawner> allNPCSpawners = new List<NPCSpawner>();
    private float nextNPCSpawnerCheck;
    public static readonly List<CombatEntity> combatEntities = new List<CombatEntity>();
    public static List<EconomyData.WorldDroppedItemEntry> allWorldDroppedItems = new List<EconomyData.WorldDroppedItemEntry>();
    
    public static bool combatEnabled = true;
    public static bool inCombatOverriden = false;
    
    private void Awake()
    {
        if (Instance != null) return;
        Instance = this;
    }
    
    private void OnEnable()
    {
        GameEvents.NewGameSceneLoaded += InitializeCurrentGameScene;
        GameEvents.StartNewGameSceneLoad += ResetStates;
        GameEvents.BackToMainMenu += BackToMainMenu;
    }

    private void OnDisable()
    {
        GameEvents.NewGameSceneLoaded -= InitializeCurrentGameScene;
        GameEvents.StartNewGameSceneLoad -= ResetStates;
        GameEvents.BackToMainMenu -= BackToMainMenu;
    }
    

    private void InitializeCurrentGameScene()
    {
        CurrentGameScene = RPGBuilderUtilities.GetGameSceneFromName(SceneManager.GetActiveScene().name);
        allGraveyards.Clear();
        allGraveyards = FindObjectsOfType<CharacterGraveyard>().ToList();
        nextNPCSpawnerCheck = 0;
        inGame = true;
    }

    private void ResetStates()
    {
        allNPCSpawners.Clear();
        combatEntities.Clear();
    }

    public void RegisterEntity(CombatEntity entity)
    {
        if(!combatEntities.Contains(entity)) combatEntities.Add(entity);
    }
    public void UnregisterEntity(CombatEntity entity)
    {
        if(combatEntities.Contains(entity)) combatEntities.Remove(entity);
    }
    
    public void AddSpawnerToList(NPCSpawner spawner)
    {
        if (allNPCSpawners.Contains(spawner)) return;
        allNPCSpawners.Add(spawner);
    }
    
    private void BackToMainMenu()
    {
        CurrentGameScene = null;
        inGame = false;
    }

    private void FixedUpdate()
    {
        if (!inGame) return;
        CheckSpawnersWithinRange();
    }
    
    public void RemoveSpawnerFromList(NPCSpawner spawner)
    {
        foreach (var npc in spawner.CurrentNPCs)
        {
            if(npc != null) Destroy(npc.gameObject);
        }
        foreach (var npc in spawner.CurrentPersistentNPCs)
        {
            if(npc != null) Destroy(npc.gameObject);
        }
        if (allNPCSpawners.Contains(spawner)) allNPCSpawners.Remove(spawner);
    }

    public void InstantNPCSpawnerCheck()
    {
        nextNPCSpawnerCheck = 0;
    }
    
    private void CheckSpawnersWithinRange()
    {
        if (Time.time >= nextNPCSpawnerCheck)
        {
            nextNPCSpawnerCheck = Time.time + GameDatabase.Instance.GetCombatSettings().NPCSpawnerDistanceCheckInterval;
            foreach (var spawner in allNPCSpawners)
            {
                if (!IsSpawnerInRange(spawner))
                {
                    if (!spawner.IsActive) continue;
                    DeactivateSpawner(spawner);
                }
                else
                {
                    if (spawner.IsActive) continue;
                    ActivateSpawner(spawner);
                }
            }
        }
    }

    public bool IsSpawnerInRange(NPCSpawner spawner)
    {
        return Vector3.Distance(playerEntity.transform.position, spawner.transform.position) <=
               spawner.PlayerDistanceMax;
    }

    public void DeactivateSpawner(NPCSpawner spawner)
    {
        spawner.IsActive = false;
        foreach (var NPC in spawner.CurrentNPCs)
        {
            Destroy(NPC.gameObject);
        }
        
        foreach (var NPC in spawner.CurrentPersistentNPCs)
        {
            Destroy(NPC.gameObject);
        }

        foreach (var crt in spawner.SpawningCoroutines)
        {
            if(crt != null)  spawner.StopCoroutine(crt);
        }

    }

    public void ActivateSpawner(NPCSpawner spawner)
    {
        spawner.IsActive = true;
        spawner.Initialize();
    }
}
