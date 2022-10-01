using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.Logic;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.Templates;
using BLINK.RPGBuilder.WorldPersistence;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace BLINK.RPGBuilder.AI
{
    public class NPCSpawner : MonoBehaviour
    {
        public NPCSpawnerSaver Saver;
        public AIData.SpawnerType spawnerType;
        public bool IsActive;

        public List<Coroutine> SpawningCoroutines = new List<Coroutine>();

        [Serializable]
        public class NPC_SPAWN_DATA
        {
            public RPGNpc npc;
            public float spawnChance = 100;
            public bool IsPersistent;
        }

        public List<NPC_SPAWN_DATA> spawnData = new List<NPC_SPAWN_DATA>();

        public int npcCountMax = 1;
        public List<CombatEntity> CurrentNPCs = new List<CombatEntity>();
        public List<CombatEntity> CurrentPersistentNPCs = new List<CombatEntity>();

        public RequirementsTemplate RequirementsTemplate;
        public float PlayerDistanceMax = 100;

        public int spawnedCount;
        public int spawnedCountMax;

        public bool OverrideLevels;
        public int MinLevel = 1, MaxLevel = 2;
        public bool ScaleWithPlayer;
        public bool OverrideFaction;
        public RPGFaction Faction;
        public bool OverrideSpecies;
        public RPGSpecies Species;
        public bool OverrideRespawn;
        public float MinRespawn = 10, MaxRespawn = 20;

        public float areaRadius = 10f, areaHeight = 20f;
        public Color gizmoColor = Color.yellow;
        public Color lineColor = Color.black;

        public LayerMask groundLayers;

        public bool usePosition;

        private void Awake()
        {
            if (Saver == null) Saver = GetComponent<NPCSpawnerSaver>();
        }

        private void OnEnable()
        {
            GameEvents.NewGameSceneLoaded += GameSceneReady;
        }

        private void OnDisable()
        {
            GameEvents.NewGameSceneLoaded -= GameSceneReady;
        }

        private void OnDestroy()
        {
            if (RPGBuilderEssentials.Instance == null) return;
            if (RPGBuilderEssentials.Instance.getCurrentScene().name ==
                GameDatabase.Instance.GetGeneralSettings().mainMenuSceneName) return;
            GameState.Instance.RemoveSpawnerFromList(this);
        }

        private void GameSceneReady()
        {
            GameState.Instance.AddSpawnerToList(this);
        }

        public void Initialize()
        {
            if (!IsActive) return;
            if (RequirementsTemplate != null && !RequirementsManager.Instance
                .RequirementsMet(GameState.playerEntity, RequirementsTemplate.Requirements).Result) return;
            if (spawnerType == AIData.SpawnerType.Limited && spawnedCount >= spawnedCountMax) return;
            if (Saver != null)
            {
                SpawnPersistentNPCs();
            }

            if (GetTotalNPCs() >= npcCountMax) return;
            for (int i = 0; i < npcCountMax; i++)
            {
                StartCoroutine(ExecuteSpawner(0));
            }
        }

        private void SpawnPersistentNPCs()
        {
            NPCSpawnerSaverTemplate saverTemplate =
                PersistenceManager.Instance.GetNPCSpawnerTemplateData(Saver.GetIdentifier());
            if (saverTemplate == null) return;

            foreach (var npc in saverTemplate.persistentNPCs)
            {
                RPGNpc npcEntry = GameDatabase.Instance.GetNPCs()[npc.ID];
                if (npcEntry == null) continue;

                CombatEntity newNPC = CombatManager.Instance.GenerateNPCEntity(npcEntry, false, true, null,
                    npc.position, Quaternion.Euler(npc.rotation), this);
                CurrentPersistentNPCs.Add(newNPC);
                newNPC.InitializeVitalityStats(npc.VitalityStats);
                spawnedCount++;
            }
        }

        public IEnumerator ExecuteSpawner(float delay)
        {
            switch (spawnerType)
            {
                case AIData.SpawnerType.Limited:
                    if (spawnedCount >= spawnedCountMax) yield break;
                    yield return new WaitForSeconds(delay);

                    if (GetTotalNPCs() >= npcCountMax) yield break;
                    SpawnNPC();
                    spawnedCount++;
                    break;

                case AIData.SpawnerType.Endless:
                    yield return new WaitForSeconds(delay);
                    if (GetTotalNPCs() >= npcCountMax) yield break;
                    SpawnNPC();
                    break;
            }
        }

        private CombatData.NPCPickData PickRandomNPC()
        {
            CombatData.NPCPickData npc = new CombatData.NPCPickData();
            float rdmNPC = Random.Range(0f, 100f);
            float offset = 0;
            foreach (var t in spawnData)
            {
                if (rdmNPC >= 0 + offset && rdmNPC <= t.spawnChance + offset)
                {
                    npc.npc = t.npc;
                    npc.Persistent = t.IsPersistent;
                    return npc;
                }

                offset += t.spawnChance;
            }

            return null;
        }

        private void SpawnNPC()
        {
            CombatData.NPCPickData pickedNPC = PickRandomNPC();
            if (pickedNPC == null || pickedNPC.npc == null) return;

            CombatEntity newNPC = CombatManager.Instance.GenerateNPCEntity(pickedNPC.npc, false, pickedNPC.Persistent,
                null, GetNPCPosition(), transform.rotation, this);
            if (pickedNPC.Persistent) CurrentPersistentNPCs.Add(newNPC);
            else CurrentNPCs.Add(newNPC);
        }

        private Vector3 GetNPCPosition()
        {
            if (usePosition) return transform.position;

            bool foundPoint = false;
            Vector3 spawnPos = new Vector3();
            int maxAttempt = 15;
            while (!foundPoint)
            {
                maxAttempt--;
                spawnPos = GetSpawnPoint();
                if (spawnPos != transform.position) foundPoint = true;
                if (maxAttempt == 0) foundPoint = true;
            }

            return spawnPos;
        }

        private Vector3 GetSpawnPoint()
        {
            var position = transform.position;
            Vector3 spawnPos = new Vector3(Random.Range(position.x - areaRadius, position.x + areaRadius),
                position.y + areaHeight,
                Random.Range(position.z - areaRadius, position.z + areaRadius));
            return Physics.Raycast(spawnPos, -transform.up, out var hit, areaHeight, groundLayers)
                ? hit.point
                : position;
        }

        public void ManualSpawnNPC()
        {
            if (GetTotalNPCs() >= npcCountMax)
            {
                if (CurrentNPCs.Count > 0)
                {
                    Destroy(CurrentNPCs[0].gameObject);
                    CurrentNPCs.RemoveAt(0);
                }
                else if (CurrentPersistentNPCs.Count > 0)
                {
                    Destroy(CurrentPersistentNPCs[0].gameObject);
                    CurrentPersistentNPCs.RemoveAt(0);
                }
            }

            CombatData.NPCPickData pickedNPC = PickRandomNPC();
            if (pickedNPC == null) return;
            CombatEntity newNPC = CombatManager.Instance.GenerateNPCEntity(pickedNPC.npc, false, false, null,
                GetNPCPosition(),
                transform.rotation, this);
            if (pickedNPC.Persistent) CurrentPersistentNPCs.Add(newNPC);
            else CurrentNPCs.Add(newNPC);
        }

        public int GetTotalNPCs()
        {
            return CurrentNPCs.Count + CurrentPersistentNPCs.Count;
        }

        private void OnDrawGizmos()
        {
            if (areaHeight < 1) areaHeight = 1;
            if (usePosition)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(transform.position, 0.5f);
                
                Vector3 origin = transform.position;
                Vector3 start = transform.right * areaRadius;
                Vector3 lastPos = origin + start;
                float angle = 0;
                while (angle <= 360)
                {
                    angle += 360 / 50f;
                    Vector3 nextPosition = origin + (Quaternion.Euler(0, angle, 0) * start);

                    Gizmos.color = gizmoColor;
                    Gizmos.DrawLine(lastPos, nextPosition);
                    Gizmos.DrawLine(new Vector3(lastPos.x, lastPos.y + areaHeight, lastPos.z),
                        new Vector3(nextPosition.x, nextPosition.y + areaHeight, nextPosition.z));
                    lastPos = nextPosition;
                }
            }
            else
            {
                Vector3 origin = transform.position;
                Vector3 start = transform.right * areaRadius;
                Vector3 lastPos = origin + start;
                float angle = 0;
                while (angle <= 360)
                {
                    angle += 360 / 50f;
                    Vector3 nextPosition = origin + (Quaternion.Euler(0, angle, 0) * start);

                    Gizmos.color = gizmoColor;
                    Gizmos.DrawLine(lastPos, nextPosition);
                    Gizmos.DrawLine(new Vector3(lastPos.x, lastPos.y + areaHeight, lastPos.z),
                        new Vector3(nextPosition.x, nextPosition.y + areaHeight, nextPosition.z));

                    Gizmos.color = lineColor;
                    Gizmos.DrawLine(nextPosition,
                        new Vector3(nextPosition.x, nextPosition.y + areaHeight, nextPosition.z));
                    lastPos = nextPosition;
                }
            }
        }
    }
}
