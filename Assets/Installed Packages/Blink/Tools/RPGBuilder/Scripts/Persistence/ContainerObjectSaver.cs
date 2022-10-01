using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Characters;
using UnityEngine;

namespace BLINK.RPGBuilder.WorldPersistence
{
    [RequireComponent(typeof(SaverIdentifier))]
    public class ContainerObjectSaver : ObjectSaver
    {
        public ContainerObject container;

        private void Start()
        {
            if (container == null) container = GetComponent<ContainerObject>();
        }

        private void OnEnable()
        {
            GameEvents.NewGameSceneLoaded += RegisterSelf;
        }

        private void OnDisable()
        {
            PersistenceManager.Instance.UnregisterContainerObjectSaver(this);
            GameEvents.NewGameSceneLoaded -= RegisterSelf;
        }

        public override void RegisterSelf()
        {
            if (PersistenceManager.Instance.IsSaverDestroyed(this))
            {
                Destroy(gameObject);
                return;
            }

            PersistenceManager.Instance.RegisterContainerSaver(this);
            LoadState();
        }

        protected override void LoadState()
        {
            base.LoadState();

            if (!PersistenceManager.Instance.ContainerObjectListContainsIdentifier(GetIdentifier())) return;
            ContainerObjectSaverTemplate template = PersistenceManager.Instance.GetContainerObjectTemplateData(GetIdentifier());
            if (template.Slots.Count != container.SlotAmount)
            {
                if (container.SlotAmount > template.Slots.Count)
                {
                    int missingSlots = container.SlotAmount - template.Slots.Count;
                    for (int i = 0; i < missingSlots; i++)
                    {
                        template.Slots.Add(new ContainerSlot());
                    }
                }
                else
                {
                    int extraSlots = template.Slots.Count - container.SlotAmount;
                    template.Slots.RemoveRange(template.Slots.Count-extraSlots, extraSlots);
                }
            }
        }

        public void MoveItemInContainer(int previousIndex, int newIndex)
        {
            ContainerObjectSaverTemplate template = PersistenceManager.Instance.GetContainerObjectTemplateData(GetIdentifier());
            if (template.Slots[newIndex].ItemID == -1)
            {
                template.Slots[newIndex].ItemID = template.Slots[previousIndex].ItemID;
                template.Slots[newIndex].ItemDataID = template.Slots[previousIndex].ItemDataID;
                template.Slots[newIndex].Stack = template.Slots[previousIndex].Stack;

                template.Slots[previousIndex].ItemID = -1;
                template.Slots[previousIndex].ItemDataID = -1;
                template.Slots[previousIndex].Stack = 0;

            }
            else if (template.Slots[newIndex].ItemID != -1)
            {
                int cachedNewIndexItemID = template.Slots[newIndex].ItemID;
                int cachedNewIndexItemDataID = template.Slots[newIndex].ItemDataID;
                int cachedNewIndexItemStack = template.Slots[newIndex].Stack;

                template.Slots[newIndex].ItemID = template.Slots[previousIndex].ItemID;
                template.Slots[newIndex].ItemDataID = template.Slots[previousIndex].ItemDataID;
                template.Slots[newIndex].Stack = template.Slots[previousIndex].Stack;

                template.Slots[previousIndex].ItemID = cachedNewIndexItemID;
                template.Slots[previousIndex].ItemDataID = cachedNewIndexItemDataID;
                template.Slots[previousIndex].Stack = cachedNewIndexItemStack;
            }
            GameEvents.Instance.OnContainerContentChanged();
        }

