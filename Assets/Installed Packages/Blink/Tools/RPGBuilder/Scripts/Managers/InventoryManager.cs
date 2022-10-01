using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Data;
using BLINK.RPGBuilder.Logic;
using BLINK.RPGBuilder.World;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class InventoryManager : MonoBehaviour
    {
        public Transform draggedItemParent;
        public GameObject draggedItemImage;

        
        
        private void Awake()
        {
            if (Instance != null) return;
            Instance = this;
        }

        public void LootWorldDroppedItem(WorldDroppedItem worldDroppedItemREF)
        {
            for (int i = 0; i < GameState.allWorldDroppedItems.Count; i++)
            {
                if (GameState.allWorldDroppedItems[i].worldDroppedItemREF != worldDroppedItemREF) continue;
                int itemsLeftOver = RPGBuilderUtilities.HandleItemLooting(GameState.allWorldDroppedItems[i].item.ID, GameState.allWorldDroppedItems[i].itemDataID,
                    GameState.allWorldDroppedItems[i].count, false, true);
                if (itemsLeftOver == 0)
                {
                    RPGBuilderUtilities.SetNewItemDataState(GameState.allWorldDroppedItems[i].itemDataID,
                        CharacterEntries.ItemEntryState.InBag);
                    Destroy(GameState.allWorldDroppedItems[i].worldDroppedItemREF.gameObject);
                    GameState.allWorldDroppedItems.RemoveAt(i);
                }
                else
                {
                    UIEvents.Instance.OnShowAlertMessage("The inventory is full", 3);
                }

                return;
            }
        }

        public int getWorldDroppedItemDataID(WorldDroppedItem worldDroppedItemREF)
        {
            foreach (var t in  GameState.allWorldDroppedItems)
            {
                if (t.worldDroppedItemREF != worldDroppedItemREF) continue;
                return t.itemDataID;
            }

            return -1;
        }

        public void DestroyWorldDroppedItem(WorldDroppedItem worldItemREF)
        {
            for (var index = 0; index <  GameState.allWorldDroppedItems.Count; index++)
            {
                var v =  GameState.allWorldDroppedItems[index];
                if (v.worldDroppedItemREF != worldItemREF) continue;
                Destroy(v.worldDroppedItemREF.gameObject);
                GameState.allWorldDroppedItems.RemoveAt(index);
            }
        }


        public void InitEquippedItems()
        {
            foreach (var t in Character.Instance.CharacterData.ArmorPiecesEquipped)
                if (t.itemID != -1)
                    InitEquipArmor(GameDatabase.Instance.GetItems()[t.itemID], t.itemDataID);

            for (int i = 0; i < Character.Instance.CharacterData.WeaponsEquipped.Count; i++)
            {
                if (Character.Instance.CharacterData.WeaponsEquipped[i].itemID == -1) continue;
                InitEquipWeapon(GameDatabase.Instance.GetItems()[Character.Instance.CharacterData.WeaponsEquipped[i].itemID],
                    i, Character.Instance.CharacterData.WeaponsEquipped[i].itemDataID);
            }
        }


        
        public void SellItemToMerchant(int itemID, int count, int bagIndex, int bagSlotIndex)
        {
            RemoveItem(itemID, -1, count, bagIndex, bagSlotIndex, true);
            var itemREF = GameDatabase.Instance.GetItems()[itemID];
            for (var i = 0; i < count; i++) AddCurrency(itemREF.sellCurrencyID, itemREF.sellPrice);
        }



        public void HideAllItemsMainMenu(PlayerAppearance appearance)
        {
            foreach (var armorPiece in appearance.armorPieces)
            {
                if(armorPiece == null) continue;
                armorPiece.SetActive(false);
            }

            if (appearance.weapon1GO != null) Destroy(appearance.weapon1GO);
            if (appearance.weapon2GO != null) Destroy(appearance.weapon2GO);

            foreach (var bodyPart in appearance.BodyParts)
            {
                if(bodyPart == null) continue;
                if(bodyPart.bodyRenderer != null) bodyPart.bodyRenderer.enabled = true;
            }
        }

        public void InitEquipItemMainMenu(RPGItem itemToEquip, PlayerAppearance appearanceRef, int i, int itemDataID)
        {
            if (itemToEquip == null || !itemToEquip.ItemType.CanBeEquipped) return;
            if (itemToEquip.ItemType.EquipType == EconomyData.EquipFunction.Armor)
            {
                if (itemToEquip.itemModelName != "")
                {
                    appearanceRef.ShowArmor(itemToEquip, getMeshManager(itemDataID));
                }
            }
            else if (itemToEquip.ItemType.EquipType == EconomyData.EquipFunction.Weapon)
            {
                var weaponID = 0;
                switch (i)
                {
                    case 0:
                        weaponID = 1;
                        break;
                    case 1:
                        weaponID = 2;
                        break;
                }

                if (itemToEquip.weaponModel != null) appearanceRef.ShowWeapon(itemToEquip, weaponID, getMeshManager(itemDataID), false);
            }
        }
        
        public void InitEquipClassItemMainMenu(RPGItem itemToEquip, PlayerAppearance appearanceRef, int i)
        {
            if (!itemToEquip.ItemType.CanBeEquipped) return;
            if (itemToEquip.ItemType.EquipType == EconomyData.EquipFunction.Armor)
            {
                if (itemToEquip.itemModelName != "")
                {
                    appearanceRef.ShowArmor(itemToEquip, null);
                }
            }
            else if (itemToEquip.ItemType.EquipType == EconomyData.EquipFunction.Weapon)
            {
                var weaponID = 0;
                switch (itemToEquip.slotType)
                {
                    case "TWO HAND":
                        weaponID = 1;
                        break;
                    case "MAIN HAND":
                        weaponID = 1;
                        break;
                    case "OFF HAND":
                        weaponID = 2;
                        break;
                    case "ANY HAND":
                        weaponID = appearanceRef.weapon1GO == null ? 1 : 2;
                        break;
                }

                if (itemToEquip.weaponModel != null) appearanceRef.ShowWeapon(itemToEquip, weaponID, null, false);
            }
        }

        private void InitEquipArmor(RPGItem itemToEquip, int itemDataID)
        {
            if (itemToEquip.ItemType.EquipType != EconomyData.EquipFunction.Armor) return;
            int armorSlotIndex = RPGBuilderUtilities.getArmorSlotIndex(itemToEquip.ArmorSlot);
            GameState.playerEntity.equippedArmors[armorSlotIndex].item = itemToEquip;
            GameState.playerEntity.equippedArmors[armorSlotIndex].temporaryItemDataID = itemDataID;

            if (itemToEquip.itemModelName != "")
            {
                GameState.playerEntity.appearance.ShowArmor(itemToEquip, getMeshManager(itemDataID));
            }

            StatCalculator.CalculateItemStats();
            
            GeneralEvents.Instance.OnPlayerEquippedItem(itemToEquip);
        }

        private void InitEquipWeapon(RPGItem itemToEquip, int weaponIndex, int itemDataID)
        {
            if (itemToEquip.ItemType.EquipType != EconomyData.EquipFunction.Weapon) return;
            // EQUIP WEAPON

            GameState.playerEntity.equippedWeapons[weaponIndex].item = itemToEquip;
            GameState.playerEntity.equippedWeapons[weaponIndex].temporaryItemDataID = itemDataID;

            var weaponID = 0;
            switch (weaponIndex)
            {
                case 0:
                {
                    weaponID = 1;
                    if (itemToEquip.autoAttackAbilityID != -1)
                    {
                        GameState.playerEntity.autoAttackData.CurrentAutoAttackAbilityID =
                            itemToEquip.autoAttackAbilityID;
                        GameState.playerEntity.autoAttackData.WeaponItem =
                            itemToEquip;
                    }

                    break;
                }
                case 1:
                {
                    weaponID = 2;
                    if (GameState.playerEntity.autoAttackData.CurrentAutoAttackAbilityID == -1 &&
                        itemToEquip.autoAttackAbilityID != -1)
                    {
                        GameState.playerEntity.autoAttackData.CurrentAutoAttackAbilityID =
                            itemToEquip.autoAttackAbilityID;
                        GameState.playerEntity.autoAttackData.WeaponItem =
                            itemToEquip;
                    }

                    break;
                }
            }

            if (itemToEquip.weaponModel != null)
                GameState.playerEntity.appearance.ShowWeapon(itemToEquip, weaponID, getMeshManager(itemDataID), GameState.playerEntity.IsInCombat());

            StatCalculator.CalculateItemStats();
            GeneralEvents.Instance.OnPlayerEquippedItem(itemToEquip);
        }

        public void UseItemFromBar(RPGItem itemToUse)
        {
            int slotIndex = -1;

            for (int i = 0; i < Character.Instance.CharacterData.Inventory.baseSlots.Count; i++)
            {
                if (Character.Instance.CharacterData.Inventory.baseSlots[i].itemID == -1) continue;
                if (GameDatabase.Instance.GetItems()[Character.Instance.CharacterData.Inventory.baseSlots[i].itemID] == itemToUse)
                {
                    slotIndex = i;
                }
            }
            
            UseItem(itemToUse, 0 , slotIndex);
        }
        
        public void UseItem(RPGItem itemUsed, int bagIndex, int slotIndex)
        {
            if (!RequirementsManager.Instance.RequirementsMet(GameState.playerEntity, (itemUsed.UseRequirementsTemplate && itemUsed.RequirementsTemplate != null) ? itemUsed.RequirementsTemplate.Requirements : itemUsed.Requirements).Result) return;
            
            GameActionsManager.Instance.TriggerGameActions(GameState.playerEntity,
                (itemUsed.UseRequirementsTemplate && itemUsed.GameActionsTemplate != null) ? itemUsed.GameActionsTemplate.GameActions : itemUsed.GameActions);

            if (itemUsed.ItemType.CanBeEquipped)
            {
                EquipItem(itemUsed, bagIndex, slotIndex, Character.Instance.CharacterData.Inventory.baseSlots[slotIndex].itemDataID);
            }
            
            GeneralEvents.Instance.OnPlayerUsedItem(itemUsed);
            ActionBarManager.Instance.CheckItemBarState();
        }

        GameObject getMeshManager(int itemDataID)
        {
            CharacterEntries.ItemEntry itemEntryRef = RPGBuilderUtilities.GetItemDataFromDataID(itemDataID);
            if (itemEntryRef == null || itemEntryRef.enchantmentID == -1) return null;
            RPGEnchantment enchantRef = GameDatabase.Instance.GetEnchantments()[itemEntryRef.enchantmentID];
            if (enchantRef != null && enchantRef.enchantmentTiers[itemEntryRef.enchantmentTierIndex].enchantingParticle != null)
            {
                return enchantRef.enchantmentTiers[itemEntryRef.enchantmentTierIndex]
                    .enchantingParticle;
            }

            return null;
        }
        
        

        private void EquipItem(RPGItem itemToEquip, int bagIndex, int slotIndex, int itemDataID)
        {
            if (!itemToEquip.ItemType.CanBeEquipped) return;
            if (itemToEquip.ItemType.EquipType == EconomyData.EquipFunction.Armor)
            {
                int armorSlotIndex = RPGBuilderUtilities.getArmorSlotIndex(itemToEquip.ArmorSlot);
                var itemToUnequip = RPGBuilderUtilities.getEquippedArmor(itemToEquip.ArmorSlot);
                if (itemToUnequip != null)
                {
                    UnequipItem(itemToUnequip, 0);
                }

                GameState.playerEntity.equippedArmors[armorSlotIndex].item = itemToEquip;
                GameState.playerEntity.equippedArmors[armorSlotIndex].temporaryItemDataID = itemDataID;

                Character.Instance.CharacterData.ArmorPiecesEquipped[armorSlotIndex].itemID = itemToEquip.ID;
                Character.Instance.CharacterData.ArmorPiecesEquipped[armorSlotIndex].itemDataID = itemDataID;

                RemoveEquippedItem(bagIndex, slotIndex);

                if (itemToEquip.itemModelName != "")
                {
                    GameState.playerEntity.appearance.ShowArmor(itemToEquip, getMeshManager(itemDataID));
                }
            }
            else if (itemToEquip.ItemType.EquipType == EconomyData.EquipFunction.Weapon)
            {
                RPGItem weaponEquipped1 = GameState.playerEntity.equippedWeapons[0].item;
                RPGItem weaponEquipped2 = GameState.playerEntity.equippedWeapons[1].item;

                bool unequipWeapon1 = false, unequipWeapon2 = false;
                
                int EquippedInIndex = -1;
                int HandVisual = -1;
                
                /// TODO
                /// 
                Character.Instance.CharacterData.Inventory.baseSlots[slotIndex].itemStack = 0;
                Character.Instance.CharacterData.Inventory.baseSlots[slotIndex].itemID = -1;
                Character.Instance.CharacterData.Inventory.baseSlots[slotIndex].itemDataID = -1;
                
                if (weaponEquipped1 == null && weaponEquipped2 == null)
                {
                    if (itemToEquip.WeaponSlot.entryName == "OFF HAND")
                    {
                        EquippedInIndex = 1;
                        HandVisual = 2;
                    }
                    else
                    {
                        EquippedInIndex = 0;
                        HandVisual = 1;
                    }
                } else if (weaponEquipped1 != null && weaponEquipped2 == null)
                {
                    switch (itemToEquip.WeaponSlot.entryName)
                    {
                        case "TWO HAND":
                        case "MAIN HAND":
                        {
                            unequipWeapon1 = true;
                            EquippedInIndex = 0;
                            HandVisual = 1;
                            break;
                        }
                        case "ANY HAND" when weaponEquipped1.WeaponSlot.entryName == "MAIN HAND" ||
                                             weaponEquipped1.WeaponSlot.entryName == "ANY HAND":
                        {
                            EquippedInIndex = 1;
                            HandVisual = 2;
                            break;
                        }
                        case "ANY HAND":
                            unequipWeapon1 = true;
                            EquippedInIndex = 0;
                            HandVisual = 1;
                            break;
                        case "OFF HAND":
                        {
                            if (weaponEquipped1.WeaponSlot.entryName == "TWO HAND") UnequipItem(weaponEquipped1, 1);
                            EquippedInIndex = 1;
                            HandVisual = 2;
                            break;
                        }
                    }
                } else if (weaponEquipped1 == null && weaponEquipped2 != null)
                {
                    switch (itemToEquip.WeaponSlot.entryName)
                    {
                        case "TWO HAND":
                        {
                            unequipWeapon2 = true;
                            EquippedInIndex = 0;
                            HandVisual = 1;
                            break;
                        }
                        case "MAIN HAND":
                        case "ANY HAND":
                        {
                            EquippedInIndex = 0;
                            HandVisual = 1;
                            break;
                        }
                        case "OFF HAND":
                        {
                            unequipWeapon2 = true;
                            EquippedInIndex = 1;
                            HandVisual = 2;
                            break;
                        }
                    }
                } else if (weaponEquipped1 != null && weaponEquipped2 != null)
                {
                    switch (itemToEquip.WeaponSlot.entryName)
                    {
                        case "TWO HAND":
                        {
                            unequipWeapon1 = true;
                            unequipWeapon2 = true;
                            EquippedInIndex = 0;
                            HandVisual = 1;
                            break;
                        }
                        case "MAIN HAND":
                        case "ANY HAND":
                        {
                            unequipWeapon1 = true;
                            EquippedInIndex = 0;
                            HandVisual = 1;
                            break;
                        }
                        case "OFF HAND":
                        {
                            unequipWeapon2 = true;
                            EquippedInIndex = 1;
                            HandVisual = 2;
                            break;
                        }
                    }
                }

                if (unequipWeapon1 && unequipWeapon2)
                {
                    if (getEmptySlotsCount() < 2)
                    {
                        UIEvents.Instance.OnShowAlertMessage("The inventory is full", 3);
                        return;
                    }
                } else if (unequipWeapon1 || unequipWeapon2)
                {
                    if (getEmptySlotsCount() < 1)
                    {
                        UIEvents.Instance.OnShowAlertMessage("The inventory is full", 3);
                        return;
                    }
                }

                if (unequipWeapon1)
                {
                    UnequipItem(weaponEquipped1, 1);
                }
                if (unequipWeapon2)
                {
                    UnequipItem(weaponEquipped2, 2);
                }

                GameState.playerEntity.equippedWeapons[EquippedInIndex].item = itemToEquip;
                GameState.playerEntity.equippedWeapons[EquippedInIndex].temporaryItemDataID = itemDataID;
                Character.Instance.CharacterData.WeaponsEquipped[EquippedInIndex].itemID = itemToEquip.ID;
                Character.Instance.CharacterData.WeaponsEquipped[EquippedInIndex].itemDataID = itemDataID;

                if (itemToEquip.weaponModel != null)
                {
                    GameState.playerEntity.appearance.ShowWeapon(itemToEquip, HandVisual, getMeshManager(itemDataID), GameState.playerEntity.IsInCombat());
                }

                if (EquippedInIndex == 0)
                {
                    if (GameState.playerEntity.equippedWeapons[1].item == null)
                    {
                        if (GameState.playerEntity.equippedWeapons[0].item.autoAttackAbilityID != -1)
                        {
                            GameState.playerEntity.autoAttackData.CurrentAutoAttackAbilityID =
                                GameState.playerEntity.equippedWeapons[0].item.autoAttackAbilityID;
                            GameState.playerEntity.autoAttackData.WeaponItem =
                                GameState.playerEntity.equippedWeapons[0].item;
                        }
                    }
                    else
                    {
                        if (GameState.playerEntity.equippedWeapons[0].item.autoAttackAbilityID == -1)
                            if (GameState.playerEntity.equippedWeapons[1].item.autoAttackAbilityID != -1)
                            {
                                GameState.playerEntity.autoAttackData.CurrentAutoAttackAbilityID =
                                    GameState.playerEntity.equippedWeapons[1].item.autoAttackAbilityID;
                                GameState.playerEntity.autoAttackData.WeaponItem =
                                    GameState.playerEntity.equippedWeapons[1].item;
                            }
                    }
                }
                else
                {
                    if (GameState.playerEntity.equippedWeapons[0].item == null)
                    {
                        if (GameState.playerEntity.equippedWeapons[1].item.autoAttackAbilityID != -1)
                        {
                            GameState.playerEntity.autoAttackData.CurrentAutoAttackAbilityID =
                                GameState.playerEntity.equippedWeapons[1].item.autoAttackAbilityID;
                            GameState.playerEntity.autoAttackData.WeaponItem =
                                GameState.playerEntity.equippedWeapons[1].item;
                        }
                    }
                    else
                    {
                        if (GameState.playerEntity.equippedWeapons[0].item.autoAttackAbilityID != -1)
                        {
                            GameState.playerEntity.autoAttackData.CurrentAutoAttackAbilityID =
                                GameState.playerEntity.equippedWeapons[0].item.autoAttackAbilityID;
                            GameState.playerEntity.autoAttackData.WeaponItem =
                                GameState.playerEntity.equippedWeapons[0].item;
                        }
                    }
                }
                
                BonusManager.Instance.InitBonuses();
                GameState.playerEntity.appearance.HandleAnimatorOverride();
            }

            RPGBuilderUtilities.SetNewItemDataState(itemDataID, CharacterEntries.ItemEntryState.Equipped);
            StatCalculator.CalculateItemStats();
            RPGBuilderUtilities.UpdateActionAbilities(itemToEquip);
            GeneralEvents.Instance.OnPlayerEquippedItem(itemToEquip);
        }

        public void UnequipItem(RPGItem itemToUnequip, int weaponID)
        {
            int cachedItemDataID = -1;
            if (itemToUnequip.ItemType.EquipType == EconomyData.EquipFunction.Armor)
            {
                int armorSlotIndex = RPGBuilderUtilities.getArmorSlotIndex(itemToUnequip.ArmorSlot);
                cachedItemDataID = GameState.playerEntity.equippedArmors[armorSlotIndex].temporaryItemDataID;
                GameState.playerEntity.equippedArmors[armorSlotIndex].item = null;
                GameState.playerEntity.equippedArmors[armorSlotIndex].temporaryItemDataID = -1;
                
                Character.Instance.CharacterData.ArmorPiecesEquipped[armorSlotIndex].itemID = -1;
                Character.Instance.CharacterData.ArmorPiecesEquipped[armorSlotIndex].itemDataID = -1;

                if (itemToUnequip.itemModelName != "")
                {
                    GameState.playerEntity.appearance.HideArmor(itemToUnequip);
                }
                    
                RPGBuilderUtilities.SetNewItemDataState(
                    GameState.playerEntity.equippedArmors[armorSlotIndex].temporaryItemDataID, CharacterEntries.ItemEntryState.InBag);
            }
            else if (itemToUnequip.ItemType.EquipType == EconomyData.EquipFunction.Weapon)
            {
                if (weaponID == 1)
                {
                    cachedItemDataID = GameState.playerEntity.equippedWeapons[0].temporaryItemDataID;
                    GameState.playerEntity.equippedWeapons[0].item = null;
                    GameState.playerEntity.equippedWeapons[0].temporaryItemDataID = -1;
                    Character.Instance.CharacterData.WeaponsEquipped[0].itemID = -1;
                    Character.Instance.CharacterData.WeaponsEquipped[0].itemDataID = -1;
                }
                else
                {
                    cachedItemDataID = GameState.playerEntity.equippedWeapons[1].temporaryItemDataID;
                    GameState.playerEntity.equippedWeapons[1].item = null;
                    GameState.playerEntity.equippedWeapons[1].temporaryItemDataID = -1;
                    Character.Instance.CharacterData.WeaponsEquipped[1].itemID = -1;
                    Character.Instance.CharacterData.WeaponsEquipped[1].itemDataID = -1;
                }

                if (itemToUnequip.weaponModel != null)
                    GameState.playerEntity.appearance.HideWeapon(weaponID);

                BonusManager.Instance.CancelBonusFromUnequippedWeapon(itemToUnequip.WeaponType);

                if (!GameDatabase.Instance.GetCharacterSettings().NoClasses)
                {
                    if (weaponID == 1)
                    {
                        if (GameState.playerEntity.equippedWeapons[1].item == null)
                        {
                            GameState.playerEntity.autoAttackData.CurrentAutoAttackAbilityID =
                                GameDatabase.Instance.GetClasses()[Character.Instance.CharacterData.ClassID].autoAttackAbilityID;
                            GameState.playerEntity.autoAttackData.WeaponItem = null;
                        }
                        else
                        {
                            if (GameState.playerEntity.equippedWeapons[1].item.autoAttackAbilityID != -1)
                            {
                                GameState.playerEntity.autoAttackData.CurrentAutoAttackAbilityID =
                                    GameState.playerEntity.equippedWeapons[1].item.autoAttackAbilityID;
                                GameState.playerEntity.autoAttackData.WeaponItem =
                                    GameState.playerEntity.equippedWeapons[1].item;
                            }
                            else
                            {
                                GameState.playerEntity.autoAttackData.CurrentAutoAttackAbilityID =
                                    GameDatabase.Instance.GetClasses()[Character.Instance.CharacterData.ClassID]
                                        .autoAttackAbilityID;
                                GameState.playerEntity.autoAttackData.WeaponItem = null;
                            }
                        }
                    }
                    else
                    {
                        if (GameState.playerEntity.equippedWeapons[0].item == null)
                        {
                            GameState.playerEntity.autoAttackData.CurrentAutoAttackAbilityID =
                                GameDatabase.Instance.GetClasses()[Character.Instance.CharacterData.ClassID].autoAttackAbilityID;
                            GameState.playerEntity.autoAttackData.WeaponItem = null;
                        }
                        else
                        {
                            if (GameState.playerEntity.equippedWeapons[0].item.autoAttackAbilityID != -1)
                            {
                                GameState.playerEntity.autoAttackData.CurrentAutoAttackAbilityID =
                                    GameState.playerEntity.equippedWeapons[0].item.autoAttackAbilityID;
                                GameState.playerEntity.autoAttackData.WeaponItem =
                                    GameState.playerEntity.equippedWeapons[0].item;
                            }
                            else
                            {
                                GameState.playerEntity.autoAttackData.CurrentAutoAttackAbilityID =
                                    GameDatabase.Instance.GetClasses()[Character.Instance.CharacterData.ClassID]
                                        .autoAttackAbilityID;
                                GameState.playerEntity.autoAttackData.WeaponItem = null;
                            }
                        }
                    }
                }

                GameState.playerEntity.appearance.HandleAnimatorOverride();
            }

            RPGBuilderUtilities.SetNewItemDataState(cachedItemDataID, CharacterEntries.ItemEntryState.InBag);
            AddItem(itemToUnequip.ID, 1, false, cachedItemDataID);

            StatCalculator.CalculateItemStats();
            RPGBuilderUtilities.CheckRemoveActionAbilities(itemToUnequip);
            
            GeneralEvents.Instance.OnPlayerUnequippedItem(itemToUnequip);
        }

        private void RemoveEquippedItem(int bagIndex, int slotIndex)
        {
            Character.Instance.CharacterData.Inventory.baseSlots[slotIndex].itemStack = 0;
            Character.Instance.CharacterData.Inventory.baseSlots[slotIndex].itemID = -1;
            Character.Instance.CharacterData.Inventory.baseSlots[slotIndex].itemDataID = -1;
        }

        public void AddCurrency(int currencyID, int amount)
        {
            foreach (var t in Character.Instance.CharacterData.Currencies)
            {
                if (t.currencyID != currencyID) continue;
                var currencyREF = GameDatabase.Instance.GetCurrencies()[t.currencyID];
                if (currencyREF == null) continue;
                var curCurrencyAmount = t.amount;
                if (GameDatabase.Instance.GetCurrencies().TryGetValue(currencyREF.convertToCurrencyID, out var convertCurrencyREF) &&
                    currencyREF.AmountToConvert > 0)
                {
                    // CONVERT CURRENCY
                    if (curCurrencyAmount + amount >= currencyREF.AmountToConvert)
                    {
                        var amountToAdd = amount;
                        while (amountToAdd + curCurrencyAmount >= currencyREF.AmountToConvert)
                        {
                            var amountAdded = currencyREF.AmountToConvert - curCurrencyAmount;
                            curCurrencyAmount = 0;
                            amountToAdd -= amountAdded;

                            // ADD THE CONVERTED CURRENCY
                            AddCurrency(convertCurrencyREF.ID, 1);
                        }

                        curCurrencyAmount = amountToAdd;
                    }
                    else
                    {
                        curCurrencyAmount += amount;
                    }
                }
                else
                {
                    if (currencyREF.maxValue > 0 && curCurrencyAmount + amount >= currencyREF.maxValue)
                        curCurrencyAmount = currencyREF.maxValue;
                    else
                        curCurrencyAmount += amount;
                }

                t.amount = curCurrencyAmount;
                GeneralEvents.Instance.OnPlayerCurrencyChanged(currencyREF);
            }
        }
        
        public void RemoveCurrency(int currencyID, int amount)
        {
            foreach (var t in Character.Instance.CharacterData.Currencies)
            {
                if (t.currencyID != currencyID) continue;
                var currencyREF = GameDatabase.Instance.GetCurrencies()[t.currencyID];
                if (currencyREF == null) continue;
                t.amount -= amount;
                if (t.amount < 0) t.amount = 0;
                GeneralEvents.Instance.OnPlayerCurrencyChanged(currencyREF);
            }
        }

        public void MoveItem(int prevBagIndex, int prevSlotIndex, int newBagIndex, int newSlotIndex)
        {
            if (Character.Instance.CharacterData.Inventory.baseSlots[newSlotIndex].itemID != -1)
            {
                var previousSlot = new CharacterEntries.InventorySlotEntry
                {
                    itemDataID = Character.Instance.CharacterData.Inventory.baseSlots[prevSlotIndex].itemDataID,
                    itemID = Character.Instance.CharacterData.Inventory.baseSlots[prevSlotIndex].itemID,
                    itemStack = Character.Instance.CharacterData.Inventory.baseSlots[prevSlotIndex].itemStack
                };

                var newSlot = new CharacterEntries.InventorySlotEntry
                {
                    itemDataID = Character.Instance.CharacterData.Inventory.baseSlots[newSlotIndex].itemDataID,
                    itemID = Character.Instance.CharacterData.Inventory.baseSlots[newSlotIndex].itemID,
                    itemStack = Character.Instance.CharacterData.Inventory.baseSlots[newSlotIndex].itemStack
                };

                // Get current stack size
                var prevStackSize = previousSlot.itemStack;
                var newStackSize = newSlot.itemStack;

                RPGItem newItemREF = GameDatabase.Instance.GetItems()[newSlot.itemID];

                if (previousSlot.itemID == newSlot.itemID && newStackSize < newItemREF.stackLimit)
                {
                    // Total stack count less then or equal to stack limit
                    if (prevStackSize + newStackSize <= newItemREF.stackLimit)
                    {
                        Character.Instance.CharacterData.Inventory.baseSlots[newSlotIndex].itemDataID =
                            previousSlot.itemDataID;
                        Character.Instance.CharacterData.Inventory.baseSlots[newSlotIndex].itemID = previousSlot.itemID;
                        Character.Instance.CharacterData.Inventory.baseSlots[newSlotIndex].itemStack =
                            prevStackSize + newStackSize;

                        Character.Instance.CharacterData.Inventory.baseSlots[prevSlotIndex].itemDataID = -1;
                        Character.Instance.CharacterData.Inventory.baseSlots[prevSlotIndex].itemID = -1;
                        Character.Instance.CharacterData.Inventory.baseSlots[prevSlotIndex].itemStack = 0;
                    }
                    else
                    {
                        var removedFromStack = newItemREF.stackLimit - newStackSize;

                        Character.Instance.CharacterData.Inventory.baseSlots[newSlotIndex].itemDataID = newSlot.itemDataID;
                        Character.Instance.CharacterData.Inventory.baseSlots[newSlotIndex].itemID = newSlot.itemID;
                        Character.Instance.CharacterData.Inventory.baseSlots[newSlotIndex].itemStack = newItemREF.stackLimit;

                        Character.Instance.CharacterData.Inventory.baseSlots[prevSlotIndex].itemDataID =
                            previousSlot.itemDataID;
                        Character.Instance.CharacterData.Inventory.baseSlots[prevSlotIndex].itemID = previousSlot.itemID;
                        Character.Instance.CharacterData.Inventory.baseSlots[prevSlotIndex].itemStack =
                            prevStackSize - removedFromStack;
                    }
                }
                else
                {
                    Character.Instance.CharacterData.Inventory.baseSlots[newSlotIndex].itemDataID = previousSlot.itemDataID;
                    Character.Instance.CharacterData.Inventory.baseSlots[newSlotIndex].itemID = previousSlot.itemID;
                    Character.Instance.CharacterData.Inventory.baseSlots[newSlotIndex].itemStack = previousSlot.itemStack;

                    Character.Instance.CharacterData.Inventory.baseSlots[prevSlotIndex].itemDataID = newSlot.itemDataID;
                    Character.Instance.CharacterData.Inventory.baseSlots[prevSlotIndex].itemID = newSlot.itemID;
                    Character.Instance.CharacterData.Inventory.baseSlots[prevSlotIndex].itemStack = newSlot.itemStack;
                }
            }
            else
            {
                Character.Instance.CharacterData.Inventory.baseSlots[newSlotIndex].itemDataID =
                    Character.Instance.CharacterData.Inventory.baseSlots[prevSlotIndex].itemDataID;
                Character.Instance.CharacterData.Inventory.baseSlots[newSlotIndex].itemID =
                    Character.Instance.CharacterData.Inventory.baseSlots[prevSlotIndex].itemID;
                Character.Instance.CharacterData.Inventory.baseSlots[newSlotIndex].itemStack =
                    Character.Instance.CharacterData.Inventory.baseSlots[prevSlotIndex].itemStack;

                Character.Instance.CharacterData.Inventory.baseSlots[prevSlotIndex].itemID = -1;
                Character.Instance.CharacterData.Inventory.baseSlots[prevSlotIndex].itemDataID = -1;
                Character.Instance.CharacterData.Inventory.baseSlots[prevSlotIndex].itemStack = 0;
            }
            GeneralEvents.Instance.OnInventoryUpdated();
        }

        public void RemoveItem(int itemID, int itemDataID, int amount, int bagIndex, int bagSlotIndex,
            bool removeAtSlot)
        {
            if (itemID == -1) return;
            if (removeAtSlot)
            {
                if (Character.Instance.CharacterData.Inventory.baseSlots[bagSlotIndex].itemStack == amount)
                {
                    Character.Instance.CharacterData.Inventory.baseSlots[bagSlotIndex].itemID = -1;
                    Character.Instance.CharacterData.Inventory.baseSlots[bagSlotIndex].itemDataID = -1;
                    Character.Instance.CharacterData.Inventory.baseSlots[bagSlotIndex].itemStack = 0;
                }
                else if (Character.Instance.CharacterData.Inventory.baseSlots[bagSlotIndex].itemStack > amount)
                {
                    Character.Instance.CharacterData.Inventory.baseSlots[bagSlotIndex].itemStack -= amount;
                }
            }
            else
            {
                foreach (var slot in Character.Instance.CharacterData.Inventory.baseSlots.Where(slot =>
                    slot.itemID != -1 && slot.itemID == itemID))
                {
                    if (slot.itemStack == amount)
                    {
                        slot.itemDataID = -1;
                        slot.itemID = -1;
                        slot.itemStack = 0;
                        break;
                    }

                    if (slot.itemStack > amount)
                    {
                        slot.itemStack -= amount;
                        break;
                    }

                    if (slot.itemStack >= amount) continue;
                    var remainingStacks = amount - slot.itemStack;
                    slot.itemDataID = -1;
                    slot.itemID = -1;
                    slot.itemStack = 0;
                    RemoveItem(itemID, itemDataID, remainingStacks, -1, -1, false);
                    break;
                }
            }

            if (UIEvents.Instance.IsPanelOpen("Crafting")) UIEvents.Instance.OnUpdateCraftingPanel();
            ActionBarManager.Instance.CheckItemBarState();
            GeneralEvents.Instance.OnPlayerLostItem(GameDatabase.Instance.GetItems()[itemID], amount);

            RPGItem item = GameDatabase.Instance.GetItems()[itemID];
            if (item != null)
            {
                if (item.ItemType.CanBeEquipped)
                {
                    int randomizedItemIndex = RPGBuilderUtilities.GetRandomItemIndexFromDataID(itemDataID);
                    if (randomizedItemIndex != -1)
                    {
                        Character.Instance.CharacterData.RandomizedItems.Remove(
                            Character.Instance.CharacterData.RandomizedItems[randomizedItemIndex]);
                    }

                    if (itemDataID != -1)
                    {
                        Character.Instance.CharacterData.ItemEntries.Remove(
                            Character.Instance.CharacterData.ItemEntries[
                                RPGBuilderUtilities.GetItemDataIndexFromDataID(itemDataID)]);
                    }
                }
            }
        }


        public int canGetItem(int itemID, int Amount)
        {
            RPGItem itemREF = GameDatabase.Instance.GetItems()[itemID];
            ItemCheckDATA itemCheckData = slotsNeededForLoot(itemID, Amount, itemREF.stackLimit);
            return itemCheckData.canLootAmount;
        }

        public int getEmptySlotsCount()
        {
            int total = 0;
            foreach (var slot in Character.Instance.CharacterData.Inventory.baseSlots)
            {
                if (slot.itemID == -1) total++;
            }

            return total;
        }

        public class ItemCheckDATA
        {
            public bool canBeLooted;
            public int canLootAmount;
            public int slotsNeeded;
        }

        public ItemCheckDATA slotsNeededForLoot(int itemID, int count, int stackMax)
        {
            ItemCheckDATA newItemCheckData = new ItemCheckDATA();

            int StacksNeededLeft = count;
            int total = 0;
            int canLootMax = 0;
            
                foreach (var slot in Character.Instance.CharacterData.Inventory.baseSlots)
                {
                    if (slot.itemID != -1)
                    {
                        if (slot.itemID != itemID) continue;
                        if (slot.itemStack == stackMax) continue;
                        StacksNeededLeft -= stackMax - slot.itemStack;
                        canLootMax += stackMax - slot.itemStack;
                    }
                    else
                    {
                        StacksNeededLeft -= stackMax;
                        canLootMax += stackMax;
                        total++;
                    }

                    if (StacksNeededLeft < 0)
                    {
                        StacksNeededLeft = 0;
                    }

                    if (StacksNeededLeft == 0)
                    {
                        break;
                    }
                }

            newItemCheckData.slotsNeeded = total;
            newItemCheckData.canLootAmount = canLootMax;
            return newItemCheckData;
        }


        public void AddItem(int itemID, int Amount, bool automaticallyEquip, int itemDataID)
        {
            if (itemID == -1) return;
            var itemToAdd = GameDatabase.Instance.GetItems()[itemID];
            var stacked = false;

            if (itemToAdd.ItemType.ItemTypeFunction == EconomyData.ItemTypeFunction.Currency)
            {
                if (itemToAdd.convertToCurrency != -1)
                {
                    AddCurrency(itemToAdd.convertToCurrency, Amount);
                }
            }
            else
            {
                int slotIndex = -1;
                foreach (var slot in Character.Instance.CharacterData.Inventory.baseSlots)
                {
                    slotIndex++;
                    if (slot.itemID == -1 || slot.itemID != itemID) continue;
                    if (slot.itemStack >= itemToAdd.stackLimit) continue;
                    if (slot.itemStack + Amount > itemToAdd.stackLimit) continue;
                    slot.itemStack += Amount;
                    GeneralEvents.Instance.OnPlayerGainedItem(itemToAdd, Amount);
                    stacked = true;
                    break;
                }

                if (!stacked)
                {
                    slotIndex = -1;
                    foreach (var slot in Character.Instance.CharacterData.Inventory.baseSlots)
                    {
                        slotIndex++;
                        if (slot.itemID != -1) continue;
                        if (Amount <= itemToAdd.stackLimit)
                        {
                            slot.itemID = itemToAdd.ID;
                            slot.itemStack += Amount;
                            slot.itemDataID = itemDataID;

                            GeneralEvents.Instance.OnPlayerGainedItem(itemToAdd, Amount);

                            if (automaticallyEquip)
                            {
                                EquipItem(itemToAdd, -1, slotIndex, slot.itemDataID);
                            }

                            break;
                        }

                        var remainingStacks = Amount - itemToAdd.stackLimit;
                        slot.itemID = itemToAdd.ID;
                        slot.itemStack = itemToAdd.stackLimit;
                        AddItem(itemID, remainingStacks, false, -1);
                        GeneralEvents.Instance.OnPlayerGainedItem(itemToAdd, itemToAdd.stackLimit);
                        if (automaticallyEquip)
                        {
                            EquipItem(itemToAdd, -1, slotIndex,
                                Character.Instance.CharacterData.Inventory.baseSlots[slotIndex].itemDataID);
                        }

                        break;
                    }
                }
            }
            
            if (UIEvents.Instance.IsPanelOpen("Crafting")) UIEvents.Instance.OnUpdateCraftingPanel();

            ActionBarManager.Instance.CheckItemBarState();
        }

        


        

        private bool isitemAlreadyInLootList(int itemID, List<TemporaryLootItemData> lootList)
        {
            return lootList.Any(item => item.itemID == itemID);
        }
        
        
        public List<TemporaryLootItemData> updateItemDataInLootList(int itemID, List<TemporaryLootItemData> lootList, int amount)
        {
            foreach (var item in lootList.Where(item => item.itemID == itemID))
            {
                item.count += amount;
            }

            return lootList;
        }

        public List<TemporaryLootItemData> HandleLootList(int itemID, List<TemporaryLootItemData> lootList, int count)
        {
            if (isitemAlreadyInLootList(itemID, lootList))
            {
                return updateItemDataInLootList(itemID, lootList, count);
            }

            TemporaryLootItemData newCraftedItem = new TemporaryLootItemData();
            newCraftedItem.itemID = itemID;
            newCraftedItem.count = count;
            lootList.Add(newCraftedItem);
            return lootList;
        }

        public class TemporaryLootItemData
        {
            public int itemID;
            public int count;
        }

        
        


        public static InventoryManager Instance { get; private set; }
    }
}