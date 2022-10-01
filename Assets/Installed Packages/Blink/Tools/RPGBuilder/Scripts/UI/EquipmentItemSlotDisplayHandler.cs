using System.Collections.Generic;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Data;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.DisplayHandler
{
    public class EquipmentItemSlotDisplayHandler : MonoBehaviour, IPointerClickHandler, IDragHandler, IEndDragHandler, IBeginDragHandler, IDropHandler
    {
        public EconomyData.EquipFunction EquipFunction;
        public RPGBArmorSlot ArmorSlot;
        public RPGBWeaponSlot WeaponSlot;
        public CanvasGroup itemCG;
        public Image icon, background;
        [HideInInspector] public RPGItem curItem;
        [HideInInspector] public int weaponID;

        private GameObject curDraggedItem;
        private int itemDataID = -1;
        
        public void InitItem(RPGItem item, int dataID)
        {
            curItem = item;
            itemDataID = dataID;
            RPGBuilderUtilities.EnableCG(itemCG);
            icon.sprite = item.entryIcon;
            background.enabled = true;
            background.sprite = item.ItemRarity.background;
        }

        public void ResetItem()
        {
            RPGBuilderUtilities.DisableCG(itemCG);
            icon.sprite = null;
            background.enabled = false;
            background.sprite = null;
            curItem = null;
            itemDataID = -1;
        }

        public void ShowTooltip()
        {
            if (curItem != null)
                ItemTooltip.Instance.Show(curItem.ID, itemDataID, false);
            else
                HideTooltip();
        }

        public void HideTooltip()
        {
            ItemTooltip.Instance.Hide();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Right) return;
            if (curItem == null || RPGBuilderUtilities.isInventoryFull()) return;
            InventoryManager.Instance.UnequipItem(curItem, weaponID);
            curItem = null;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (curItem == null) return;
            if (curDraggedItem != null) Destroy(curDraggedItem);
            curDraggedItem = Instantiate(InventoryManager.Instance.draggedItemImage, transform.position,
                Quaternion.identity);
            curDraggedItem.transform.SetParent(InventoryManager.Instance.draggedItemParent);
            curDraggedItem.GetComponent<Image>().sprite = curItem.entryIcon;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (curItem == null || curDraggedItem == null) return;
            curDraggedItem.transform.position = Input.mousePosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (UIEvents.Instance.IsPanelOpen("Inventory"))
            {
                List<RectTransform> allSlots = (List<RectTransform>)UIEvents.Instance.GetPanelEntryData("Inventory", "allSlots");
                if(allSlots == null) return;
                for (var index = 0; index < allSlots.Count; index++)
                {
                    var t = allSlots[index];
                    if (!RectTransformUtility.RectangleContainsScreenPoint(
                        t, Input.mousePosition)) continue;
                    
                    if (RPGBuilderUtilities.isInventoryFull())
                    {
                        UIEvents.Instance.OnShowAlertMessage("The inventory is full", 3);
                        Destroy(curDraggedItem);
                        return;
                    }
                    if (Character.Instance.CharacterData.Inventory.baseSlots[index].itemID == -1)
                    {
                        InventoryManager.Instance.UnequipItem(curItem, weaponID);
                        Destroy(curDraggedItem);
                        return;
                    }

                    Destroy(curDraggedItem);
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
