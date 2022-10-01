using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BLINK.RPGBuilder.Characters;

namespace BLINK.RPGBuilder.Managers
{
    public class ActionBarManager : MonoBehaviour
    {
        public static ActionBarManager Instance { get; private set; }
        
        public List<ActionBarSlot> actionBarSlots = new List<ActionBarSlot>();

        
        private void Awake()
        {
            if (Instance != null) return;
            Instance = this;
        }

        public int GetMainActionBarSlots()
        {
            int total = 0;
            foreach (var actionBarSlot in actionBarSlots)
            {
                if(actionBarSlot.actionBarType != CharacterEntries.ActionBarType.Main) continue;
                total++;
            }

            return total;
        }
        
        public ActionBarSlot GetActionBarSlotFromActionKeyName(string actionKeyName)
        {
            foreach (var actionBarSlot in actionBarSlots)
            {
                if(actionBarSlot.actionKeyName != actionKeyName) continue;
                return actionBarSlot;
            }

            return null;
        }
        
        public void SetItemToSlot (RPGItem item, int index)
        {
            if (index + 1 > actionBarSlots.Count) return;
            actionBarSlots[index].Init(item);

            if (actionBarSlots[index].actionBarType == CharacterEntries.ActionBarType.Main)
            {
                if (!GameState.playerEntity.IsStealth())
                {
                    Character.Instance.CharacterData.ActionBarSlots[index].contentType =
                        CharacterEntries.ActionBarSlotContentType.Item;
                    Character.Instance.CharacterData.ActionBarSlots[index].slotType = actionBarSlots[index].actionBarType;
                    Character.Instance.CharacterData.ActionBarSlots[index].ID = item.ID;
                }
                else
                {
                    Character.Instance.CharacterData.StealthedActionBarSlots[index].contentType =
                        CharacterEntries.ActionBarSlotContentType.Item;
                    Character.Instance.CharacterData.StealthedActionBarSlots[index].slotType = actionBarSlots[index].actionBarType;
                    Character.Instance.CharacterData.StealthedActionBarSlots[index].ID = item.ID;
                }
            }
            else
            {
                Character.Instance.CharacterData.ActionBarSlots[index].contentType =
                    CharacterEntries.ActionBarSlotContentType.Item;
                Character.Instance.CharacterData.ActionBarSlots[index].slotType = actionBarSlots[index].actionBarType;
                Character.Instance.CharacterData.ActionBarSlots[index].ID = item.ID;
            }
            
            

            UpdateToggledAbilities();
        }
        
        
        public void SetAbilityToSlot (RPGAbility ab, int index)
        {
            if (index + 1 > actionBarSlots.Count) return;
            actionBarSlots[index].Init(ab);

            if (actionBarSlots[index].actionBarType == CharacterEntries.ActionBarType.Main)
            {
                if (GameState.playerEntity.IsShapeshifted())
                {
                    Character.Instance.CharacterData.ShapeshiftingActionBarSlots[index].contentType =
                        CharacterEntries.ActionBarSlotContentType.Ability;
                    Character.Instance.CharacterData.ShapeshiftingActionBarSlots[index].slotType =
                        actionBarSlots[index].actionBarType;
                    Character.Instance.CharacterData.ShapeshiftingActionBarSlots[index].ID = ab.ID;
                }
                else if (GameState.playerEntity.IsStealth())
                {
                    Character.Instance.CharacterData.StealthedActionBarSlots[index].contentType =
                        CharacterEntries.ActionBarSlotContentType.Ability;
                    Character.Instance.CharacterData.StealthedActionBarSlots[index].slotType =
                        actionBarSlots[index].actionBarType;
                    Character.Instance.CharacterData.StealthedActionBarSlots[index].ID = ab.ID;
                } 
                else
                {
                    Character.Instance.CharacterData.ActionBarSlots[index].contentType =
                        CharacterEntries.ActionBarSlotContentType.Ability;
                    Character.Instance.CharacterData.ActionBarSlots[index].slotType = actionBarSlots[index].actionBarType;
                    Character.Instance.CharacterData.ActionBarSlots[index].ID = ab.ID;
                }
            }
            else
            {
                Character.Instance.CharacterData.ActionBarSlots[index].contentType =
                    CharacterEntries.ActionBarSlotContentType.Ability;
                Character.Instance.CharacterData.ActionBarSlots[index].slotType = actionBarSlots[index].actionBarType;
                Character.Instance.CharacterData.ActionBarSlots[index].ID = ab.ID;
            }

            UpdateToggledAbilities();
        }

