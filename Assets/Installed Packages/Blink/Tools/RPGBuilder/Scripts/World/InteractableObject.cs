using System.Collections.Generic;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Data;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.Templates;
using BLINK.RPGBuilder.UIElements;
using BLINK.RPGBuilder.WorldPersistence;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BLINK.RPGBuilder.World
{
    public class InteractableObject : MonoBehaviour, IPlayerInteractable
    {
        public InteractableObjectData.InteractableObjectState State;

        public List<InteractableObjectData.InteractableObjectAction> Actions =
            new List<InteractableObjectData.InteractableObjectAction>();

        public List<InteractableObjectData.InteractableObjectVisualEffect> VisualEfects =
            new List<InteractableObjectData.InteractableObjectVisualEffect>();
        public List<InteractableObjectData.InteractableObjectAnimation> Animations =
            new List<InteractableObjectData.InteractableObjectAnimation>();
        public List<InteractableObjectData.InteractableObjectSound> Sounds =
            new List<InteractableObjectData.InteractableObjectSound>();

        public bool IsPersistent;
        public InteractableObjectSaver Saver;

        public bool UseResourceValues;
        public RPGResourceNode Resource;

        public RequirementsTemplate RequirementsTemplate;
        public Animator anim;
        
        public bool LimitedUseAmount;
        public int MaxUseAmount, MaxActions = 1;
        public float Cooldown = 1, InteractionTime = 2, MaxDistance = 4;
        public GameObject ReadyAppearance, OnCooldownAppearance, UnavailableAppearance;
        public bool IsTrigger, IsClick = true;

        public float UIOffsetY = 1.5f;
        public string InteractableName;

        public float CooldownLeft;

        public void SetState(InteractableObjectData.InteractableObjectState newState)
        {
            State = newState;
            ResetAppearance();
            switch (State)
            {
                case InteractableObjectData.InteractableObjectState.Ready:
                    EnableStateApperance(ReadyAppearance);
                    break;
                case InteractableObjectData.InteractableObjectState.OnCooldown:
                    EnableStateApperance(OnCooldownAppearance);
                    break;
                case InteractableObjectData.InteractableObjectState.Unavailable:
                    EnableStateApperance(UnavailableAppearance);
                    break;
            }
        }

        private void EnableStateApperance(GameObject go)
        {
            if (go != null) go.SetActive(true);
        }

        private void ResetAppearance()
        {
            if (ReadyAppearance != null) ReadyAppearance.SetActive(false);
            if (OnCooldownAppearance != null) OnCooldownAppearance.SetActive(false);
            if (UnavailableAppearance != null) UnavailableAppearance.SetActive(false);
        }

        private void FixedUpdate()
        {
            if (State != InteractableObjectData.InteractableObjectState.OnCooldown) return;
            if(CooldownLeft > 0) CooldownLeft -= Time.deltaTime;
            else
            {
                CooldownLeft = 0;
                if(State != InteractableObjectData.InteractableObjectState.Unavailable) SetState(InteractableObjectData.InteractableObjectState.Ready);
            }
        }

        public void TriggerVisualEffects(ActivationType activationType)
        {
            foreach (var visualEffect in VisualEfects)
            {
                if(visualEffect.ActivationType != activationType) continue;
                
                if (visualEffect.TargetType == InteractableObjectData.InteractableObjectTemplateTarget.Object)
                {
                    GameEvents.Instance.OnTriggerVisualEffect(gameObject, visualEffect.VisualEntry);
                } else if (visualEffect.TargetType == InteractableObjectData.InteractableObjectTemplateTarget.User)
                {
                    GameEvents.Instance.OnTriggerVisualEffect(GameState.playerEntity, visualEffect.VisualEntry);
                }
            }
        }
        
        public void TriggerAnimations(ActivationType activationType)
        {
            foreach (var animation in Animations)
            {
                if(animation.ActivationType != activationType) continue;
                
                if (animation.TargetType == InteractableObjectData.InteractableObjectTemplateTarget.Object && anim != null)
                {
                    GameEvents.Instance.OnTriggerAnimation(anim, animation.AnimationEntry);
                } else if (animation.TargetType == InteractableObjectData.InteractableObjectTemplateTarget.User)
                {
                    GameEvents.Instance.OnTriggerAnimation(GameState.playerEntity, animation.AnimationEntry);
                }
            }
        }
        
        public void TriggerSounds(ActivationType activationType)
        {
            foreach (var sound in Sounds)
            {
                if(sound.ActivationType != activationType) continue;
                
                if (sound.TargetType == InteractableObjectData.InteractableObjectTemplateTarget.Object)
                {
                    GameEvents.Instance.OnTriggerSound(gameObject, sound.SoundEntry, gameObject.transform);
                } else if (sound.TargetType == InteractableObjectData.InteractableObjectTemplateTarget.User)
                {
                    GameEvents.Instance.OnTriggerSound(GameState.playerEntity, sound.SoundEntry, GameState.playerEntity.transform);
                }
            }
        }

        public void UseObject()
        {
            CooldownLeft = Cooldown;
            SetState(InteractableObjectData.InteractableObjectState.OnCooldown);

            TriggerVisualEffects(ActivationType.Completed);
            TriggerAnimations(ActivationType.Completed);
            TriggerSounds(ActivationType.Completed);

            if ((InteractableObject) WorldInteractableDisplayManager.Instance.cachedInteractable == this)
                WorldInteractableDisplayManager.Instance.Hide();

            int totalActionsTriggered = 0;
            foreach (var action in Actions)
            {
                var chance = Random.Range(0, 100f);
                if (action.chance != 0 && !(chance <= action.chance)) continue;

                switch (action.type)
                {
                    case InteractableObjectData.InteractableObjectActionType.Effect:
                        GameActionsManager.Instance.ApplyEffect(RPGCombatDATA.TARGET_TYPE.Caster,
                            GameState.playerEntity, action.Effect.ID);
                        break;
                    case InteractableObjectData.InteractableObjectActionType.Quest:
                        UIEvents.Instance.OnDisplayQuestInJournal(GameDatabase.Instance.GetQuests()[action.Quest.ID]);
                        break;
                    case InteractableObjectData.InteractableObjectActionType.Point:
                        TreePointsManager.Instance.AddTreePoint(action.entryID, action.Point.ID);
                        break;
                    case InteractableObjectData.InteractableObjectActionType.GiveCharacterExperience:
                        LevelingManager.Instance.AddCharacterEXP(action.amount);
                        break;
                    case InteractableObjectData.InteractableObjectActionType.GiveSkillExperience:
                        LevelingManager.Instance.AddSkillEXP(action.entryID, action.Skill.ID);
                        break;
                    case InteractableObjectData.InteractableObjectActionType.GiveWeaponTemplateExperience:
                        LevelingManager.Instance.AddWeaponTemplateEXP(action.entryID, action.WeaponTemplate.ID);
                        break;
                    case InteractableObjectData.InteractableObjectActionType.CompleteTask:
                        for (var index = 0; index < Character.Instance.CharacterData.Quests.Count; index++)
                        {
                            var quest = Character.Instance.CharacterData.Quests[index];
                            for (var i = 0; i < quest.objectives.Count; i++)
                            {
                                var objective = quest.objectives[i];
                                if (objective.taskID != action.Task.ID) continue;
                                TaskCheckerManager.Instance.CompleteTaskInstantly(index, i);
                                WorldEvents.Instance.OnQuestProgressChanged(
                                    GameDatabase.Instance.GetQuests()[quest.questID], i);
                            }
                        }

                        break;
                    case InteractableObjectData.InteractableObjectActionType.UnityEvent:
                        action.unityEvents.Invoke();
                        break;
                    case InteractableObjectData.InteractableObjectActionType.SaveCharacter:
                        RPGBuilderJsonSaver.SaveCharacterData();
                        break;
                    case InteractableObjectData.InteractableObjectActionType.Resource:
                        if (RPGBuilderUtilities.isResourceNodeKnown(action.Resource.ID))
                        {
                            var curRank = RPGBuilderUtilities.getResourceNodeRank(action.Resource.ID);
                            var rankREF = action.Resource.ranks[curRank];
                            foreach (var t in GameDatabase.Instance.GetLootTables()[rankREF.lootTableID].lootItems)
                            {
                                chance = Random.Range(0f, 100f);
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
                                    var newLoot = new EconomyData.WorldDroppedItemEntry {item = itemREF, count = stack};
                                    GameObject newLootGO = GameEvents.Instance.InstantiateGameobject(
                                        itemREF.itemWorldModel, new Vector3(
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
                                    RPGBuilderUtilities.HandleItemLooting(itemREF.ID,
                                        RPGBuilderUtilities.HandleNewItemDATA(itemREF.ID,
                                            CharacterEntries.ItemEntryState.InWorld), stack, false, true);
                                }
                            }

                            LevelingManager.Instance.GenerateSkillEXP(action.Resource.skillRequiredID,
                                rankREF.Experience);
                        }

                        break;
                    case InteractableObjectData.InteractableObjectActionType.Chest:
                        foreach (var t in action.LootTable.lootItems)
                        {
                            chance = Random.Range(0f, 100f);
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
                                var newLoot = new EconomyData.WorldDroppedItemEntry {item = itemREF, count = stack};
                                GameObject newLootGO = GameEvents.Instance.InstantiateGameobject(
                                    itemREF.itemWorldModel, new Vector3(
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
                                RPGBuilderUtilities.HandleItemLooting(itemREF.ID,
                                    RPGBuilderUtilities.HandleNewItemDATA(itemREF.ID,
                                        CharacterEntries.ItemEntryState.InWorld), stack, false, true);
                            }
                        }

                        break;
                    case InteractableObjectData.InteractableObjectActionType.GameActions:
                        if (action.GameActionsTemplate != null)
                            GameActionsManager.Instance.TriggerGameActions(GameState.playerEntity, action.GameActionsTemplate.GameActions);
                        break;
                }

                totalActionsTriggered++;
                if (totalActionsTriggered >= MaxActions) break;
            }

            if (Saver != null)
            {
                if(Saver.SaveUsedCount) Saver.IncreaseUseCount();
                if(Saver.SaveCooldown) Saver.UpdateCoolDownValue(Cooldown);
            }
        }

        private void InitInteractionTime(float interactionTime)
        {
            TriggerVisualEffects(ActivationType.Start);
            TriggerAnimations(ActivationType.Start);
            TriggerSounds(ActivationType.Start);
            GameState.playerEntity.InitObjectInteraction(this, interactionTime);
        }

        private void OnMouseOver()
        {
            if (!IsClick) return;
            if (UIEvents.Instance.CursorHoverUI)
            {
                UIEvents.Instance.OnSetCursorToDefault();
                return;
            }

            if (State == InteractableObjectData.InteractableObjectState.Ready && Input.GetMouseButtonUp(1) && !GameState.playerEntity.IsInteractingWithObject())
            {
                if (Vector3.Distance(transform.position, GameState.playerEntity.transform.position) <= MaxDistance)
                {
                    if (RequirementsTemplate == null || RequirementsManager.Instance.RequirementsMet(GameState.playerEntity, RequirementsTemplate.Requirements).Result)
                    {
                        if (InteractionTime == 0)
                            UseObject();
                        else
                            InitInteractionTime(InteractionTime);
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
            }

            if (State == InteractableObjectData.InteractableObjectState.Ready)
                UIEvents.Instance.OnSetNewCursor(CursorType.InteractiveObject);
        }

        private void OnMouseExit()
        {
            UIEvents.Instance.OnSetCursorToDefault();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsTrigger || other.gameObject != GameState.playerEntity.gameObject || State != InteractableObjectData.InteractableObjectState.Ready) return;
            if (GameState.playerEntity.IsInteractingWithObject()) return;
            if(RequirementsTemplate != null && !RequirementsManager.Instance.RequirementsMet(GameState.playerEntity, RequirementsTemplate.Requirements).Result) return;

            if (InteractionTime == 0)
                UseObject();
            else
                InitInteractionTime(InteractionTime);
        }

        public void Interact()
        {
            if (UIEvents.Instance.CursorHoverUI) return;
            if (GameState.playerEntity.IsInteractingWithObject()) return;
            if (!(Vector3.Distance(transform.position, GameState.playerEntity.transform.position) <=
                  MaxDistance)) return;
            if (RequirementsTemplate != null && !RequirementsManager.Instance.RequirementsMet(GameState.playerEntity, RequirementsTemplate.Requirements).Result) return;

            if (InteractionTime == 0)
                UseObject();
            else
                InitInteractionTime(InteractionTime);
        }

        public void ShowInteractableUI()
        {
            var pos = transform;
            Vector3 worldPos = new Vector3(pos.position.x, pos.position.y + UIOffsetY, pos.position.z);
            var screenPos = Camera.main.WorldToScreenPoint(worldPos);
            WorldInteractableDisplayManager.Instance.transform.position =
                new Vector3(screenPos.x, screenPos.y, screenPos.z);

            if ((InteractableObject) WorldInteractableDisplayManager.Instance.cachedInteractable == this) return;
            WorldInteractableDisplayManager.Instance.Show(this);
        }

        public string getInteractableName()
        {
            return InteractableName;
        }

        public bool isReadyToInteract()
        {
            return State == InteractableObjectData.InteractableObjectState.Ready;
        }

        public RPGCombatDATA.INTERACTABLE_TYPE getInteractableType()
        {
            return RPGCombatDATA.INTERACTABLE_TYPE.InteractableObject;
        }
        
        
    }
}
