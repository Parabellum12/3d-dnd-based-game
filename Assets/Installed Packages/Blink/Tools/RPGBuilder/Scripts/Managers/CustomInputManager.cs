using System;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.LogicMono;
using UnityEngine;
using Random = System.Random;

namespace BLINK.RPGBuilder.Managers
{
    public class CustomInputManager : MonoBehaviour
    {
        private string currentlyModifiedActionKey;
        private bool isKeyChecking;
        
        public List<CanvasGroup> allOpenedPanels = new List<CanvasGroup>();

        private int tabTargetSelected;
        private List<CombatEntity> previouslySelectedCbtNodes = new List<CombatEntity>();
        private void Awake()
        {
            if (Instance != null) return;
            Instance = this;
        }

        public void AddOpenedPanel(CanvasGroup cg)
        {
            if(!allOpenedPanels.Contains(cg)) allOpenedPanels.Insert(0, cg);
        }

        private void Update()
        {
            if (!RPGBuilderEssentials.Instance.isInGame) return;
            if (Input.GetKeyDown(KeyCode.Escape) && !HandleEscape()) return;
            
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                HandleTabTarget();
            }

            if (GameState.playerEntity == null) return;
            if (UIEvents.Instance.IsPanelOpen("--- DEVELOPER UI ---") && UIEvents.Instance.IsTyping()) return;

            if (CheckComboKeys()) return;
            CheckActionKeys();
            HandleActionAbilities();
                    
            if (!isKeyChecking) return;
            HandleKeyChange();
        }

        void HandleActionAbilities()
        {
            if (UIEvents.Instance.CursorHoverUI) return;
            if (GameState.playerEntity.IsShapeshifted() && GameState.playerEntity.ShapeshiftedEffect.ranks[GameState.playerEntity.ShapeshiftedEffectRank].shapeshiftingNoActionAbilities) return;
            foreach (var ActionAbility in Character.Instance.CharacterData.ActionAbilities)
            {
                KeyCode key = KeyCode.None;
                switch (ActionAbility.keyType)
                {
                    case RPGCombatDATA.ActionAbilityKeyType.OverrideKey:
                        key = ActionAbility.key;
                        break;
                    case RPGCombatDATA.ActionAbilityKeyType.ActionKey:
                        key = RPGBuilderUtilities.GetCurrentKeyByActionKeyName(ActionAbility.actionKeyName);
                        break;
                }
                if (Input.GetKeyDown(key) && Time.time >= ActionAbility.NextTimeUse)
                {
                    CombatManager.Instance.InitAbility(GameState.playerEntity, ActionAbility.ability, GameState.playerEntity.GetCurrentAbilityRank(ActionAbility.ability, false),false);
                }
                if (Input.GetKeyUp(key))
                {
                    CombatManager.Instance.AbilityKeyUp(ActionAbility.ability, false);
                }
            }
        }

        private bool HandleEscape()
        {
            if (allOpenedPanels.Count > 0)
            {
                if (allOpenedPanels[0].gameObject == null)
                {
                    allOpenedPanels.Clear();
                    return true;
                }

                allOpenedPanels[0].gameObject.GetComponent<DisplayPanel>().Hide();
                return false;
            }

            if (GameState.playerEntity.IsCasting())
            {
                GameState.playerEntity.ResetCasting();
                return false;
            }

            if (GameState.playerEntity.GetTarget() == null) return true;
            GameState.playerEntity.ResetTarget();
            return false;
        }

        void HandleTabTarget()
        {
            float maxAngle = 70;
            float maxDist = 50;

            CombatEntity newTarget = null;
            float lastDist = 1000;
            int validTargets = 0;

            foreach (var cbtNode in GameState.combatEntities)
            {
                if (cbtNode == GameState.playerEntity) continue;
                CombatData.EntityAlignment thisNodeAlignment = FactionManager.Instance.GetAlignment(cbtNode.GetFaction(),
                    FactionManager.Instance.GetEntityStanceToFaction(GameState.playerEntity, cbtNode.GetFaction()));
                
                if (thisNodeAlignment == CombatData.EntityAlignment.Ally) continue;
                if (cbtNode == GameState.playerEntity.GetTarget()) continue;
                float thisDist = Vector3.Distance(cbtNode.transform.position,
                    GameState.playerEntity.transform.position);
                if (thisDist > maxDist) continue;
                var pointDirection = cbtNode.transform.position - GameState.playerEntity.transform.position;
                var angle = Vector3.Angle(GameState.playerEntity.transform.forward, pointDirection);
                if (!(angle < maxAngle)) continue;
                validTargets++;
                if (previouslySelectedCbtNodes.Contains(cbtNode)) continue;
                if (!(lastDist > thisDist)) continue;
                newTarget = cbtNode;
                lastDist = thisDist;
            }

            if (newTarget == null)
            {
                previouslySelectedCbtNodes.Clear();
                tabTargetSelected = 0;
                if(validTargets>0) HandleTabTarget();
                return;
            }

            GameState.playerEntity.SetTarget(newTarget);
            previouslySelectedCbtNodes.Add(newTarget);
            tabTargetSelected++;
            if (tabTargetSelected > 5)
            {
                previouslySelectedCbtNodes.RemoveAt(0);
            }
        }