        public void ResetActionSlot(int index, bool clearData)
        {
            actionBarSlots[index].Reset();
            actionBarSlots[index].icon.enabled = false;
            actionBarSlots[index].background.enabled = false;
            actionBarSlots[index].stackText.enabled = false;
            actionBarSlots[index].cooldownOverlay.fillAmount = 0;
            actionBarSlots[index].cooldownText.enabled = false;
            actionBarSlots[index].ThisAbility = null;
            actionBarSlots[index].ThisItem = null;
            
            if (clearData)
            {
                if (actionBarSlots[index].actionBarType == CharacterEntries.ActionBarType.Main)
                {
                    if (GameState.playerEntity.IsShapeshifted())
                    {
                        Character.Instance.CharacterData.ShapeshiftingActionBarSlots[index].contentType =
                            CharacterEntries.ActionBarSlotContentType.None;
                        Character.Instance.CharacterData.ShapeshiftingActionBarSlots[index].ID = -1;
                    }
                    else if (GameState.playerEntity.IsStealth())
                    {
                        Character.Instance.CharacterData.StealthedActionBarSlots[index].contentType =
                            CharacterEntries.ActionBarSlotContentType.None;
                        Character.Instance.CharacterData.StealthedActionBarSlots[index].ID = -1;
                    } 
                    else
                    {
                        Character.Instance.CharacterData.ActionBarSlots[index].contentType =
                            CharacterEntries.ActionBarSlotContentType.None;
                        Character.Instance.CharacterData.ActionBarSlots[index].ID = -1;
                    }
                }
                else
                {
                    Character.Instance.CharacterData.ActionBarSlots[index].contentType =
                        CharacterEntries.ActionBarSlotContentType.None;
                    Character.Instance.CharacterData.ActionBarSlots[index].ID = -1;
                }
            }

            UpdateToggledAbilities();
        }

        public void UpdateSlotKeyText(int index, KeyCode newKey)
        {
            actionBarSlots[index].keyText.text = RPGBuilderUtilities.GetKeybindText(newKey);
        }

        public void InitActionBar()
        {
            actionBarSlots.Clear();
            int slotIndex = 0;
            foreach (var actionBarSlot in FindObjectsOfType<ActionBarSlot>())
            {
                actionBarSlots.Add(actionBarSlot);
                actionBarSlot.SlotIndex = slotIndex;
            }

            actionBarSlots = actionBarSlots.OrderBy(w => int.Parse(w.actionKeyName.Replace("ACTION_BAR_SLOT_", "")))
                .ToList();

            foreach (var slot in actionBarSlots)
            {
                slot.SlotIndex = slotIndex;
                slotIndex++;
            }
        }
        
        public void InitStealthActionBar()
        {
            for (var index = 0; index < actionBarSlots.Count; index++)
            {
                var slot = actionBarSlots[index];
                if (slot.actionBarType != CharacterEntries.ActionBarType.Main) continue;
                if (Character.Instance.CharacterData.StealthedActionBarSlots[index].ID != -1)
                {
                    switch (Character.Instance.CharacterData.StealthedActionBarSlots[index].contentType)
                    {
                        case CharacterEntries.ActionBarSlotContentType.None:
                            break;
                        case CharacterEntries.ActionBarSlotContentType.Ability:
                            SetAbilityToSlot(GameDatabase.Instance.GetAbilities()[Character.Instance.CharacterData.StealthedActionBarSlots[index].ID], index);
                            break;
                        case CharacterEntries.ActionBarSlotContentType.Item:
                            SetItemToSlot(GameDatabase.Instance.GetItems()[Character.Instance.CharacterData.StealthedActionBarSlots[index].ID], index);
                            break;
                    }
                }
                else
                {
                    ResetActionSlot(index, false);
                }
            }
        }
        
        public void InitShapeshiftingActionBar()
        {
            for (var index = 0; index < actionBarSlots.Count; index++)
            {
                var slot = actionBarSlots[index];
                if (slot.actionBarType != CharacterEntries.ActionBarType.Main) continue;
                ResetActionSlot(index, false);
            }

            for (var index = 0; index < Character.Instance.CharacterData.ShapeshiftingActionBarSlots.Count; index++)
            {
                var shapeshiftingAbility = Character.Instance.CharacterData.ShapeshiftingActionBarSlots[index];
                if (shapeshiftingAbility.ID != -1)
                {
                    SetAbilityToSlot(GameDatabase.Instance.GetAbilities()[Character.Instance.CharacterData.ShapeshiftingActionBarSlots[index].ID], index);
                }
                else
                {
                    ResetActionSlot(index, false);
                }
            }

        }
        
