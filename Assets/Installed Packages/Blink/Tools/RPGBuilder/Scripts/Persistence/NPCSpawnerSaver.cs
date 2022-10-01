using System;
using BLINK.RPGBuilder.AI;
using BLINK.RPGBuilder.Characters;
using UnityEngine;

namespace BLINK.RPGBuilder.WorldPersistence
{
    [RequireComponent(typeof(SaverIdentifier))]
    public class NPCSpawnerSaver : ObjectSaver
    {
        public NPCSpawner spawner;

        private void Start()
        {
            if (spawner == null) spawner = GetComponent<NPCSpawner>();
        }

        private void OnEnable()
        {
            GameEvents.NewGameSceneLoaded += RegisterSelf;
        }
        
        private void OnDisable()
        {
            if(PersistenceManager.Instance != null) PersistenceManager.Instance.UnregisterNPCSpawnerSaver(this);
            GameEvents.NewGameSceneLoaded -= RegisterSelf;
        }

        public override void RegisterSelf()
        {
            if (!IsDynamic() && PersistenceManager.Instance.IsSaverDestroyed(this))
            {
                Destroy(gameObject);
                return;
            }

            PersistenceManager.Instance.RegisterNPCSpawnerSaver(this);
            LoadState();
        }

        protected override void LoadState()
        {
            base.LoadState();

            if (IsDynamic())
            {
                if (!PersistenceManager.Instance.NPCSpawnerListContainsIdentifier(GetIdentifier()))
                {
                    Character.Instance.InitializeNPCSpawner(this);
                    return;
                }

                NPCSpawnerSaverTemplate template = PersistenceManager.Instance.GetNPCSpawnerTemplateData(GetIdentifier());
                spawner.spawnedCount = template.spawnedCount;
                StartNPCSpawner();
            }
            else
            {
                if (!PersistenceManager.Instance.NPCSpawnerListContainsIdentifier(GetIdentifier())) return;
                NPCSpawnerSaverTemplate template = PersistenceManager.Instance.GetNPCSpawnerTemplateData(GetIdentifier());
                spawner.spawnedCount = template.spawnedCount;
                StartNPCSpawner();
            }
        }

        public void StartNPCSpawner()
        {
            if (!GameState.Instance.IsSpawnerInRange(spawner)) return;
            spawner.IsActive = true;
            spawner.Initialize();
        }
    }
}
