using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.UIElements
{
    public class LootItemSlotHolder : MonoBehaviour, IPointerClickHandler, IDragHandler, IEndDragHandler, IBeginDragHandler,
        IDropHandler
    {
        public Image itemIcon, background;
        public TextMeshProUGUI itemStackText, itemNameText;

        private int thisLootIndex;
        private LootBag holder;

        private GameObject curDraggedItem;

        public void Init(int lootIndex, LootBag bag)
        {
            thisLootIndex = lootIndex;
            holder = bag;
            itemIcon.sprite = holder.lootData[thisLootIndex].item.entryIcon;
            background.sprite = holder.lootData[thisLootIndex].item.ItemRarity.background;
            itemStackText.text = holder.lootData[thisLootIndex].count.ToString();
            itemNameText.text = holder.lootData[thisLootIndex].item.entryDisplayName;
        }


        public void ShowTooltip()
        {
            ItemTooltip.Instance.Show(holder.lootData[thisLootIndex].item.ID, holder.lootData[thisLootIndex].itemDataID, true);
        }

        public void HideTooltip()
        {
            ItemTooltip.Instance.Hide();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Right) return;
            int itemsLeftOver = RPGBuilderUtilities.HandleItemLooting(holder.lootData[thisLootIndex].item.ID, -1, holder.lootData[thisLootIndex].count, false, false);
            if (itemsLeftOver == 0)
            {
                RPGBuilderUtilities.SetNewItemDataState(holder.lootData[thisLootIndex].itemDataID, CharacterEntries.ItemEntryState.InBag);
                holder.lootData[thisLootIndex].looted = true;
                holder.CheckLootState();
                UIEvents.Instance.OnDeleteLootedItemSlot(gameObject);
            }
            else
            {
                holder.lootData[thisLootIndex].count = itemsLeftOver;
                holder.CheckLootState();
            }
            ItemTooltip.Instance.Hide();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (curDraggedItem != null) Destroy(curDraggedItem);
            if (RPGBuilderUtilities.isInventoryFull())
            {
                UIEvents.Instance.OnShowAlertMessage("The inventory is full", 3);
                return;
            }
            curDraggedItem = Instantiate(InventoryManager.Instance.draggedItemImage, transform.position,
                Quaternion.identity);
            curDraggedItem.transform.SetParent(InventoryManager.Instance.draggedItemParent);
            curDraggedItem.GetComponent<Image>().sprite = holder.lootData[thisLootIndex].item.entryIcon;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (curDraggedItem == null) return;
            curDraggedItem.transform.position = Input.mousePosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (curDraggedItem == null) return;
            if (UIEvents.Instance.IsPanelOpen("Inventory"))
            {
                List<RectTransform> allSlots = (List<RectTransform>)UIEvents.Instance.GetPanelEntryData("Inventory", "allSlots");
                if(allSlots == null) return;
                if (allSlots.Any(t => RectTransformUtility.RectangleContainsScreenPoint(t, Input.mousePosition)))
                {
                    int itemsLeftOver = RPGBuilderUtilities.HandleItemLooting(
                        holder.lootData[thisLootIndex].item.ID, -1, holder.lootData[thisLootIndex].count, false, false);
                    if (itemsLeftOver == 0)
                    {
                        Destroy(curDraggedItem);
                        holder.lootData[thisLootIndex].looted = true;
                        holder.CheckLootState();
                        UIEvents.Instance.OnDeleteLootedItemSlot(gameObject);
                    }
                    else
                    {
                        holder.lootData[thisLootIndex].count = itemsLeftOver;
                        holder.CheckLootState();
                    }

                    return;
                }
            }

            Destroy(curDraggedItem);
        }

        public void OnDrop(PointerEventData eventData)
        {
        }
    }
}