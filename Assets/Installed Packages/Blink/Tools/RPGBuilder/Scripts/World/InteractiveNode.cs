using System;
using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.Data;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.Templates;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace BLINK.RPGBuilder.World
{
    public class InteractiveNode : MonoBehaviour, IPlayerInteractable
    {
        public enum InteractiveNodeType
        {
            resourceNode,
            effectNode,
            abilityNode,
            questNode,
            giveTreePoint,
            teachSkill,
            giveClassEXP,
            giveSkillEXP,
            completeTask,
            container,
            UnityEvent
        }

        public InteractiveNodeType nodeType;


        [Serializable]
        public class containerLootTablesDATA
        {
            public RPGLootTable lootTable;
            public float chance = 100f;
        }

        public List<containerLootTablesDATA> containerTablesData = new List<containerLootTablesDATA>();

        [Serializable]
        public class effectsDATA
        {
            public RPGEffect effect;
            public float chance = 100f;
        }

        public List<effectsDATA> effectsData = new List<effectsDATA>();

        [Serializable]
        public class abilitiesDATA
        {
            public RPGAbility ability;
            public float chance = 100f;
        }

        public List<abilitiesDATA> abilitiesData = new List<abilitiesDATA>();

        [Serializable]
        public class questsDATA
        {
            public RPGQuest quest;
            public float chance = 100f;
        }

        public questsDATA questsData;

        [Serializable]
        public class treePointsDATA
        {
            public RPGTreePoint treePoint;
            public int amount;
            public float chance = 100f;
        }

        public List<treePointsDATA> treePointsData = new List<treePointsDATA>();

        [Serializable]
        public class skillsDATA
        {
            public RPGSkill skill;
            public float chance = 100f;
        }

        public List<skillsDATA> skillsData = new List<skillsDATA>();

        [Serializable]
        public class classExpDATA
        {
            public int expAmount;
            public float chance = 100f;
        }

        public classExpDATA classExpData;

        [Serializable]
        public class skillExpDATA
        {
            public RPGSkill skill;
            public int expAmount;
            public float chance = 100f;
        }

        public List<skillExpDATA> skillExpData = new List<skillExpDATA>();

        [Serializable]
        public class taskDATA
        {
            public RPGTask task;
            public float chance = 100f;
        }

        public List<taskDATA> taskData = new List<taskDATA>();

        public RPGResourceNode resourceNodeData;

        public UnityEvent unityEvent;

        public enum InteractiveNodeState
        {
            ready,
            cooldown,
            disabled
        }

        public InteractiveNodeState nodeState;

        public int useCount;
        public float cooldown, nextUse, interactionTime, useDistanceMax = 2;

        public GameObject readyVisual, onCooldownVisual, disabledVisual;
        public GameObject currentVisualGO;
        public bool isTrigger, isClick = true;

        public string nodeUseAnimation;

        public AudioClip nodeUseSound;

        public float interactableUIOffsetY = 2;
        public string interactableName;

        public List<VisualEffectEntry> visualEffects = new List<VisualEffectEntry>();
        public List<AnimationEntry> animations = new List<AnimationEntry>();
        public List<SoundEntry> sounds = new List<SoundEntry>();

        public RPGBWeaponType weaponEquippedRequired;
        private void Start()
        {
            SetNodeState(InteractiveNodeState.ready);
        }

        private void HideAllVisuals()
        {
            if (readyVisual != null) readyVisual.SetActive(false);

            if (onCooldownVisual != null) onCooldownVisual.SetActive(false);

            if (disabledVisual != null) disabledVisual.SetActive(false);
        }

        private void ShowVisual(GameObject go)
        {
            if (go != null) go.SetActive(true);
        }

        private IEnumerator resetNode(float delay)
        {
            yield return new WaitForSeconds(delay);

            SetNodeState(InteractiveNodeState.ready);
        }

        private void SetNodeState(InteractiveNodeState newState)
        {
            nodeState = newState;
            HideAllVisuals();
            switch (nodeState)
            {
                case InteractiveNodeState.ready:
                    ShowVisual(readyVisual);
                    break;
                case InteractiveNodeState.cooldown:
                    ShowVisual(onCooldownVisual);
                    break;
                case InteractiveNodeState.disabled:
                    ShowVisual(disabledVisual);
                    break;
            }
        }

        public void UseNode()
        {
            if (!(Time.time >= nextUse)) return;

            if ((nodeType == InteractiveNodeType.resourceNode || nodeType == InteractiveNodeType.container) &&
                RPGBuilderUtilities.isInventoryFull())
            {
                UIEvents.Instance.OnShowAlertMessage("The inventory is full", 3);
                return;
            }

            if (!string.IsNullOrEmpty(nodeUseAnimation))
            {
                Animator anim = GetComponent<Animator>();
                if (anim != null) anim.SetTrigger(nodeUseAnimation);
            }

            if (nodeUseSound != null)
            {
                RPGBuilderUtilities.PlaySound(gameObject, gameObject, nodeUseSound, true);
            }

            if (nodeType != InteractiveNodeType.resourceNode)
            {
                nextUse = Time.time + cooldown;
                StartCoroutine(resetNode(cooldown));
            }
            else
            {
                var curRank = RPGBuilderUtilities.getResourceNodeRank(resourceNodeData.ID);
                var rankREF = resourceNodeData.ranks[curRank];
                nextUse = Time.time + rankREF.respawnTime;
                StartCoroutine(resetNode(rankREF.respawnTime));
            }

            SetNodeState(InteractiveNodeState.cooldown);
            
            if((InteractiveNode) WorldInteractableDisplayManager.Instance.cachedInteractable == this) WorldInteractableDisplayManager.Instance.Hide();

            var totalItemDropped = 0;
            switch (nodeType)
            {
                case InteractiveNodeType.container:
                    float LOOTCHANCEMOD = CombatUtilities.GetTotalOfStatType(GameState.playerEntity,
                        RPGStat.STAT_TYPE.LOOT_CHANCE_MODIFIER);
                    foreach (var t in containerTablesData)
                    {
                        var chance = Random.Range(0, 100f);
                        if (t.chance != 0 && !(chance <= t.chance)) continue;
                        foreach (var t1 in t.lootTable.lootItems)
                        {
                            var itemDropAmount = Random.Range(0f, 100f);
                            if (LOOTCHANCEMOD > 0) itemDropAmount += itemDropAmount * (LOOTCHANCEMOD / 100);
                            if (!(itemDropAmount <= t1.dropRate)) continue;
                            var stack = 0;
                            if (t1.min ==
                                t1.max)
                                stack = t1.min;
                            else
                                stack = Random.Range(t1.min,
                                    t1.max + 1);

                            RPGItem itemREF = GameDatabase.Instance.GetItems()[t1.itemID];
                            if (itemREF.dropInWorld && itemREF.itemWorldModel != null)
                            {
                                var newLoot = new EconomyData.WorldDroppedItemEntry {item = itemREF, count = stack};
                                GameObject newLootGO = Instantiate(itemREF.itemWorldModel, new Vector3(
                                    transform.position.x,
                                    transform.position.y + 1, transform.position.z), Quaternion.identity);
                                newLootGO.layer = itemREF.worldInteractableLayer;
                                newLoot.worldDroppedItemREF = newLootGO.AddComponent<WorldDroppedItem>();
                                newLoot.worldDroppedItemREF.curLifetime = 0;
                                newLoot.worldDroppedItemREF.maxDuration = itemREF.durationInWorld;
                                newLoot.worldDroppedItemREF.item = itemREF;
                                newLoot.itemDataID =
                                    RPGBuilderUtilities.HandleNewItemDATA(itemREF.ID,
                                        CharacterEntries.ItemEntryState.InWorld);

                                newLoot.worldDroppedItemREF.InitPhysics();
                                GameState.allWorldDroppedItems.Add(newLoot);
                            }
                            else
                            {
                                // TODOINVENTORYHANDLING SHOW A WINDOW, SO THAT WE CAN LEAVE THE ITEMS THAT COULD NOT BE LOOTED
                                RPGBuilderUtilities.HandleItemLooting(itemREF.ID, -1, stack, false, true);
                            }

                            totalItemDropped++;
                        }
                    }

                    break;

                case InteractiveNodeType.resourceNode:
                    if (RPGBuilderUtilities.isResourceNodeKnown(resourceNodeData.ID))
                    {
                        var curRank = RPGBuilderUtilities.getResourceNodeRank(resourceNodeData.ID);
                        var rankREF = resourceNodeData.ranks[curRank];
                        var lootTableREF = GameDatabase.Instance.GetLootTables()[rankREF.lootTableID];
                        foreach (var t in lootTableREF.lootItems)
                        {
                            var chance = Random.Range(0f, 100f);
                            if (!(chance <= t.dropRate)) continue;
                            var stack = 0;
                            if (t.min == t.max)
                                stack = t.min;
                            else
                                stack = Random.Range(t.min,
                                    t.max + 1);

                            RPGItem itemREF = GameDatabase.Instance.GetItems()[t.itemID];
                            if (itemREF.dropInWorld && itemREF.itemWorldModel != null)
                            {
                                var newLoot = new EconomyData.WorldDroppedItemEntry();
                                newLoot.item = itemREF;
                                newLoot.count = stack;
                                GameObject newLootGO = Instantiate(itemREF.itemWorldModel, new Vector3(
                                    transform.position.x,
                                    transform.position.y + 1, transform.position.z), Quaternion.identity);
                                newLootGO.layer = itemREF.worldInteractableLayer;
                                newLoot.worldDroppedItemREF = newLootGO.AddComponent<WorldDroppedItem>();
                                newLoot.worldDroppedItemREF.curLifetime = 0;
                                newLoot.worldDroppedItemREF.maxDuration = itemREF.durationInWorld;
                                newLoot.worldDroppedItemREF.item = itemREF;
                                newLoot.itemDataID =
                                    RPGBuilderUtilities.HandleNewItemDATA(itemREF.ID,
                                        CharacterEntries.ItemEntryState.InWorld);

                                newLoot.worldDroppedItemREF.InitPhysics();
                                GameState.allWorldDroppedItems.Add(newLoot);
                            }
                            else
                            {
                                // TODOINVENTORYHANDLING SHOW A WINDOW, SO THAT WE CAN LEAVE THE ITEMS THAT COULD NOT BE LOOTED
                                RPGBuilderUtilities.HandleItemLooting(itemREF.ID, -1, stack, false, true);
                            }

                            totalItemDropped++;
                        }

                        LevelingManager.Instance.GenerateSkillEXP(resourceNodeData.skillRequiredID, rankREF.Experience);
                    }

                    break;

                case InteractiveNodeType.effectNode:
                    foreach (var t in effectsData)
                    {
                        var chance = Random.Range(0f, 100f);
                        if (t.chance == 0 || chance <= t.chance)
                            CombatManager.Instance.ExecuteEffect(GameState.playerEntity,
                                GameState.playerEntity, t.effect, 0, null, 0);
                    }

                    break;

                case InteractiveNodeType.questNode:
                    var chance2 = Random.Range(0f, 100f);
                    if (questsData.chance == 0 || chance2 <= questsData.chance)
                        UIEvents.Instance.OnDisplayQuestInJournal(questsData.quest);

                    break;

                case InteractiveNodeType.giveTreePoint:
                    foreach (var t in treePointsData)
                    {
                        var chance = Random.Range(0, 100f);
                        if (t.chance == 0 || chance <= t.chance)
                            TreePointsManager.Instance.AddTreePoint(t.treePoint.ID,
                                t.amount);
                    }

                    break;

                case InteractiveNodeType.giveClassEXP:
                {
                    if (GameDatabase.Instance.GetCharacterSettings().NoClasses) return;
                    var chance = Random.Range(0, 100f);
                    if (classExpData.chance == 0 || chance <= classExpData.chance)
                        LevelingManager.Instance.AddCharacterEXP(classExpData.expAmount);
                }
                    break;

                case InteractiveNodeType.giveSkillEXP:
                    foreach (var t in skillExpData)
                    {
                        var chance = Random.Range(0, 100f);
                        if (t.chance == 0 || chance <= t.chance)
                            LevelingManager.Instance.AddSkillEXP(t.skill.ID,
                                t.expAmount);
                    }

                    break;

                case InteractiveNodeType.completeTask:
                    foreach (var t in taskData)
                    {
                        var chance = Random.Range(0, 100f);
                        if (t.chance != 0 && !(chance <= t.chance)) continue;
                        for (var index = 0; index < Character.Instance.CharacterData.Quests.Count; index++)
                        {
                            var quest = Character.Instance.CharacterData.Quests[index];
                            for (var i = 0; i < quest.objectives.Count; i++)
                            {
                                var objective = quest.objectives[i];
                                if (objective.taskID != t.task.ID) continue;
                                TaskCheckerManager.Instance.CompleteTaskInstantly(index, i);
                                WorldEvents.Instance.OnQuestProgressChanged(GameDatabase.Instance.GetQuests()[quest.questID], i);
                            }
                        }
                    }

                    break;

                case InteractiveNodeType.UnityEvent:
                    unityEvent.Invoke();
                    break;
            }
        }

        private void InitInteractionTime(float interactionTime)
        {
            //GameState.playerEntity.InitNodeInteracting(this, interactionTime);
        }

        private bool UseRequirementsMet()
        {
            return false;
        }


        private void OnMouseOver()
        {
            if (!isClick) return;
            if (UIEvents.Instance.CursorHoverUI)
            {
                UIEvents.Instance.OnSetCursorToDefault();
                return;
            }
            if (Input.GetMouseButtonUp(1) && !GameState.playerEntity.IsInteractiveNodeCasting())
                if (Vector3.Distance(transform.position, GameState.playerEntity.transform.position) <= useDistanceMax)
                {
                    if (UseRequirementsMet())
                    {
                        
                        GameEvents.Instance.OnTriggerAnimationsList(GameState.playerEntity, animations, ActivationType.Start);
                        
                        if (nodeType == InteractiveNodeType.resourceNode)
                        {
                            var curRank = RPGBuilderUtilities.getResourceNodeRank(resourceNodeData.ID);
                            var rankREF = resourceNodeData.ranks[curRank];
                            if (rankREF.gatherTime == 0)
                                UseNode();
                            else
                                InitInteractionTime(rankREF.gatherTime);
                        }
                        else
                        {
                            if (interactionTime == 0)
                                UseNode();
                            else
                                InitInteractionTime(interactionTime);
                        }
                    }
                }
                else
                {
                    if (GameState.playerEntity.controllerEssentials.GETControllerType() ==
                        RPGBuilderGeneralSettings.ControllerTypes.TopDownClickToMove)
                    {
                        
                    }
                    else
                    {
                        UIEvents.Instance.OnShowAlertMessage("This is too far", 3);
                    }
                }

            if (nodeState == InteractiveNodeState.ready)
                UIEvents.Instance.OnSetNewCursor(CursorType.InteractiveObject);
        }

        private void OnMouseExit()
        {
            UIEvents.Instance.OnSetCursorToDefault();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isTrigger || other.gameObject != GameState.playerEntity.gameObject) return;
            if (GameState.playerEntity.IsInteractiveNodeCasting() || !UseRequirementsMet()) return;
            
            GameEvents.Instance.OnTriggerAnimationsList(GameState.playerEntity, animations, ActivationType.Start);
            
            if (nodeType == InteractiveNodeType.resourceNode)
            {
                var curRank = RPGBuilderUtilities.getResourceNodeRank(resourceNodeData.ID);
                var rankREF = resourceNodeData.ranks[curRank];
                if (rankREF.gatherTime == 0)
                    UseNode();
                else
                    InitInteractionTime(rankREF.gatherTime);
            }
            else
            {
                if (interactionTime == 0)
                    UseNode();
                else
                    InitInteractionTime(interactionTime);
            }
        }

        public void ResetAllInteractionAnimations()
        {
            
        }
        
        public void Interact()
        {
            if (UIEvents.Instance.CursorHoverUI) return;
            if (GameState.playerEntity.IsInteractiveNodeCasting()) return;
            if (!(Vector3.Distance(transform.position, GameState.playerEntity.transform.position) <= useDistanceMax)) return;
            if (!UseRequirementsMet()) return;

            GameEvents.Instance.OnTriggerAnimationsList(GameState.playerEntity, animations, ActivationType.Start);
            
            if (nodeType == InteractiveNodeType.resourceNode)
            {
                var curRank = RPGBuilderUtilities.getResourceNodeRank(resourceNodeData.ID);
                var rankREF = resourceNodeData.ranks[curRank];
                if (rankREF.gatherTime == 0)
                    UseNode();
                else
                    InitInteractionTime(rankREF.gatherTime);
            }
            else
            {
                if (interactionTime == 0)
                    UseNode();
                else
                    InitInteractionTime(interactionTime);
            }
        }

        public void ShowInteractableUI()
        {
            var pos = transform;
            Vector3 worldPos = new Vector3(pos.position.x, pos.position.y + interactableUIOffsetY, pos.position.z);
            var screenPos = Camera.main.WorldToScreenPoint(worldPos);
            WorldInteractableDisplayManager.Instance.transform.position = new Vector3(screenPos.x, screenPos.y, screenPos.z);
            
            if ((InteractiveNode) WorldInteractableDisplayManager.Instance.cachedInteractable == this) return;
            WorldInteractableDisplayManager.Instance.Show(this);
        }

        public string getInteractableName()
        {
            return nodeType == InteractiveNodeType.resourceNode ? resourceNodeData.entryDisplayName : interactableName;
        }

        public bool isReadyToInteract()
        {
            return nodeState == InteractiveNodeState.ready;
        }

        public RPGCombatDATA.INTERACTABLE_TYPE getInteractableType()
        {
            return RPGCombatDATA.INTERACTABLE_TYPE.InteractableObject;
        }
    }
}