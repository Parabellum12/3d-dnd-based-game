using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.Managers;
using UnityEngine;

public class CombosDisplayManager : MonoBehaviour
{
    [SerializeField] private GameObject comboSlotPrefab;
    [SerializeField] private Transform comboSlotsParent;

    private void OnEnable()
    {
        CombatEvents.PlayerComboStarted += InitNewCombo;
        CombatEvents.PlayerComboUpdated += UpdateComboEntry;
        CombatEvents.PlayerComboRemoved += ResetActionBarSlotsImage;
    }

    private void OnDisable()
    {
        CombatEvents.PlayerComboStarted -= InitNewCombo;
        CombatEvents.PlayerComboUpdated -= UpdateComboEntry;
        CombatEvents.PlayerComboRemoved -= ResetActionBarSlotsImage;
    }

    private void InitNewCombo(int activeComboIndex, RPGCombo.ComboEntry comboEntry, KeyCode key)
    {
        CombatData.ActiveCombo activeCombo = GameState.playerEntity.GetActiveCombos()[activeComboIndex];
        activeCombo.keyRequired = key;
        RPGBuilderUtilities.SetAbilityComboActive(activeCombo.initialAbilityID, true);
        
        RPGAbility abREF = GameDatabase.Instance.GetAbilities()[comboEntry.abilityID];
        if (abREF == null) return;
        GameObject newComboSlot = Instantiate(comboSlotPrefab, comboSlotsParent);
        
        activeCombo.UISlotREF = newComboSlot.GetComponent<ComboSlot>();
        activeCombo.UISlotREF.abilityIcon.sprite = abREF.entryIcon;
        activeCombo.UISlotREF.expireTimeBar.fillAmount = 1;
        activeCombo.UISlotREF.abilityNameText.text = abREF.entryDisplayName;
        activeCombo.UISlotREF.expireTimeText.text = comboEntry.expireTime + "s";
        activeCombo.UISlotREF.KeyText.text = GetKeybindText(key);

        if(comboEntry.keyType == RPGCombo.KeyType.StartAbilityKey) UpdateActionBarSlotsImage(abREF, activeComboIndex);
    }

    private void UpdateComboEntry(int activeComboIndex, KeyCode key, bool useInitialKey)
    {
        RPGBuilderUtilities.SetAbilityComboActive(GameState.playerEntity.GetActiveCombos()[activeComboIndex].initialAbilityID, true);
        CombatData.ActiveCombo activeCombo = GameState.playerEntity.GetActiveCombos()[activeComboIndex];
        RPGAbility abREF = GameDatabase.Instance.GetAbilities()[activeCombo.combo.combos[activeCombo.comboIndex].abilityID];
        if (abREF == null) return;
        activeCombo.readyTime = activeCombo.combo.combos[activeCombo.comboIndex].readyTime;
        activeCombo.curLoadTime = 0;
        activeCombo.expireTime = activeCombo.combo.combos[activeCombo.comboIndex].expireTime;
        activeCombo.curTime = activeCombo.expireTime;
        activeCombo.keyRequired = key;

        activeCombo.UISlotREF.abilityIcon.sprite = abREF.entryIcon;
        activeCombo.UISlotREF.expireTimeBar.fillAmount = 1;
        activeCombo.UISlotREF.abilityNameText.text = abREF.entryDisplayName;
        activeCombo.UISlotREF.expireTimeText.text =
            activeCombo.expireTime + "s";
        activeCombo.UISlotREF.KeyText.text = GetKeybindText(key);
        
        activeCombo.UISlotREF.gameObject.SetActive(false);
        activeCombo.UISlotREF.gameObject.SetActive(true);
        
        if(useInitialKey) UpdateActionBarSlotsImage(abREF, activeComboIndex);
    }

    private void UpdateActionBarSlotsImage(RPGAbility abREF, int activeComboIndex)
    {
        foreach (var abSlot in ActionBarManager.Instance.actionBarSlots.Where(abSlot => abSlot.contentType == CharacterEntries.ActionBarSlotContentType.Ability).Where(abSlot => abSlot.ThisAbility.ID == GameState.playerEntity.GetActiveCombos()[activeComboIndex].initialAbilityID))
        {
            abSlot.icon.sprite = abREF.entryIcon;
        }
    }

    private void ResetActionBarSlotsImage(int abilityID)
    {
        foreach (var abSlot in ActionBarManager.Instance.actionBarSlots.Where(abSlot => abSlot.contentType == CharacterEntries.ActionBarSlotContentType.Ability).Where(abSlot => abSlot.ThisAbility.ID == abilityID))
        {
            abSlot.icon.sprite = abSlot.ThisAbility.entryIcon;
        }
    }

    private void FixedUpdate()
    {
        if (GameState.playerEntity == null) return;
        if (GameState.playerEntity.GetActiveCombos().Count == 0) return;
        foreach (var combo in GameState.playerEntity.GetActiveCombos())
        {
            if (combo.readyTime > 0)
            {
                combo.UISlotREF.expireTimeBar.fillAmount =
                    combo.curLoadTime /
                    combo.readyTime;
                combo.UISlotREF.expireTimeText.text =
                    combo.curLoadTime.ToString("F1") + "s";
            }
            else
            {
                combo.UISlotREF.expireTimeBar.fillAmount =
                    combo.curTime /
                    combo.expireTime;
                combo.UISlotREF.expireTimeText.text =
                    combo.curTime.ToString("F1") + "s";
            }
            
        }
    }
    
    private string GetKeybindText(KeyCode key)
    {
        var KeyBindString = key.ToString();
        if (KeyBindString.Contains("Alpha"))
        {
            var alphakey = KeyBindString.Remove(0, 5);
            return alphakey;
        }

        if (!KeyBindString.Contains("Mouse")) return KeyBindString;
        {
            var alphakey = KeyBindString.Remove(0, 5);
            return "M" + alphakey;
        }

    }
}
