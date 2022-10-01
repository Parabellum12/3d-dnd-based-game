using System;
using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ActionBarSlot : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler, IDropHandler
{
    public CharacterEntries.ActionBarType actionBarType;
    public string actionKeyName;
    private int slotIndex;
    public int SlotIndex
    {
        get => slotIndex;
        set => slotIndex = value;
    }
    public CharacterEntries.ActionBarSlotContentType contentType;

    public bool acceptAbilities = true, acceptItems = true;
    
    public Image icon, background, cooldownOverlay, toggledOverlay;
    public TextMeshProUGUI cooldownText, keyText, stackText;
    
    [SerializeField] private GameObject draggedNodeImage;
    
    private RPGAbility thisAb;
    public RPGAbility ThisAbility
    {
        get => thisAb;
        set => thisAb = value;
    }
    private RPGItem thisItem;
    public RPGItem ThisItem
    {
        get => thisItem;
        set => thisItem = value;
    }

    private GameObject curDraggedSlot;
    
    public bool dragAllowed = true;

    public void Init(RPGAbility ab)
    {
        contentType = CharacterEntries.ActionBarSlotContentType.Ability;
        thisItem = null;
        thisAb = ab;
        cooldownText.enabled = true;
        cooldownText.text = "";
        cooldownOverlay.fillAmount = 0;
        icon.enabled = true;
        icon.sprite = thisAb.entryIcon;
        background.enabled = false;
        stackText.enabled = false;
        UpdateKeyText();
        
    }
    public void Init(RPGItem item)
    {
        contentType = CharacterEntries.ActionBarSlotContentType.Item;
        thisAb = null;
        thisItem = item;
        cooldownText.enabled = true;
        cooldownText.text = "";
        cooldownOverlay.fillAmount = 0;
        icon.enabled = true;
        icon.sprite = thisItem.entryIcon;
        background.enabled = true;
        
        Sprite itemQualitySprite = item.ItemRarity.background;
        if (itemQualitySprite != null)
        {
            background.enabled = true;
            background.sprite = item.ItemRarity.background;
        }
        else
        {
            background.enabled = false;
        }
        
        int ttlCount = EconomyUtilities.GetTotalItemCount(item.ID);
        UpdateSlot(ttlCount);
        UpdateKeyText();
    }

    private void UpdateKeyText()
    {
        keyText.text = RPGBuilderUtilities.GetKeybindText(RPGBuilderUtilities.GetCurrentKeyByActionKeyName(actionKeyName));
    }
    
    public void UpdateSlot(int ttlStack)
    {
        stackText.enabled = true;
        stackText.text = ttlStack.ToString();
    }

    public void Reset()
    {
        contentType = CharacterEntries.ActionBarSlotContentType.None;
        thisAb = null;
        thisItem = null;
        
        UpdateKeyText();
    }
    
    public void ClickUseSlot()
    {
        switch (contentType)
        {
            case CharacterEntries.ActionBarSlotContentType.Ability:
                CombatManager.Instance.InitAbility(GameState.playerEntity, thisAb, GameState.playerEntity.GetCurrentAbilityRank(thisAb, true),true);
                UIEvents.Instance.OnHideAbilityTooltip();
                break;
            case CharacterEntries.ActionBarSlotContentType.Item:
                InventoryManager.Instance.UseItemFromBar(thisItem);
                ItemTooltip.Instance.Hide();
                break;
        }
    }

    public void ShowTooltip()
    {
        switch (contentType)
        {
            case CharacterEntries.ActionBarSlotContentType.Ability:
                UIEvents.Instance.OnShowAbilityTooltip(GameState.playerEntity, thisAb, RPGBuilderUtilities.GetCharacterAbilityRank(thisAb));
                break;
            case CharacterEntries.ActionBarSlotContentType.Item:
                ItemTooltip.Instance.Show(thisItem.ID, -1, false);
                break;
        }
    }

    public void HideTooltip()
    {
        switch (contentType)
        {
            case CharacterEntries.ActionBarSlotContentType.Ability:
                UIEvents.Instance.OnHideAbilityTooltip();
                break;
            case CharacterEntries.ActionBarSlotContentType.Item:
                ItemTooltip.Instance.Hide();
                break;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!dragAllowed) return;
        if (curDraggedSlot != null) Destroy(curDraggedSlot);
        if (contentType == CharacterEntries.ActionBarSlotContentType.None) return;
        curDraggedSlot = Instantiate(draggedNodeImage, transform.position,
            Quaternion.identity);
        curDraggedSlot.transform.SetParent(UIEvents.Instance.draggedSlotParent);

        switch (contentType)
        {
            case CharacterEntries.ActionBarSlotContentType.Ability:
                curDraggedSlot.GetComponent<Image>().sprite = thisAb.entryIcon;
                break;
            case CharacterEntries.ActionBarSlotContentType.Item:
                curDraggedSlot.GetComponent<Image>().sprite = thisItem.entryIcon;
                break;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (curDraggedSlot == null) return;
        if (contentType == CharacterEntries.ActionBarSlotContentType.None) return;
        curDraggedSlot.transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (curDraggedSlot == null) return;
        if (contentType == CharacterEntries.ActionBarSlotContentType.None) return;
        for (var i = 0; i < ActionBarManager.Instance.actionBarSlots.Count; i++)
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(
                ActionBarManager.Instance.actionBarSlots[i].GetComponent<RectTransform>(),
                Input.mousePosition)) continue;
            
            switch (contentType)
            {
                case CharacterEntries.ActionBarSlotContentType.Ability when !ActionBarManager.Instance.actionBarSlots[i].acceptAbilities:
                    UIEvents.Instance.OnShowAlertMessage("This action bar slot do not accept abilities", 3);
                    Destroy(curDraggedSlot);
                    return;
                case CharacterEntries.ActionBarSlotContentType.Item when !ActionBarManager.Instance.actionBarSlots[i].acceptItems:
                    UIEvents.Instance.OnShowAlertMessage("This action bar slot do not accept items", 3);
                    Destroy(curDraggedSlot);
                    return;
            }

            switch (ActionBarManager.Instance.actionBarSlots[i].contentType)
            {
                case CharacterEntries.ActionBarSlotContentType.None:
                    ActionBarManager.Instance.HandleSlotSetup(contentType, thisItem, thisAb, i);
                    ActionBarManager.Instance.ResetActionSlot(slotIndex, true);
                    break;
                case CharacterEntries.ActionBarSlotContentType.Ability:
                    RPGItem cachedItem = ActionBarManager.Instance.actionBarSlots[i].ThisItem;
                    RPGAbility cachedAbility = ActionBarManager.Instance.actionBarSlots[i].ThisAbility;
                    CharacterEntries.ActionBarSlotContentType cachedContentType =
                        ActionBarManager.Instance.actionBarSlots[i].contentType;
                    ActionBarManager.Instance.HandleSlotSetup(contentType, thisItem, thisAb, i);
                    ActionBarManager.Instance.HandleSlotSetup(cachedContentType, cachedItem,
                        cachedAbility, slotIndex);
                    break;
                case CharacterEntries.ActionBarSlotContentType.Item:
                    break;
            }

            Destroy(curDraggedSlot);
            return;
        }
        
        ActionBarManager.Instance.ResetActionSlot(slotIndex, true);
        Destroy(curDraggedSlot);
    }

    public void OnDrop(PointerEventData eventData)
    {
    }
}