        public void ResetTemporaryActionBar()
        {
            for (var index = 0; index < actionBarSlots.Count; index++)
            {
                var slot = actionBarSlots[index];
                if (slot.actionBarType != CharacterEntries.ActionBarType.Main) continue;
                if(Character.Instance.CharacterData.ActionBarSlots[index].slotType != CharacterEntries.ActionBarType.Main) continue;
                
                ResetActionSlot(index, false);
                
                if (Character.Instance.CharacterData.ActionBarSlots[index].ID != -1)
                {
                    switch (Character.Instance.CharacterData.ActionBarSlots[index].contentType)
                    {
                        case CharacterEntries.ActionBarSlotContentType.Ability:
                            SetAbilityToSlot(GameDatabase.Instance.GetAbilities()[Character.Instance.CharacterData.ActionBarSlots[index].ID], index);
                            break;
                        case CharacterEntries.ActionBarSlotContentType.Item:
                            SetItemToSlot(GameDatabase.Instance.GetItems()[Character.Instance.CharacterData.ActionBarSlots[index].ID], index);
                            break;
                    }
                }
            }
        }

        private void FixedUpdate()
        {
            if (GameState.playerEntity == null) return;

            foreach (var t in actionBarSlots)
            {
                switch (t.contentType)
                {
                    case CharacterEntries.ActionBarSlotContentType.None:
                        continue;
                    case CharacterEntries.ActionBarSlotContentType.Ability when t.ThisAbility == null || RPGBuilderUtilities.IsAbilityInCombo(t.ThisAbility.ID):
                        t.cooldownOverlay.fillAmount = 0;
                        t.cooldownText.text = "";
                        continue;
                    case CharacterEntries.ActionBarSlotContentType.Ability:
                    {
                        CharacterEntries.AbilityCDState cdState = Character.Instance.getAbilityCDState(t.ThisAbility);
                        if (cdState != null)
                        {
                            if (!cdState.canUseDuringGCD && CombatManager.Instance.currentGCD > 0 && cdState.CDLeft < CombatManager.Instance.currentGCD)
                            {
                                t.cooldownOverlay.fillAmount = CombatManager.Instance.currentGCD / GameDatabase.Instance.GetCombatSettings().GlobalCooldownDuration;
                                t.cooldownText.text = CombatManager.Instance.currentGCD.ToString("F0");
                            }
                            else if (cdState.NextUse > 0)
                            {
                                t.cooldownOverlay.fillAmount = cdState.CDLeft / cdState.NextUse;
                                t.cooldownText.text = cdState.CDLeft.ToString("F0");
                            }
                            else
                            {
                                t.cooldownOverlay.fillAmount = 0;
                                t.cooldownText.text = "";
                            }
                        }
                        else
                        {
                            t.cooldownOverlay.fillAmount = 0;
                            t.cooldownText.text = "";
                        }

                        break;
                    }
                }
            }
        }

        public void InitializeSlots()
        {
            foreach (var slot in actionBarSlots)
            {
                switch (slot.contentType)
                {
                    case CharacterEntries.ActionBarSlotContentType.None:
                        slot.icon.enabled = false;
                        slot.Reset();
                        break;
                    case CharacterEntries.ActionBarSlotContentType.Ability:
                        slot.icon.enabled = true;
                        slot.Init(slot.ThisAbility);
                        break;
                    case CharacterEntries.ActionBarSlotContentType.Item:
                        slot.icon.enabled = true;
                        slot.Init(slot.ThisItem);
                        break;
                }
            }
        }

        public void CheckItemBarState()
        {
            for (int i = 0; i < actionBarSlots.Count; i++)
            {
                if (actionBarSlots[i].contentType != CharacterEntries.ActionBarSlotContentType.Item) continue;
                int ttlCount = EconomyUtilities.GetTotalItemCount(actionBarSlots[i].ThisItem.ID);
                if (ttlCount <= 0)
                {
                    ResetActionSlot(i, true);
                }
                else
                {
                    actionBarSlots[i].UpdateSlot(ttlCount);
                }
            }
        }

        public void HandleSlotSetup(CharacterEntries.ActionBarSlotContentType contentType, RPGItem thisItem,
            RPGAbility thisAb, int draggedOnIndex)
        {
            switch (contentType)
            {
                case CharacterEntries.ActionBarSlotContentType.Ability:
                    SetAbilityToSlot(thisAb, draggedOnIndex);
                    break;
                case CharacterEntries.ActionBarSlotContentType.Item:
                    SetItemToSlot(thisItem, draggedOnIndex);
                    break;
            }
        }

        public void UpdateToggledAbilities()
        {
            foreach (var actionSlot in actionBarSlots)
            {
                actionSlot.toggledOverlay.gameObject.SetActive(
                    actionSlot.ThisAbility != null &&
                    RPGBuilderUtilities.isAbilityToggled(GameState.playerEntity, actionSlot.ThisAbility));
            }
        }
    }
}
