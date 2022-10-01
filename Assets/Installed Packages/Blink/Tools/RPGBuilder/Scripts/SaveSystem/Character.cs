using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.Templates;
using BLINK.RPGBuilder.WorldPersistence;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BLINK.RPGBuilder.Characters
{
    public class Character : MonoBehaviour
    {
        public static Character Instance => instance;
        private static Character instance;

        public CharacterData CharacterData = new CharacterData();
        
        private void Awake()
        {
            instance = this;
        }

        private void OnEnable()
        {
            GeneralEvents.PlayerGainedItem += CheckFirstTimeItemGain;
            GameEvents.PlayerLearnedAbility += CheckFirstTimeAbilityLearned;
            GameEvents.PlayerLearnedBonus += CheckFirstTimeBonusLearned;
            GameEvents.PlayerLearnedRecipe += CheckFirstTimeRecipeLearned;
            GameEvents.PlayerLearnedResourceNode += CheckFirstTimeResourceLearned;
            GameEvents.SceneEntered += CheckFirstTimeSceneEntered;
            WorldEvents.RegionEntered += CheckFirstTimeRegionEntered;
            CombatEvents.PlayerKilledNPC += CheckFirstTimeNPCKilled;
        }

        private void OnDisable()
        {
            GeneralEvents.PlayerGainedItem -= CheckFirstTimeItemGain;
            GameEvents.PlayerLearnedAbility -= CheckFirstTimeAbilityLearned;
            GameEvents.PlayerLearnedBonus -= CheckFirstTimeBonusLearned;
            GameEvents.PlayerLearnedRecipe -= CheckFirstTimeRecipeLearned;
            GameEvents.PlayerLearnedResourceNode -= CheckFirstTimeResourceLearned;
            GameEvents.SceneEntered -= CheckFirstTimeSceneEntered;
            WorldEvents.RegionEntered -= CheckFirstTimeRegionEntered;
            CombatEvents.PlayerKilledNPC -= CheckFirstTimeNPCKilled;
        }

        private void CheckFirstTimeAbilityLearned(RPGAbility ability)
        {
            var currentIndex = abilitylearnedBefore(ability);
            if (currentIndex != -1) return;
            var newDATA = new CharacterEntries.AbilityLearnedEntry {abilityID = ability.ID};
            CharacterData.LearnedAbilities.Add(newDATA);
        }

        private int abilitylearnedBefore(RPGAbility ab)
        {
            for (var i = 0; i < CharacterData.LearnedAbilities.Count; i++)
                if (CharacterData.LearnedAbilities[i].abilityID == ab.ID)
                    return i;
            return -1;
        }

        private void CheckFirstTimeBonusLearned(RPGBonus bonus)
        {
            var currentIndex = bonuslearnedBefore(bonus);
            if (currentIndex != -1) return;
            var newDATA = new CharacterEntries.BonusLearnedEntry {bonusID = bonus.ID};
            CharacterData.LearnedBonuses.Add(newDATA);
        }

        private int bonuslearnedBefore(RPGBonus bonus)
        {
            for (var i = 0; i < CharacterData.LearnedBonuses.Count; i++)
                if (CharacterData.LearnedBonuses[i].bonusID == bonus.ID)
                    return i;
            return -1;
        }

        private void CheckFirstTimeItemGain(RPGItem item, int amount)
        {
            var currentIndex = itemGainedBefore(item);
            if (currentIndex != -1) return;
            var newDATA = new CharacterEntries.ItemGainedEntry {itemID = item.ID};
            CharacterData.GainedItems.Add(newDATA);
        }

        private int itemGainedBefore(RPGItem item)
        {
            for (var i = 0; i < CharacterData.GainedItems.Count; i++)
                if (CharacterData.GainedItems[i].itemID == item.ID)
                    return i;
            return -1;
        }

        private void CheckFirstTimeResourceLearned(RPGResourceNode resource)
        {
            var currentIndex = resourceNodeLearnedBefore(resource);
            if (currentIndex != -1) return;
            var newDATA = new CharacterEntries.ResourceNodeLearnedEntry {resourceNodeID = resource.ID};
            CharacterData.LearnedResourceNodes.Add(newDATA);
        }

        private int resourceNodeLearnedBefore(RPGResourceNode ab)
        {
            for (var i = 0; i < CharacterData.LearnedAbilities.Count; i++)
                if (CharacterData.LearnedAbilities[i].abilityID == ab.ID)
                    return i;
            return -1;
        }

        private void CheckFirstTimeRecipeLearned(RPGCraftingRecipe recipe)
        {
            var currentIndex = recipelearnedBefore(recipe);
            if (currentIndex != -1) return;
            var newDATA = new CharacterEntries.RecipeLearnedEntry {recipeID = recipe.ID};
            CharacterData.LearnedRecipes.Add(newDATA);
        }

        private int recipelearnedBefore(RPGCraftingRecipe recipe)
        {
            for (var i = 0; i < CharacterData.LearnedAbilities.Count; i++)
                if (CharacterData.LearnedAbilities[i].abilityID == recipe.ID)
                    return i;
            return -1;
        }

        private void CheckFirstTimeSceneEntered(string sceneName)
        {
            var currentIndex = sceneAlreadyEnteredBefore(sceneName);
            if (currentIndex != -1) return;
            var newDATA = new CharacterEntries.SceneEnteredEntry {sceneName = sceneName};
            CharacterData.EnteredScenes.Add(newDATA);
        }

        private int sceneAlreadyEnteredBefore(string sceneName)
        {
            for (var i = 0; i < CharacterData.EnteredScenes.Count; i++)
                if (CharacterData.EnteredScenes[i].sceneName == sceneName)
                    return i;
            return -1;
        }

        private void CheckFirstTimeRegionEntered(RegionTemplate regionData)
        {
            var currentIndex = regionAlreadyEnteredBefore(regionData.entryName);
            if (currentIndex != -1) return;
            var newDATA = new CharacterEntries.RegionEnteredEntry {regionName = regionData.entryName};
            CharacterData.EnteredRegions.Add(newDATA);
        }

        private int regionAlreadyEnteredBefore(string regionName)
        {
            for (var i = 0; i < CharacterData.EnteredRegions.Count; i++)
                if (CharacterData.EnteredRegions[i].regionName == regionName)
                    return i;
            return -1;
        }

        private void CheckFirstTimeNPCKilled(RPGNpc npc)
        {
            var currentIndex = npcIsAlreadyKilledBefore(npc);
            if (currentIndex != -1)
            {
                CharacterData.KilledNPCs[currentIndex].killedAmount++;
            }
            else
            {
                var newDATA = new CharacterEntries.NPCKilledEntry {npcID = npc.ID, killedAmount = 1};
                CharacterData.KilledNPCs.Add(newDATA);
            }
        }

        private int npcIsAlreadyKilledBefore(RPGNpc npc)
        {
            for (var i = 0; i < CharacterData.KilledNPCs.Count; i++)
                if (CharacterData.KilledNPCs[i].npcID == npc.ID)
                    return i;
            return -1;
        }

        public int getCurrencyAmount(RPGCurrency currency)
        {
            foreach (var t in CharacterData.Currencies.Where(t => t.currencyID == currency.ID))
                return t.amount;

            return -1;
        }

        public int getCurrencyIndex(RPGCurrency currency)
        {
            for (var i = 0; i < CharacterData.Currencies.Count; i++)
                if (CharacterData.Currencies[i].currencyID == currency.ID)
                    return i;
            return -1;
        }

        public int getTreePointsAmountByPoint(int ID)
        {
            foreach (var t in CharacterData.Points.Where(t => t.treePointID == ID))
                return t.amount;

            return -1;
        }

        public CharacterEntries.QuestEntry getQuestDATA(RPGQuest quest)
        {
            return CharacterData.Quests.FirstOrDefault(t => t.questID == quest.ID);
        }

        public int getQuestINDEX(RPGQuest quest)
        {
            for (var i = 0; i < CharacterData.Quests.Count; i++)
                if (CharacterData.Quests[i].questID == quest.ID)
                    return i;
            return -1;
        }

        public bool isAbilityCDReady(RPGAbility ab)
        {
            if (ab.abilityType == RPGAbility.AbilityType.PlayerAutoAttack) return false;
            return ab.abilityType == RPGAbility.AbilityType.PlayerActionAbility
                ? CombatManager.Instance.actionAbIsReady(ab)
                : CharacterData.Abilities.Where(t => t.ID == ab.ID).Any(t => t.NextTimeUse == 0);
        }

        public void InitAbilityCooldown(int ID, float duration)
        {
            RPGAbility ab = GameDatabase.Instance.GetAbilities()[ID];
            switch (ab.abilityType)
            {
                case RPGAbility.AbilityType.PlayerAutoAttack:
                    GameState.playerEntity.InitAACooldown(duration);
                    break;
                case RPGAbility.AbilityType.PlayerActionAbility:
                    GameState.playerEntity.InitActionAbilityCooldown(ID, duration);
                    break;
                default:
                {
                    foreach (var t in CharacterData.Abilities.Where(t => t.ID == ID))
                    {
                        t.NextTimeUse = duration;
                        t.CDLeft = duration;
                    }

                    break;
                }
            }
        }

        public CharacterEntries.AbilityCDState getAbilityCDState(RPGAbility ab)
        {
            CharacterEntries.AbilityCDState cdState = new CharacterEntries.AbilityCDState();
            foreach (var t in CharacterData.Abilities)
            {
                if (t.ID != ab.ID) continue;
                int rank = t.known ? t.rank - 1 : t.rank;
                if (rank == -1) rank = 0;
                cdState.canUseDuringGCD = GameDatabase.Instance.GetAbilities()[t.ID].ranks[rank].CanUseDuringGCD;
                cdState.NextUse = t.NextTimeUse;
                cdState.CDLeft = t.CDLeft;
                return cdState;
            }

            return null;
        }

        private void FixedUpdate()
        {
            if (Time.frameCount < 10)
                return;

            if (!RPGBuilderEssentials.Instance.isInGame) return;

            foreach (var t in CharacterData.Abilities.Where(t => t.NextTimeUse > 0))
            {
                t.CDLeft -= Time.deltaTime;
                if (!(t.CDLeft <= 0)) continue;
                t.CDLeft = 0;
                t.NextTimeUse = 0;
            }
        }

        public int GetCurrentGameSceneIndex()
        {
            for (var index = 0; index < CharacterData.GameScenes.Count; index++)
            {
                if (CharacterData.GameScenes[index].GameSceneID !=
                    RPGBuilderUtilities.GetGameSceneFromName(SceneManager.GetActiveScene().name).ID) continue;
                return index;
            }

            return -1;
        }

        public int GetGameSceneIndexByID(int ID)
        {
            for (var index = 0; index < CharacterData.GameScenes.Count; index++)
            {
                if (CharacterData.GameScenes[index].GameSceneID != ID) continue;
                return index;
            }

            return -1;
        }

        public void InitializeTransform(TransformSaver saver)
        {
            TransformSaverTemplate newTransform = new TransformSaverTemplate
            {
                indentifier = saver.GetIdentifier(),
                Dynamic = saver.IsDynamic(),
                position = saver.transform.position,
                rotation = saver.transform.eulerAngles,
                scale = saver.transform.localScale
            };

            if (newTransform.Dynamic)
            {
                newTransform.ID =
                    PersistenceManager.Instance.GetDynamicObjectIDByName(
                        PersistenceManager.Instance.GetPrefabName(saver.gameObject.name));
            }

            CharacterData.GameScenes[CharacterData.GameSceneEntryIndex].SavedTransforms.Add(newTransform);
        }

        public void UpdateTransformState(TransformSaver saver, List<ObjectSaverTemplate> templateList)
        {
            if (!PersistenceManager.Instance.SaverListContainsIdentifier(saver.GetIdentifier(), templateList))
            {
                InitializeTransform(saver);
                return;
            }

            foreach (var savedTransform in CharacterData.GameScenes[CharacterData.GameSceneEntryIndex].SavedTransforms)
            {
                if (savedTransform.indentifier != saver.GetIdentifier()) continue;
                savedTransform.position = saver.transform.position;
                savedTransform.rotation = saver.transform.eulerAngles;
                savedTransform.scale = saver.transform.localScale;
            }
        }

        public void InitializeAnimator(AnimatorSaver saver)
        {
            AnimatorSaverTemplate newAnimator = new AnimatorSaverTemplate
            {
                indentifier = saver.GetIdentifier(),
                Dynamic = saver.IsDynamic(),
            };

            if (newAnimator.Dynamic)
            {
                newAnimator.ID =
                    PersistenceManager.Instance.GetDynamicObjectIDByName(
                        PersistenceManager.Instance.GetPrefabName(saver.gameObject.name));
            }

            foreach (var parameter in saver.animator.parameters)
            {
                AnimatorParameterEntry newParameter = new AnimatorParameterEntry
                {
                    ParameterName = parameter.name, ParameterType = parameter.type
                };

                switch (newParameter.ParameterType)
                {
                    case AnimatorControllerParameterType.Float:
                        newParameter.FloatValue = saver.animator.GetFloat(parameter.name);
                        break;
                    case AnimatorControllerParameterType.Int:
                        newParameter.IntValue = saver.animator.GetInteger(parameter.name);
                        break;
                    case AnimatorControllerParameterType.Bool:
                        newParameter.BoolValue = saver.animator.GetBool(parameter.name);
                        break;
                }

                newAnimator.Parameters.Add(newParameter);
            }

            CharacterData.GameScenes[CharacterData.GameSceneEntryIndex].SavedAnimators.Add(newAnimator);
        }

        public void UpdateAnimatorState(AnimatorSaver saver, List<ObjectSaverTemplate> templateList)
        {
            if (!PersistenceManager.Instance.SaverListContainsIdentifier(saver.GetIdentifier(), templateList))
            {
                InitializeAnimator(saver);
                return;
            }

            foreach (var savedAnimator in CharacterData.GameScenes[CharacterData.GameSceneEntryIndex].SavedAnimators)
            {
                if (savedAnimator.indentifier != saver.GetIdentifier()) continue;
                savedAnimator.Parameters.Clear();
                foreach (var parameter in saver.animator.parameters)
                {
                    AnimatorParameterEntry newParameter = new AnimatorParameterEntry
                    {
                        ParameterName = parameter.name, ParameterType = parameter.type
                    };
                    switch (newParameter.ParameterType)
                    {
                        case AnimatorControllerParameterType.Float:
                            newParameter.FloatValue = saver.animator.GetFloat(parameter.name);
                            break;
                        case AnimatorControllerParameterType.Int:
                            newParameter.IntValue = saver.animator.GetInteger(parameter.name);
                            break;
                        case AnimatorControllerParameterType.Bool:
                            newParameter.BoolValue = saver.animator.GetBool(parameter.name);
                            break;
                    }

                    savedAnimator.Parameters.Add(newParameter);
                }
            }
        }

        public void InitializeRigidbody(RigidbodySaver saver)
        {
            RigidbodySaverTemplate newRigidbody = new RigidbodySaverTemplate
            {
                indentifier = saver.GetIdentifier(),
                Dynamic = saver.IsDynamic(),
                Mass = saver.Rigidbody.mass,
                Drag = saver.Rigidbody.drag,
                AngularDrag = saver.Rigidbody.angularDrag,
                UseGravity = saver.Rigidbody.useGravity,
                IsKinematic = saver.Rigidbody.isKinematic,
                Interpolation = saver.Rigidbody.interpolation,
                CollisionDetection = saver.Rigidbody.collisionDetectionMode,
                Constraints = saver.Rigidbody.constraints,
                Velocity = saver.Rigidbody.velocity,
            };

            if (newRigidbody.Dynamic)
            {
                newRigidbody.ID =
                    PersistenceManager.Instance.GetDynamicObjectIDByName(
                        PersistenceManager.Instance.GetPrefabName(saver.gameObject.name));
            }

            CharacterData.GameScenes[CharacterData.GameSceneEntryIndex].SavedRigidbodies.Add(newRigidbody);
        }

        public void UpdateRigidbodyState(RigidbodySaver saver, List<ObjectSaverTemplate> templateList)
        {
            if (!PersistenceManager.Instance.SaverListContainsIdentifier(saver.GetIdentifier(), templateList))
            {
                InitializeRigidbody(saver);
                return;
            }

            foreach (var savedRigidbody in CharacterData.GameScenes[CharacterData.GameSceneEntryIndex].SavedRigidbodies)
            {
                if (savedRigidbody.indentifier != saver.GetIdentifier()) continue;
                savedRigidbody.Mass = saver.Rigidbody.mass;
                savedRigidbody.Drag = saver.Rigidbody.drag;
                savedRigidbody.AngularDrag = saver.Rigidbody.angularDrag;
                savedRigidbody.UseGravity = saver.Rigidbody.useGravity;
                savedRigidbody.IsKinematic = saver.Rigidbody.isKinematic;
                savedRigidbody.Interpolation = saver.Rigidbody.interpolation;
                savedRigidbody.CollisionDetection = saver.Rigidbody.collisionDetectionMode;
                savedRigidbody.Constraints = saver.Rigidbody.constraints;
                savedRigidbody.Velocity = saver.Rigidbody.velocity;
            }
        }

        public void InitializeCollider(ColliderSaver saver)
        {
            ColliderSaverTemplate newCollider = new ColliderSaverTemplate
            {
                indentifier = saver.GetIdentifier(),
                Dynamic = saver.IsDynamic(),
                Enabled = saver.Collider.enabled,
                IsTrigger = saver.Collider.isTrigger,
            };

            if (newCollider.Dynamic)
            {
                newCollider.ID =
                    PersistenceManager.Instance.GetDynamicObjectIDByName(
                        PersistenceManager.Instance.GetPrefabName(saver.gameObject.name));
            }

            CharacterData.GameScenes[CharacterData.GameSceneEntryIndex].SavedColliders.Add(newCollider);
        }

        public void UpdateColliderState(ColliderSaver saver, List<ObjectSaverTemplate> templateList)
        {
            if (!PersistenceManager.Instance.SaverListContainsIdentifier(saver.GetIdentifier(), templateList))
            {
                InitializeCollider(saver);
                return;
            }

            foreach (var savedCollider in CharacterData.GameScenes[CharacterData.GameSceneEntryIndex].SavedColliders)
            {
                if (savedCollider.indentifier != saver.GetIdentifier()) continue;
                savedCollider.Enabled = saver.Collider.enabled;
                savedCollider.IsTrigger = saver.Collider.isTrigger;
            }
        }

        public void InitializeInteractableObject(InteractableObjectSaver saver)
        {
            InteractableObjectSaverTemplate newTransform = new InteractableObjectSaverTemplate
            {
                indentifier = saver.GetIdentifier(),
                Dynamic = saver.IsDynamic(),
                UsedCount = 0,
                Cooldown = saver.interactable.CooldownLeft
            };

            if (newTransform.Dynamic)
            {
                newTransform.ID =
                    PersistenceManager.Instance.GetDynamicObjectIDByName(
                        PersistenceManager.Instance.GetPrefabName(saver.gameObject.name));
            }

            CharacterData.GameScenes[CharacterData.GameSceneEntryIndex].SavedInteractableObjects.Add(newTransform);
        }
        
        public void UpdateInteractableObjectState(InteractableObjectSaver saver, List<ObjectSaverTemplate> templateList)
        {
            if (!PersistenceManager.Instance.SaverListContainsIdentifier(saver.GetIdentifier(), templateList))
            {
                InitializeInteractableObject(saver);
                return;
            }

            foreach (var savedInteractableObject in CharacterData.GameScenes[CharacterData.GameSceneEntryIndex].SavedInteractableObjects)
            {
                if (savedInteractableObject.indentifier != saver.GetIdentifier()) continue;
                savedInteractableObject.Cooldown = saver.interactable.CooldownLeft;
            }
        }
        
        public void InitializeContainerObject(ContainerObjectSaver saver)
        {
            ContainerObjectSaverTemplate newTransform = new ContainerObjectSaverTemplate
            {
                indentifier = saver.GetIdentifier(),
                Dynamic = saver.IsDynamic(),
            };

            for (int i = 0; i < saver.container.SlotAmount; i++)
            {
                newTransform.Slots.Add(new ContainerSlot());
            }

            if (newTransform.Dynamic)
            {
                newTransform.ID =
                    PersistenceManager.Instance.GetDynamicObjectIDByName(
                        PersistenceManager.Instance.GetPrefabName(saver.gameObject.name));
            }

            CharacterData.GameScenes[CharacterData.GameSceneEntryIndex].SavedContainerObjects.Add(newTransform);
        }
        
        public void UpdateContainerObjectState(ContainerObjectSaver saver, List<ObjectSaverTemplate> templateList)
        {
            if (!PersistenceManager.Instance.SaverListContainsIdentifier(saver.GetIdentifier(), templateList))
            {
                InitializeContainerObject(saver);
                return;
            }

            foreach (var savedContainerObject in CharacterData.GameScenes[CharacterData.GameSceneEntryIndex].SavedContainerObjects)
            {
                if (savedContainerObject.indentifier != saver.GetIdentifier()) continue;
            }
        }

        public void InitializeNPCSpawner(NPCSpawnerSaver saver)
        {
            NPCSpawnerSaverTemplate newNPCSpawner = new NPCSpawnerSaverTemplate
            {
                indentifier = saver.GetIdentifier(),
                Dynamic = saver.IsDynamic(),
            };

            if (newNPCSpawner.Dynamic)
            {
                newNPCSpawner.ID =
                    PersistenceManager.Instance.GetDynamicObjectIDByName(
                        PersistenceManager.Instance.GetPrefabName(saver.gameObject.name));
            }

            CharacterData.GameScenes[CharacterData.GameSceneEntryIndex].SavedNPCSpawners.Add(newNPCSpawner);
        }

        public void UpdateNPCSpawnerState(NPCSpawnerSaver saver, List<ObjectSaverTemplate> templateList)
        {
            if (!PersistenceManager.Instance.SaverListContainsIdentifier(saver.GetIdentifier(), templateList))
            {
                InitializeNPCSpawner(saver);
            }

            if (!saver.spawner.IsActive) return;
            
            foreach (var savedNPCSpawner in CharacterData.GameScenes[CharacterData.GameSceneEntryIndex].SavedNPCSpawners)
            {
                if (savedNPCSpawner.indentifier != saver.GetIdentifier()) continue;
                savedNPCSpawner.persistentNPCs.Clear();
                savedNPCSpawner.spawnedCount = saver.spawner.spawnedCount;

                foreach (var npc in saver.spawner.CurrentPersistentNPCs)
                {
                    if (!npc.IsPersistentNPC()) continue;
                    CombatData.PersistentNPCEntry persistentNpcEntry = new CombatData.PersistentNPCEntry
                    {
                        NPCName = npc.GetNPCData().entryName,
                        ID = npc.GetNPCData().ID,
                        position = npc.transform.position,
                        rotation = npc.transform.eulerAngles
                    };

                    foreach (var stat in npc.GetStats().Values)
                    {
                        if (!stat.stat.IsPersistent) continue;

                        CombatData.VitalityStatEntry savedState = new CombatData.VitalityStatEntry
                        {
                            StatName = stat.stat.entryName,
                            StatID = stat.stat.ID,
                            value = stat.currentValue,
                        };

                        persistentNpcEntry.VitalityStats.Add(savedState);
                    }
                    savedNPCSpawner.persistentNPCs.Add(persistentNpcEntry);
                }
            }
        }

    }
}
