using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Data;
using BLINK.RPGBuilder.DisplayHandler;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.WorldPersistence;
using UnityEngine.Serialization;

namespace BLINK.RPGBuilder.UIElements
{
    public class ItemSlotHolder : MonoBehaviour, IPointerClickHandler, IDragHandler, IEndDragHandler, IBeginDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public Image icon;
        [FormerlySerializedAs("quality")] public Image rarity;
        public TextMeshProUGUI stackText;

        public RPGItem thisItem;
        public int bagIndex;
        public int slotIndex;

        private GameObject curDraggedItem;

        public void InitSlot(RPGItem item, int bag_index, int slot_index)
        {
            thisItem = item;
            bagIndex = bag_index;
            slotIndex = slot_index;
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
            var curstack = Character.Instance.CharacterData.Inventory.baseSlots[slotIndex].itemStack;
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
            ItemTooltip.Instance.Show(thisItem.ID, Character.Instance.CharacterData.Inventory.baseSlots[slotIndex].itemDataID, true);
        }

        public void HideTooltip()
        {
            ItemTooltip.Instance.Hide();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Right) return;
            ItemTooltip.Instance.Hide();
            if (UIEvents.Instance.IsPanelOpen("Container"))
            {
                ContainerObject currentContainer = UIEvents.Instance.GetContainer("Container");
                if (currentContainer != null)
                {
                    currentContainer.Saver.MoveItemToContainer(-1, slotIndex, -1);
                }
                return;
            }
            if (UIEvents.Instance.IsPanelOpen("Merchant"))
            {
                UIEvents.Instance.OnShowItemConfirmationPopUp(ConfirmationPopupType.sellItem,
                    thisItem, Character.Instance.CharacterData.Inventory.baseSlots[slotIndex].itemDataID, Character.Instance.CharacterData.Inventory.baseSlots[slotIndex].itemStack, bagIndex, slotIndex);
                return;
            }
            if (UIEvents.Instance.IsPanelOpen("Enchanting"))
            {
                UIEvents.Instance.OnAssignItemToEnchant(thisItem, Character.Instance.CharacterData.Inventory.baseSlots[slotIndex].itemDataID);
                return;
            }
            if (UIEvents.Instance.IsPanelOpen("Socketing"))
            {
                UIEvents.Instance.OnAssignItemToSocket(thisItem, Character.Instance.CharacterData.Inventory.baseSlots[slotIndex].itemDataID);
                return;
            }

