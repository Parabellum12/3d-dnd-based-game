using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Data;
using BLINK.RPGBuilder.DisplayHandler;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.UI;
using BLINK.RPGBuilder.WorldPersistence;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ContainerUISlot : MonoBehaviour, IPointerClickHandler, IDragHandler, IEndDragHandler, IBeginDragHandler,
    IDropHandler
{
    public Image icon;
    public Image rarity;
    public TextMeshProUGUI stackText;

    public RPGItem thisItem;
    public int thisStackAmount;
    public int thisItemDataID = -1;

    public ContainerObject container;
    public int currentSlotIndex;

    private GameObject curDraggedItem;

    public void InitSlot(RPGItem item, int stackAmount, int itemDataID, ContainerObject containerObject, int slotIndex)
    {
        thisItem = item;
        thisStackAmount = stackAmount;
        thisItemDataID = itemDataID;
        container = containerObject;
        currentSlotIndex = slotIndex;
        icon.sprite = item.entryIcon;
        Sprite itemQualitySprite = item.ItemRarity.background;
        if (itemQualitySprite != null)
        {
            rarity.enabled = true;
            rarity.sprite = item.ItemRarity.background;
        }
        else
        {
            rarity.enabled = false;
        }

        var curstack = stackAmount;
        stackText.text = curstack > 1 ? curstack.ToString() : "";
    }

    public void ClearDraggedSlot()
    {
        if (curDraggedItem != null)
        {
            Destroy(curDraggedItem);
        }
    }

    public void ShowTooltip()
    {
        ItemTooltip.Instance.Show(thisItem.ID, thisItemDataID, false);
    }

    public void HideTooltip()
    {
        ItemTooltip.Instance.Hide();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right) return;
        ItemTooltip.Instance.Hide();
        if (UIEvents.Instance.IsPanelOpen("Inventory"))
        {
            ContainerObject currentContainer = UIEvents.Instance.GetContainer("Container");
            if (currentContainer != null)
            {
                currentContainer.Saver.MoveItemToInventory(-1, -1, currentSlotIndex);
            }
        }

    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (thisItem == null) return;
        if (curDraggedItem != null) Destroy(curDraggedItem);
        curDraggedItem = Instantiate(InventoryManager.Instance.draggedItemImage, transform.position,
            Quaternion.identity);
        curDraggedItem.transform.SetParent(InventoryManager.Instance.draggedItemParent);
        curDraggedItem.GetComponent<Image>().sprite = thisItem.entryIcon;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (thisItem == null || curDraggedItem == null) return;
        curDraggedItem.transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (thisItem == null || curDraggedItem == null) return;
        if (UIEvents.Instance.IsPanelOpen("Inventory"))
        {
            List<RectTransform> allSlots =
                (List<RectTransform>) UIEvents.Instance.GetPanelEntryData("Inventory", "allSlots");
            if (allSlots == null) return;
            for (var i = 0; i < Character.Instance.CharacterData.Inventory.baseSlots.Count; i++)
            {
                if (!RectTransformUtility.RectangleContainsScreenPoint(allSlots[i], Input.mousePosition)) continue;
                container.Saver.MoveItemToInventory(-1, i, currentSlotIndex);
                Destroy(curDraggedItem);
                return;
            }
        }

        if (UIEvents.Instance.IsPanelOpen("Container"))
        {
            List<RectTransform> allSlots =
                (List<RectTransform>) UIEvents.Instance.GetPanelEntryData("Container", "allSlots");
            if (allSlots == null) return;
            for (var i = 0;
                i < PersistenceManager.Instance.GetContainerObjectTemplateData(container.Saver.GetIdentifier()).Slots
                    .Count;
                i++)
            {
                if (!RectTransformUtility.RectangleContainsScreenPoint(allSlots[i], Input.mousePosition)) continue;
                container.Saver.MoveItemInContainer(currentSlotIndex, i);
                Destroy(curDraggedItem);
                return;
            }
        }


        List<RaycastResult> results = new List<RaycastResult>();
        PointerEventData pEventData = new PointerEventData(EventSystem.current) {position = Input.mousePosition};

        EventSystem.current.RaycastAll(pEventData, results);

        foreach (RaycastResult result in results)
        {
            switch (result.gameObject.name)
            {
                case "Inventory":
                case "Character":
                case "SkillBook":
                case "Spellbook":
                case "EnchantingPanel":
                case "SocketingPanel":
                case "QuestJournal":
                case "QuestStatesPanel":
                case "QuestInteractionPanel":
                case "Options":
                case "CraftingPanel":
                case "Minimap":
                {
                    Destroy(curDraggedItem);
                    return;
                }
            }
        }

        UIEvents.Instance.OnShowItemConfirmationPopUp(ConfirmationPopupType.deleteItem,
            thisItem, thisItemDataID, thisStackAmount, -1, -1);
        Destroy(curDraggedItem);
    }

    public void OnDrop(PointerEventData eventData)
    {
    }
}