        public void MoveItemToContainer(int bagIndex, int slotIndex, int containerSlotIndex)
        {
            ContainerObjectSaverTemplate template =
                PersistenceManager.Instance.GetContainerObjectTemplateData(GetIdentifier());
            if (template == null) return;
            if (containerSlotIndex == -1)
            {
                for (int i = template.Slots.Count - 1; i >= 0; i--)
                {
                    var slot = template.Slots[i];
                    if (slot.ItemID != -1) continue;
                    containerSlotIndex = i;
                }
            }

            if (containerSlotIndex == -1) return;
            if (template.Slots[containerSlotIndex].ItemID == -1)
            {
                template.Slots[containerSlotIndex].ItemID = Character.Instance.CharacterData.Inventory.baseSlots[slotIndex].itemID;
                template.Slots[containerSlotIndex].ItemDataID =
                    Character.Instance.CharacterData.Inventory.baseSlots[slotIndex].itemDataID;
                template.Slots[containerSlotIndex].Stack = Character.Instance.CharacterData.Inventory.baseSlots[slotIndex].itemStack;

                Character.Instance.CharacterData.Inventory.baseSlots[slotIndex].itemID = -1;
                Character.Instance.CharacterData.Inventory.baseSlots[slotIndex].itemDataID = -1;
                Character.Instance.CharacterData.Inventory.baseSlots[slotIndex].itemStack = 0;

            }
            else if (template.Slots[containerSlotIndex].ItemID != -1)
            {
                int cachedNewIndexItemID = template.Slots[containerSlotIndex].ItemID;
                int cachedNewIndexItemDataID = template.Slots[containerSlotIndex].ItemDataID;
                int cachedNewIndexItemStack = template.Slots[containerSlotIndex].Stack;

                template.Slots[containerSlotIndex].ItemID = Character.Instance.CharacterData.Inventory.baseSlots[slotIndex].itemID;
                template.Slots[containerSlotIndex].ItemDataID =
                    Character.Instance.CharacterData.Inventory.baseSlots[slotIndex].itemDataID;
                template.Slots[containerSlotIndex].Stack = Character.Instance.CharacterData.Inventory.baseSlots[slotIndex].itemStack;

                Character.Instance.CharacterData.Inventory.baseSlots[slotIndex].itemID = cachedNewIndexItemID;
                Character.Instance.CharacterData.Inventory.baseSlots[slotIndex].itemDataID = cachedNewIndexItemDataID;
                Character.Instance.CharacterData.Inventory.baseSlots[slotIndex].itemStack = cachedNewIndexItemStack;
            }

            GameEvents.Instance.OnContainerContentChanged();
            GeneralEvents.Instance.OnInventoryUpdated();
        }
        
        public void MoveItemToInventory(int bagIndex, int slotIndex, int containerSlotIndex)
        {
            ContainerObjectSaverTemplate template = PersistenceManager.Instance.GetContainerObjectTemplateData(GetIdentifier());
            if (template == null) return;
            if (slotIndex == -1)
            {
                for (int i = Character.Instance.CharacterData.Inventory.baseSlots.Count - 1; i >= 0; i--)
                {
                    var slot = Character.Instance.CharacterData.Inventory.baseSlots[i];
                    if (slot.itemID != -1) continue;
                    slotIndex = i;
                }
            }
            if (slotIndex == -1) return;
            
            if (Character.Instance.CharacterData.Inventory.baseSlots[slotIndex].itemID == -1)
            {
                Character.Instance.CharacterData.Inventory.baseSlots[slotIndex].itemID = template.Slots[containerSlotIndex].ItemID;
                Character.Instance.CharacterData.Inventory.baseSlots[slotIndex].itemDataID = template.Slots[containerSlotIndex].ItemDataID;
                Character.Instance.CharacterData.Inventory.baseSlots[slotIndex].itemStack = template.Slots[containerSlotIndex].Stack;
                
                template.Slots[containerSlotIndex].ItemID = -1;
                template.Slots[containerSlotIndex].ItemDataID = -1;
                template.Slots[containerSlotIndex].Stack = -1;
            }
            else if (Character.Instance.CharacterData.Inventory.baseSlots[slotIndex].itemID != -1)
            {
                int cachedNewIndexItemID = Character.Instance.CharacterData.Inventory.baseSlots[slotIndex].itemID;
                int cachedNewIndexItemDataID = Character.Instance.CharacterData.Inventory.baseSlots[slotIndex].itemDataID;
                int cachedNewIndexItemStack = Character.Instance.CharacterData.Inventory.baseSlots[slotIndex].itemStack;

                Character.Instance.CharacterData.Inventory.baseSlots[slotIndex].itemID = template.Slots[containerSlotIndex].ItemID;
                Character.Instance.CharacterData.Inventory.baseSlots[slotIndex].itemDataID = template.Slots[containerSlotIndex].ItemDataID;
                Character.Instance.CharacterData.Inventory.baseSlots[slotIndex].itemStack = template.Slots[containerSlotIndex].Stack;
                
                template.Slots[containerSlotIndex].ItemID = cachedNewIndexItemID;
                template.Slots[containerSlotIndex].ItemDataID = cachedNewIndexItemDataID;
                template.Slots[containerSlotIndex].Stack = cachedNewIndexItemStack;
            }

            GameEvents.Instance.OnContainerContentChanged();
            GeneralEvents.Instance.OnInventoryUpdated();
        }
    }
}