            if(!GameState.playerEntity.IsDead()) InventoryManager.Instance.UseItem(thisItem, bagIndex, slotIndex);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (thisItem == null) return;
            if (GameState.playerEntity.IsDead()) return;
            if(curDraggedItem != null) Destroy(curDraggedItem);
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
                List<RectTransform> allSlots = (List<RectTransform>)UIEvents.Instance.GetPanelEntryData("Inventory", "allSlots");
                if(allSlots == null) return;
                for (var i = 0; i < Character.Instance.CharacterData.Inventory.baseSlots.Count; i++)
                {
                    if (!RectTransformUtility.RectangleContainsScreenPoint( allSlots[i], Input.mousePosition)) continue;

                    if (slotIndex == i)
                    {
                        Destroy(curDraggedItem);
                        return;
                    }

                    InventoryManager.Instance.MoveItem(bagIndex, slotIndex, 0, i);
                    Destroy(curDraggedItem);
                    return;
                }
            }
            
            if (UIEvents.Instance.IsPanelOpen("Container"))
            {
                List<RectTransform> allSlots = (List<RectTransform>)UIEvents.Instance.GetPanelEntryData("Container", "allSlots");
                if(allSlots == null) return;
                ContainerObject currentContainer = UIEvents.Instance.GetContainer("Container");
                if (currentContainer == null) return;
                for (var i = 0; i < PersistenceManager.Instance.GetContainerObjectTemplateData(currentContainer.Saver.GetIdentifier()).Slots.Count; i++)
                {
                    if (!RectTransformUtility.RectangleContainsScreenPoint( allSlots[i], Input.mousePosition)) continue;
                    currentContainer.Saver.MoveItemToContainer(bagIndex, slotIndex, i);
                    Destroy(curDraggedItem);
                    return;
                }
            }

            if (UIEvents.Instance.IsPanelOpen("Character"))
            {
                if (thisItem.ItemType.CanBeEquipped)
                {
                    List<EquipmentItemSlotDisplayHandler> allArmorSlots = (List<EquipmentItemSlotDisplayHandler>) UIEvents.Instance.GetPanelEntryData("Character", "ArmorSlots");
                    List<EquipmentItemSlotDisplayHandler> allWeaponSlots = (List<EquipmentItemSlotDisplayHandler>) UIEvents.Instance.GetPanelEntryData("Character", "WeaponSlots");

                    if (thisItem.ItemType.EquipType == EconomyData.EquipFunction.Armor)
                    {
                       
                        if (allArmorSlots != null && allArmorSlots.Any(t => RectTransformUtility.RectangleContainsScreenPoint(t.GetComponent<RectTransform>(), Input.mousePosition)))
                        {
                            InventoryManager.Instance.UseItem(thisItem, bagIndex, slotIndex);
                            Destroy(curDraggedItem);
                            return;
                        }
                    }
                    else if (thisItem.ItemType.EquipType == EconomyData.EquipFunction.Weapon)
                    {
                        if (allWeaponSlots != null && allWeaponSlots.Any(t => RectTransformUtility.RectangleContainsScreenPoint(t.GetComponent<RectTransform>(), Input.mousePosition)))
                        {
                            InventoryManager.Instance.UseItem(thisItem, bagIndex, slotIndex);
                            Destroy(curDraggedItem);
                            return;
                        }
                    }
                }
            }

            if (UIEvents.Instance.IsPanelOpen("Enchanting"))
            {
                if (thisItem.ItemType.CanBeEquipped)
                {
                    if (thisItem.ItemType.EquipType == EconomyData.EquipFunction.Armor ||
                        thisItem.ItemType.EquipType == EconomyData.EquipFunction.Weapon)
                    {
                        Transform enchantingSlot = (Transform)UIEvents.Instance.GetPanelEntryData("Enchanting", "slotParent");
                        if(enchantingSlot == null) return;
                        if (RectTransformUtility.RectangleContainsScreenPoint(
                            enchantingSlot.GetComponent<RectTransform>(), Input.mousePosition))
                        {
                            UIEvents.Instance.OnAssignItemToEnchant(thisItem, Character.Instance.CharacterData.Inventory.baseSlots[slotIndex].itemDataID);
                            Destroy(curDraggedItem);
                            return;
                        }
                    }
                }
            }

            if (UIEvents.Instance.IsPanelOpen("Socketing"))
            {
                if (thisItem.ItemType.CanBeEquipped)
                {
                    if (thisItem.ItemType.EquipType == EconomyData.EquipFunction.Armor ||
                        thisItem.ItemType.EquipType == EconomyData.EquipFunction.Weapon)
                    {
                        Transform socketingSlot =
                            (Transform) UIEvents.Instance.GetPanelEntryData("Socketing", "slotParent");
                        if (socketingSlot == null) return;
                        if (RectTransformUtility.RectangleContainsScreenPoint(
                            socketingSlot.GetComponent<RectTransform>(),
                            Input.mousePosition))
                        {
                            UIEvents.Instance.OnAssignItemToSocket(thisItem,
                                Character.Instance.CharacterData.Inventory.baseSlots[slotIndex].itemDataID);
                            Destroy(curDraggedItem);
                            return;
                        }
                    }
                }
                else
                {
                    if (thisItem.ItemType.ItemTypeFunction == EconomyData.ItemTypeFunction.Gem)
                    {
                        List<SocketSlot> allSlots = (List<SocketSlot>) UIEvents.Instance.GetPanelEntryData("Socketing", "allSlots");
                        if (allSlots == null) return;
                        foreach (var t in allSlots)
                        {
                            if (RectTransformUtility.RectangleContainsScreenPoint(t.GetComponent<RectTransform>(), Input.mousePosition))
                            {
                                UIEvents.Instance.OnAssignGemToSocket(t, thisItem);
                                Destroy(curDraggedItem);
                                return;
                            }
                        }
                    }
                }
            }

            for (var i = 0; i < ActionBarManager.Instance.actionBarSlots.Count; i++)
                if (RectTransformUtility.RectangleContainsScreenPoint(
                    ActionBarManager.Instance.actionBarSlots[i].GetComponent<RectTransform>(),
                    Input.mousePosition))
                {
                    if (ActionBarManager.Instance.actionBarSlots[i].acceptItems)
                    {
                        ActionBarManager.Instance.SetItemToSlot(thisItem, i);
                    }
                    else
                    {
                        UIEvents.Instance.OnShowAlertMessage("This action bar slot do not accept items", 3);
                    }
                    Destroy(curDraggedItem);
                    return;
                }

            
            List<RaycastResult> results = new List<RaycastResult>();
            PointerEventData pEventData = new PointerEventData(EventSystem.current);

            pEventData.position = Input.mousePosition;
            EventSystem.current.RaycastAll(pEventData, results);

            foreach (RaycastResult result in results)
            {
                switch (result.gameObject.name)
                {
                    case "Inventory":
                    case "Character":
                    case "SkillBook":
                    case "Spellbook": //book is lower case currently
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
                thisItem, Character.Instance.CharacterData.Inventory.baseSlots[slotIndex].itemDataID,
                Character.Instance.CharacterData.Inventory.baseSlots[slotIndex].itemStack, bagIndex, slotIndex);
            Destroy(curDraggedItem);
        }

        public void OnDrop(PointerEventData eventData)
        {
            
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            ShowTooltip();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            HideTooltip();
        }
    }
}