        private void HandleKeyChange()
        {
            foreach (KeyCode keyPressed in Enum.GetValues(typeof(KeyCode)))
                if (Input.GetKeyDown(keyPressed))
                {
                    ModifyKeybind(currentlyModifiedActionKey, keyPressed);
                    isKeyChecking = false;
                    currentlyModifiedActionKey = "";
                }
        }

        private void CheckActionKeyUp(string actionKeyName)
        {
            switch (actionKeyName)
            {
                case "SPRINT":
                    GameState.playerEntity.controllerEssentials.EndSprint();
                    break;
            }
        }
        
        private void CheckActionKeys()
        {
            foreach (var t in Character.Instance.CharacterData.ActionKeys)
            {
                bool up = Input.GetKeyUp(t.currentKey);
                bool down = Input.GetKeyDown(t.currentKey);
                if (up) CheckActionKeyUp(t.actionKeyName);
                if (t.actionKeyName.Contains("ACTION_BAR_SLOT_"))
                {
                    if (!down && !up) continue;
                    if (!up && UIEvents.Instance.CursorHoverUI) return;
                    ActionBarSlot slot = ActionBarManager.Instance.GetActionBarSlotFromActionKeyName(t.actionKeyName);
                    if(slot == null) continue;
                    if(up && slot.contentType != CharacterEntries.ActionBarSlotContentType.Ability) continue;
                    
                    switch (slot.contentType)
                    {
                        case CharacterEntries.ActionBarSlotContentType.None:
                            break;
                        case CharacterEntries.ActionBarSlotContentType.Ability:
                            if (up)
                            {
                                CombatManager.Instance.AbilityKeyUp(slot.ThisAbility,false);
                            }
                            else
                            {
                                if (RPGBuilderUtilities.IsAbilityInCombo(slot.ThisAbility.ID)) continue;
                                CombatManager.Instance.InitAbility(GameState.playerEntity, slot.ThisAbility, GameState.playerEntity.GetCurrentAbilityRank(slot.ThisAbility, true),true);
                            }
                            break;
                        case CharacterEntries.ActionBarSlotContentType.Item:
                            InventoryManager.Instance.UseItemFromBar(slot.ThisItem);
                            break;
                    }
                    
                }
                else if (t.actionKeyName.Contains("UI_PANEL_"))
                {
                    if (!down) continue;
                    // WE USED AN UI KEY
                    var uiPanelString = t.actionKeyName.Replace("UI_PANEL_", "");
                    switch (uiPanelString)
                    {
                        case "CHARACTER":
                            UIEvents.Instance.OnPanelToggle("Character");
                            break;
                        case "INVENTORY":
                            UIEvents.Instance.OnPanelToggle("Inventory");
                            break;
                        case "SKILLS":
                            UIEvents.Instance.OnPanelToggle("Skill_Book");
                            break;
                        case "QUESTS":
                            UIEvents.Instance.OnPanelToggle("Quest_Log");
                            break;
                        case "OPTIONS":
                            if (GameState.playerEntity.GetTarget() == null) UIEvents.Instance.OnPanelToggle("Options");
                            break;
                        case "LOOTALL":
                            if (UIEvents.Instance.IsPanelOpen("Loot")) GameEvents.Instance.OnLootAllBag();
                            break;
                        case "ENCHANTING":
                            UIEvents.Instance.OnPanelToggle("Enchanting");
                            break;
                        case "SOCKETING":
                            UIEvents.Instance.OnPanelToggle("Socketing");
                            break;
                        case "SPELLBOOK":
                            UIEvents.Instance.OnPanelToggle("Spellbook");
                            break;
                        case "WEAPON_TEMPLATES":
                            UIEvents.Instance.OnPanelToggle("Weapon_Templates");
                            break;
                        case "STATS_ALLOCATION":
                            UIEvents.Instance.OnPanelToggle("Stats_Allocation");
                            break;
                    }
                }
                else
                {
                    if (!down) continue;
                    switch (t.actionKeyName)
                    {
                        case "TOGGLE_CURSOR":
                            GameState.playerEntity.controllerEssentials.ToggleCameraMouseLook();
                            break;
                        case "SPRINT":
                            GameState.playerEntity.controllerEssentials.StartSprint();
                            break;
                        case "INTERACT":
                            if(WorldInteractableDisplayManager.Instance.cachedInteractable != null) WorldInteractableDisplayManager.Instance.Interact();
                            break;
                        case "TOGGLE_COMBAT_STATE":
                            if (GameDatabase.Instance.GetCombatSettings().AutomaticCombatStates) return;
                            if(!GameState.playerEntity.IsInCombat()) GameState.playerEntity.EnterCombat();
                            else GameState.playerEntity.ResetCombat();
                            break;
                        case "PETS_FOLLOW":
                            CombatEvents.Instance.OnPlayerPetsFollow();
                            break;
                        case "PETS_STAY":
                            CombatEvents.Instance.OnPlayerPetsStay();
                            break;
                        case "PETS_AGGRESSIVE":
                            CombatEvents.Instance.OnPlayerPetsAggro();
                            break;
                        case "PETS_DEFEND":
                            CombatEvents.Instance.OnPlayerPetsDefend();
                            break;
                        case "PETS_RESET":
                            CombatEvents.Instance.OnPlayerPetsReset();
                            break;
                        case "PETS_ATTACK":
                            CombatEvents.Instance.OnPlayerPetsAttack();
                            break;
                    }

                    if (t.actionKeyName.Contains("SHAPESHIFT_"))
                    {
                        string numberText = t.actionKeyName.Replace("SHAPESHIFT_", "");
                        int shapeshiftNumber = int.Parse(numberText);

                        if (ShapeshiftingSlotsDisplayManager.Instance.slots.Count >= shapeshiftNumber)
                        {
                            ShapeshiftingSlotsDisplayManager.Instance.ActivateShapeshift(shapeshiftNumber-1);
                        }
                    }
                }
            }
        }

        public void HandleUIPanelClose(CanvasGroup cg)
        {
            if (allOpenedPanels.Contains(cg))
            {
                allOpenedPanels.Remove(cg);

                if (GameState.playerEntity != null && allOpenedPanels.Count == 0)
                {
                    GameState.playerEntity.controllerEssentials.GameUIPanelAction(false);
                }
            }
            else
            {
                GameState.playerEntity.controllerEssentials.GameUIPanelAction(false);
            }
        }

        private bool CheckComboKeys()
        {
            if (UIEvents.Instance.CursorHoverUI) return false;
            foreach (var t in GameState.playerEntity.GetActiveCombos())
            {
                if (!Input.GetKeyDown(t.keyRequired)) continue;
                if (t.readyTime > 0) continue;
                CombatManager.Instance.CancelOtherComboOptions(GameState.playerEntity, t.combo);
                CombatManager.Instance.InitAbility(GameState.playerEntity,
                    GameDatabase.Instance.GetAbilities()[t.combo.combos[t.comboIndex].abilityID], GameState.playerEntity.GetCurrentAbilityRank(GameDatabase.Instance.GetAbilities()[t.combo.combos[t.comboIndex].abilityID], t.combo.combos[t.comboIndex].abMustBeKnown),
                    t.combo.combos[t.comboIndex].abMustBeKnown);
                return true;
            }

            return false;
        }

        public void InitKeyChecking(string keybindName)
        {
            isKeyChecking = true;
            currentlyModifiedActionKey = keybindName;
        }

        private void ModifyKeybind(string actionKeyName, KeyCode newKey)
        {
            foreach (var actionKey in Character.Instance.CharacterData.ActionKeys.Where(actionKey => actionKey.actionKeyName == actionKeyName))
            {
                if (!isKeyAvailable(newKey))
                {
                    UIEvents.Instance.OnShowAlertMessage("This key is not available", 3);
                    return;
                }
                actionKey.currentKey = newKey;

                if (actionKey.actionKeyName.Contains("ACTION_BAR_SLOT_"))
                {
                    var slotIDString = actionKey.actionKeyName.Replace("ACTION_BAR_SLOT_", "");
                    var ID = int.Parse(slotIDString);
                    ActionBarManager.Instance.UpdateSlotKeyText(ID-1, newKey);
                }
            }

            GameEvents.Instance.OnKeybindChanged(actionKeyName, newKey);
        }

        private bool isKeyAvailable(KeyCode key)
        {
            foreach (var actionKey in Character.Instance.CharacterData.ActionKeys.Where(actionKey => actionKey.currentKey == key))
            {
                return !RPGBuilderUtilities.isActionKeyUnique(actionKey.actionKeyName);
            }

            return true;
        }

        public void ResetKey(string actionKeyName)
        {
            foreach (var actionKey in Character.Instance.CharacterData.ActionKeys.Where(actionKey => actionKey.actionKeyName == actionKeyName))
            {
                actionKey.currentKey = KeyCode.None;

                if (actionKey.actionKeyName.Contains("ACTION_BAR_SLOT_"))
                {
                    var slotIDString = actionKey.actionKeyName.Replace("ACTION_BAR_SLOT_", "");
                    var ID = int.Parse(slotIDString);
                    ActionBarManager.Instance.UpdateSlotKeyText(ID - 1, actionKey.currentKey);
                }
                
                GameEvents.Instance.OnKeybindChanged(actionKeyName, actionKey.currentKey);
            }
        }


        public static CustomInputManager Instance { get; private set; }
    }
}